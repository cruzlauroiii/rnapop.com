/*
 * ============================================================
 * RNA Therapy Platform — ESP32-S3 Lab Controller
 * Complete production firmware
 * ============================================================
 *
 * Controls 5 pieces of lab equipment for mRNA synthesis:
 *   1. Heat Block (PID-controlled, DS18B20)
 *   2. Syringe Pump A (stepper, A4988)
 *   3. Syringe Pump B (stepper, A4988)
 *   4. UV Spectrophotometer (260/280nm LEDs + photodiode)
 *   5. Centrifuge (brushless ESC + tachometer)
 *   6. Gel Electrophoresis (relay + current monitor)
 *
 * Communication: WebSocket server on port 9123
 *   Blazor WASM client connects to wss://ESP32_IP:9123
 *   Protocol: JSON over WebSocket (bidirectional)
 *     Client → Server: LabCommandMessage
 *     Server → Client: LabStatus (every 500ms)
 *
 * Hardware: ESP32-S3-DevKitC-1 ($4)
 * Build: PlatformIO (pio run -t upload)
 */

#include <Arduino.h>
#include <WiFi.h>
#include <WebSocketsServer.h>
#include <ArduinoJson.h>
#include <OneWire.h>
#include <DallasTemperature.h>

#include "config.h"
#include "pid.h"
#include "stepper.h"
#include "spectro.h"
#include "centrifuge.h"
#include "gel.h"

#if DUAL_ESP32_MODE
#include <Wire.h>
#endif

// ===== I2C Pump Proxy (dual-ESP32 mode) =====
#if DUAL_ESP32_MODE
namespace I2CPump {
    struct PumpStatus {
        bool running = false;
        float dispensed = 0;
        float flowRate = 0;
        float volume = 0;
        bool atEndstop = false;
    };
    PumpStatus statusA, statusB;

    void begin() {
        Wire.begin(PIN_I2C_SDA, PIN_I2C_SCL);
        Wire.setClock(400000);  // 400kHz Fast I2C
        Serial.println("I2C master initialized for stepper slave.");
    }

    void sendCommand(uint8_t cmd, float flow, float vol, bool forward) {
        uint8_t buf[16] = {0};
        buf[0] = cmd;
        memcpy(buf + 1, &flow, 4);
        memcpy(buf + 5, &vol, 4);
        buf[9] = forward ? 0 : 1;
        Wire.beginTransmission(I2C_STEPPER_ADDR);
        Wire.write(buf, 16);
        Wire.endTransmission();
    }

    void readStatus() {
        Wire.requestFrom(I2C_STEPPER_ADDR, 16);
        if (Wire.available() >= 16) {
            uint8_t buf[16];
            for (int i = 0; i < 16; i++) buf[i] = Wire.read();
            statusA.running = buf[0];
            memcpy(&statusA.dispensed, buf + 1, 4);
            statusB.running = buf[5];
            memcpy(&statusB.dispensed, buf + 6, 4);
            statusA.atEndstop = buf[10];
            statusB.atEndstop = buf[11];
        }
    }

    void startPump(int idx, float flow, float vol, bool forward = true) {
        if (idx == 0) { statusA.flowRate = flow; statusA.volume = vol; }
        else { statusB.flowRate = flow; statusB.volume = vol; }
        sendCommand(idx == 0 ? 1 : 3, flow, vol, forward);
    }

    void stopPump(int idx) {
        sendCommand(idx == 0 ? 2 : 4, 0, 0, true);
    }

    void homePump(int idx) {
        sendCommand(idx == 0 ? 5 : 6, 0, 0, true);
    }
}
#endif

// ===== Global Objects =====

// Temperature
OneWire oneWire(PIN_TEMP_SENSOR);
DallasTemperature tempSensor(&oneWire);
float currentTemp = 22.0;
float targetTemp = 37.0;
bool heaterOn = false;
bool heaterAtTarget = false;
unsigned long heaterStartTime = 0;

// PID
PidController pid(PID_KP, PID_KI, PID_KD, PID_INTEGRAL_LIMIT);

// Syringe Pumps
StepperPump pumpA(PIN_PUMP_A_STEP, PIN_PUMP_A_DIR, PIN_PUMP_A_EN,
                  PIN_PUMP_A_ENDSTOP, STEPS_PER_ML, MAX_STEP_FREQ);
StepperPump pumpB(PIN_PUMP_B_STEP, PIN_PUMP_B_DIR, PIN_PUMP_B_EN,
                  PIN_PUMP_B_ENDSTOP, STEPS_PER_ML, MAX_STEP_FREQ);

// Spectrophotometer
Spectrophotometer spectro;

// Centrifuge
Centrifuge centrifuge;

// Gel
GelElectrophoresis gel;

// WebSocket server
WebSocketsServer ws(WSS_PORT);

