# Fountain of Youth RNA — .NET 11 Native Blazor WASM

## Comprehensive Implementation Plan

---

## 1. Project Overview

A Blazor WebAssembly application (.NET 11, Native AOT) that accepts a user's blood/DNA biomarker inputs and outputs a **PASS/FAIL** rejuvenation eligibility score. The system evaluates biological age vs chronological age across multiple RNA and molecular aging axes, determines senescence burden, and assesses readiness for mRNA-based cellular reprogramming (Yamanaka factor therapy).

**Output:** Binary PASS (candidate for rejuvenation) or FAIL (not yet eligible — with actionable breakdown).

---

## 2. Scientific Foundation

### 2.1 The Six Pillars of RNA-Based Age Assessment

| # | Pillar | Key Molecules | What It Measures |
|---|--------|--------------|------------------|
| 1 | **PhenoAge Clock** | 9 blood biomarkers (albumin, ALP, creatinine, CRP, glucose, WBC, lymphocyte %, MCV, RDW) + chronological age | Mortality-calibrated biological age |
| 2 | **piRNA Longevity Panel** | 6 piRNAs (Duke 2026 study) | 2-year survival prediction (86% accuracy); lower levels = longer life |
| 3 | **miRNA Aging Signature** | miR-34a (senescence), miR-21 (inflammaging), miR-155 (metabolic), miR-15b-5p, miR-373-5p | Vascular aging, brain aging, inflammation burden |
| 4 | **Senescence Burden (SASP)** | p16^INK4a expression in T-cells, GDF15, IL-6, TNF-α, SERPINE1 (PAI-1), activin A, FGF21, TIMP-1 | Senescent cell load and secretory toxicity |
| 5 | **Longevity Pathways** | NAD+/NADH ratio, SIRT1/SIRT6 expression, AMPK activity, mTOR signaling markers | Cellular energy & repair pathway health |
| 6 | **Telomere & Telomerase** | Leukocyte telomere length (LTL), TERT expression, TERC levels | Replicative aging capacity |

### 2.2 Rejuvenation Readiness (Yamanaka Factor Eligibility)

Based on partial reprogramming research (OSK/OSKM mRNA delivery):

- **OSK** (Oct4, Sox2, Klf4) — safer, no c-Myc oncogene
- **OSKM** — more potent but higher cancer risk
- **SB000** — novel single-factor with comparable potency to OSKM
- Eligibility requires: low senescence burden, adequate NAD+, functional immune system, no active malignancy markers

### 2.3 cfRNA Liquid Biopsy Integration

Cell-free RNA (cfRNA) from blood plasma provides non-invasive tissue-specific transcriptome snapshots, enabling real-time multi-organ aging assessment without biopsies.

---

## 3. Architecture

```
┌─────────────────────────────────────────────────────┐
│              Blazor WASM (.NET 11 AOT)              │
│                                                     │
│  ┌──────────┐  ┌──────────┐  ┌───────────────────┐ │
│  │  Input    │  │ Scoring  │  │  Result Display   │ │
│  │  Forms    │→ │ Engine   │→ │  PASS / FAIL      │ │
│  │ (6 tabs) │  │ (C#)     │  │  + Breakdown      │ │
│  └──────────┘  └──────────┘  └───────────────────┘ │
│                     │                               │
│        ┌────────────┼────────────┐                  │
│        ▼            ▼            ▼                  │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐            │
│  │PhenoAge  │ │piRNA     │ │Senescence│            │
│  │Calculator│ │Scorer    │ │Evaluator │            │
│  └──────────┘ └──────────┘ └──────────┘            │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐            │
│  │miRNA     │ │Pathway   │ │Telomere  │            │
│  │Analyzer  │ │Analyzer  │ │Assessor  │            │
│  └──────────┘ └──────────┘ └──────────┘            │
│                     │                               │
│                     ▼                               │
│            ┌────────────────┐                       │
│            │ Rejuvenation   │                       │
│            │ Eligibility    │                       │
│            │ Composite Score│                       │
│            └────────────────┘                       │
└─────────────────────────────────────────────────────┘
         100% client-side, no server needed
```

