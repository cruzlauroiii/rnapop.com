#pragma once
#include <Arduino.h>

/// Non-blocking stepper motor driver for A4988.
/// Supports acceleration ramp, precise volume dispensing, and endstop detection.
class StepperPump {
public:
    int pinStep, pinDir, pinEnable, pinEndstop;
    int stepsPerMl;
    double maxStepFreq;

    // State
    volatile bool running = false;
    volatile double flowRateMlMin = 0;
    volatile double volumeMl = 0;
    volatile double dispensedMl = 0;

    StepperPump(int step, int dir, int en, int endstop, int stepsPerMl, double maxFreq)
        : pinStep(step), pinDir(dir), pinEnable(en), pinEndstop(endstop),
          stepsPerMl(stepsPerMl), maxStepFreq(maxFreq),
          stepCount_(0), lastStepUs_(0), currentFreq_(0), targetFreq_(0),
          accelStepsPerSec2_(2000) {}

    void begin() {
        pinMode(pinStep, OUTPUT);
        pinMode(pinDir, OUTPUT);
        pinMode(pinEnable, OUTPUT);
        if (pinEndstop >= 0) {
            pinMode(pinEndstop, INPUT_PULLUP);
        }
        disable();
    }

    void enable() { digitalWrite(pinEnable, LOW); }   // A4988 EN is active-low
    void disable() { digitalWrite(pinEnable, HIGH); }

    void setDirection(bool forward) {
        digitalWrite(pinDir, forward ? HIGH : LOW);
    }

    /// Start dispensing: flow in mL/min, total volume in mL.
    void start(double flowMlMin, double volMl, bool forward = true) {
        flowRateMlMin = flowMlMin;
        volumeMl = volMl;
        dispensedMl = 0;
        stepCount_ = 0;

        // Calculate target step frequency
        // stepsPerMl * flowRate(mL/min) / 60 = steps/sec
        targetFreq_ = stepsPerMl * flowMlMin / 60.0;
        if (targetFreq_ > maxStepFreq) targetFreq_ = maxStepFreq;
        currentFreq_ = 0;  // start from zero (acceleration ramp)

        setDirection(forward);
        enable();
        running = true;
        lastStepUs_ = micros();
    }

    void stop() {
        running = false;
        currentFreq_ = 0;
        disable();
    }

    /// Call from loop() as fast as possible. Non-blocking.
    void update() {
        if (!running) return;

        // Check endstop
        if (pinEndstop >= 0 && digitalRead(pinEndstop) == LOW) {
            stop();
            return;
        }

        // Check volume limit
        if (dispensedMl >= volumeMl) {
            stop();
            return;
        }

        // Acceleration ramp (trapezoidal)
        if (currentFreq_ < targetFreq_) {
            double dt = (micros() - lastStepUs_) / 1e6;
            currentFreq_ += accelStepsPerSec2_ * dt;
            if (currentFreq_ > targetFreq_) currentFreq_ = targetFreq_;
        }

        if (currentFreq_ <= 0) return;

        // Step timing
        unsigned long intervalUs = (unsigned long)(1000000.0 / currentFreq_);
        unsigned long now = micros();

        if (now - lastStepUs_ >= intervalUs) {
            // Pulse STEP pin
            digitalWrite(pinStep, HIGH);
            delayMicroseconds(2);  // A4988 minimum pulse width: 1us
            digitalWrite(pinStep, LOW);

            lastStepUs_ = now;
            stepCount_++;
            dispensedMl = (double)stepCount_ / stepsPerMl;
        }
    }

    /// Home to endstop (blocking). Returns true if endstop found.
    bool home(double speedMlMin = 1.0) {
        if (pinEndstop < 0) return false;

        setDirection(false);  // reverse
        enable();

        double freq = stepsPerMl * speedMlMin / 60.0;
        unsigned long intervalUs = (unsigned long)(1000000.0 / freq);

        for (int i = 0; i < stepsPerMl * 25; i++) {  // max 25mL travel
            if (digitalRead(pinEndstop) == LOW) {
                disable();
                return true;
            }
            digitalWrite(pinStep, HIGH);
            delayMicroseconds(2);
            digitalWrite(pinStep, LOW);
            delayMicroseconds(intervalUs);
        }

        disable();
        return false;
    }

private:
    volatile unsigned long stepCount_;
    volatile unsigned long lastStepUs_;
    double currentFreq_;
    double targetFreq_;
    double accelStepsPerSec2_;
};
