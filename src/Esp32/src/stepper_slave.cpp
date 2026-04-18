/*
 * ============================================================
 * RNA Therapy Platform — ESP32 #2 Stepper Slave
 * ============================================================
 *
 * Dedicated ESP32 for syringe pump stepper motor control.
 * No WiFi — zero interrupts — pure step timing precision.
 * Receives commands from ESP32 #1 via I2C.
 *
 * I2C Protocol:
 *   Address: 0x10
 *   Master writes 16 bytes:
 *     [0]    = command (1=start_A, 2=stop_A, 3=start_B, 4=stop_B, 5=home_A, 6=home_B, 7=status)
 *     [1-4]  = flow rate (float, mL/min)
 *     [5-8]  = volume (float, mL)
 *     [9]    = direction (0=forward, 1=reverse)
 *     [10-15]= reserved
 *   Master reads 16 bytes:
 *     [0]    = pump A running (0/1)
 *     [1-4]  = pump A dispensed (float, mL)
 *     [5]    = pump B running (0/1)
 *     [6-9]  = pump B dispensed (float, mL)
 *     [10]   = pump A at endstop (0/1)
 *     [11]   = pump B at endstop (0/1)
 *     [12-15]= reserved
 *
 * Build: Compile with PlatformIO as separate firmware.
 *        Upload to ESP32 #2 (different USB port).
 *
 * Wiring to ESP32 #1:
 *   SDA → GPIO 21 (both ESP32s)
 *   SCL → GPIO 22 (both ESP32s)
 *   GND → GND (shared)
 */

#include <Arduino.h>
#include <Wire.h>

// Pin assignments for ESP32 #2
#define PIN_PUMP_A_STEP     12
#define PIN_PUMP_A_DIR      13
#define PIN_PUMP_A_EN       27
#define PIN_PUMP_A_ENDSTOP  32

#define PIN_PUMP_B_STEP     14
#define PIN_PUMP_B_DIR      15
#define PIN_PUMP_B_EN       26
#define PIN_PUMP_B_ENDSTOP  33

#define I2C_SLAVE_ADDR      0x10
#define STEPS_PER_ML        3200
#define MAX_STEP_FREQ       10000   // Higher than controller version — no WiFi stealing cycles
#define ACCEL_STEPS_PER_S2  3000

// ===== Pump State =====
struct Pump {
    int pinStep, pinDir, pinEn, pinEnd;
    volatile bool running = false;
    volatile float flowRate = 0;        // mL/min
    volatile float volume = 0;          // target mL
    volatile float dispensed = 0;       // actual mL
    volatile unsigned long stepCount = 0;
    volatile unsigned long lastStepUs = 0;
    volatile float currentFreq = 0;
    volatile float targetFreq = 0;
    volatile bool atEndstop = false;

    void begin() {
        pinMode(pinStep, OUTPUT);
        pinMode(pinDir, OUTPUT);
        pinMode(pinEn, OUTPUT);
        pinMode(pinEnd, INPUT_PULLUP);
        digitalWrite(pinEn, HIGH);  // Disabled
    }

    void start(float flowMlMin, float volMl, bool forward) {
        flowRate = flowMlMin;
        volume = volMl;
        dispensed = 0;
        stepCount = 0;
        targetFreq = STEPS_PER_ML * flowMlMin / 60.0f;
        if (targetFreq > MAX_STEP_FREQ) targetFreq = MAX_STEP_FREQ;
        currentFreq = 0;
        digitalWrite(pinDir, forward ? HIGH : LOW);
        digitalWrite(pinEn, LOW);  // Enable
        running = true;
        lastStepUs = micros();
    }

    void stop() {
        running = false;
        currentFreq = 0;
        digitalWrite(pinEn, HIGH);  // Disable
    }

    // Called in tight loop — no delay, no yield
    inline void step() {
        if (!running) return;

        // Endstop check
        if (digitalRead(pinEnd) == LOW) {
            atEndstop = true;
            stop();
            return;
        }
        atEndstop = false;

        // Volume check
        if (dispensed >= volume) {
            stop();
            return;
        }

        // Acceleration ramp
        unsigned long now = micros();
        float dt = (now - lastStepUs) * 1e-6f;
        if (currentFreq < targetFreq) {
            currentFreq += ACCEL_STEPS_PER_S2 * dt;
            if (currentFreq > targetFreq) currentFreq = targetFreq;
        }
        // Deceleration near end
        float remaining = volume - dispensed;
        float stepsRemaining = remaining * STEPS_PER_ML;
        float decelFreq = sqrtf(2.0f * ACCEL_STEPS_PER_S2 * stepsRemaining);
        if (decelFreq < currentFreq) currentFreq = decelFreq;

        if (currentFreq <= 0) return;

        unsigned long intervalUs = (unsigned long)(1000000.0f / currentFreq);

        if (now - lastStepUs >= intervalUs) {
            // Pulse — absolute minimum time
            GPIO.out_w1ts = (1 << pinStep);  // Direct register write — faster than digitalWrite
            __asm__ __volatile__("nop; nop; nop; nop;");  // ~16ns per nop at 240MHz, need >1us
            delayMicroseconds(1);
            GPIO.out_w1tc = (1 << pinStep);

            lastStepUs = now;
            stepCount++;
            dispensed = (float)stepCount / STEPS_PER_ML;
        }
    }