### 3.1 Project Structure

```
FountainOfYouth/
├── FountainOfYouth.sln
├── src/
│   ├── FountainOfYouth.Wasm/           # Blazor WASM host
│   │   ├── Program.cs
│   │   ├── App.razor
│   │   ├── Pages/
│   │   │   ├── Home.razor              # Landing + quick summary
│   │   │   ├── InputWizard.razor       # Multi-step input form
│   │   │   └── Results.razor           # PASS/FAIL verdict
│   │   ├── Components/
│   │   │   ├── BloodPanel.razor        # PhenoAge 9 biomarkers
│   │   │   ├── RnaPanel.razor          # piRNA + miRNA inputs
│   │   │   ├── SenescencePanel.razor   # SASP markers
│   │   │   ├── PathwayPanel.razor      # NAD+, SIRT, AMPK, mTOR
│   │   │   ├── TelomerePanel.razor     # LTL, TERT, TERC
│   │   │   ├── DnaPanel.razor          # Methylation / genetic
│   │   │   ├── ScoreGauge.razor        # Visual gauge component
│   │   │   └── VerdictCard.razor       # PASS/FAIL display
│   │   └── wwwroot/
│   │       ├── index.html
│   │       └── css/
│   ├── FountainOfYouth.Core/           # Scoring engine (pure C#)
│   │   ├── Models/
│   │   │   ├── BiomarkerInput.cs       # All input data models
│   │   │   ├── ScoringResult.cs        # Per-pillar + composite
│   │   │   └── Verdict.cs              # PASS/FAIL enum + reasons
│   │   ├── Scoring/
│   │   │   ├── PhenoAgeCalculator.cs   # Levine PhenoAge algorithm
│   │   │   ├── PiRnaScorer.cs          # 6-piRNA longevity model
│   │   │   ├── MiRnaAnalyzer.cs        # miRNA aging signature
│   │   │   ├── SenescenceEvaluator.cs  # SASP burden scoring
│   │   │   ├── PathwayAnalyzer.cs      # NAD+/SIRT/AMPK/mTOR
│   │   │   ├── TelomereAssessor.cs     # Telomere length scoring
│   │   │   └── CompositeScorer.cs      # Weighted aggregate → PASS/FAIL
│   │   ├── Reference/
│   │   │   ├── ReferenceRanges.cs      # Age/sex-stratified normal ranges
│   │   │   └── ThresholdConfig.cs      # PASS/FAIL cutoff config
│   │   └── Validation/
│   │       └── InputValidator.cs       # Range & consistency checks
│   └── FountainOfYouth.Tests/          # xUnit tests
│       ├── PhenoAgeTests.cs
│       ├── CompositeScoreTests.cs
│       └── VerdictTests.cs
└── docs/
    └── biomarker-guide.md              # User guide for lab values
```

---

## 4. Input Specification

### 4.1 Demographics (Required)

| Field | Type | Example |
|-------|------|---------|
| Chronological Age | int | 55 |
| Sex | enum | Male/Female |
| Smoking Pack-Years | float | 10.5 |

### 4.2 PhenoAge Blood Panel (Required — 9 Biomarkers)

| Biomarker | Unit | Typical Range |
|-----------|------|---------------|
| Albumin | g/dL | 3.5–5.5 |
| Alkaline Phosphatase | U/L | 44–147 |
| Creatinine | mg/dL | 0.7–1.3 |
| C-Reactive Protein (CRP) | mg/dL | 0.0–1.0 |
| Fasting Glucose | mg/dL | 70–100 |
| White Blood Cell Count | 10³/µL | 4.5–11.0 |
| Lymphocyte % | % | 20–40 |
| Mean Cell Volume (MCV) | fL | 80–100 |
| Red Cell Distribution Width (RDW) | % | 11.5–14.5 |

### 4.3 piRNA Panel (Optional — 6 Values)

