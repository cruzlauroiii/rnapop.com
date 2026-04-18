# Driver & Module Instructions — Complete Wiring Guide

Every driver board listed here is a pre-made module you buy for $1-6. No soldering of ICs needed — just connect wires to screw terminals or header pins.

---

## 1. Relay Module (5V, 4-Channel) — $4

**Used for:** Heater, UV-C lamp, fume hood fan, gel power supply

**What it does:** ESP32 sends 3.3V signal → relay clicks → connects/disconnects 12V or 120V power to device.

**Buy:** Search "4 channel relay module 5V" on Amazon/AliExpress

### Pinout (per channel)

```
Module side (low voltage):        Load side (high voltage):
┌─────────────────────┐          ┌──────────────────────┐
│ VCC ── 5V           │          │ COM ── power source + │
│ GND ── GND          │          │ NO  ── device +       │
│ IN1 ── ESP32 GPIO   │          │ NC  ── (not used)     │
│ IN2 ── ESP32 GPIO   │          │                       │
│ IN3 ── ESP32 GPIO   │          │ [Device − goes to     │
│ IN4 ── ESP32 GPIO   │          │  power source −]      │
└─────────────────────┘          └──────────────────────┘
```

### Wiring Example: Heater

```
Step 1: Module power
  5V rail ──────── VCC
  GND rail ─────── GND

Step 2: Signal from ESP32
  ESP32 GPIO 5 ─── IN1

Step 3: Load (heater)
  12V PSU (+) ──── COM (common)
  NO (normally open) ── PTC Heater wire 1
  PTC Heater wire 2 ── 12V PSU (−)
```

### How it works
- ESP32 GPIO LOW → relay open → heater OFF
- ESP32 GPIO HIGH → relay clicks closed → heater ON
- You hear an audible "click" when it switches

### Safety notes
- **Never** touch the load side (COM/NO/NC) when powered
- Keep high-voltage wires (if using 120V AC) away from low-voltage side
- The relay has a max rating printed on it (usually 10A 250VAC or 10A 30VDC) — don't exceed it
- For the heater: a 50W PTC element at 12V draws ~4A — well within relay rating

---

## 2. A4988 Stepper Driver — $1.50 each (need 2)

**Used for:** Syringe pump NEMA17 stepper motors

**What it does:** ESP32 sends step pulses (HIGH/LOW) → A4988 converts each pulse into one motor step with correct coil energization.

**Buy:** Search "A4988 stepper driver module" on Amazon/AliExpress (red or green PCB)

### Pinout

```
Top view of A4988 module:

         ┌──────────┐
  ENABLE │●         │ VMOT ← 12V motor power
      MS1│          │ GND  ← motor power GND
      MS2│          │ 2B   ← motor coil B wire 2
      MS3│          │ 2A   ← motor coil B wire 1
    RESET│          │ 1A   ← motor coil A wire 1
    SLEEP│          │ 1B   ← motor coil A wire 2
     STEP│          │ VDD  ← 5V logic power
      DIR│          │ GND  ← logic GND
         └──────────┘
```

### Wiring Step-by-Step

```
Step 1: Logic power (MUST connect before motor power)
  5V rail ───── VDD
  GND rail ──── GND (bottom right)

Step 2: Motor power
  12V PSU (+) ── VMOT
  12V PSU (−) ── GND (top right)
  ⚠ Put a 100µF capacitor across VMOT and GND (close to the board)
    This prevents voltage spikes that kill the chip

Step 3: Motor wires (NEMA17 has 4 wires)
  Find the two coil pairs:
    - Touch two wires together and try to spin the shaft by hand
    - If it feels "sticky"/resistant → those two are a coil pair
    - The other two are the second pair
  
  Coil A (pair 1): wire 1 → 1A, wire 2 → 1B
  Coil B (pair 2): wire 1 → 2A, wire 2 → 2B
  
  If motor spins wrong direction: swap 1A and 1B

Step 4: Control signals from ESP32
  ESP32 STEP GPIO ── STEP
  ESP32 DIR GPIO ─── DIR
  ESP32 EN GPIO ──── ENABLE

Step 5: Microstepping (solder or jumper)
  MS1 → 5V ┐
  MS2 → 5V ├── 1/16 microstepping (smoothest, most precise)
  MS3 → GND ┘

Step 6: RESET and SLEEP
  Connect RESET to SLEEP (jumper wire between them)
  This keeps the driver active
```

