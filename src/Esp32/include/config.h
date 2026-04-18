#pragma once

// ============================================================
// RNA Therapy Platform — ESP32-S3 Hardware Configuration
// ============================================================

// ----- Architecture -----
// Set to 1 for dual-ESP32 mode (I2C stepper slave on ESP32 #2)
// Set to 0 for single-ESP32 mode (steppers on same board)
#define DUAL_ESP32_MODE     0
#define I2C_STEPPER_ADDR    0x10    // I2C address of ESP32 #2 stepper slave
#define PIN_I2C_SDA         21
#define PIN_I2C_SCL         22

// ----- WiFi -----
#define WIFI_SSID       "YOUR_WIFI_SSID"
#define WIFI_PASS       "YOUR_WIFI_PASS"
#define WSS_PORT        9123  // WebSocket server port (wss://0.0.0.0:9123)

// ----- Pin Assignments (ESP32-S3 DevKitC-1) -----

// Heat Block
#define PIN_TEMP_SENSOR     4   // DS18B20 data (OneWire) — 4.7k pullup to 3.3V
#define PIN_HEATER_RELAY    5   // Active-high relay module to PTC heater

// Syringe Pump A (mRNA / aqueous buffer)
#define PIN_PUMP_A_STEP     12  // A4988 STEP
#define PIN_PUMP_A_DIR      13  // A4988 DIR
#define PIN_PUMP_A_EN       27  // A4988 ENABLE (active low)
#define PIN_PUMP_A_ENDSTOP  32  // Limit switch (normally open, pull-up)

// Syringe Pump B (lipid / ethanol)
#define PIN_PUMP_B_STEP     14  // A4988 STEP
#define PIN_PUMP_B_DIR      15  // A4988 DIR
#define PIN_PUMP_B_EN       26  // A4988 ENABLE (active low)
#define PIN_PUMP_B_ENDSTOP  33  // Limit switch (normally open, pull-up)

// UV Spectrophotometer
#define PIN_UV_260          16  // 260nm UV LED (via MOSFET)
#define PIN_UV_280          17  // 280nm UV LED (via MOSFET)
#define PIN_PHOTODIODE      34  // ADC1_CH6 — UV photodiode + transimpedance amp
#define PIN_PHOTODIODE_REF  35  // ADC1_CH7 — Reference photodiode (no sample)

// Centrifuge
#define PIN_CENTRIFUGE_PWM  18  // ESC signal (50Hz PWM)
#define PIN_CENTRIFUGE_TACH 36  // Hall sensor / IR tachometer (pulse counting)
#define PIN_CENTRIFUGE_LID  39  // Safety interlock (lid switch, active low)

// Gel Electrophoresis
#define PIN_GEL_RELAY       19  // Relay to DC-DC boost converter (100V output)
#define PIN_GEL_CURRENT     25  // ADC — current sense shunt (mA measurement)

// Status LED
#define PIN_STATUS_LED      2   // Built-in LED

// ----- Stepper Motor Config -----
#define STEPS_PER_REV       200     // NEMA17 = 200 steps/rev
#define MICROSTEPS          16      // A4988 microstepping (MS1=MS2=HIGH, MS3=LOW)
#define STEPS_PER_ML        3200    // Calibrated: full steps * microsteps * leadscrew pitch
                                    // 200 * 16 / (pi * syringe_diameter^2 / 4 * pitch)
#define MAX_STEP_FREQ       4000    // Max steps/sec before motor stalls

// ----- PID Defaults -----
#define PID_KP              2.0
#define PID_KI              0.5
#define PID_KD              1.0
#define PID_INTEGRAL_LIMIT  50.0
#define PID_UPDATE_MS       500

// ----- Spectrophotometer Config -----
#define SPECTRO_SAMPLES     64      // ADC readings to average per measurement
#define SPECTRO_SETTLE_MS   200     // LED settle time before reading
#define SPECTRO_BLANK_MS    100     // Blank settle time
#define RNA_FACTOR          40.0    // A260 of 1.0 = 40 ng/uL for ssRNA

// ----- Centrifuge Config -----
#define ESC_MIN_US          1000    // ESC pulse width: stop (microseconds)
#define ESC_MAX_US          2000    // ESC pulse width: full speed
#define CENTRIFUGE_MAX_RPM  10000
#define CENTRIFUGE_RAMP_MS  3000    // Ramp-up time to target RPM

// ----- Safety -----
#define HEATER_MAX_TEMP     100.0   // Emergency shutoff temperature
#define HEATER_TIMEOUT_MS   14400000 // 4 hours max continuous heating
#define PUMP_MAX_VOLUME_ML  20.0    // Safety limit per run
#define GEL_MAX_VOLTAGE     150.0   // Maximum allowed voltage
#define GEL_MAX_MINUTES     120     // Maximum run time