| Marker | Unit | Note |
|--------|------|------|
| piRNA-1 through piRNA-6 | Relative expression (RPM) | Lower = better longevity signal |

### 4.4 miRNA Aging Panel (Optional)

| Marker | Role |
|--------|------|
| miR-34a | Senescence / brain aging |
| miR-21 | Inflammaging |
| miR-155 | Metabolic regulation |
| miR-15b-5p | Downregulated in long-lived |
| miR-373-5p | Upregulated in long-lived |

### 4.5 Senescence / SASP Panel (Optional)

| Marker | Unit |
|--------|------|
| p16^INK4a (T-cell expression) | Relative expression |
| GDF15 | pg/mL |
| IL-6 | pg/mL |
| TNF-α | pg/mL |
| SERPINE1 (PAI-1) | ng/mL |
| Activin A | pg/mL |
| FGF21 | pg/mL |
| TIMP-1 | ng/mL |

### 4.6 Longevity Pathway Markers (Optional)

| Marker | Unit |
|--------|------|
| NAD+ level | µM |
| NAD+/NADH ratio | ratio |
| SIRT1 expression | Relative |
| SIRT6 expression | Relative |
| AMPK activity | Relative |

### 4.7 Telomere Panel (Optional)

| Marker | Unit |
|--------|------|
| Leukocyte Telomere Length (LTL) | kb |
| TERT expression | Relative |
| TERC level | Relative |

---

## 5. Scoring Engine

### 5.1 PhenoAge Calculation (Levine Algorithm)

```
Mortality Score = b0 + b1*Albumin + b2*Creatinine + b3*Glucose
                + b4*log(CRP) + b5*Lymphocyte% + b6*MCV
                + b7*RDW + b8*ALP + b9*WBC + b10*Age

PhenoAge = 141.50225 + ln(-0.00553 × ln(1 − exp(MortalityScore × (exp(120×0.0076927)−1) / 0.0076927))) / 0.090165
```

- Coefficients from Levine et al. 2018 (NHANES III calibration)
- Output: Biological age in years

### 5.2 piRNA Longevity Score

```
piRNA_Score = Σ(w_i × normalize(piRNA_i))  for i=1..6
// Lower composite → PASS direction
// Threshold: score < age-stratified median → favorable
```

### 5.3 miRNA Composite

```
Aging_miRNA = w1×miR34a + w2×miR21 + w3×miR155
Longevity_miRNA = w4×(1/miR15b) + w5×miR373
miRNA_Score = Aging_miRNA − Longevity_miRNA
// Lower → younger biological profile
```

### 5.4 Senescence Burden Index

```
SASP_Index = Σ(normalize_to_reference(marker_i) × weight_i)
// Markers: p16, GDF15, IL-6, TNF-α, SERPINE1, Activin A, FGF21, TIMP-1
// Higher = worse (more senescent cells)
```

### 5.5 Pathway Health Score

```
Pathway = (NAD_ratio × 0.3) + (SIRT1 × 0.2) + (SIRT6 × 0.2)
        + (AMPK × 0.2) − (mTOR_overactivation × 0.1)
// Higher = healthier longevity pathway state
```

### 5.6 Telomere Score

```
Telomere = normalize_by_age_sex(LTL) × 0.6
         + normalize(TERT) × 0.2
         + normalize(TERC) × 0.2
// Higher = better replicative capacity
```

### 5.7 Composite Score & Verdict

```csharp
// Weights (configurable)
const double W_PHENO    = 0.30;  // PhenoAge delta
const double W_PIRNA    = 0.15;  // piRNA longevity
const double W_MIRNA    = 0.15;  // miRNA aging
const double W_SASP     = 0.15;  // Senescence burden
const double W_PATHWAY  = 0.15;  // NAD+/SIRT/AMPK/mTOR
const double W_TELOMERE = 0.10;  // Telomere health

// Each pillar normalized to 0–100 (100 = youthful)
double composite = W_PHENO * phenoScore
                 + W_PIRNA * pirnaScore
                 + W_MIRNA * mirnaScore
                 + W_SASP * saspScore
                 + W_PATHWAY * pathwayScore
                 + W_TELOMERE * telomereScore;

// PASS/FAIL
Verdict = composite >= 60.0 ? PASS : FAIL;

// Sub-verdicts for each pillar also displayed
```

