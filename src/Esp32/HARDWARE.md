# Complete 4-ESP32 Lab System — Hardware Guide

## System Overview

```
┌──────────────────────────────────────────────────────────────────────┐
│                    Blazor WASM (Browser)                             │
│                    .NET 11 Native AOT                                │
│                    8 Pages, 8 Components                             │
│                    3 WebSocket Clients                               │
└────────┬──────────────────┬──────────────────┬───────────────────────┘
         │ wss://:9123      │ wss://:9124      │ wss://:9125
         ▼                  ▼                  ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ ESP32 #1        │ │ ESP32 #3        │ │ ESP32 #4        │
│ Controller      │ │ Environment     │ │ QC & Imaging    │
│ $4              │ │ $4              │ │ $4              │
│                 │ │                 │ │                 │
│ • Heat block    │ │ • Room temp/hum │ │ • Gel imager    │
│ • UV spectro    │ │ • -80°C freezer │ │   (camera+LED)  │
│ • Centrifuge    │ │ • 2-8°C fridge  │ │ • DLS particle  │
│ • Gel electro   │ │ • pH meter      │ │   sizer         │
│                 │ │ • Mag stirrer   │ │ • Turbidity     │
│     │ I2C       │ │ • Vortex mixer  │ │   sensor        │
│     ▼           │ │ • UV-C steril.  │ │                 │
│ ┌───────────┐   │ │ • Fume hood     │ │                 │
│ │ ESP32 #2  │   │ │ • E-stop        │ │                 │
│ │ Stepper   │   │ │ • Gas sensor    │ │                 │
│ │ $4        │   │ │ • Door sensor   │ │                 │
│ │ Pump A+B  │   │ │                 │ │                 │
│ └───────────┘   │ │                 │ │                 │
└─────────────────┘ └─────────────────┘ └─────────────────┘
```

## Total Cost: $16 (4x ESP32-S3 at $4 each)

## ESP32 #1 — Controller (main.cpp)

### Pin Assignment

| Pin | Device | Type | Notes |
|-----|--------|------|-------|
| GPIO 4 | DS18B20 temp sensor | OneWire | 4.7k pullup to 3.3V |
| GPIO 5 | Heater relay | Digital out | Active-high relay module |
| GPIO 12 | Pump A STEP* | Digital out | *Only in single-ESP32 mode |
| GPIO 13 | Pump A DIR* | Digital out | |
| GPIO 14 | Pump B STEP* | Digital out | |
| GPIO 15 | Pump B DIR* | Digital out | |
| GPIO 16 | 260nm UV LED | Digital out | Via N-MOSFET (IRLZ44N) |
| GPIO 17 | 280nm UV LED | Digital out | Via N-MOSFET |
| GPIO 18 | Centrifuge ESC | PWM (50Hz) | Standard RC ESC signal |
| GPIO 19 | Gel relay | Digital out | To DC-DC boost converter |
| GPIO 21 | I2C SDA** | I2C | **Only in dual-ESP32 mode |
| GPIO 22 | I2C SCL** | I2C | |
| GPIO 25 | Gel current sense | ADC | 1-ohm shunt resistor |
| GPIO 26 | Pump B EN* | Digital out | A4988 enable (active low) |
| GPIO 27 | Pump A EN* | Digital out | |
| GPIO 32 | Pump A endstop* | Digital in | N.O. limit switch, pullup |
| GPIO 33 | Pump B endstop* | Digital in | |
| GPIO 34 | Photodiode (sample) | ADC | Transimpedance amp output |
| GPIO 35 | Photodiode (reference) | ADC | |
| GPIO 36 | Centrifuge tachometer | Digital in | Hall effect sensor |
| GPIO 39 | Centrifuge lid switch | Digital in | Safety interlock |
| GPIO 2 | Status LED | Digital out | Built-in |

### Wiring: Heat Block
```
ESP32 GPIO 4 ──── DS18B20 Data (yellow) ──┐
                                           ├── 4.7kΩ ── 3.3V
3.3V ──────────── DS18B20 Vcc (red)        │
GND ───────────── DS18B20 GND (black) ─────┘

ESP32 GPIO 5 ──── Relay IN
Relay COM ──────── 12V PSU +
Relay NO ───────── PTC Heater Element (+)
PTC Heater (−) ── 12V PSU GND
```

