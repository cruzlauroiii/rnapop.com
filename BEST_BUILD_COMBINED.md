# Optimal Combined RNA Synthesis Lab — Best & Cheapest Build

---

## Controller Comparison: ESP32 Wins

| Feature | ESP32-S3 | Arduino Nano | RPi Pico W | RPi 4/5 |
|---------|----------|-------------|------------|---------|
| **Price** | **$3–$5** | $5–$25 | $6 | $35–$80 |
| CPU | 240MHz dual-core | 16MHz | 133MHz dual-core | 1.5–2.4GHz quad |
| WiFi | Yes | No | Yes | Yes |
| Bluetooth | BLE 5.0 | No | BLE | Yes |
| ADC (for sensors) | 12-bit, 20 channels | 10-bit, 8 channels | 12-bit, 3 channels | Via HAT |
| PWM (motor control) | 16 channels | 6 channels | 16 channels | Via HAT |
| GPIO | 45 | 22 | 26 | 40 |
| PID control | Excellent | Good | Good | Overkill |
| Stepper control | Yes (with driver) | Yes (with driver) | Yes | Overkill |
| Web dashboard | Built-in (WiFi) | Needs shield ($10+) | Built-in | Built-in |
| Power consumption | 80mA active | 19mA | 50mA | 600mA–3A |
| **Best for** | **All lab equipment** | Legacy projects | Alternative to ESP32 | Camera/imaging only |

### Verdict

**ESP32-S3 ($3–$5) is the best choice for all lab equipment.** It has WiFi for a web dashboard, enough ADC/PWM for sensors and motors, and costs less than everything else. Use **one RPi Zero 2W ($15)** as the central hub + gel imager camera.

---

## The Optimal Build: 1x ESP32 Hub + 1x RPi Zero

Instead of buying separate controllers for each device, use **one ESP32-S3** to control ALL equipment through a multiplexed design, plus **one RPi Zero 2W** for the camera-based gel imager and central web dashboard.

```
┌──────────────────────────────────────────────┐
│           RPi Zero 2W — Central Hub          │
│  • Web dashboard (Flask/FastAPI)             │
│  • Gel imager (Pi Camera)                    │
│  • Data logging & export                     │
│  • WiFi link to ESP32                        │
│                    $15                        │
└──────────────┬───────────────────────────────┘
               │ WiFi / HTTP
┌──────────────▼───────────────────────────────┐
│         ESP32-S3 — Equipment Controller      │
│                                              │
│  GPIO 1-4  → Stepper drivers (syringe pumps) │
│  GPIO 5    → Relay (heat block)              │
│  GPIO 6    → DS18B20 (temp sensor)           │
│  GPIO 7-8  → UV LED + photodiode (spec)      │
│  GPIO 9    → Brushless ESC (centrifuge)      │
│  GPIO 10   → Gel box relay (power supply)    │
│  ADC       → Photodiode reading (A260/A280)  │
│  WiFi      → Sends data to RPi dashboard     │
│                    $4                         │
└──────────────────────────────────────────────┘
```

---

## Complete Parts List — Optimized Build

### Controllers ($19 total)

| Part | Qty | Source | Price |
|------|-----|--------|-------|
| ESP32-S3 DevKit | 1 | AliExpress | $4 |
| Raspberry Pi Zero 2W | 1 | raspberrypi.com / Adafruit | $15 |

### Heat Block — 37°C IVT Incubator ($25)

| Part | Qty | Price |
|------|-----|-------|
| DS18B20 waterproof temp sensor | 1 | $2 |
| 5V relay module | 1 | $2 |
| 50W PTC heater element | 1 | $5 |
| Aluminum block (drilled for 1.5mL tubes) | 1 | $8 |
| 12V 5A power supply | 1 | $6 |
| Styrofoam insulation box | 1 | $2 |

### Syringe Pumps x2 — LNP Mixing ($55)