### Current Limiting (CRITICAL — do this before connecting motor)

The A4988 has a tiny brass potentiometer on top. You MUST set the current limit or you'll burn the motor or driver.

```
1. Power the board (5V to VDD, 12V to VMOT) with NO motor connected
2. Measure voltage on the brass potentiometer center with multimeter
   (touch multimeter + to potentiometer, − to GND)
3. Formula: I_motor = V_ref / (8 × R_sense)
   Most boards: R_sense = 0.1Ω → I_motor = V_ref × 1.25
4. NEMA17 typical rating: 1.2A
   → Set V_ref = 1.2 / 1.25 = 0.96V
5. Turn potentiometer with tiny screwdriver until multimeter reads ~0.96V
6. NOW connect motor
```

### How it works
- Each HIGH pulse on STEP = motor moves one microstep (1/16 of 1.8°)
- DIR pin: HIGH = clockwise, LOW = counterclockwise
- ENABLE: LOW = motor energized (holds position), HIGH = motor free (can spin by hand)
- The firmware sends thousands of step pulses per second to push the syringe plunger

---

## 3. L298N Motor Driver — $3

**Used for:** Magnetic stirrer DC motor

**What it does:** ESP32 sends PWM signal → L298N drives DC motor at variable speed.

**Buy:** Search "L298N motor driver module" on Amazon/AliExpress (blue PCB with heatsink)

### Pinout

```
┌──────────────────────────────────┐
│  +12V   GND   +5V               │ ← Power screw terminals
│                                  │
│  OUT1   OUT2   OUT3   OUT4      │ ← Motor screw terminals
│                                  │
│  IN1  IN2  ENA  IN3  IN4  ENB  │ ← Control header pins
└──────────────────────────────────┘
```

### Wiring

```
Step 1: Power
  12V PSU (+) ── +12V terminal
  12V PSU (−) ── GND terminal
  Remove the 5V jumper if powering ESP32 separately
  (Leave jumper ON to get 5V output from the board's regulator)

Step 2: Motor
  DC motor wire 1 ── OUT1
  DC motor wire 2 ── OUT2
  (OUT3/OUT4 for a second motor — not used here)

Step 3: Control
  ESP32 PWM GPIO ── ENA (speed control via PWM)
  ESP32 GPIO (or 5V) ── IN1 (direction 1)
  GND ── IN2 (direction 2)
  
  For one-direction operation:
    IN1 = HIGH (5V or 3.3V), IN2 = LOW (GND)
    ENA = PWM from ESP32 (0-255 duty cycle = 0-100% speed)
```

### How it works
- ENA controls speed: PWM duty 0% = stopped, 100% = full speed
- IN1/IN2 control direction: IN1=H,IN2=L = forward. Swap = reverse.
- The heatsink gets warm under load — that's normal

---

## 4. ESC (Electronic Speed Controller) — $6-10

**Used for:** Centrifuge brushless motor

**What it does:** ESP32 sends servo-style PWM (1-2ms pulses at 50Hz) → ESC commutates 3 motor phases at variable speed.

**Buy:** Search "30A brushless ESC" on Amazon/AliExpress (any RC airplane/drone ESC works)

### Wiring

```
ESC has 3 sets of wires:

1. INPUT (thick red + black):
   Red (+) ── 12V PSU (+)
   Black (−) ── 12V PSU (−)

2. OUTPUT (3 thick wires, usually blue/yellow/orange):
   All 3 ── Brushless motor (3 phase wires, any order)
   If motor spins wrong direction: swap any 2 of the 3 wires

3. SIGNAL (thin 3-wire servo connector):
   White/Yellow (signal) ── ESP32 GPIO 18
   Red (BEC 5V out) ── NOT CONNECTED (don't power ESP32 from ESC)
   Black (GND) ── ESP32 GND
```

### Arming Sequence (MUST do on first power-up)

```
The ESC must be "armed" before it will spin the motor:

1. Power on ESP32 (sends 1.5ms pulse = zero throttle)
2. Wait 2 seconds (ESC beeps: ♪♪ = armed)
3. Now you can change throttle:
   - 1.0ms pulse = stop
   - 1.5ms pulse = stop (neutral)
   - 2.0ms pulse = full speed

The firmware handles this automatically in centrifuge.begin()
(2-second delay during setup with zero throttle)
```