### Wiring: UV Spectrophotometer
```
ESP32 GPIO 16 ──── Gate of N-MOSFET #1 (IRLZ44N)
MOSFET #1 Drain ── 260nm UV LED cathode
260nm UV LED anode ── 100Ω resistor ── 5V

ESP32 GPIO 17 ──── Gate of N-MOSFET #2
MOSFET #2 Drain ── 280nm UV LED cathode
280nm UV LED anode ── 100Ω resistor ── 5V

UV Photodiode ──── Transimpedance Amp (OPA380) ──── ESP32 GPIO 34
Reference Photodiode ── Transimpedance Amp ──── ESP32 GPIO 35

[Cuvette holder: 3D printed, light-tight box]
[UV LED → Cuvette → Photodiode (inline, 10mm path)]
[Reference photodiode: same LED, no cuvette (measures LED drift)]
```

### Wiring: Centrifuge
```
ESP32 GPIO 18 ──── ESC Signal (white wire)
ESC Red (+) ────── 12V PSU
ESC Black (−) ──── GND
ESC Motor wires ── Brushless motor (3 phase)

ESP32 GPIO 36 ──── Hall sensor output (tachometer)
Hall sensor Vcc ── 3.3V
Hall sensor GND ── GND
[Magnet glued to rotor, Hall sensor on frame]

ESP32 GPIO 39 ──── Lid microswitch (N.C.)
Microswitch COM ── GND
Microswitch N.C. ── GPIO 39 (internal pullup)
[Switch opens when lid lifted → GPIO goes HIGH → emergency stop]
```

### Wiring: Gel Electrophoresis
```
ESP32 GPIO 19 ──── Relay IN
Relay COM ──────── 12V PSU +
Relay NO ───────── DC-DC Boost Converter Input (+)
Boost Converter Input (−) ── 12V PSU GND
Boost Converter Output (+) ── Platinum wire electrode (anode)
Boost Converter Output (−) ── Platinum wire electrode (cathode)
[Boost converter: XL6009 adjusted to 100V output]

ESP32 GPIO 25 ──── Across 1Ω shunt resistor (in series with gel)
[V = IR → 1Ω × current(A) = voltage on ADC → mA measurement]
```

## ESP32 #2 — Stepper Slave (stepper_slave.cpp)

### Pin Assignment

| Pin | Device | Type |
|-----|--------|------|
| GPIO 12 | Pump A STEP | Digital out |
| GPIO 13 | Pump A DIR | Digital out |
| GPIO 27 | Pump A ENABLE | Digital out |
| GPIO 32 | Pump A endstop | Digital in (pullup) |
| GPIO 14 | Pump B STEP | Digital out |
| GPIO 15 | Pump B DIR | Digital out |
| GPIO 26 | Pump B ENABLE | Digital out |
| GPIO 33 | Pump B endstop | Digital in (pullup) |
| GPIO 21 | I2C SDA | I2C slave |
| GPIO 22 | I2C SCL | I2C slave |

### Wiring: Syringe Pumps
```
For each pump (A and B):

ESP32 STEP ──── A4988 STEP
ESP32 DIR ───── A4988 DIR
ESP32 EN ────── A4988 ENABLE
GND ─────────── A4988 GND

A4988 VMOT ──── 12V PSU (motor power)
A4988 VDD ───── 5V (logic power)
A4988 GND ───── GND (both motor and logic)

A4988 1A, 1B ── NEMA17 coil A (wires 1,2)
A4988 2A, 2B ── NEMA17 coil B (wires 3,4)

A4988 MS1 ───── 5V ┐
A4988 MS2 ───── 5V ├── 1/16 microstepping
A4988 MS3 ───── GND ┘

ESP32 ENDSTOP ── Limit switch (N.O.)
Limit switch other terminal ── GND
[Internal pullup: HIGH = not triggered, LOW = triggered]

[NEMA17 shaft → M8 leadscrew coupling → threaded rod → 3D printed pusher → syringe plunger]
[Leadscrew pitch 1.25mm/rev → 200 steps × 16 microsteps = 3200 steps/rev = 3200 steps/1.25mm]
```

### I2C Connection to ESP32 #1
```
ESP32 #1 GPIO 21 (SDA) ──── ESP32 #2 GPIO 21 (SDA) ──── 4.7kΩ ── 3.3V
ESP32 #1 GPIO 22 (SCL) ──── ESP32 #2 GPIO 22 (SCL) ──── 4.7kΩ ── 3.3V
ESP32 #1 GND ───────────── ESP32 #2 GND
```

## ESP32 #3 — Environment & Safety (env_controller.cpp)

### Pin Assignment

