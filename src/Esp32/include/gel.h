#pragma once
#include <Arduino.h>
#include "config.h"

/// Gel electrophoresis controller.
/// Controls relay to DC-DC boost converter, monitors current via shunt resistor.
class GelElectrophoresis {
public:
    double voltage = 0;
    double currentMa = 0;
    bool running = false;
    int remainingMinutes = 0;

    void begin() {
        pinMode(PIN_GEL_RELAY, OUTPUT);
        digitalWrite(PIN_GEL_RELAY, LOW);

        if (PIN_GEL_CURRENT >= 0) {
            analogReadResolution(12);
        }
    }

    /// Start gel run at given voltage for duration in minutes.
    void start(double volts, int minutes) {
        if (volts > GEL_MAX_VOLTAGE) {
            Serial.printf("SAFETY: Requested voltage %.0f exceeds max %.0f!\n", volts, GEL_MAX_VOLTAGE);
            return;
        }
        if (minutes > GEL_MAX_MINUTES) {
            Serial.printf("SAFETY: Requested duration %d exceeds max %d minutes!\n", minutes, GEL_MAX_MINUTES);
            minutes = GEL_MAX_MINUTES;
        }

        voltage = volts;
        remainingMinutes = minutes;
        startTime_ = millis();
        totalMs_ = (unsigned long)minutes * 60000UL;
        running = true;

        // Note: voltage is set by the DC-DC converter's potentiometer/feedback.
        // The relay simply connects/disconnects power.
        // For programmable voltage, use a DAC + feedback circuit.
        digitalWrite(PIN_GEL_RELAY, HIGH);

        Serial.printf("Gel: started at %.0fV for %d minutes\n", voltage, minutes);
    }

    void stop() {
        running = false;
        remainingMinutes = 0;
        voltage = 0;
        currentMa = 0;
        digitalWrite(PIN_GEL_RELAY, LOW);
        Serial.println("Gel: stopped.");
    }

    /// Call periodically (every 500ms-1s).
    void update() {
        if (!running) return;

        unsigned long elapsed = millis() - startTime_;
        if (elapsed >= totalMs_) {
            stop();
            Serial.println("Gel: run complete.");
            return;
        }

        remainingMinutes = (int)((totalMs_ - elapsed) / 60000UL) + 1;

        // Read current through shunt resistor
        if (PIN_GEL_CURRENT >= 0) {
            int raw = analogRead(PIN_GEL_CURRENT);
            // Assuming 1-ohm shunt, 12-bit ADC, 3.3V reference
            // V_shunt = raw / 4095 * 3.3
            // I = V / R = V_shunt / 1.0
            // mA = I * 1000
            double vShunt = (double)raw / 4095.0 * 3.3;
            currentMa = vShunt * 1000.0;  // Adjust for actual shunt value
        }
    }

private:
    unsigned long startTime_ = 0;
    unsigned long totalMs_ = 0;
};
