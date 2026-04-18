/*
 * ============================================================
 * ESP32 #3 — Environment & Safety Controller
 * ============================================================
 *
 * Monitors lab environment, controls ancillary equipment,
 * and provides safety systems for mRNA synthesis.
 *
 * Equipment:
 *   - Room temperature + humidity (DHT22)
 *   - -80°C freezer temperature (DS18B20, long wire)
 *   - 2-8°C fridge temperature (DS18B20)
 *   - pH meter (analog probe + temp compensation)
 *   - Magnetic stirrer (DC motor, PWM speed control, tachometer)
 *   - Vortex mixer (eccentric DC motor, PWM)
 *   - UV-C sterilization lamp (relay + door interlock)
 *   - Fume hood exhaust fan (relay + airflow verification)
 *   - Emergency stop (N.C. button → kills all relays)
 *   - Smoke/VOC detector (MQ-2/MQ-135)
 *   - Door sensor (reed switch)
 *   - Alarm buzzer
 *
 * Communication: WebSocket server on port 9124
 *   Blazor WASM connects alongside ESP32 #1's WebSocket.
 */

#include <Arduino.h>
#include <WiFi.h>
#include <WebSocketsServer.h>
#include <ArduinoJson.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include <DHT.h>

#include "config.h"
#include "config_multi.h"

// ===== Peripherals =====
DHT dht(PIN_ENV_ROOM_TEMP, DHT22);
OneWire freezerWire(PIN_ENV_FREEZER);
OneWire fridgeWire(PIN_ENV_FRIDGE);
DallasTemperature freezerSensor(&freezerWire);
DallasTemperature fridgeSensor(&fridgeWire);
WebSocketsServer ws(ENV_WSS_PORT);

// ===== State =====
struct EnvState {
    // Room
    float roomTempC = 22.0;
    float roomHumidity = 45.0;
    // Freezer
    float freezerTempC = -80.0;
    bool freezerAlarm = false;
    // Fridge
    float fridgeTempC = 4.0;
    bool fridgeAlarm = false;
    // pH
    float pH = 7.0;
    float pHTempC = 22.0;      // solution temperature for compensation
    bool pHReady = false;
    // Stirrer
    int stirrerRpm = 0;
    int stirrerTargetRpm = 0;
    bool stirrerRunning = false;
    // Vortex
    int vortexSpeed = 0;        // 0-100%
    bool vortexRunning = false;
    // UV-C
    bool uvcOn = false;
    int uvcRemainingMin = 0;
    unsigned long uvcStopTime = 0;
    // Fume hood
    bool fumeHoodOn = false;
    float airflowReading = 0;
    // Safety
    bool estopTriggered = false;
    bool doorOpen = false;
    float gasLevel = 0;
    bool gasAlarm = false;
} state;

// Timing
unsigned long lastSensorRead = 0;
unsigned long lastBroadcast = 0;
unsigned long lastStirrerTach = 0;
int stirrerPulseCount = 0;

// ===== pH Meter =====
float readPH() {
    // Read analog pH probe
    long sum = 0;
    for (int i = 0; i < 32; i++) {
        sum += analogRead(PIN_PH_PROBE);
        delayMicroseconds(100);
    }
    float voltage = (sum / 32.0) / 4095.0 * 3.3;

    // Read solution temperature for compensation
    long tempSum = 0;
    for (int i = 0; i < 16; i++) {
        tempSum += analogRead(PIN_PH_TEMP_COMP);
        delayMicroseconds(100);
    }
    float tempVoltage = (tempSum / 16.0) / 4095.0 * 3.3;
    // NTC thermistor: approximate conversion (Steinhart-Hart simplified)
    // Assuming 10k NTC with 10k voltage divider, Vcc = 3.3V
    float resistance = 10000.0 * tempVoltage / (3.3 - tempVoltage);
    float tempK = 1.0 / (1.0/298.15 + (1.0/3950.0) * log(resistance/10000.0));
    state.pHTempC = tempK - 273.15;

    // Two-point calibration: linear interpolation
    // pH = slope * voltage + intercept
    float slope = (7.0 - 4.0) / (PH_CALIBRATION_7 - PH_CALIBRATION_4);
    float intercept = 7.0 - slope * PH_CALIBRATION_7;
    float rawPH = slope * voltage + intercept;

    // Temperature compensation: Nernst equation
    // pH_corrected = pH_measured + (pH_measured - 7.0) * (temp - 25) * 0.003
    float compensation = (rawPH - 7.0) * (state.pHTempC - 25.0) * 0.003;
    float correctedPH = rawPH - compensation;

    return constrain(correctedPH, 0.0, 14.0);
}