### Safety
- **ALWAYS** arm at zero throttle — never power up with throttle above zero
- The centrifuge lid interlock (microswitch on GPIO 39) prevents spinning with lid open
- Remove propeller/rotor before testing — a spinning brushless motor is DANGEROUS

---

## 5. N-Channel MOSFET (IRLZ44N) — $1 for 4

**Used for:** Switching UV LEDs, transilluminator LED strip

**What it does:** ESP32 sends 3.3V signal to gate → MOSFET conducts → high-current LED circuit completes.

**Buy:** Search "IRLZ44N MOSFET" (must be logic-level — IRLZ44N turns on at 3.3V, regular IRF540N needs 10V)

### Wiring

```
              ┌─── 5V
              │
         100Ω resistor
              │
         LED anode (+)
              │
         LED cathode (−)
              │
    DRAIN ────┘
    ┌────┐
    │IRLZ│    (TO-220 package, heatsink tab = drain)
    │44N │
    └────┘
    GATE ──── ESP32 GPIO (16 or 17)
    SOURCE ── GND
              │
        10kΩ pulldown resistor to GND
        (keeps MOSFET OFF during ESP32 boot)
```

### How it works
- GPIO HIGH (3.3V) → MOSFET gate opens → current flows from 5V through LED to GND
- GPIO LOW → MOSFET closes → LED off
- The 10kΩ pulldown resistor is important: without it, the MOSFET may turn on randomly during ESP32 startup (floating gate)

### For the transilluminator (LED strip)
Same circuit but the LED strip replaces the single LED:
```
5V ── LED strip (+red wire)
LED strip (−black wire) ── MOSFET DRAIN
MOSFET SOURCE ── GND
MOSFET GATE ── ESP32 GPIO
```

---

## 6. OPA380 Transimpedance Amplifier — $4 each (need 3)

**Used for:** Converting photodiode current to voltage for ESP32 ADC (spectrophotometer, DLS, turbidity)

**What it does:** Photodiode produces tiny current (nanoamps to microamps) when light hits it → OPA380 converts to 0-3.3V voltage → ESP32 ADC reads it.

**Buy:** Search "OPA380 breakout board" or buy OPA380 DIP chip + solder to perfboard

### Circuit

```
                    Rf (feedback resistor)
              ┌────┤ 10MΩ (for very low light) ├────┐
              │    └────────────────────────────┘    │
              │         Cf (optional)                │
              │    ┌──┤ 1pF ├──┐                     │
              │    │           │                     │
 Photodiode   │    │   ┌──────┐│                     │
  cathode ────┴────┴───┤−     ├┴──── Output ── ESP32 ADC GPIO
                       │OPA380│
  3.3V ──── 100kΩ ────┤+     │
                       └──┬───┘
                          │
                     Vcc = 3.3V
                     GND = GND

 Photodiode anode ── GND
```

### Simplified (if using breakout board)

```
Breakout board typically has 3 pins:
  VCC ── 3.3V
  GND ── GND
  OUT ── ESP32 ADC GPIO

Connect photodiode:
  Cathode (short leg) ── board input pad
  Anode (long leg) ── GND
```

### Choosing the feedback resistor (Rf)

| Rf Value | Sensitivity | Use Case |
|----------|-------------|----------|
| 1MΩ | Low | Bright signals (turbidity reference) |
| 10MΩ | Medium | UV spectrophotometer |
| 100MΩ | High | DLS scattered light (very dim) |

Higher Rf = more sensitive but slower response and more noise.

### How it works
- Light hits photodiode → photodiode generates current proportional to light intensity
- OPA380 converts: V_out = I_photodiode × Rf
- ESP32 ADC reads V_out (0-3.3V → 0-4095 digital)
- Beer-Lambert law: Absorbance = -log10(V_sample / V_reference)

---

## 7. pH Amplifier Board (DFRobot SEN0161) — $15

**Used for:** Measuring pH of citrate buffer (must be pH 4.0 for LNP encapsulation)

**Buy:** Search "DFRobot analog pH meter V2" or "SEN0161" on Amazon

### Wiring

```
pH Probe (BNC connector) ──── BNC socket on amplifier board

Board pins:
  V+  ── 5V
  GND ── GND
  A   ── ESP32 GPIO 34 (ADC)
```

That's it — 3 wires + BNC plug.

### Calibration (MUST do before first use)