// Timing
unsigned long lastStatusBroadcast = 0;
unsigned long lastTempRead = 0;
unsigned long lastPidUpdate = 0;
unsigned long lastSlowUpdate = 0;

// ===== WiFi Setup =====

void setupWiFi() {
    WiFi.mode(WIFI_STA);
    WiFi.begin(WIFI_SSID, WIFI_PASS);

    Serial.print("Connecting to WiFi");
    int attempts = 0;
    while (WiFi.status() != WL_CONNECTED && attempts < 40) {
        delay(500);
        Serial.print(".");
        attempts++;

        // Blink LED while connecting
        digitalWrite(PIN_STATUS_LED, !digitalRead(PIN_STATUS_LED));
    }

    if (WiFi.status() == WL_CONNECTED) {
        Serial.printf("\nWiFi connected! IP: %s\n", WiFi.localIP().toString().c_str());
        Serial.printf("WebSocket server: ws://%s:%d\n", WiFi.localIP().toString().c_str(), WSS_PORT);
        digitalWrite(PIN_STATUS_LED, HIGH);  // Solid = connected
    } else {
        Serial.println("\nWiFi FAILED. Running in offline mode.");
        digitalWrite(PIN_STATUS_LED, LOW);
    }
}

// ===== Temperature Reading =====

void readTemperature() {
    if (millis() - lastTempRead < 750) return;  // DS18B20 needs ~750ms for 12-bit
    lastTempRead = millis();

    tempSensor.requestTemperatures();
    float t = tempSensor.getTempCByIndex(0);

    // Validate reading (DS18B20 returns -127 on error)
    if (t > -50 && t < 150) {
        currentTemp = t;
    } else {
        Serial.println("WARN: DS18B20 read error!");
    }

    heaterAtTarget = fabs(currentTemp - targetTemp) < 0.5;
}

// ===== PID Heater Control =====

void updateHeater() {
    if (millis() - lastPidUpdate < PID_UPDATE_MS) return;
    lastPidUpdate = millis();

    if (!heaterOn) {
        digitalWrite(PIN_HEATER_RELAY, LOW);
        pid.reset();
        return;
    }

    // Safety: emergency shutoff
    if (currentTemp > HEATER_MAX_TEMP) {
        Serial.println("SAFETY: Temperature exceeds maximum! Emergency shutoff.");
        heaterOn = false;
        digitalWrite(PIN_HEATER_RELAY, LOW);
        pid.reset();
        return;
    }

    // Safety: timeout
    if (millis() - heaterStartTime > HEATER_TIMEOUT_MS) {
        Serial.println("SAFETY: Heater timeout exceeded! Shutting off.");
        heaterOn = false;
        digitalWrite(PIN_HEATER_RELAY, LOW);
        pid.reset();
        return;
    }

    double dt = PID_UPDATE_MS / 1000.0;
    double output = pid.compute(targetTemp, currentTemp, dt);

    // Relay control: on/off with hysteresis (not PWM — relay can't handle fast switching)
    // Use time-proportional control: in each PID_UPDATE_MS window,
    // relay is ON for (output/100)*PID_UPDATE_MS, OFF for the rest.
    // For simplicity with slow relay, use threshold.
    if (output > 0.5) {
        digitalWrite(PIN_HEATER_RELAY, HIGH);
    } else if (output < -0.5) {
        digitalWrite(PIN_HEATER_RELAY, LOW);
    }
    // In deadband (-0.5 to 0.5), hold current state
}

// ===== WebSocket Event Handler =====

void onWebSocketEvent(uint8_t clientNum, WStype_t type, uint8_t* payload, size_t length) {
    switch (type) {
        case WStype_CONNECTED:
            Serial.printf("WS client %u connected from %s\n", clientNum,
                          ws.remoteIP(clientNum).toString().c_str());
            // Send immediate status
            broadcastStatus();
            break;

        case WStype_DISCONNECTED:
            Serial.printf("WS client %u disconnected\n", clientNum);
            break;

        case WStype_TEXT: {
            // Parse command
            JsonDocument doc;
            DeserializationError err = deserializeJson(doc, payload, length);
            if (err) {
                Serial.printf("JSON parse error: %s\n", err.c_str());
                break;
            }

            int cmd = doc["command"].as<int>();
            int idx = doc["deviceIndex"].as<int>();
            double val = doc["value"].as<double>();
            double val2 = doc["value2"].as<double>();

            handleCommand(cmd, idx, val, val2);

            // Send immediate status update after command
            broadcastStatus();
            break;
        }

        case WStype_PING:
            // Library handles pong automatically
            break;

        default:
            break;
    }
}

