# RNA Printing Hardware (Raspberry Pi / Arduino) & Legality

---

## Part 1: DIY Biolab Hardware — Arduino & Raspberry Pi Builds

You can build nearly every piece of lab equipment needed for mRNA synthesis using open-source Arduino/Raspberry Pi projects and 3D-printed parts. Here's the full hardware stack:

---

### 1. Thermocycler / Heat Block (for IVT incubation at 37°C)

mRNA in vitro transcription requires holding a reaction at 37°C for 2-4 hours. This is the most critical piece.

| Project | Controller | Cost | Link |
|---------|-----------|------|------|
| **OpenPCR** | Arduino Uno | ~$499 (kit) or ~$85 DIY | [openpcr.org](https://openpcr.org/) |
| **NinjaPCR** | Arduino | ~$100 DIY | [github.com/hisashin/NinjaPCR](https://github.com/hisashin/NinjaPCR/) |
| **$5 DNA Replicator** | Arduino | ~$5–$20 | [hackaday.io/project/1864](https://hackaday.io/project/1864-5-dna-replicator) |
| **Arduino Incubator** | Arduino Uno + DS18B20 | ~$15–$25 | [instructables.com](https://www.instructables.com/Temperature-Controlled-Incubator-Using-Arduino/) |
| **Precision Temp Controller** | Arduino Nano + polyimide heater | ~$50 | [PMC paper](https://pmc.ncbi.nlm.nih.gov/articles/PMC10846474/) — stability <0.06°C |

**Parts list (DIY heat block):**
- Arduino Uno/Nano — $5–$25
- DS18B20 waterproof temp sensor — $3
- Relay module (5V) — $3
- Peltier element or heating resistor — $5–$15
- 12V power supply — $8
- Aluminum heat block (drilled for tubes) — $10–$20
- Styrofoam insulation box — $5
- **Total: ~$40–$80**

---

### 2. Syringe Pump (for LNP microfluidic mixing)

LNP encapsulation requires precise, controlled mixing of lipids in ethanol with mRNA in aqueous buffer. Open-source syringe pumps replace a $50K–$100K NanoAssemblr.

| Project | Controller | Channels | Cost | Link |
|---------|-----------|----------|------|------|
| **Poseidon System** | Arduino | Multi | ~$400 | [github.com/pachterlab/poseidon](https://github.com/pachterlab/poseidon) |
| **OpenSyringePump** | Arduino | 1 | ~$50–$100 | [github.com/manimino/OpenSyringePump](https://github.com/manimino/OpenSyringePump) |
| **Spring-Driven Pump** | None (passive) | 1 | ~$25–$30 | [HardwareX paper](https://www.hardware-x.com/article/S2468-0672(24)00044-0/fulltext) |
| **Multichannel Syringe Pump** | Arduino UNO + CNC shield | 4 | ~$120 | [ACS J. Chem. Ed.](https://pubs.acs.org/doi/10.1021/acs.jchemed.4c00033) |
| **Open-Source Peristaltic Pump** | Arduino | 1 | ~$120 | [Nature Scientific Reports](https://www.nature.com/articles/s41598-020-58246-6) |

**Parts list (multichannel syringe pump):**
- Arduino UNO — $10
- CNC Shield V3 — $5
- A4988 stepper motor drivers (x2–4) — $2 each
- NEMA 17 stepper motors (x2–4) — $10 each
- 3D-printed frame + syringe holders — $5–$15 (filament)
- Linear rail / threaded rod — $10
- Syringes (various sizes) — $5
- **Total: ~$80–$150**

**For LNP mixing:** Connect two syringe pumps to a Y-junction or T-junction mixer (3D-printed or PEEK tubing). Flow rate ratio typically 3:1 (aqueous:ethanol). Faster mixing = smaller nanoparticles.

---

### 3. Spectrophotometer (RNA QC — measure concentration at 260nm)

| Project | Controller | Range | Cost | Link |
|---------|-----------|-------|------|------|
| **Arduino UV-Vis Spectrophotometer** | Arduino + Bluetooth | 190–1100nm | ~$50–$100 | [ScienceDirect paper](https://www.sciencedirect.com/science/article/abs/pii/S0924424721001618) |
| **openSpectrometer** | Arduino | UV-Vis | ~$30–$80 | [github.com/ewimberley/openSpectrometer](https://github.com/ewimberley/openSpectrometer) |
| **Raspberry Pi Spectrometer** | RPi + Camera | Visible-NIR | ~$50 | [hackaday.io/project/181144](https://hackaday.io/project/181144-raspberry-pi-spectrometer) |
| **Fiber-Optic Spectrometer** | RPi | Custom range | ~$100–$200 | [HardwareX paper](https://www.hardware-x.com/article/S2468-0672(24)00024-5/fulltext) |

**Parts list (Arduino UV-Vis):**
- Arduino Nano — $5
- UV LED (260nm) — $10–$20
- Photodiode (UV-sensitive, e.g., SG01D-C18) — $15–$30
- Cuvette holder (3D-printed) — $2
- Quartz micro-cuvette — $15–$30
- Op-amp circuit (for signal amplification) — $5
- Bluetooth module HC-05 (optional) — $5
- **Total: ~$60–$120**

> A260/A280 ratio of ~2.0 indicates pure RNA.

---

### 4. Gel Electrophoresis (verify mRNA size & integrity)

| Project | Controller | Cost | Link |
|---------|-----------|------|------|
| **openPFGE** | Arduino Nano + Bluetooth | ~$850 (full PFGE) | [ResearchGate](https://www.researchgate.net/publication/343388019_openPFGE_An_open_source_and_low_cost_pulsed-field_gel_electrophoresis_equipment) |
| **DIY Mini Gel Box** | DC power supply only | ~$20–$50 | Standard biohacker build |
| **RPi Gel Imager** | Raspberry Pi + Camera | ~$50–$100 | [Make Magazine](https://makezine.com/projects/document-your-dna-with-a-raspberry-pi-gel-imager/) |
| **Arduino Transilluminator** | Arduino Nano + blue LEDs | ~$20–$30 | [electronics-lab.com](https://www.electronics-lab.com/dna-transilluminator-with-arduino-nano/) |

**Parts list (basic gel system):**
- Gel box (3D-printed or tupperware + platinum wire electrodes) — $10–$30
- DC power supply (100–150V) — $20–$40
- Agarose powder — $15
- TAE/TBE buffer — $10
- SYBR Safe / GelRed stain — $30
- Blue LED transilluminator (Arduino Nano + LEDs) — $20
- Raspberry Pi + Camera Module for imaging — $50
- **Total: ~$150–$200**

---

### 5. Centrifuge (for RNA purification)

| Project | Controller | RPM | Cost | Link |
|---------|-----------|-----|------|------|
| **OpenFuge** | Arduino | up to 9,000 RPM | ~$100–$200 | [Hackaday](https://hackaday.com/2013/09/23/openfuge-an-open-source-centrifuge/) |
| **3D-Printed Centrifuge** | Brushless motor + ESC | Variable | ~$30–$60 | Various DIYbio builds |
| **Paperfuge** (manual) | None | 125,000 RPM | ~$0.20 | Stanford research |

**Parts list (OpenFuge):**
- Arduino — $10
- Brushless DC motor + ESC — $20
- 3D-printed rotor + housing — $10
- Tube adapters — $5
- Speed sensor (IR) — $3
- **Total: ~$50–$100**

---

### 6. All-in-One: BioBlocksLab Portable Bio Lab

A complete modular portable lab with 4 modules:
- Centrifuge
- Thermocycler
- Gel electrophoresis
- Incubator

All controlled via a visual programming language. [Paper](https://www.sciencedirect.com/science/article/abs/pii/S1046202323000075)

---

### 7. Lab Automation Controller (Raspberry Pi Hub)

| Project | Purpose | Link |
|---------|---------|------|
| **PyLabRobot** | Hardware-agnostic lab automation SDK | [github.com/PyLabRobot/pylabrobot](https://github.com/PyLabRobot/pylabrobot) |
| **openBlab** | Mini-lab automation system | [github.com/builder555/openBlab](https://github.com/builder555/openBlab) |
| **RPi Microfluidics Platform** | Camera + pump + sensor integration | [PMC paper](https://pmc.ncbi.nlm.nih.gov/articles/PMC10605846/) |

---

## Complete DIY Hardware BOM Summary

| Equipment | Open-Source Build | Commercial Equivalent | Savings |
|-----------|------------------|----------------------|---------|
| Heat block / Thermocycler | $40–$100 (Arduino) | $2,000–$5,000 | 95%+ |
| Syringe pump (x2 for LNP) | $80–$150 (Arduino) | $5,000–$100,000 | 98%+ |
| UV-Vis Spectrophotometer | $60–$120 (Arduino) | $3,000–$9,000 | 97%+ |
| Gel electrophoresis + imager | $150–$200 (Arduino+RPi) | $2,000–$5,000 | 92%+ |
| Centrifuge | $50–$100 (Arduino) | $500–$3,000 | 90%+ |
| Automation hub | $50 (RPi) | N/A | — |
| **TOTAL HARDWARE** | **$430–$670** | **$12,500–$122,000** | **95%+** |

> Add ~$2,000–$10,000 for reagents (IVT kits, plasmids, lipids, modified nucleotides) — these cannot be DIY'd.

---

## Shopping List — Where to Buy Components

### Electronics

| Component | Source | Price |
|-----------|--------|-------|
| Arduino Uno / Nano | Amazon, Adafruit, SparkFun | $5–$25 |
| Raspberry Pi 4/5 | raspberrypi.com, Adafruit | $35–$80 |
| NEMA 17 stepper motors | Amazon, AliExpress | $8–$12 |
| A4988 stepper drivers | Amazon, AliExpress | $1–$3 |
| CNC Shield V3 | Amazon | $5 |
| DS18B20 temp sensors | Amazon, SparkFun | $3–$5 |
| Relay modules | Amazon | $2–$5 |
| UV LED 260nm | Mouser, Digi-Key, LED Engin | $10–$30 |
| UV photodiode | Mouser, Digi-Key | $15–$30 |
| Peltier elements | Amazon, AliExpress | $5–$15 |
| Brushless DC motor + ESC | Amazon, HobbyKing | $15–$25 |
| Camera Module (RPi) | raspberrypi.com | $15–$25 |

### 3D Printing

| Item | Source | Notes |
|------|--------|-------|
| PLA/PETG filament | Amazon, MatterHackers | $20/kg |
| STL files for all above | GitHub repos linked above | Free |
| 3D printer (if needed) | Creality Ender 3 — Amazon | ~$200 |

### Reagents (Cannot DIY)

| Reagent | Source | Est. Cost |
|---------|--------|-----------|
| OSK plasmids | Addgene | $75 each |
| HiScribe T7 mRNA Kit + CleanCap | NEB | ~$600 |
| Modified nucleotides (m1Ψ-UTP) | TriLink, Jena Bioscience | $200–$500 |
| LNP lipids (SM-102, DSPC, cholesterol, PEG-lipid) | CordenPharma, Avanti Polar Lipids | $500–$2,000 |
| Agarose, buffers, stains | Fisher Scientific, Sigma-Aldrich | $50–$100 |
| RNase-free consumables | Any mol bio supplier | $50–$100 |

---

## Part 2: Is This Illegal?

### Short Answer

**Synthesizing mRNA in your own lab: LEGAL (for research/personal use).**
**Selling it or administering it to others: ILLEGAL without FDA approval.**
**Self-administration: Legal gray area.**

### Detailed Breakdown

| Activity | Legal Status (US) | Details |
|----------|------------------|---------|
| Buying lab equipment (Arduino, RPi, etc.) | **Legal** | No restrictions |
| Buying IVT reagent kits | **Legal** | Sold as "Research Use Only" |
| Buying plasmids from Addgene | **Legal** | May require institutional affiliation |
| Synthesizing mRNA in your home lab | **Legal** | No law prohibits personal synthesis |
| Making LNPs | **Legal** | Standard chemistry |
| Self-injecting your own mRNA-LNP | **Gray area** | Self-experimentation has legal precedent; not explicitly prohibited by federal law |
| Selling mRNA therapies to others | **ILLEGAL** | FDA considers this an unapproved drug/biologic |
| Selling DIY gene therapy kits | **ILLEGAL** | FDA explicitly warned against this (2017) |
| Selling kits for human self-administration | **ILLEGAL in California** | CA law (2019) requires warning labels; kits for self-admin banned |
| Administering to others without IND | **ILLEGAL** | Requires FDA Investigational New Drug application |

### Key Legal References

1. **FDA Statement (Nov 2017):** "The sale of [DIY gene therapy] products is against the law. FDA is concerned about the safety risks involved." — [Nature Biotechnology](https://www.nature.com/articles/nbt0218-119)

2. **California SB-180 (2019):** First US law directly regulating CRISPR/gene therapy kits. Bans sale of kits designed for human self-administration without warnings. — [MIT Technology Review](https://www.technologyreview.com/2019/08/09/65433/dont-change-your-dna-at-home-says-americas-first-crispr-law/)

3. **FDA Position:** Any gene therapy product intended for use in humans is a "drug" or "biologic" under the Federal Food, Drug, and Cosmetic Act. — [PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC7004414/)

4. **Self-experimentation precedent:** Self-experimentation itself is not against federal law. Historically, scientists have self-experimented (Barry Marshall drank H. pylori, won Nobel Prize). The FDA regulates *products in commerce*, not personal use.

5. **FDA 2026 Update:** New framework for individualized gene therapies for ultra-rare diseases, potentially creating pathways for personalized RNA therapies under medical supervision. — [HHS.gov](https://www.hhs.gov/press-room/fda-launches-framework-accelerating-development-individualized-therapies-ultra-rare-diseases.html)

### International

| Country | Status |
|---------|--------|
| **US** | Self-experiment = gray area. Sell = illegal. |
| **EU** | Stricter. Gene therapy products regulated under ATMP (Advanced Therapy Medicinal Products). |
| **UK** | Similar to EU. Human Medicines Regulations 2012. |
| **Honduras** | Some biohackers have gone there for unregulated trials — [NAD.com report](https://www.nad.com/news/biohackers-convene-in-honduras-for-unregulated-gene-therapy-trials-without-fda-oversight) |

### Bottom Line on Legality

> **Building the lab: 100% legal.**
> **Making the mRNA: Legal for research.**
> **Injecting yourself: Not explicitly illegal federally, but no safety net — you're on your own.**
> **Selling or giving to others: Illegal without FDA approval.**

---

## Sources

- [OpenPCR Thermocycler](https://openpcr.org/)
- [NinjaPCR — GitHub](https://github.com/hisashin/NinjaPCR/)
- [Arduino Incubator — Instructables](https://www.instructables.com/Temperature-Controlled-Incubator-Using-Arduino/)
- [Precision Temp Control Protocol — PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC10846474/)
- [Poseidon Syringe Pump — GitHub](https://github.com/pachterlab/poseidon)
- [OpenSyringePump — GitHub](https://github.com/manimino/OpenSyringePump)
- [Arduino Multichannel Syringe Pump — ACS](https://pubs.acs.org/doi/10.1021/acs.jchemed.4c00033)
- [Arduino UV-Vis Spectrophotometer — ScienceDirect](https://www.sciencedirect.com/science/article/abs/pii/S0924424721001618)
- [openSpectrometer — GitHub](https://github.com/ewimberley/openSpectrometer)
- [RPi Gel Imager — Make Magazine](https://makezine.com/projects/document-your-dna-with-a-raspberry-pi-gel-imager/)
- [Arduino Transilluminator](https://www.electronics-lab.com/dna-transilluminator-with-arduino-nano/)
- [OpenFuge Centrifuge — Hackaday](https://hackaday.com/2013/09/23/openfuge-an-open-source-centrifuge/)
- [BioBlocksLab Portable Bio Lab](https://www.sciencedirect.com/science/article/abs/pii/S1046202323000075)
- [PyLabRobot — GitHub](https://github.com/PyLabRobot/pylabrobot)
- [RPi Microfluidics — PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC10605846/)
- [Open-Source DIY Microfluidics — ScienceDirect](https://www.sciencedirect.com/science/article/abs/pii/S0925400521011928)
- [Open Source DNA/RNA Lab Hardware — PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC4598940/)
- [3D-Printed Inkjet DNA Synthesizer — Nature](https://www.nature.com/articles/s41598-024-53944-x)
- [FDA Warning on DIY Gene Therapy — Nature Biotech](https://www.nature.com/articles/nbt0218-119)
- [Regulating Genetic Biohacking — PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC7004414/)
- [California CRISPR Law — MIT Tech Review](https://www.technologyreview.com/2019/08/09/65433/dont-change-your-dna-at-home-says-americas-first-crispr-law/)
- [FDA 2026 Individualized Therapies Framework — HHS.gov](https://www.hhs.gov/press-room/fda-launches-framework-accelerating-development-individualized-therapies-ultra-rare-diseases.html)
- [Biohackers in Honduras — NAD.com](https://www.nad.com/news/biohackers-convene-in-honduras-for-unregulated-gene-therapy-trials-without-fda-oversight)