**PASS Criteria:**
- Composite ≥ 60/100 AND
- No single pillar below 30/100 (hard floor) AND
- Senescence burden not in critical zone (SASP < 80th percentile for age)

**FAIL + Actionable Output:**
- Which pillars failed and by how much
- Which specific biomarkers are out of range
- Suggested intervention categories (senolytic, NAD+ boosting, lifestyle, etc.)

---

## 6. .NET 11 Native AOT + Blazor WASM Implementation

### 6.1 Project Setup

```bash
dotnet new blazorwasm -n FountainOfYouth.Wasm --framework net11.0
dotnet new classlib -n FountainOfYouth.Core --framework net11.0
dotnet new xunit -n FountainOfYouth.Tests --framework net11.0
```

**csproj (Wasm):**
```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <RunAOTCompilation>true</RunAOTCompilation>
    <WasmStripILAfterAOT>true</WasmStripILAfterAOT>
  </PropertyGroup>
  <ProjectReference Include="..\FountainOfYouth.Core\FountainOfYouth.Core.csproj" />
</Project>
```

### 6.2 Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Runtime | Blazor WASM + AOT | 100% client-side, no PHI leaves browser |
| State | In-memory only | No persistence of health data |
| UI Framework | Built-in Blazor components | No JS dependencies |
| Scoring | Pure C# in Core lib | Testable, no WASM dependency |
| Optional panels | Graceful degradation | Score with available data, weight redistribution |

### 6.3 Adaptive Scoring (Missing Data)

When optional panels are omitted, their weight is redistributed proportionally:

```csharp
double totalWeight = panels.Where(p => p.HasData).Sum(p => p.Weight);
foreach (var panel in panels.Where(p => p.HasData))
    panel.EffectiveWeight = panel.Weight / totalWeight;
```

Minimum required: Demographics + PhenoAge blood panel (9 markers).

---

## 7. UI Flow

```
[Landing] → [Step 1: Demographics]
          → [Step 2: Blood Panel (required)]
          → [Step 3: RNA Panels (optional)]
          → [Step 4: SASP Markers (optional)]
          → [Step 5: Pathway & Telomere (optional)]
          → [Review & Calculate]
          → [RESULT: PASS ✓ or FAIL ✗]
             ├── Composite Score (0–100)
             ├── Per-Pillar Breakdown (6 gauges)
             ├── Biological Age vs Chronological Age
             ├── Rejuvenation Eligibility Assessment
             └── Actionable Recommendations (if FAIL)
```

---

## 8. Rejuvenation Protocol Assessment

If **PASS**, the system additionally evaluates eligibility for three reprogramming approaches:

| Protocol | Requirements | Risk Level |
|----------|-------------|------------|
| **OSK mRNA** (Oct4/Sox2/Klf4) | Senescence < 50th %ile, NAD+ adequate, no malignancy | Low |
| **OSKM mRNA** (+ c-Myc) | All OSK + exceptional pathway health + immune function | Medium |
| **SB000 Single-Factor** | Minimal requirements, broadest eligibility | Lowest |

Output per protocol: **Eligible / Not Eligible / Borderline** with specific blockers listed.

---

## 9. Implementation Phases

### Phase 1: Core Engine (Week 1–2)
- [ ] Solution scaffold with .NET 11
- [ ] `BiomarkerInput` / `ScoringResult` / `Verdict` models
- [ ] `PhenoAgeCalculator` with Levine coefficients
- [ ] `CompositeScorer` with weight redistribution
- [ ] `InputValidator` with reference ranges
- [ ] xUnit tests for PhenoAge against published examples

