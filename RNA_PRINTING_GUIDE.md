# Can You Print "Passing" Rejuvenation RNA? — Complete Guide

---

## TL;DR

**Yes, it is technically possible to synthesize the mRNA for Yamanaka factors (OSK/OSKM) today.** The components are commercially available. However, going from "synthesized mRNA" to "safe self-administration for rejuvenation" has massive gaps in safety, delivery, dosing, and legality. Below is everything you need to know.

---

## 1. What RNA Needs to Be "Printed"

For rejuvenation via partial reprogramming, you need mRNA encoding:

| Factor | Gene | Size (~nt) | Function |
|--------|------|-----------|----------|
| **Oct4** (POU5F1) | OCT4 | ~1,100 | Master pluripotency TF |
| **Sox2** | SOX2 | ~960 | Stem cell maintenance |
| **Klf4** | KLF4 | ~1,400 | Tumor suppressor / reprogramming |
| **c-Myc** (optional) | MYC | ~1,350 | Proliferation (oncogenic risk) |

**Safer alternatives:**
- **OSK only** (no c-Myc) — lower cancer risk
- **SB000** — single synthetic factor, comparable to OSKM potency, safest profile

Each mRNA must have:
- 5' Cap (Cap1 or CleanCap)
- 5' UTR + Kozak sequence
- Coding sequence (codon-optimized)
- 3' UTR
- Poly(A) tail (100-150nt)
- Modified nucleosides (N1-methylpseudouridine) to avoid immune activation

---

## 2. Three Paths to Get the mRNA

### Path A: Order Custom mRNA from a Synthesis Service

**Easiest. No equipment needed. Research-use only.**