```
You need two buffer solutions: pH 4.0 (red) and pH 7.0 (yellow/green)
Buy them for ~$5 at Amazon ("pH calibration buffer")

1. Rinse probe in distilled water, pat dry
2. Put probe in pH 7.0 buffer
3. Wait 2 minutes for reading to stabilize
4. Record ADC voltage (this is PH_CALIBRATION_7 in config_multi.h)
5. Rinse probe, pat dry
6. Put probe in pH 4.0 buffer
7. Wait 2 minutes
8. Record ADC voltage (this is PH_CALIBRATION_4 in config_multi.h)
9. Update config_multi.h with your values
10. Rinse probe, store in KCl storage solution (comes with probe)
```

### Care
- **Never** let the glass tip dry out — always store in KCl solution
- Recalibrate monthly
- Replace probe every 12-18 months (glass membrane degrades)
- Don't touch the glass bulb with fingers (oil affects readings)

---

## 8. DHT22 Temperature & Humidity Sensor — $4

**Used for:** Room environment monitoring

**Buy:** Search "DHT22" or "AM2302" on Amazon

### Wiring

```
DHT22 has 3 or 4 pins (left to right, facing the grid):

Pin 1 (VCC) ── 3.3V
Pin 2 (DATA) ── ESP32 GPIO 4
                 └── 10kΩ pullup resistor ── 3.3V
Pin 3 (NC)   ── not connected (4-pin version only)
Pin 4 (GND)  ── GND
```

### How it works
- Single-wire digital protocol (not I2C, not SPI — proprietary)
- The DHT library handles the protocol automatically
- Read every 2+ seconds (sensor needs time between readings)
- Accuracy: ±0.5°C temperature, ±2% humidity

---

## 9. DS18B20 Temperature Sensor — $2 each (need 3)

**Used for:** Heat block, -80°C freezer, 2-8°C fridge monitoring

**Buy:** Search "DS18B20 waterproof" on Amazon (get the ones with stainless steel probe + cable)

### Wiring (each sensor)

```
Red wire ──── 3.3V
Black wire ── GND
Yellow wire ── ESP32 GPIO (4, 16, or 17)
               └── 4.7kΩ pullup resistor ── 3.3V
```

### Multiple sensors on one pin (optional)
DS18B20 is a 1-Wire bus — you CAN put multiple sensors on the same GPIO with one pullup resistor. Each sensor has a unique 64-bit address. But for this project we use separate pins for clarity.

### For the -80°C freezer
- Use the waterproof probe version (stainless steel tube)
- Run the cable through the freezer door seal
- The DS18B20 works down to -55°C officially, but many report readings down to -80°C
- If readings cut out below -55°C, use a PT100 RTD + MAX31865 instead

---

## 10. MQ-2 Gas/Smoke Sensor — $3

**Used for:** Detecting ethanol vapor during LNP mixing, smoke alarm

**Buy:** Search "MQ-2 sensor module" on Amazon

### Wiring

```
Module has 4 pins:
  VCC ── 5V (needs 5V, not 3.3V — built-in heater)
  GND ── GND
  AO  ── ESP32 GPIO 32 (analog output → ADC)
  DO  ── not used (digital threshold output)
```

### First-use burn-in
- The MQ-2 needs **24-48 hours** of continuous power to burn off manufacturing residue
- During burn-in, readings will be erratic — this is normal
- After burn-in, readings stabilize

### How it works
- Internal heater warms a tin dioxide (SnO2) semiconductor
- Gas molecules change the resistance → voltage on AO changes
- Higher AO voltage = more gas detected
- The firmware triggers alarm at ADC > 800 (configurable in config_multi.h)

---

## 11. Hall Effect Sensor (A3144) — $1 for 5

**Used for:** Centrifuge RPM, stirrer RPM measurement

**Buy:** Search "A3144 Hall effect sensor" on Amazon

### Wiring

```
       ┌────┐
       │A   │  (flat face with text toward you)
       │3144│
       └┬┬┬┘
        │││
        ││└── Pin 3 (GND) ── GND
        │└─── Pin 2 (OUT) ── ESP32 GPIO ── 10kΩ pullup ── 3.3V
        └──── Pin 1 (VCC) ── 3.3V
```

### Installation
- Glue a small neodymium magnet to the spinning shaft/rotor
- Mount the Hall sensor 2-5mm from the magnet's path
- Each time the magnet passes → output goes LOW → ESP32 counts the pulse
- Pulses per second × 60 = RPM