// ===== Magnetic Stirrer =====
void setStirrerSpeed(int rpm) {
    state.stirrerTargetRpm = constrain(rpm, 0, STIRRER_MAX_RPM);
    if (rpm > 0) {
        int duty = map(rpm, 0, STIRRER_MAX_RPM, 30, 255);  // Min duty 30 to overcome stiction
        analogWrite(PIN_STIRRER_PWM, duty);
        state.stirrerRunning = true;
    } else {
        analogWrite(PIN_STIRRER_PWM, 0);
        state.stirrerRunning = false;
    }
}

void updateStirrerTach() {
    // Count hall sensor pulses over 1-second window
    bool hallState = digitalRead(PIN_STIRRER_TACH);
    static bool lastHall = false;
    if (hallState && !lastHall) stirrerPulseCount++;
    lastHall = hallState;

    if (millis() - lastStirrerTach >= 1000) {
        // Assuming 1 magnet on stir bar: pulses/sec = RPS
        state.stirrerRpm = stirrerPulseCount * 60;
        stirrerPulseCount = 0;
        lastStirrerTach = millis();
    }
}

// ===== Vortex Mixer =====
void setVortexSpeed(int percent) {
    percent = constrain(percent, 0, 100);
    state.vortexSpeed = percent;
    if (percent > 0) {
        int duty = map(percent, 0, 100, 50, 255);
        analogWrite(PIN_VORTEX_PWM, duty);
        state.vortexRunning = true;
    } else {
        analogWrite(PIN_VORTEX_PWM, 0);
        state.vortexRunning = false;
    }
}

// ===== UV-C Sterilizer =====
void startUVC(int minutes) {
    if (minutes > UVC_MAX_MINUTES) minutes = UVC_MAX_MINUTES;

    // Safety: check door
    if (digitalRead(PIN_UVC_DOOR) == LOW) {  // Door open
        Serial.println("SAFETY: Cannot start UV-C with door open!");
        return;
    }

    state.uvcOn = true;
    state.uvcRemainingMin = minutes;
    state.uvcStopTime = millis() + (unsigned long)minutes * 60000UL;
    digitalWrite(PIN_UVC_RELAY, HIGH);
    Serial.printf("UV-C: started for %d minutes\n", minutes);
}

void stopUVC() {
    state.uvcOn = false;
    state.uvcRemainingMin = 0;
    digitalWrite(PIN_UVC_RELAY, LOW);
    Serial.println("UV-C: stopped");
}

void updateUVC() {
    if (!state.uvcOn) return;

    // Door interlock
    if (digitalRead(PIN_UVC_DOOR) == LOW) {
        Serial.println("SAFETY: Door opened during UV-C! Emergency stop.");
        stopUVC();
        return;
    }

    // Timer
    if (millis() >= state.uvcStopTime) {
        stopUVC();
        Serial.println("UV-C: cycle complete");
        return;
    }

    state.uvcRemainingMin = (int)((state.uvcStopTime - millis()) / 60000UL) + 1;
}

// ===== Emergency Stop =====
void checkEmergencyStop() {
    // N.C. button: LOW = normal, HIGH = STOP pressed (wire broken or button pushed)
    bool estop = digitalRead(PIN_ESTOP) == HIGH;

    if (estop && !state.estopTriggered) {
        state.estopTriggered = true;
        Serial.println("*** EMERGENCY STOP ACTIVATED ***");

        // Kill everything
        setStirrerSpeed(0);
        setVortexSpeed(0);
        stopUVC();
        digitalWrite(PIN_FUMEHOOD_RELAY, LOW);
        state.fumeHoodOn = false;

        // Sound alarm
        tone(PIN_ESTOP_BUZZER, 2000);  // 2kHz alarm
        digitalWrite(PIN_ESTOP_LED, HIGH);
    }

    if (!estop && state.estopTriggered) {
        // E-stop released — clear alarm but don't restart equipment
        state.estopTriggered = false;
        noTone(PIN_ESTOP_BUZZER);
        digitalWrite(PIN_ESTOP_LED, LOW);
        Serial.println("E-stop released. Equipment remains off — restart manually.");
    }
}

// ===== Safety Monitoring =====
void checkGasSensor() {
    state.gasLevel = analogRead(PIN_GAS_SENSOR);
    bool newAlarm = state.gasLevel > GAS_ALARM_THRESHOLD;

    if (newAlarm && !state.gasAlarm) {
        Serial.printf("GAS ALARM: Level %.0f exceeds threshold %d!\n", state.gasLevel, GAS_ALARM_THRESHOLD);
        // Auto-start fume hood
        if (!state.fumeHoodOn) {
            digitalWrite(PIN_FUMEHOOD_RELAY, HIGH);
            state.fumeHoodOn = true;
            Serial.println("Fume hood auto-started due to gas alarm.");
        }
        // Sound alarm
        tone(PIN_ESTOP_BUZZER, 1500, 500);  // 1.5kHz, 500ms beep
    }
    state.gasAlarm = newAlarm;
}