### Phase 2: All Scorers (Week 3–4)
- [ ] `PiRnaScorer` — 6-piRNA model
- [ ] `MiRnaAnalyzer` — 5-miRNA composite
- [ ] `SenescenceEvaluator` — SASP index
- [ ] `PathwayAnalyzer` — NAD+/SIRT/AMPK
- [ ] `TelomereAssessor` — LTL normalization
- [ ] Reference ranges by age/sex

### Phase 3: Blazor WASM UI (Week 5–6)
- [ ] Multi-step wizard form
- [ ] Per-panel input components with validation
- [ ] Score gauge visualization (SVG/CSS)
- [ ] PASS/FAIL verdict card
- [ ] Per-pillar breakdown display
- [ ] Rejuvenation protocol eligibility section

### Phase 4: AOT & Polish (Week 7)
- [ ] Enable `RunAOTCompilation`
- [ ] Performance profiling (load time, calc time)
- [ ] Accessibility pass (ARIA, keyboard nav)
- [ ] Sample data presets for demo
- [ ] Biomarker reference guide page

---

## 10. Key Algorithms — Reference Coefficients

### PhenoAge (Levine 2018)

```
Biomarker         | Coefficient
------------------|------------
Albumin           | -0.0336
Creatinine        | 0.0095
Glucose           | 0.1953
log(CRP)          | 0.0954
Lymphocyte %      | -0.0120
MCV               | 0.0268
RDW               | 0.3306
ALP               | 0.0019
WBC               | 0.0554
Age               | 0.0804
Intercept (b0)    | -19.9067
```

### Gompertz Mortality Parameters
```
γ  = 0.0076927  (Gompertz slope)
λ₀ = exp(-19.907)  (baseline hazard)
Conversion constant = 141.50225
Scaling factor = 0.090165
```

---

## 11. Data Privacy

- **Zero server calls** — all computation in WASM
- **No storage** — data lives only in browser memory during session
- **No telemetry** — no analytics, no tracking
- **Export only** — user can download their results as PDF (client-side generation)

---

## 12. Sources

- [Duke piRNA Longevity Blood Test (2026)](https://corporate.dukehealth.org/news/new-blood-test-signals-who-most-likely-live-longer-study-finds)
- [mRNA Immune Rejuvenation — Nature (2025)](https://www.nature.com/articles/d41586-025-04082-5)
- [PhenoAge Algorithm — Levine 2018](https://pmc.ncbi.nlm.nih.gov/articles/PMC5940111/)
- [BioAge R Package (Reference Implementation)](https://github.com/dayoonkwon/BioAge)
- [Yamanaka OSK Lifespan Extension in Mice](https://pmc.ncbi.nlm.nih.gov/articles/PMC10909732/)
- [SB000 Single-Factor Rejuvenation](https://www.biorxiv.org/content/10.1101/2025.06.05.657370v1.full)
- [Epigenetic Clocks Computational Challenges — Nature Reviews Genetics](https://www.nature.com/articles/s41576-024-00807-w)
- [miRNA Aging Clock Pilot (2025)](https://www.fightaging.org/archives/2025/12/a-pilot-microrna-aging-clock/)
- [Plasma miRNA Signatures of Aging](https://pmc.ncbi.nlm.nih.gov/articles/PMC12188677/)
- [SASP Biomarkers — SenMayo Gene Set](https://www.nature.com/articles/s41467-022-32552-1)
- [NAD+/Sirtuins/AMPK/mTOR Network](https://www.frontiersin.org/journals/physiology/articles/10.3389/fphys.2021.724506/full)
- [cfRNA Liquid Biopsy — Nature Machine Intelligence](https://www.nature.com/articles/s42256-025-01148-x)
- [Telomere Biology — Digital Measurement](https://www.nature.com/articles/s41467-024-49007-4)
- [.NET 11 Preview 1](https://www.infoq.com/news/2026/02/dotnet-11-preview1/)
- [Blazor WASM AOT Compilation](https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-build-tools-and-aot)
