#pragma once

/// Discrete PID controller with integral windup protection and derivative filtering.
class PidController {
public:
    double kp, ki, kd;
    double integralLimit;

    PidController(double kp, double ki, double kd, double integralLimit)
        : kp(kp), ki(ki), kd(kd), integralLimit(integralLimit),
          integral_(0), lastError_(0), lastOutput_(0), initialized_(false) {}

    /// Compute PID output. dt in seconds.
    double compute(double setpoint, double measured, double dt) {
        double error = setpoint - measured;

        // Proportional
        double p = kp * error;

        // Integral with anti-windup
        integral_ += error * dt;
        if (integral_ > integralLimit) integral_ = integralLimit;
        if (integral_ < -integralLimit) integral_ = -integralLimit;
        double i = ki * integral_;

        // Derivative (on measurement, not error, to avoid kick on setpoint change)
        double d = 0;
        if (initialized_ && dt > 0) {
            double dmeas = (measured - lastMeasured_) / dt;
            d = -kd * dmeas;  // negative because derivative of measurement
        }

        lastError_ = error;
        lastMeasured_ = measured;
        initialized_ = true;

        double output = p + i + d;
        lastOutput_ = output;
        return output;
    }

    void reset() {
        integral_ = 0;
        lastError_ = 0;
        lastOutput_ = 0;
        initialized_ = false;
    }

    double getIntegral() const { return integral_; }
    double getLastOutput() const { return lastOutput_; }

private:
    double integral_;
    double lastError_;
    double lastMeasured_;
    double lastOutput_;
    bool initialized_;
};