// ===== Command Handler =====
// Enum values must match C# LabCommand enum:
//   0=HeatBlockSetTemp, 1=HeatBlockStart, 2=HeatBlockStop,
//   3=PumpSetFlow, 4=PumpStart, 5=PumpStop,
//   6=SpectrometerMeasure,
//   7=CentrifugeStart, 8=CentrifugeStop,
//   9=GelStart, 10=GelStop

void handleCommand(int cmd, int idx, double val, double val2) {
    Serial.printf("CMD: %d idx=%d val=%.2f val2=%.2f\n", cmd, idx, val, val2);

    switch (cmd) {
        case 0:  // HeatBlockSetTemp
            targetTemp = constrain(val, 20.0, HEATER_MAX_TEMP);
            Serial.printf("Heat block target: %.1f°C\n", targetTemp);
            break;

        case 1:  // HeatBlockStart
            heaterOn = true;
            heaterStartTime = millis();
            Serial.println("Heat block ON");
            break;

        case 2:  // HeatBlockStop
            heaterOn = false;
            digitalWrite(PIN_HEATER_RELAY, LOW);
            pid.reset();
            Serial.println("Heat block OFF");
            break;

        case 3: {  // PumpSetFlow — start pump with flow rate and volume
            double flow = constrain(val, 0.01, 30.0);
            double vol = constrain(val2, 0.01, PUMP_MAX_VOLUME_ML);
#if DUAL_ESP32_MODE
            I2CPump::startPump(idx, flow, vol);
#else
            StepperPump& pump3 = (idx == 0) ? pumpA : pumpB;
            pump3.start(flow, vol, true);
#endif
            Serial.printf("Pump %c: %.2f mL/min, %.2f mL\n", idx == 0 ? 'A' : 'B', flow, vol);
            break;
        }

        case 4: {  // PumpStart (resume)
#if DUAL_ESP32_MODE
            I2CPump::startPump(idx, idx == 0 ? I2CPump::statusA.flowRate : I2CPump::statusB.flowRate,
                                     idx == 0 ? I2CPump::statusA.volume : I2CPump::statusB.volume);
#else
            StepperPump& pump4 = (idx == 0) ? pumpA : pumpB;
            pump4.running = true;
            pump4.enable();
#endif
            break;
        }

        case 5: {  // PumpStop
#if DUAL_ESP32_MODE
            I2CPump::stopPump(idx);
#else
            StepperPump& pump5 = (idx == 0) ? pumpA : pumpB;
            pump5.stop();
#endif
            Serial.printf("Pump %c stopped\n", idx == 0 ? 'A' : 'B');
            break;
        }

        case 6:  // SpectrometerMeasure
            Serial.println("Spectro: measuring...");
            spectro.measure();
            break;

        case 7:  // CentrifugeStart
            centrifuge.start((int)val, (unsigned long)(val2 * 1000));
            break;

        case 8:  // CentrifugeStop
            centrifuge.stop();
            break;

        case 9:  // GelStart
            gel.start(val, (int)val2);
            break;

        case 10:  // GelStop
            gel.stop();
            break;

        default:
            Serial.printf("Unknown command: %d\n", cmd);
            break;
    }
}

// ===== Status Broadcast =====

