#pragma once
#include <Arduino.h>
#include "config.h"

/// Brushless motor centrifuge controller via ESC (Electronic Speed Controller).
/// Features: safety interlock, RPM measurement via tachometer, acceleration ramp.
class Centrifuge {
public:
    int currentRpm = 0;
    int targetRpm = 0;
    bool running = false;
    unsigned long remainingMs = 0;

    void begin() {
        // ESC PWM: 50Hz, 16-bit resolution
        ledcAttach(PIN_CENTRIFUGE_PWM, 50, 16);
        setThrottle(0);  // Arm ESC with zero throttle

        if (PIN_CENTRIFUGE_TACH >= 0) {
            pinMode(PIN_CENTRIFUGE_TACH, INPUT_PULLUP);
        }
        if (PIN_CENTRIFUGE_LID >= 0) {
            pinMode(PIN_CENTRIFUGE_LID, INPUT_PULLUP);
        }

        // ESC arming sequence: hold minimum for 2 seconds
        delay(2000);
        Serial.println("Centrifuge ESC armed.");
    }

    /// Start spinning at given RPM for duration in milliseconds.
    void start(int rpm, unsigned long durationMs) {
        // Safety: check lid interlock
        if (PIN_CENTRIFUGE_LID >= 0 && digitalRead(PIN_CENTRIFUGE_LID) == HIGH) {
            Serial.println("SAFETY: Centrifuge lid open! Cannot start.");
            return;
        }

        targetRpm = constrain(rpm, 0, CENTRIFUGE_MAX_RPM);
        remainingMs = durationMs;
        startTime_ = millis();
        rampStartTime_ = millis();
        running = true;
        currentRpm = 0;

        Serial.printf("Centrifuge: starting at %d RPM for %lu seconds\n",
                       targetRpm, durationMs / 1000);
    }

    void stop() {
        running = false;
        targetRpm = 0;
        currentRpm = 0;
        remainingMs = 0;
        setThrottle(0);
        Serial.println("Centrifuge stopped.");
    }

    /// Call every loop iteration. Handles ramping, timing, safety.
    void update() {
        if (!running) return;

        // Safety: check lid
        if (PIN_CENTRIFUGE_LID >= 0 && digitalRead(PIN_CENTRIFUGE_LID) == HIGH) {
            Serial.println("SAFETY: Centrifuge lid opened during run! Emergency stop.");
            stop();
            return;
        }

        // Timer
        unsigned long elapsed = millis() - startTime_;
        if (elapsed >= remainingMs + (unsigned long)CENTRIFUGE_RAMP_MS) {
            // Ramp down
            currentRpm = 0;
            setThrottle(0);
            running = false;
            Serial.println("Centrifuge: run complete.");
            return;
        }

        // Calculate remaining
        if (elapsed < remainingMs) {
            remainingMs -= elapsed;
            startTime_ = millis();
        }

        // Acceleration ramp
        unsigned long rampElapsed = millis() - rampStartTime_;
        if (rampElapsed < (unsigned long)CENTRIFUGE_RAMP_MS) {
            double rampFraction = (double)rampElapsed / CENTRIFUGE_RAMP_MS;
            currentRpm = (int)(targetRpm * rampFraction);
        } else {
            currentRpm = targetRpm;
        }

        setThrottle(currentRpm);

        // Read tachometer (if available) for actual RPM
        updateTachometer();
    }

    int getActualRpm() const { return actualRpm_; }

private:
    unsigned long startTime_ = 0;
    unsigned long rampStartTime_ = 0;
    volatile int actualRpm_ = 0;
    unsigned long lastTachPulse_ = 0;
    int tachPulseCount_ = 0;
    unsigned long tachWindow_ = 0;

    /// Set ESC throttle from RPM.
    void setThrottle(int rpm) {
        // Map RPM to ESC pulse width: 1000us (stop) to 2000us (full)
        int pulseUs = map(constrain(rpm, 0, CENTRIFUGE_MAX_RPM),
                          0, CENTRIFUGE_MAX_RPM, ESC_MIN_US, ESC_MAX_US);

        // Convert to 16-bit duty cycle at 50Hz (20ms period)
        // duty = pulseUs / 20000 * 65536
        uint32_t duty = (uint32_t)((double)pulseUs / 20000.0 * 65536.0);
        ledcWrite(PIN_CENTRIFUGE_PWM, duty);
    }

    /// Count tachometer pulses to measure actual RPM.
    void updateTachometer() {
        if (PIN_CENTRIFUGE_TACH < 0) return;

        // Simple pulse counting over 1-second windows
        bool tachState = digitalRead(PIN_CENTRIFUGE_TACH);
        unsigned long now = millis();

        if (tachState == LOW && (now - lastTachPulse_) > 5) {  // debounce
            tachPulseCount_++;
            lastTachPulse_ = now;
        }

        if (now - tachWindow_ >= 1000) {
            // Assuming 1 magnet on rotor: pulses/sec = RPS, RPM = RPS * 60
            actualRpm_ = tachPulseCount_ * 60;
            tachPulseCount_ = 0;
            tachWindow_ = now;
        }
    }
};