| Pin | Device | Type |
|-----|--------|------|
| GPIO 4 | DHT22 (room temp+humidity) | Digital (DHT protocol) |
| GPIO 5 | UV-C lamp relay | Digital out |
| GPIO 13 | Alarm buzzer | PWM (tone) |
| GPIO 16 | -80°C freezer DS18B20 | OneWire |
| GPIO 17 | 2-8°C fridge DS18B20 | OneWire |
| GPIO 18 | Magnetic stirrer motor | PWM |
| GPIO 19 | Vortex mixer motor | PWM |
| GPIO 25 | Airflow sensor (fume hood) | ADC |
| GPIO 26 | Fume hood fan relay | Digital out |
| GPIO 27 | Emergency stop button | Digital in (pullup) |
| GPIO 32 | Gas/VOC sensor (MQ-2) | ADC |
| GPIO 33 | Door sensor (reed switch) | Digital in (pullup) |
| GPIO 34 | pH probe analog | ADC |
| GPIO 35 | pH temp compensation NTC | ADC |
| GPIO 36 | Stirrer tachometer (Hall) | Digital in |
| GPIO 39 | UV-C door interlock | Digital in (pullup) |
| GPIO 2 | E-stop LED | Digital out |

### Wiring: pH Meter
```
pH Probe BNC ──── pH Amplifier Board (E-201-C or DFRobot SEN0161) ──── ESP32 GPIO 34
pH Amp Vcc ────── 5V
pH Amp GND ────── GND
[Calibrate with pH 4.0 and pH 7.0 buffer solutions]
[Store probe in KCl storage solution when not in use]

NTC Thermistor ──┬── 10kΩ resistor ── 3.3V
                 └── ESP32 GPIO 35
NTC other leg ──── GND
[Insert thermistor into solution alongside pH probe for temp compensation]
```

### Wiring: Magnetic Stirrer
```
ESP32 GPIO 18 ──── L298N IN1 (or MOSFET gate)
L298N OUT1 ──────── DC motor (+)
L298N OUT2 ──────── DC motor (−)
L298N 12V ───────── 12V PSU
L298N 5V ────────── 5V regulator output
L298N GND ───────── GND

Hall sensor ────── ESP32 GPIO 36
[Magnet on motor shaft, Hall sensor nearby for RPM counting]
[DC motor drives rotating magnet assembly under stir plate]
[PTFE stir bar in beaker sits on plate, spins with magnetic coupling]
```

### Wiring: Emergency Stop
```
E-Stop button (N.C.) ──── ESP32 GPIO 27
E-Stop other terminal ── GND
[IMPORTANT: Normally Closed = wire intact = GPIO LOW = safe]
[Button pressed OR wire broken = GPIO HIGH = STOP]
[This is fail-safe: any wire fault triggers stop]

ESP32 GPIO 13 ──── Piezo buzzer (+)
Buzzer (−) ─────── GND

ESP32 GPIO 2 ───── 220Ω ── Red LED anode
Red LED cathode ── GND
```

## ESP32 #4 — QC & Imaging (qc_imaging.cpp)

### Pin Assignment

| Pin | Device | Type |
|-----|--------|------|
| GPIO 4 | Camera SDA (OV2640) | I2C/SCCB |
| GPIO 5 | Camera SCL | I2C/SCCB |
| GPIO 6-17 | Camera DVP data bus | Digital (8-bit parallel) |
| GPIO 18 | Orange filter servo | PWM (50Hz) |
| GPIO 19 | DLS laser diode | Digital out |
| GPIO 5 | Turbidity LED | Digital out |
| GPIO 34 | DLS photodetector | ADC (high-speed sampling) |
| GPIO 35 | Turbidity detector | ADC |
| GPIO 36 | Turbidity reference | ADC |
| GPIO 2 | Status LED | Digital out |

### Wiring: Gel Imager
```
[Use ESP32-S3-EYE or ESP32-CAM board with built-in OV2640]

Blue LED strip (470nm) ──── MOSFET gate ── ESP32 GPIO 16
MOSFET drain ──── LED strip (−)
LED strip (+) ──── 5V via 100Ω current limiting
[LED strip mounted UNDER gel tray = transillumination]

Servo (SG90) signal ──── ESP32 GPIO 18
Servo Vcc ──── 5V
Servo GND ──── GND
[Servo swings orange filter (Roscolux #21) in front of camera lens]
[0° = filter out (white light viewing), 90° = filter in (GelRed/SYBR imaging)]

Camera lens ── above gel, looking down through orange filter
```