| Vendor | Product | Scale | Est. Cost | Link |
|--------|---------|-------|-----------|------|
| **TriLink BioTechnologies** | CleanCap custom mRNA | mg to multi-gram | $500–$5,000+ per construct | [trilinkbiotech.com](https://www.trilinkbiotech.com/discovery-mrna-synthesis) |
| **OZ Biosciences** | Custom mRNA synthesis | µg to grams | Quote-based | [ozbiosciences.com](https://ozbiosciences.com/content/31-custom-mrna-synthesis-service) |
| **BOC Sciences** | GMP-grade mRNA | µg to grams | Quote-based | [rna.bocsci.com](https://rna.bocsci.com/products-services/custom-mrna-synthesis.html) |
| **Creative Biolabs** | mRNA Cell Reprogramming Service | Custom | Quote-based | [mrna.creative-biolabs.com](https://mrna.creative-biolabs.com/custom-mrna-based-cell-reprogramming-service.htm) |
| **Creative Biogene** | Custom mRNA | µg to grams | Quote-based | [creative-biogene.com](https://www.creative-biogene.com/services/custom-mrna-synthesis-service.html) |
| **BiCell Scientific** | Custom mRNA | Custom | Quote-based | [bicellscientific.com](https://bicellscientific.com/product/custom-mrna-synthesis/) |

**What you provide:** Gene sequence (codon-optimized for human), desired modifications (Cap1, m1Ψ, poly-A length).

**What you get:** Purified mRNA, QC'd, ready for formulation.

**Restrictions:** Most vendors sell for "research use only" (RUO). They may require institutional affiliation.

---

### Path B: DIY In Vitro Transcription (IVT)

**Moderate difficulty. Requires basic molecular biology lab.**

#### Step 1: Get the DNA Template

| Source | Product | Cost |
|--------|---------|------|
| **Addgene** | OSKM plasmids (e.g., #193346, #24603, #20924) | ~$75–$85/plasmid |
| **Vector Biolabs** | Ad-h-OKSIM Adenovirus (KOSM) | ~$500 |
| **Addgene** | VEE RNA replicon with OSK 2A construct | ~$75 |

Addgene has **297+ OCT4-related plasmids** and **39 OSKM combinations** available.

> **Note:** Addgene materials are typically restricted to academic/nonprofit institutions.

#### Step 2: In Vitro Transcription Kit

| Kit | Vendor | Reactions | Yield/Rxn | Price |
|-----|--------|-----------|-----------|-------|
| **HiScribe T7 mRNA Kit + CleanCap AG** | NEB (E2080) | 20 rxns | High yield + capped | ~$600 |
| **HiScribe T7 High Yield RNA Synthesis** | NEB (E2040) | 50 rxns | 180 µg/rxn | ~$300 |
| **MEGAscript T7** | Thermo Fisher (AMB13345) | 25 rxns | 100 µg/rxn | ~$400 |
| **mMESSAGE mMACHINE T7** | Thermo Fisher (AM1344) | 25 rxns | Capped mRNA | ~$500 |
| **TranscriptAid T7 High Yield** | Thermo (K0441) | 50 rxns | 10x higher than standard | ~$250 |

#### Step 3: Additional Reagents Needed

| Reagent | Purpose | Source |
|---------|---------|--------|
| N1-methylpseudouridine-5'-triphosphate | Modified nucleoside (immune evasion) | TriLink, Jena Bioscience |
| CleanCap Reagent AG | Co-transcriptional capping | TriLink / NEB |
| Poly(A) Polymerase | Add poly-A tail | NEB, Thermo |
| DNase I | Remove DNA template | NEB, Thermo |
| RNA purification columns/beads | Cleanup | Qiagen, Zymo Research |
| RNase-free water & consumables | Prevent degradation | Any molecular bio supplier |

#### Step 4: Equipment Needed

| Equipment | Est. Cost | Notes |
|-----------|-----------|-------|
| Thermal cycler / heat block | $200–$2,000 | For incubation |
| Microcentrifuge | $500–$3,000 | For purification |
| NanoDrop / spectrophotometer | $3,000–$9,000 | QC (concentration, purity) |
| -20°C / -80°C freezer | $1,000–$5,000 | mRNA storage |
| Gel electrophoresis system | $500–$2,000 | Size verification |
| Laminar flow hood | $2,000–$8,000 | Sterile work |
| Micropipettes + tips | $500–$1,000 | Standard |

**Total DIY IVT setup: ~$10,000–$30,000** (one-time) + ~$2,000–$5,000/batch in consumables.

---

### Path C: Benchtop RNA Synthesizer

**Highest automation. Significant investment.**

| Device | Vendor | Capability | Price |
|--------|--------|-----------|-------|
| **Kilobaser one-XT** | Kilobaser (Austria) | DNA + RNA oligo synthesis, 2.5 min/base | **$49,500** (Extended Edition) |
| **Kilobaser Basic** | Kilobaser | DNA only | **$35,500** |
| **BioXp 9600** | Telesis Bio | Full mRNA from sequence (automated IVT) | **Contact for quote** (~$100K+) |
| **BioXp Select mRNA Kit** | Telesis Bio | mRNA 0.4–10 kb, 50 µg/well | **~$1,454/kit** (via VWR) |
| **SYNTAX System** | DNA Script | Enzymatic DNA synthesis | **Contact for quote** |

> **Important:** The Kilobaser synthesizes short RNA oligos (up to ~100nt), NOT full-length mRNA (1,000+ nt). For full mRNA, you need IVT (Path B) or the BioXp system.

#### Industrial Scale: CureVac RNA Printer

CureVac's RNA Printer is a transportable, automated mRNA manufacturing facility that can produce several grams of LNP-formulated mRNA (100,000+ doses) in weeks. Built in partnership with **Tesla Automation**. Located at CureVac HQ in Tubingen, Germany. **Not commercially available to individuals.**

---

## 3. The Missing Piece: Delivery (LNP Formulation)

Naked mRNA degrades in seconds in the bloodstream. You MUST encapsulate it in **lipid nanoparticles (LNPs)**.

### LNP Components

| Component | Role | Source |
|-----------|------|--------|
| Ionizable lipid (e.g., SM-102, ALC-0315) | mRNA encapsulation + endosomal escape | CordenPharma, Avanti Polar Lipids |
| DSPC (helper lipid) | Bilayer stability | Avanti, Sigma-Aldrich |
| Cholesterol | Membrane rigidity | Sigma-Aldrich |
| PEG-lipid (DMG-PEG2000) | Stealth coating, prevents aggregation | Avanti, BroadPharm |

### LNP Assembly Kits

| Product | Vendor | Notes |
|---------|--------|-------|
| **LNP Starter Kit** | CordenPharma | 4 lipids pre-weighed, research-grade | [cordenpharma.com](https://cordenpharma.com/what-you-need/lipid-excipients/lnp-starter-kit/) |
| **NanoAssemblr Ignite** | Precision NanoSystems | Microfluidic LNP formulation device | ~$50K–$100K |
| **GenScript LNP Service** | GenScript | IVT RNA + LNP packaging service | [genscript.com](https://www.genscript.com/ivt-rna-lnp.html) |
| **NanOZ LNP-mRNA** | OZ Biosciences | Pre-made LNP-mRNA (GFP demo) | [ozbiosciences.com](https://ozbiosciences.com/lipid-nanoparticles-lnp/332-nanoz-lnp-mrnagfp.html) |
| **Premade mRNA-LNP** | PreciGenome | Custom mRNA in LNP | [precigenome.com](https://www.precigenome.com/lipid-nanoparticles-lnp/premade-lipid-nanoparticle-mrna-lnp) |

### DIY LNP Formulation (Without NanoAssemblr)

Possible with ethanol injection or T-junction mixing, but particle size control is poor. Research-grade only.

---

## 4. Full Cost Breakdown — Minimum Viable Setup

### Option 1: Order Everything (Easiest)

| Item | Vendor | Est. Cost |
|------|--------|-----------|
| Custom OSK mRNA (3 constructs, 1mg each) | TriLink / OZ Biosciences | $3,000–$10,000 |
| LNP formulation service | GenScript / PreciGenome | $2,000–$5,000 |
| **Total** | | **$5,000–$15,000** |

### Option 2: DIY IVT + Order LNP Service

| Item | Est. Cost |
|------|-----------|
| Plasmids from Addgene (3x OSK) | $250 |
| IVT kit (NEB HiScribe + CleanCap) | $600 |
| Modified nucleotides + reagents | $1,500 |
| Lab equipment (if not owned) | $10,000–$30,000 |
| LNP formulation service | $2,000–$5,000 |
| **Total** | **$14,000–$37,000** |

### Option 3: Full DIY (IVT + LNP)

| Item | Est. Cost |
|------|-----------|
| All of Option 2 minus LNP service | $12,000–$32,000 |
| LNP lipids (CordenPharma starter) | $1,000–$3,000 |
| NanoAssemblr or microfluidic setup | $5,000–$100,000 |
| **Total** | **$18,000–$135,000** |

---

## 5. Current State of Human Trials (2026)

| Company | Therapy | Target | Status |
|---------|---------|--------|--------|
| **Life Biosciences** | ER-100 (OSK gene therapy) | Optic neuropathy / glaucoma | FDA-approved IND, entering clinic Q1 2026 |
| **YouthBio Therapeutics** | YB002 (partial reprogramming) | Brain aging | FDA greenlighted pathway |
| **Turn Biotechnologies** | mRNA-based ERA platform | Skin, cartilage | Preclinical |
| **Altos Labs** | Reprogramming research | Whole-body rejuvenation | Early research (funded $3B+) |
| **Retro Biosciences** | Partial reprogramming | Lifespan extension | Preclinical |

> **First-ever human rejuvenation trial** (Life Biosciences ER-100) expected to begin in early 2026, as reported by [MIT Technology Review](https://www.technologyreview.com/2026/01/27/1131796/the-first-human-test-of-a-rejuvenation-method-will-begin-shortly/).

---

## 6. Critical Safety Warnings

| Risk | Details |
|------|---------|
| **Tumor formation** | OSKM (especially c-Myc) can cause teratomas. Even OSK carries risk if expression is too long or uncontrolled. |
| **Dosing is unknown** | Mouse studies used precise doxycycline-inducible systems. No human dosing protocol exists. |
| **No off-switch** | mRNA degrades naturally (~hours to days), but the downstream epigenetic changes may be irreversible. |
| **Immune reaction** | Even with modified nucleosides, repeated mRNA-LNP doses can trigger anti-PEG antibodies and inflammation. |
| **Organ targeting** | LNPs preferentially accumulate in the liver. Targeting other organs requires specialized lipid formulations not commercially available. |
| **Sterility** | Non-GMP mRNA/LNP may contain endotoxins, dsRNA contaminants, or degradation products. |
| **Legal** | Self-administering unapproved gene therapies may violate FDA regulations. No country has approved this for anti-aging use. |

---

## 7. Summary: Is It Possible?

| Step | Possible Today? | Difficulty | Cost |
|------|----------------|------------|------|
| Synthesize OSK mRNA | **Yes** | Low-Medium | $3K–$10K |
| Get modified + capped mRNA | **Yes** | Low (order) / Medium (DIY) | Included above |
| Encapsulate in LNPs | **Yes** | Medium-High | $2K–$100K |
| Know correct dose for humans | **No** | N/A | No data exists |
| Target specific tissues | **Partially** (liver easy, others hard) | High | Research-grade |
| Safely self-administer | **No** — no human protocol exists | Extreme risk | N/A |
| Legal approval | **No** — first trial just starting 2026 | N/A | N/A |

**Bottom line:** You can buy/make every physical component today. The missing pieces are the **dosing protocol**, **safety validation**, and **regulatory approval** — which are being addressed by Life Biosciences, YouthBio, and others in 2026 human trials.

---

## Sources

- [Kilobaser Personal DNA & RNA Synthesizer](https://kilobaser.com/)
- [NEB HiScribe T7 mRNA Kit with CleanCap](https://www.neb.com/en-us/products/e2080-hiscribe-t7-mrna-kit-with-cleancap-reagent-ag)
- [Thermo Fisher MEGAscript T7 Kit](https://www.thermofisher.com/order/catalog/product/AMB13345)
- [TriLink Custom mRNA Synthesis](https://www.trilinkbiotech.com/discovery-mrna-synthesis)
- [Creative Biolabs mRNA Cell Reprogramming](https://mrna.creative-biolabs.com/custom-mrna-based-cell-reprogramming-service.htm)
- [Addgene OSKM Plasmids](https://www.addgene.org/search/catalog/plasmids/?q=oct4+sox2+klf4+c+myc)
- [Telesis Bio BioXp mRNA Synthesis](https://telesisbio.com/products/mrna-synthesis/)
- [CordenPharma LNP Starter Kit](https://cordenpharma.com/what-you-need/lipid-excipients/lnp-starter-kit/)
- [CureVac RNA Printer](https://www.curevac.com/en/technology/rna-printer/)
- [CureVac RNA Printer Subsidiary — Fierce Pharma](https://www.fiercepharma.com/manufacturing/curevac-forms-subsidiary-centered-its-rna-printer-production-technology)
- [mRNA Printers Kick-Start Personalized Medicines — Nature Biotech](https://pmc.ncbi.nlm.nih.gov/articles/PMC9362478/)
- [First Human Rejuvenation Trial — MIT Tech Review](https://www.technologyreview.com/2026/01/27/1131796/the-first-human-test-of-a-rejuvenation-method-will-begin-shortly/)
- [FDA Greenlights Life Biosciences Human Study](https://www.nad.com/news/fda-greenlights-life-biosciences-human-study-setting-up-pivotal-test-for-aging-theory-from-harvards-david-sinclair)
- [YouthBio FDA Partial Brain Reprogramming](https://www.nmn.com/news/partial-brain-reprogramming-fda-greenlights-first-human-trial)
- [SB000 Single Factor Rejuvenation — bioRxiv](https://www.biorxiv.org/content/10.1101/2025.06.05.657370v1.full)
- [GenScript LNP Packaging](https://www.genscript.com/ivt-rna-lnp.html)
- [BioXp Select mRNA Kit — VWR](https://us-prod2.vwr.com/store/product/45963021/bioxp-select-mrna-synthesis-kits)