// ===== Sensor Reading =====
void readSensors() {
    if (millis() - lastSensorRead < 2000) return;  // Every 2 seconds
    lastSensorRead = millis();

    // Room temp + humidity
    float t = dht.readTemperature();
    float h = dht.readHumidity();
    if (!isnan(t)) state.roomTempC = t;
    if (!isnan(h)) state.roomHumidity = h;

    // Freezer temp
    freezerSensor.requestTemperatures();
    float ft = freezerSensor.getTempCByIndex(0);
    if (ft > -127) {
        state.freezerTempC = ft;
        state.freezerAlarm = ft > -70.0;  // Alarm if warmer than -70°C
        if (state.freezerAlarm) {
            Serial.printf("FREEZER ALARM: %.1f°C (expected < -70°C)\n", ft);
            tone(PIN_ESTOP_BUZZER, 1000, 200);
        }
    }

    // Fridge temp
    fridgeSensor.requestTemperatures();
    float frt = fridgeSensor.getTempCByIndex(0);
    if (frt > -127) {
        state.fridgeTempC = frt;
        state.fridgeAlarm = frt < 1.0 || frt > 10.0;  // Alarm if outside 1-10°C
        if (state.fridgeAlarm) {
            Serial.printf("FRIDGE ALARM: %.1f°C (expected 2-8°C)\n", frt);
        }
    }

    // pH (only when requested — probe degrades with continuous reading)
    // pH read triggered by command, not continuous

    // Airflow (fume hood verification)
    if (state.fumeHoodOn) {
        state.airflowReading = analogRead(PIN_AIRFLOW_SENSOR) / 4095.0 * 100.0;  // 0-100%
        if (state.airflowReading < 20.0) {
            Serial.println("WARNING: Fume hood running but low airflow detected!");
        }
    }

    // Door
    state.doorOpen = digitalRead(PIN_DOOR_SENSOR) == HIGH;

    // Gas
    checkGasSensor();
}

// ===== WebSocket =====
void onWsEvent(uint8_t num, WStype_t type, uint8_t* payload, size_t length) {
    if (type == WStype_CONNECTED) {
        Serial.printf("ENV WS client %u connected\n", num);
        return;
    }
    if (type != WStype_TEXT) return;

    JsonDocument doc;
    if (deserializeJson(doc, payload, length)) return;

    int cmd = doc["command"].as<int>();
    double val = doc["value"].as<double>();

    // Commands: 20=pH measure, 21=stirrer set RPM, 22=stirrer stop,
    //           23=vortex set%, 24=vortex stop, 25=UVC start, 26=UVC stop,
    //           27=fume hood on, 28=fume hood off, 29=e-stop reset
    switch (cmd) {
        case 20:  // pH measure
            state.pH = readPH();
            state.pHReady = true;
            Serial.printf("pH: %.2f at %.1f°C\n", state.pH, state.pHTempC);
            break;
        case 21:  // Stirrer set RPM
            setStirrerSpeed((int)val);
            Serial.printf("Stirrer: %d RPM\n", (int)val);
            break;
        case 22:  // Stirrer stop
            setStirrerSpeed(0);
            break;
        case 23:  // Vortex set speed %
            setVortexSpeed((int)val);
            Serial.printf("Vortex: %d%%\n", (int)val);
            break;
        case 24:  // Vortex stop
            setVortexSpeed(0);
            break;
        case 25:  // UV-C start (val = minutes)
            startUVC((int)val);
            break;
        case 26:  // UV-C stop
            stopUVC();
            break;
        case 27:  // Fume hood on
            digitalWrite(PIN_FUMEHOOD_RELAY, HIGH);
            state.fumeHoodOn = true;
            break;
        case 28:  // Fume hood off
            if (!state.gasAlarm) {  // Don't allow off during gas alarm
                digitalWrite(PIN_FUMEHOOD_RELAY, LOW);
                state.fumeHoodOn = false;
            } else {
                Serial.println("Cannot turn off fume hood during gas alarm!");
            }
            break;
        case 29:  // E-stop reset (acknowledge)
            state.estopTriggered = false;
            noTone(PIN_ESTOP_BUZZER);
            digitalWrite(PIN_ESTOP_LED, LOW);
            break;
    }
}