### Wiring: DLS (Dynamic Light Scattering)
```
ESP32 GPIO 19 ──── 100Ω ── 650nm laser diode anode
Laser cathode ──── GND via constant-current driver (LM317 + 10Ω sense resistor, Iset=120mA)

[Optical setup: laser → sample cuvette → 90° scattered light → photodetector]

Photodiode (BPW34) ──── Transimpedance Amp (OPA380, 10MΩ feedback) ──── ESP32 GPIO 34
[Photodetector at 90° angle to laser beam]
[Sample: 10uL LNP diluted in 990uL PBS in micro-cuvette]

[Algorithm: sample ADC at 10kHz for 5 seconds → autocorrelation → Stokes-Einstein → diameter in nm]
[Expected: 60-100nm for good LNP, PDI < 0.2 for uniform population]
```

### Wiring: Turbidity Sensor
```
ESP32 GPIO 5 ──── 100Ω ── 850nm IR LED anode (or 600nm visible)
IR LED cathode ── GND

[Optical path: LED → cuvette (sample) → photodetector]
BPW34 photodetector ──── TIA (transimpedance amp) ──── ESP32 GPIO 35

[Reference path: LED → photodetector (no sample, measures LED power)]
BPW34 reference ──── TIA ──── ESP32 GPIO 36

OD = -log10(sample_reading / reference_reading)
[Higher OD = more particles = more scattering]
[Use to quickly estimate LNP concentration before DLS]
```

## Power Supply

```
12V 10A Switching PSU ($8)
│
├── 12V direct ── Heater relay (ESP32 #1)
├── 12V direct ── A4988 VMOT x2 (ESP32 #2)
├── 12V direct ── Brushless ESC (ESP32 #1)
├── 12V direct ── DC-DC boost (gel electrophoresis)
├── 12V direct ── L298N (stirrer motor, ESP32 #3)
│
└── 12V → LM2596 Buck Converter → 5V 3A
    ├── 5V ── ESP32 #1 via VIN/USB
    ├── 5V ── ESP32 #2 via VIN/USB
    ├── 5V ── ESP32 #3 via VIN/USB
    ├── 5V ── ESP32 #4 via VIN/USB
    ├── 5V ── UV LEDs (via MOSFET)
    ├── 5V ── Camera module
    ├── 5V ── Servos
    ├── 5V ── A4988 VDD
    └── 5V ── Relay modules
```

## Complete Bill of Materials

| Item | Qty | Price | Source |
|------|-----|-------|--------|
| ESP32-S3 DevKit | 4 | $16 | AliExpress |
| NEMA17 stepper motor | 2 | $16 | Amazon |
| A4988 stepper driver | 2 | $3 | Amazon |
| DS18B20 temp sensor | 3 | $6 | Amazon |
| DHT22 temp/humidity | 1 | $4 | Amazon |
| 5V relay module (4ch) | 1 | $4 | Amazon |
| 260nm UV LED | 1 | $15 | Mouser |
| 280nm UV LED | 1 | $12 | Mouser |
| BPW34 photodiode | 4 | $8 | Mouser |
| OPA380 transimpedance amp | 3 | $12 | Mouser |
| pH probe + amplifier | 1 | $15 | Amazon (DFRobot) |
| 650nm laser diode module | 1 | $5 | Amazon |
| Brushless motor + ESC | 1 | $15 | Amazon |
| DC motor (stirrer) | 1 | $5 | Amazon |
| Eccentric motor (vortex) | 1 | $3 | Amazon |
| SG90 servo (filter) | 1 | $2 | Amazon |
| MQ-2 gas sensor | 1 | $3 | Amazon |
| Hall effect sensors | 3 | $3 | Amazon |
| Limit switches | 2 | $2 | Amazon |
| Piezo buzzer | 1 | $1 | Amazon |
| N-MOSFET IRLZ44N | 4 | $4 | Amazon |
| L298N motor driver | 1 | $3 | Amazon |
| LM2596 buck converter | 1 | $3 | Amazon |
| XL6009 boost converter | 1 | $3 | Amazon |
| 12V 10A PSU | 1 | $8 | Amazon |
| Platinum wire (0.5mm, 30cm) | 1 | $15 | Amazon |
| Quartz micro-cuvette | 2 | $10 | Amazon |
| UV-C germicidal lamp (11W) | 1 | $8 | Amazon |
| Reed switch (door) | 1 | $1 | Amazon |
| E-stop mushroom button (N.C.) | 1 | $3 | Amazon |
| Resistors, caps, PCBs, wire | 1 | $10 | Amazon |
| 3D printer filament (1kg PLA) | 1 | $10 | Amazon |
| **HARDWARE TOTAL** | | **~$250** | |

Add ~$1,500-$1,700 for reagents (IVT kits, plasmids, lipids, modified nucleotides) per BEST_BUILD_COMBINED.md.

**Grand total: ~$1,750-$1,950 for a complete mRNA synthesis lab with 17 instruments.**