    bool home(float speedMlMin = 1.0f) {
        digitalWrite(pinDir, LOW);  // Reverse
        digitalWrite(pinEn, LOW);

        float freq = STEPS_PER_ML * speedMlMin / 60.0f;
        unsigned long intervalUs = (unsigned long)(1000000.0f / freq);

        for (long i = 0; i < (long)STEPS_PER_ML * 25; i++) {
            if (digitalRead(pinEnd) == LOW) {
                atEndstop = true;
                digitalWrite(pinEn, HIGH);
                return true;
            }
            digitalWrite(pinStep, HIGH);
            delayMicroseconds(1);
            digitalWrite(pinStep, LOW);
            delayMicroseconds(intervalUs);
        }
        digitalWrite(pinEn, HIGH);
        return false;
    }
};

Pump pumpA = {PIN_PUMP_A_STEP, PIN_PUMP_A_DIR, PIN_PUMP_A_EN, PIN_PUMP_A_ENDSTOP};
Pump pumpB = {PIN_PUMP_B_STEP, PIN_PUMP_B_DIR, PIN_PUMP_B_EN, PIN_PUMP_B_ENDSTOP};

// ===== I2C Buffers =====
volatile uint8_t rxBuf[16] = {0};
volatile uint8_t txBuf[16] = {0};
volatile bool newCommand = false;

// Helper: write float to byte array at offset
void floatToBytes(volatile uint8_t* buf, int offset, float val) {
    memcpy((void*)(buf + offset), &val, 4);
}

// Helper: read float from byte array at offset
float bytesToFloat(volatile uint8_t* buf, int offset) {
    float val;
    memcpy(&val, (void*)(buf + offset), 4);
    return val;
}

// ===== I2C Callbacks =====
void onReceive(int numBytes) {
    int i = 0;
    while (Wire.available() && i < 16) {
        rxBuf[i++] = Wire.read();
    }
    newCommand = true;
}

void onRequest() {
    // Build status response
    txBuf[0] = pumpA.running ? 1 : 0;
    floatToBytes(txBuf, 1, pumpA.dispensed);
    txBuf[5] = pumpB.running ? 1 : 0;
    floatToBytes(txBuf, 6, pumpB.dispensed);
    txBuf[10] = pumpA.atEndstop ? 1 : 0;
    txBuf[11] = pumpB.atEndstop ? 1 : 0;

    Wire.write((uint8_t*)txBuf, 16);
}

// ===== Process Command =====
void processCommand() {
    uint8_t cmd = rxBuf[0];
    float flow = bytesToFloat(rxBuf, 1);
    float vol = bytesToFloat(rxBuf, 5);
    bool forward = (rxBuf[9] == 0);

    switch (cmd) {
        case 1:  // Start pump A
            pumpA.start(flow, vol, forward);
            Serial.printf("Pump A: start %.2f mL/min, %.2f mL, %s\n", flow, vol, forward ? "fwd" : "rev");
            break;
        case 2:  // Stop pump A
            pumpA.stop();
            Serial.println("Pump A: stop");
            break;
        case 3:  // Start pump B
            pumpB.start(flow, vol, forward);
            Serial.printf("Pump B: start %.2f mL/min, %.2f mL, %s\n", flow, vol, forward ? "fwd" : "rev");
            break;
        case 4:  // Stop pump B
            pumpB.stop();
            Serial.println("Pump B: stop");
            break;
        case 5:  // Home pump A
            Serial.println("Pump A: homing...");
            pumpA.home();
            Serial.println(pumpA.atEndstop ? "Pump A: home found" : "Pump A: home NOT found");
            break;
        case 6:  // Home pump B
            Serial.println("Pump B: homing...");
            pumpB.home();
            Serial.println(pumpB.atEndstop ? "Pump B: home found" : "Pump B: home NOT found");
            break;
        case 7:  // Status request — handled in onRequest
            break;
        default:
            Serial.printf("Unknown command: %d\n", cmd);
            break;
    }
}

// ===== Setup =====
void setup() {
    Serial.begin(115200);
    Serial.println("ESP32 #2 — Stepper Slave");
    Serial.printf("I2C address: 0x%02X\n", I2C_SLAVE_ADDR);

    pumpA.begin();
    pumpB.begin();

    Wire.begin(I2C_SLAVE_ADDR);  // Join as slave
    Wire.onReceive(onReceive);
    Wire.onRequest(onRequest);

    Serial.println("Ready. Waiting for commands from ESP32 #1...");
}

// ===== Loop — Tight step timing =====
void loop() {
    // Process any pending I2C command
    if (newCommand) {
        newCommand = false;
        processCommand();
    }

    // Step both pumps — this is the critical tight loop
    // No WiFi, no WebSocket, no JSON — just stepping
    pumpA.step();
    pumpB.step();

    // No delay — run as fast as possible for maximum step precision
}
