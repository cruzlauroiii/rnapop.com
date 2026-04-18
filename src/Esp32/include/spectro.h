#pragma once
#include <Arduino.h>
#include "config.h"

/// UV spectrophotometer using 260nm/280nm LEDs and photodiode on ADC.
/// Beer-Lambert law: A = -log10(I_sample / I_reference)
class Spectrophotometer {
public:
    double a260 = 0;
    double a280 = 0;
    double ratio260280 = 0;
    double concentrationNgUl = 0;
    bool measurementReady = false;

    void begin() {
        pinMode(PIN_UV_260, OUTPUT);
        pinMode(PIN_UV_280, OUTPUT);
        digitalWrite(PIN_UV_260, LOW);
        digitalWrite(PIN_UV_280, LOW);

        // Configure ADC
        analogReadResolution(12);  // 12-bit: 0-4095
        analogSetAttenuation(ADC_11db);  // Full range 0-3.3V
    }

    /// Take a complete measurement. Blocking (~1 second total).
    void measure() {
        // 1) Blank reading (both LEDs off, measure ambient + dark current)
        digitalWrite(PIN_UV_260, LOW);
        digitalWrite(PIN_UV_280, LOW);
        delay(SPECTRO_BLANK_MS);
        int darkReading = readAverage(PIN_PHOTODIODE);

        // 2) Reference reading (LED on, no sample — or reference photodiode)
        //    If using reference photodiode on PIN_PHOTODIODE_REF:
        digitalWrite(PIN_UV_260, HIGH);
        delay(SPECTRO_SETTLE_MS);
        int ref260 = readAverage(PIN_PHOTODIODE_REF);
        int sample260 = readAverage(PIN_PHOTODIODE);
        digitalWrite(PIN_UV_260, LOW);
        delay(50);

        // 3) 280nm measurement
        digitalWrite(PIN_UV_280, HIGH);
        delay(SPECTRO_SETTLE_MS);
        int ref280 = readAverage(PIN_PHOTODIODE_REF);
        int sample280 = readAverage(PIN_PHOTODIODE);
        digitalWrite(PIN_UV_280, LOW);

        // 4) Calculate absorbance
        // Subtract dark current
        ref260 -= darkReading;
        ref280 -= darkReading;
        sample260 -= darkReading;
        sample280 -= darkReading;

        // Clamp to avoid division by zero or log of negative
        ref260 = max(1, ref260);
        ref280 = max(1, ref280);
        sample260 = max(1, sample260);
        sample280 = max(1, sample280);

        // Beer-Lambert: A = -log10(I_transmitted / I_incident)
        // I_incident = ref (light through air/solvent)
        // I_transmitted = sample (light through sample)
        // Lower sample reading = more absorption = higher A
        a260 = -log10((double)sample260 / (double)ref260);
        a280 = -log10((double)sample280 / (double)ref280);

        // Clamp negative absorbance (can happen with noise)
        if (a260 < 0) a260 = 0;
        if (a280 < 0) a280 = 0;

        // 5) Purity ratio
        ratio260280 = (a280 > 0.001) ? a260 / a280 : 0;

        // 6) RNA concentration (Beer-Lambert, 10mm pathlength)
        // ssRNA: A260 of 1.0 = 40 ng/uL
        // dsRNA: 46, ssDNA: 33, dsDNA: 50
        concentrationNgUl = a260 * RNA_FACTOR;

        measurementReady = true;

        Serial.printf("Spectro: A260=%.3f A280=%.3f ratio=%.2f conc=%.1f ng/uL\n",
                       a260, a280, ratio260280, concentrationNgUl);
    }

private:
    /// Read ADC N times and return average (noise reduction).
    int readAverage(int pin) {
        long sum = 0;
        for (int i = 0; i < SPECTRO_SAMPLES; i++) {
            sum += analogRead(pin);
            delayMicroseconds(100);
        }
        return (int)(sum / SPECTRO_SAMPLES);
    }
};