| Part | Qty | Price |
|------|-----|-------|
| NEMA 17 stepper motor | 2 | $16 |
| A4988 stepper driver | 2 | $3 |
| M8 threaded rod + nut | 2 | $6 |
| 3D-printed frame + syringe holders | 2 | $6 |
| 608ZZ bearings | 4 | $3 |
| 10mL + 3mL syringes | 4 | $3 |
| T-junction mixer (PEEK or 3D-printed) | 1 | $5 |
| Tubing (1/16" ID silicone) | 1m | $3 |
| Limit switches | 2 | $2 |
| CNC Shield V3 (or direct ESP32 wiring) | 1 | $5 |
| Collection vials | 10 | $3 |

### UV Spectrophotometer — RNA QC ($45)

| Part | Qty | Price |
|------|-----|-------|
| 260nm UV LED (TO-39) | 1 | $15 |
| 280nm UV LED | 1 | $12 |
| UV photodiode (SG01D or GUVA-S12SD) | 1 | $8 |
| Quartz micro-cuvette (10mm path) | 1 | $5 |
| 3D-printed cuvette holder (light-tight) | 1 | $2 |
| Op-amp (LM358 or AD620) | 1 | $1 |
| Resistors, caps, PCB | 1 | $2 |

### Gel Electrophoresis + Imager ($65)

| Part | Qty | Price |
|------|-----|-------|
| Platinum wire (0.5mm, 30cm) for electrodes | 1 | $15 |
| 3D-printed gel box + comb | 1 | $4 |
| Adjustable DC-DC boost converter (to 120V) | 1 | $8 |
| Blue LED strip (470nm, transilluminator) | 1 | $5 |
| Orange filter film | 1 | $3 |
| RPi Camera Module v2 | 1 | $15 |
| 3D-printed dark box for imaging | 1 | $5 |
| Agarose (50g) | 1 | $10 |

### Centrifuge ($30)

| Part | Qty | Price |
|------|-----|-------|
| Brushless DC motor (2212 or similar) | 1 | $8 |
| ESC (30A) | 1 | $6 |
| 3D-printed rotor (8x 1.5mL tube) | 1 | $4 |
| 3D-printed housing + lid (safety) | 1 | $4 |
| IR speed sensor | 1 | $2 |
| 12V power supply (shared with heat block) | 0 | $0 |
| Vibration dampening feet | 4 | $3 |
| Safety interlock switch | 1 | $3 |

### Shared / Misc ($15)

| Part | Qty | Price |
|------|-----|-------|
| Breadboard + jumper wires | 1 | $5 |
| USB cables | 2 | $4 |
| MicroSD card (for RPi) | 1 | $6 |

### 3D Printer (if needed) ($170)

| Part | Qty | Price |
|------|-----|-------|
| Creality Ender 3 V3 SE (or clone) | 1 | $160 |
| PLA filament 1kg | 1 | $10 |

---

## Hardware Total

| Category | Cost |
|----------|------|
| Controllers (ESP32 + RPi Zero) | $19 |
| Heat block | $25 |
| Syringe pumps x2 | $55 |
| Spectrophotometer | $45 |
| Gel electrophoresis + imager | $65 |
| Centrifuge | $30 |
| Shared/misc | $15 |
| **Hardware subtotal** | **$254** |
| 3D printer (if needed) | +$170 |
| **Hardware total (with printer)** | **$424** |

---

## Reagent Cost (Cannot DIY — Must Buy)

### Minimum Viable: OSK mRNA Synthesis + LNP

| Reagent | Source | Price |
|---------|--------|-------|
| pCXLE-OSK plasmid (Oct4+Klf4+Sox2) | Addgene (#193298) | $75 |
| HiScribe T7 High Yield Kit (50 rxns) | NEB (E2040) | $285 |
| CleanCap Reagent AG | TriLink / NEB | $250 |
| N1-methylpseudouridine-5'-TP (100µmol) | Jena Bioscience | $180 |
| Poly(A) Polymerase kit | NEB | $75 |
| DNase I | NEB | $30 |
| RNA cleanup kit (Monarch) | NEB | $65 |
| RNase-free water + tubes | Any supplier | $25 |
| **IVT Subtotal** | | **$985** |

| LNP Reagent | Source | Price |
|-------------|--------|-------|
| Ionizable lipid (SM-102 or DLin-MC3, 100mg) | Avanti Polar Lipids / BroadPharm | $200–$400 |
| DSPC (100mg) | Avanti | $50 |
| Cholesterol (1g) | Sigma-Aldrich | $25 |
| DMG-PEG2000 (100mg) | Avanti / BroadPharm | $100 |
| Ethanol (molecular grade) | Fisher | $20 |
| PBS buffer | Fisher | $15 |
| Dialysis cassettes (for buffer exchange) | Thermo | $50 |
| 0.22µm syringe filters | Any | $15 |
| **LNP Subtotal** | | **$475–$675** |

| QC Reagent | Source | Price |
|------------|--------|-------|
| Agarose (50g) | Fisher/Sigma | $10 |
| TAE buffer | Fisher | $10 |
| GelRed stain | Biotium | $30 |
| RNA ladder | NEB | $25 |
| **QC Subtotal** | | **$75** |

### Reagent Total

| | Min | Max |
|-|-----|-----|
| IVT (mRNA synthesis) | $985 | $985 |
| LNP (delivery vehicle) | $475 | $675 |
| QC (gel, stains) | $75 | $75 |
| **Reagent Total** | **$1,535** | **$1,735** |

---

## Grand Total — Everything Combined

| Component | Min | Max |
|-----------|-----|-----|
| Hardware (ESP32 + RPi build) | $254 | $424 (with 3D printer) |
| Reagents (IVT + LNP + QC) | $1,535 | $1,735 |
| **GRAND TOTAL** | **$1,789** | **$2,159** |

> Compare to commercial lab setup: **$15,000–$135,000**
> **Savings: 85–98%**

---

## What You Get For ~$2,000

- 3x OSK mRNA constructs (capped, modified, poly-A tailed)
- ~50 reactions worth of IVT reagent (each producing ~100–180µg mRNA)
- LNP-encapsulated mRNA ready for... research
- Full QC pipeline (UV spec for concentration/purity, gel for size/integrity)
- WiFi-connected lab dashboard on your phone
- Enough for dozens of experiments

---

## Software Stack (Free)

| Layer | Tool | Notes |
|-------|------|-------|
| ESP32 firmware | Arduino IDE + ESP-IDF | PID temp control, stepper control, ADC reading |
| RPi dashboard | Python Flask + Chart.js | Real-time temp, pump status, spectrometer readings |
| RPi gel imaging | OpenCV + Pi Camera | Auto-detect bands, measure RNA integrity |
| Protocol automation | Python scripts | Programmable IVT and LNP mixing sequences |
| Data export | CSV / JSON | Log all runs |

---

## Recommended Build Order

1. **ESP32 + heat block** — Get IVT working first (this is the core)
2. **Syringe pumps** — For LNP mixing (most mechanically complex)
3. **Spectrophotometer** — QC your mRNA yield
4. **Gel system + RPi camera** — Verify mRNA size & integrity
5. **Centrifuge** — For purification steps
6. **Dashboard** — Connect everything via WiFi

---

## Where To Buy — Quick Reference

| Store | What | Link |
|-------|------|------|
| AliExpress | ESP32-S3, NEMA17, A4988, sensors, motors | aliexpress.com |
| Amazon | Same + faster shipping, 3D printer | amazon.com |
| Adafruit | RPi Zero 2W, quality breakout boards | adafruit.com |
| SparkFun | Sensors, dev boards | sparkfun.com |
| Mouser/Digi-Key | UV LEDs, photodiodes, precision parts | mouser.com / digikey.com |
| NEB | HiScribe IVT kit, enzymes, RNA ladder | neb.com |
| Addgene | OSK plasmids | addgene.org |
| Jena Bioscience | Modified nucleotides (m1Ψ-UTP) | jenabioscience.com |
| Avanti Polar Lipids | LNP lipids (SM-102, DSPC, PEG-lipid) | avantilipids.com |
| Fisher Scientific | Buffers, consumables, ethanol | fishersci.com |
| Sigma-Aldrich | Cholesterol, agarose, general reagents | sigmaaldrich.com |
| TriLink BioTech | CleanCap reagent, custom mRNA | trilinkbiotech.com |
| BroadPharm | Ionizable lipids, PEG-lipids | broadpharm.com |

---

## Sources

- [ESP32 vs Arduino vs RPi Comparison — DigiKey](https://www.digikey.com/en/maker/projects/comparing-microcontrollers-what-brain-should-i-go-with/02d2dcb1a0d441f5a11fc9956559b226)
- [ESP32 PID Controller — Instructables](https://www.instructables.com/PID-Controlled-Thermostat-Using-ESP32-Applied-to-a/)
- [ESP32-S3 Buying Guide 2026 — espboards.dev](https://www.espboards.dev/blog/esp32-soc-options/)
- [Arduino Multichannel Syringe Pump — ACS](https://pubs.acs.org/doi/10.1021/acs.jchemed.4c00033)
- [Poseidon Open Source Syringe Pump](https://github.com/pachterlab/poseidon)
- [Open Source UV-Vis Spectrophotometer](https://www.sciencedirect.com/science/article/abs/pii/S0924424721001618)
- [RPi Gel Imager — Make Magazine](https://makezine.com/projects/document-your-dna-with-a-raspberry-pi-gel-imager/)
- [OpenFuge Centrifuge](https://hackaday.com/2013/09/23/openfuge-an-open-source-centrifuge/)
- [HiScribe T7 RNA Kit — NEB](https://www.neb.com/en-us/products/e2040-hiscribe-t7-high-yield-rna-synthesis-kit)
- [Jena Bioscience Modified Nucleotides](https://www.jenabioscience.com/rna-technologies/rna-synthesis/kits-for-nucleoside-base-modified-mrna-synthesis)
- [Avanti Polar Lipids](https://avantilipids.com/)
- [Addgene OSK Plasmid #193298](https://www.addgene.org/193298/)