void broadcastStatus() {
    if (ws.connectedClients() == 0) return;

    JsonDocument doc;

    // Heat block
    auto hb = doc["heatBlock"].to<JsonObject>();
    hb["currentTemp"] = round(currentTemp * 10.0) / 10.0;
    hb["targetTemp"] = targetTemp;
    hb["heaterOn"] = heaterOn;
    hb["atTarget"] = heaterAtTarget;
    hb["pidKp"] = pid.kp;
    hb["pidKi"] = pid.ki;
    hb["pidKd"] = pid.kd;

    // Syringe pumps
    auto sp = doc["syringePumps"].to<JsonArray>();
    const char* pumpNames[] = {"A (mRNA/Aqueous)", "B (Lipid/Ethanol)"};

#if DUAL_ESP32_MODE
    I2CPump::readStatus();
    I2CPump::PumpStatus* i2cPumps[] = {&I2CPump::statusA, &I2CPump::statusB};
    for (int i = 0; i < 2; i++) {
        auto p = sp.add<JsonObject>();
        p["name"] = pumpNames[i];
        p["flowRateMlMin"] = round(i2cPumps[i]->flowRate * 100.0) / 100.0;
        p["volumeMl"] = round(i2cPumps[i]->volume * 10.0) / 10.0;
        p["dispensedMl"] = round(i2cPumps[i]->dispensed * 100.0) / 100.0;
        p["running"] = i2cPumps[i]->running;
        p["stepsPerMl"] = STEPS_PER_ML;
    }
#else
    StepperPump* pumps[] = {&pumpA, &pumpB};
    for (int i = 0; i < 2; i++) {
        auto p = sp.add<JsonObject>();
        p["name"] = pumpNames[i];
        p["flowRateMlMin"] = round(pumps[i]->flowRateMlMin * 100.0) / 100.0;
        p["volumeMl"] = round(pumps[i]->volumeMl * 10.0) / 10.0;
        p["dispensedMl"] = round(pumps[i]->dispensedMl * 100.0) / 100.0;
        p["running"] = pumps[i]->running;
        p["stepsPerMl"] = pumps[i]->stepsPerMl;
    }
#endif

    // Spectrophotometer
    auto spec = doc["spectrophotometer"].to<JsonObject>();
    spec["a260"] = round(spectro.a260 * 1000.0) / 1000.0;
    spec["a280"] = round(spectro.a280 * 1000.0) / 1000.0;
    spec["ratio260280"] = round(spectro.ratio260280 * 100.0) / 100.0;
    spec["concentrationNgUl"] = round(spectro.concentrationNgUl * 10.0) / 10.0;
    spec["measurementReady"] = spectro.measurementReady;

    // Centrifuge
    auto cf = doc["centrifuge"].to<JsonObject>();
    cf["currentRpm"] = centrifuge.currentRpm;
    cf["targetRpm"] = centrifuge.targetRpm;
    cf["running"] = centrifuge.running;
    cf["remainingSeconds"] = (int)(centrifuge.remainingMs / 1000);

    // Gel
    auto g = doc["gel"].to<JsonObject>();
    g["voltage"] = gel.voltage;
    g["currentMa"] = round(gel.currentMa * 10.0) / 10.0;
    g["running"] = gel.running;
    g["remainingMinutes"] = gel.remainingMinutes;

    doc["connected"] = true;

    // Serialize and broadcast
    String json;
    serializeJson(doc, json);
    ws.broadcastTXT(json);
}

// ===== Arduino Setup =====

void setup() {
    Serial.begin(115200);
    delay(1000);
    Serial.println("\n====================================");
    Serial.println("RNA Therapy Platform — Lab Controller");
    Serial.println("====================================\n");

    // Status LED
    pinMode(PIN_STATUS_LED, OUTPUT);
    digitalWrite(PIN_STATUS_LED, LOW);

    // Heater relay
    pinMode(PIN_HEATER_RELAY, OUTPUT);
    digitalWrite(PIN_HEATER_RELAY, LOW);

    // Temperature sensor
    tempSensor.begin();
    int sensorCount = tempSensor.getDeviceCount();
    Serial.printf("DS18B20 sensors found: %d\n", sensorCount);
    if (sensorCount > 0) {
        tempSensor.setResolution(12);  // 12-bit = 0.0625°C resolution
        tempSensor.setWaitForConversion(false);  // Non-blocking
    }

    // Syringe pumps
#if DUAL_ESP32_MODE
    I2CPump::begin();
    Serial.println("Syringe pumps: DUAL MODE (I2C to ESP32 #2).");
#else
    pumpA.begin();
    pumpB.begin();
    Serial.println("Syringe pumps: SINGLE MODE (local GPIO).");
#endif

    // Spectrophotometer
    spectro.begin();
    Serial.println("Spectrophotometer initialized.");

    // Centrifuge
    centrifuge.begin();
    // (begin() includes 2s ESC arm delay)

    // Gel electrophoresis
    gel.begin();
    Serial.println("Gel electrophoresis initialized.");

    // WiFi
    setupWiFi();

    // WebSocket server
    ws.begin();
    ws.onEvent(onWebSocketEvent);
    Serial.printf("WebSocket server started on port %d\n", WSS_PORT);

    Serial.println("\nAll systems ready. Waiting for client connection...\n");
}

// ===== Arduino Loop =====

void loop() {
    // WebSocket: handle incoming messages
    ws.loop();

    // Fast loop: stepper motors (must be called as often as possible)
#if !DUAL_ESP32_MODE
    pumpA.update();
    pumpB.update();
#endif

    // Medium loop: temperature reading + PID (every ~500ms)
    readTemperature();
    updateHeater();

    // Slow loop: centrifuge + gel update (every ~500ms)
    if (millis() - lastSlowUpdate >= 500) {
        lastSlowUpdate = millis();
        centrifuge.update();
        gel.update();
    }

    // Status broadcast to all WebSocket clients (every 500ms)
    if (millis() - lastStatusBroadcast >= 500) {
        lastStatusBroadcast = millis();
        broadcastStatus();
    }

    // WiFi reconnection
    if (WiFi.status() != WL_CONNECTED) {
        static unsigned long lastReconnect = 0;
        if (millis() - lastReconnect > 10000) {
            lastReconnect = millis();
            Serial.println("WiFi lost. Reconnecting...");
            WiFi.reconnect();
        }
        digitalWrite(PIN_STATUS_LED, (millis() / 500) % 2);  // Blink = disconnected
    }
}
