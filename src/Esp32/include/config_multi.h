#pragma once

// ============================================================
// Multi-ESP32 System Configuration
// ============================================================
//
// 4 ESP32-S3 boards total ($16):
//
//   ESP32 #1 — Controller    (WiFi + WebSocket + heat/spectro/centrifuge/gel)
//   ESP32 #2 — Stepper Slave (I2C, dedicated pump timing)
//   ESP32 #3 — Environment   (WiFi + WebSocket + safety/monitoring/pH/stir/vortex)
//   ESP32 #4 — Imaging & QC  (WiFi + WebSocket + camera/DLS/turbidity)
//
// Communication:
//   #1 ←→ Blazor WASM via WebSocket (wss://ESP32_1_IP:9123)
//   #1 ←→ #2 via I2C (SDA/SCL)
//   #3 ←→ Blazor WASM via WebSocket (wss://ESP32_3_IP:9124)
//   #4 ←→ Blazor WASM via WebSocket (wss://ESP32_4_IP:9125)
//
// Blazor WASM connects to all 3 WebSocket endpoints simultaneously.

// ===== ESP32 #3 — Environment & Safety Controller =====

// WiFi (same network as #1)
#define ENV_WSS_PORT        9124

// Temperature monitoring
#define PIN_ENV_ROOM_TEMP   4       // DHT22 (room temp + humidity)
#define PIN_ENV_FREEZER     16      // DS18B20 (in -80°C freezer, long wire)
#define PIN_ENV_FRIDGE      17      // DS18B20 (in 2-8°C fridge)

// pH Meter
#define PIN_PH_PROBE        34      // ADC — analog pH probe (0-3.3V = pH 0-14)
#define PIN_PH_TEMP_COMP    35      // ADC — temperature compensation (NTC thermistor in solution)
#define PH_CALIBRATION_4    1.85    // ADC voltage at pH 4.0 (calibrate with buffer)
#define PH_CALIBRATION_7    2.50    // ADC voltage at pH 7.0
#define PH_CALIBRATION_10   3.10    // ADC voltage at pH 10.0

// Magnetic Stirrer
#define PIN_STIRRER_PWM     18      // PWM to DC motor driver (L298N or MOSFET)
#define PIN_STIRRER_TACH    36      // Hall sensor for RPM measurement
#define STIRRER_MAX_RPM     1500

// Vortex Mixer
#define PIN_VORTEX_PWM      19      // PWM to vibration motor / eccentric DC motor
#define VORTEX_MAX_RPM      3000

// UV-C Sterilization Lamp
#define PIN_UVC_RELAY       5       // Relay to 254nm UV-C germicidal lamp
#define UVC_MAX_MINUTES     30      // Safety timeout
#define PIN_UVC_DOOR        39      // Door interlock — UV-C off if door opens

// Fume Hood Fan
#define PIN_FUMEHOOD_RELAY  26      // Relay to exhaust fan
#define PIN_AIRFLOW_SENSOR  25      // ADC — anemometer / airflow verification

// Emergency Stop
#define PIN_ESTOP           27      // N.C. pushbutton — breaks power to all relays
#define PIN_ESTOP_BUZZER    13      // Piezo buzzer for alarm
#define PIN_ESTOP_LED       2       // Red LED

// Smoke / VOC Sensor
#define PIN_GAS_SENSOR      32      // MQ-2 or MQ-135 (smoke / VOC / ethanol vapor)
#define GAS_ALARM_THRESHOLD 800     // ADC reading triggering alarm

// Door Sensor
#define PIN_DOOR_SENSOR     33      // Magnetic reed switch (lab door)

// ===== ESP32 #4 — Imaging & QC Controller =====

#define QC_WSS_PORT         9125

// Gel Imager
#define PIN_CAMERA_TRIGGER  4       // Trigger for OV2640 / OV5640 camera module
#define PIN_TRANSILLUM_LED  16      // Blue LED array (470nm) under gel
#define PIN_TRANSILLUM_PWM  17      // PWM brightness control
#define PIN_ORANGE_FILTER   18      // Servo to swing orange filter in front of camera

// Dynamic Light Scattering (simplified)
// Uses 650nm laser diode + photodetector at 90°
#define PIN_DLS_LASER       19      // Laser diode on/off
#define PIN_DLS_DETECTOR    34      // ADC — avalanche photodiode or photodiode + TIA
#define DLS_SAMPLE_RATE_HZ  10000   // ADC sampling rate for autocorrelation
#define DLS_MEASUREMENT_MS  5000    // Measurement duration

// Turbidity Sensor (OD600-style)
// LED + photodetector, sample in cuvette between them
#define PIN_TURBIDITY_LED   5       // 600nm LED (or 850nm IR)
#define PIN_TURBIDITY_DET   35      // ADC — photodetector
#define PIN_TURBIDITY_REF   36      // ADC — reference detector (no sample path)

// Status
#define PIN_QC_STATUS_LED   2