void broadcastStatus() {
    if (ws.connectedClients() == 0) return;

    JsonDocument doc;

    auto room = doc["room"].to<JsonObject>();
    room["tempC"] = round(state.roomTempC * 10) / 10.0;
    room["humidity"] = round(state.roomHumidity * 10) / 10.0;

    auto freezer = doc["freezer"].to<JsonObject>();
    freezer["tempC"] = round(state.freezerTempC * 10) / 10.0;
    freezer["alarm"] = state.freezerAlarm;

    auto fridge = doc["fridge"].to<JsonObject>();
    fridge["tempC"] = round(state.fridgeTempC * 10) / 10.0;
    fridge["alarm"] = state.fridgeAlarm;

    auto ph = doc["ph"].to<JsonObject>();
    ph["value"] = round(state.pH * 100) / 100.0;
    ph["tempC"] = round(state.pHTempC * 10) / 10.0;
    ph["ready"] = state.pHReady;

    auto stirrer = doc["stirrer"].to<JsonObject>();
    stirrer["rpm"] = state.stirrerRpm;
    stirrer["targetRpm"] = state.stirrerTargetRpm;
    stirrer["running"] = state.stirrerRunning;

    auto vortex = doc["vortex"].to<JsonObject>();
    vortex["speed"] = state.vortexSpeed;
    vortex["running"] = state.vortexRunning;

    auto uvc = doc["uvc"].to<JsonObject>();
    uvc["on"] = state.uvcOn;
    uvc["remainingMin"] = state.uvcRemainingMin;

    auto hood = doc["fumeHood"].to<JsonObject>();
    hood["on"] = state.fumeHoodOn;
    hood["airflow"] = round(state.airflowReading * 10) / 10.0;

    auto safety = doc["safety"].to<JsonObject>();
    safety["estop"] = state.estopTriggered;
    safety["doorOpen"] = state.doorOpen;
    safety["gasLevel"] = round(state.gasLevel);
    safety["gasAlarm"] = state.gasAlarm;

    doc["connected"] = true;

    String json;
    serializeJson(doc, json);
    ws.broadcastTXT(json);
}

// ===== Setup =====
void setup() {
    Serial.begin(115200);
    Serial.println("\n====================================");
    Serial.println("ESP32 #3 — Environment & Safety");
    Serial.println("====================================\n");

    // Outputs
    pinMode(PIN_UVC_RELAY, OUTPUT);     digitalWrite(PIN_UVC_RELAY, LOW);
    pinMode(PIN_FUMEHOOD_RELAY, OUTPUT); digitalWrite(PIN_FUMEHOOD_RELAY, LOW);
    pinMode(PIN_STIRRER_PWM, OUTPUT);   analogWrite(PIN_STIRRER_PWM, 0);
    pinMode(PIN_VORTEX_PWM, OUTPUT);    analogWrite(PIN_VORTEX_PWM, 0);
    pinMode(PIN_ESTOP_BUZZER, OUTPUT);
    pinMode(PIN_ESTOP_LED, OUTPUT);     digitalWrite(PIN_ESTOP_LED, LOW);

    // Inputs
    pinMode(PIN_ESTOP, INPUT_PULLUP);
    pinMode(PIN_UVC_DOOR, INPUT_PULLUP);
    pinMode(PIN_DOOR_SENSOR, INPUT_PULLUP);
    pinMode(PIN_STIRRER_TACH, INPUT_PULLUP);

    // ADC
    analogReadResolution(12);

    // Sensors
    dht.begin();
    freezerSensor.begin();
    fridgeSensor.begin();
    freezerSensor.setResolution(12);
    fridgeSensor.setResolution(12);
    freezerSensor.setWaitForConversion(false);
    fridgeSensor.setWaitForConversion(false);

    Serial.printf("Freezer sensors: %d, Fridge sensors: %d\n",
                  freezerSensor.getDeviceCount(), fridgeSensor.getDeviceCount());

    // WiFi
    WiFi.mode(WIFI_STA);
    WiFi.begin(WIFI_SSID, WIFI_PASS);
    Serial.print("WiFi connecting");
    while (WiFi.status() != WL_CONNECTED) { delay(500); Serial.print("."); }
    Serial.printf("\nIP: %s\n", WiFi.localIP().toString().c_str());

    // WebSocket
    ws.begin();
    ws.onEvent(onWsEvent);
    Serial.printf("ENV WebSocket on port %d\n", ENV_WSS_PORT);

    Serial.println("Environment controller ready.\n");
}

// ===== Loop =====
void loop() {
    ws.loop();

    checkEmergencyStop();
    readSensors();
    updateStirrerTach();
    updateUVC();

    if (millis() - lastBroadcast >= 500) {
        lastBroadcast = millis();
        broadcastStatus();
    }
}