---

## 12. SG90 Micro Servo — $2

**Used for:** Swinging orange filter in front of gel imager camera

**Buy:** Search "SG90 servo motor" on Amazon

### Wiring

```
Orange wire (signal) ── ESP32 GPIO 18
Red wire (VCC) ── 5V
Brown wire (GND) ── GND
```

### How it works
- ESP32 sends 50Hz PWM signal
- Pulse width controls angle: 1ms = 0°, 1.5ms = 90°, 2ms = 180°
- The ESP32Servo library handles the conversion: `servo.write(90)` = 90 degrees
- In this project: 0° = filter out of camera path, 90° = filter in front of lens

---

## 13. E-Stop Mushroom Button (N.C.) — $3

**Used for:** Emergency shutdown of all equipment

**Buy:** Search "emergency stop mushroom button N.C." on Amazon (get the twist-to-release type)

### Wiring

```
Button has 2 terminals (normally closed):

Terminal 1 ── ESP32 GPIO 27
Terminal 2 ── GND

ESP32 GPIO 27 has internal pullup enabled in firmware.
```

### How it works (fail-safe design)
```
Normal state:
  Button closed → wire completes circuit → GPIO reads LOW → system OK

Button pressed:
  Button opens → circuit broken → GPIO reads HIGH → EMERGENCY STOP

Wire cut/disconnected:
  Same as button pressed → GPIO reads HIGH → EMERGENCY STOP
```

This is a **fail-safe** design: ANY fault (button press, wire break, connector loose) triggers the stop. This is why we use Normally Closed, not Normally Open.

### Reset
- Twist the mushroom button clockwise to release it
- Press "E-Stop Reset" button in the Blazor UI to acknowledge and re-enable equipment
- Equipment does NOT auto-restart — you must manually restart each device

---

## Power Supply Wiring

```
                    12V 10A Switching PSU
                    ┌─────────────────┐
    AC input ──────►│  120/240V AC    │
                    │  to 12V DC      │
                    │                 │
                    │  V+ ──┬── 12V bus (red wire, use 14AWG)
                    │       │
                    │  V- ──┬── GND bus (black wire, use 14AWG)
                    └───────┘

    12V bus splits to:
    ├── Heater relay COM
    ├── A4988 #1 VMOT (+ 100µF cap to GND)
    ├── A4988 #2 VMOT (+ 100µF cap to GND)
    ├── ESC red wire
    ├── Gel boost converter input
    ├── L298N +12V terminal
    │
    └── LM2596 Buck Converter ── adjust trimpot to 5.0V output
        │
        5V bus splits to:
        ├── ESP32 #1 VIN (or USB 5V)
        ├── ESP32 #2 VIN
        ├── ESP32 #3 VIN
        ├── ESP32 #4 VIN
        ├── Relay module VCC
        ├── A4988 #1 VDD
        ├── A4988 #2 VDD
        ├── UV LED circuits (via MOSFET)
        ├── Servo VCC
        ├── DHT22 VCC
        ├── MQ-2 VCC
        ├── pH amp V+
        └── L298N 5V (if jumper removed)

    GND bus:
    ALL GND wires connect to the same GND bus.
    ESP32 GNDs, driver GNDs, sensor GNDs, PSU V- = all the same wire.
```

### LM2596 Buck Converter Setup

```
1. Connect 12V to IN+ and IN−
2. Connect multimeter to OUT+ and OUT−
3. Turn the small brass potentiometer with a screwdriver
4. Adjust until multimeter reads exactly 5.0V
5. Now connect 5V bus to OUT+
```

---

## Tools Needed

| Tool | Price | What For |
|------|-------|----------|
| Multimeter | $15 | Checking voltages, setting A4988 current, testing connections |
| Wire strippers | $8 | Stripping jumper wire insulation |
| Small screwdriver set | $5 | Adjusting potentiometers (A4988, LM2596, XL6009) |
| Soldering iron + solder | $25 | Attaching wires to boards that don't have screw terminals |
| Breadboard | $5 | Prototyping before final wiring |
| Jumper wires (M-M, M-F, F-F) | $8 | Connecting everything |
| Heat shrink tubing | $5 | Insulating soldered connections |
| Electrical tape | $3 | Quick insulation |
| **Total tools** | **~$75** | |
