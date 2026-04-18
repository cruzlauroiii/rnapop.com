namespace Core.Models;

/// <summary>
/// RNA therapy catalog. Each therapy defines target mRNA sequences,
/// required inputs, dosing parameters, and LNP formulation specs.
/// </summary>
public sealed class TherapyOption
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Description { get; init; }
    public required string Mechanism { get; init; }
    public required MrnaConstruct[] Constructs { get; init; }
    public required string[] RequiredInputs { get; init; }
    public required string LnpFormulation { get; init; }
    public required string DosingNotes { get; init; }
    public required string Status { get; init; } // Research / Preclinical / Clinical / Approved
    public required string[] References { get; init; }
}

public sealed class MrnaConstruct
{
    public required string Name { get; init; }
    public required string Gene { get; init; }
    public int LengthNt { get; init; }
    public required string Function { get; init; }
    public required string Source { get; init; } // Addgene ID, GenBank accession, etc.
}

public static class TherapyCatalog
{
    public static TherapyOption[] All =>
    [
        Rejuvenation,
        CommonCold,
        Influenza,
        Covid19,
        UniversalFlu,
        CancerNeoantigen,
        WoundHealing,
        HairRegrowth,
        MuscleLoss,
        Allergy,
    ];

    public static readonly TherapyOption Rejuvenation = new()
    {
        Id = "rejuvenation",
        Name = "Rejuvenation",
        Category = "Rejuvenation",
        Description = "Partial epigenetic reprogramming via Yamanaka factors to reverse biological aging.",
        Mechanism = "OSK mRNA transiently loosens epigenetic marks locking cells into aged state, reverting transcriptomic age without inducing pluripotency. Requires precise temporal control — mRNA's short half-life provides built-in safety window.",
        Constructs =
        [
            new() { Name = "OCT4", Gene = "POU5F1", LengthNt = 1100, Function = "Master pluripotency TF", Source = "Addgene #193298" },
            new() { Name = "SOX2", Gene = "SOX2", LengthNt = 960, Function = "Stem cell maintenance", Source = "Addgene #193298" },
            new() { Name = "KLF4", Gene = "KLF4", LengthNt = 1400, Function = "Tumor suppressor / reprogramming", Source = "Addgene #193298" },
        ],
        RequiredInputs = ["Blood Panel (PhenoAge)", "piRNA Panel", "miRNA Panel", "Senescence Panel", "Pathway Panel", "Telomere Panel"],
        LnpFormulation = "SM-102:DSPC:Cholesterol:PEG-DMG = 50:10:38.5:1.5 molar. Liver-tropic standard formulation.",
        DosingNotes = "No human dosing established. Mouse studies: cyclic dosing 2 days on / 5 days off. Life Biosciences ER-100 entering first human trial 2026.",
        Status = "Preclinical (first human trial 2026)",
        References = ["Levine 2018 (PhenoAge)", "Duke 2026 (piRNA)", "Life Biosciences ER-100"],
    };

    public static readonly TherapyOption CommonCold = new()
    {
        Id = "common-cold",
        Name = "Common Cold",
        Category = "Antiviral",
        Description = "mRNA-encoded broadly neutralizing antibody against rhinovirus, targeting the conserved VP4 protein canyon region shared across 160+ rhinovirus serotypes.",
        Mechanism = "Single mRNA encodes a bispecific antibody targeting rhinovirus VP4 (conserved across serotypes) and ICAM-1 receptor-binding site. Translated in liver, secreted antibody neutralizes virus in upper respiratory tract. Alternative approach: mRNA encoding interferon-lambda (IFN-λ) for broad-spectrum innate antiviral response.",
        Constructs =
        [
            new() { Name = "Anti-RV bispecific Ab", Gene = "Custom scFv-Fc", LengthNt = 2400, Function = "Broadly neutralizing antibody vs rhinovirus", Source = "Custom design from PDB 4LLQ + 1D3E" },
            new() { Name = "IFN-lambda", Gene = "IFNL1", LengthNt = 600, Function = "Innate antiviral cytokine — broad spectrum", Source = "GenBank NM_172140" },
        ],
        RequiredInputs = ["Symptom onset (hours)", "Temperature", "Rhinovirus serotype (if known)"],
        LnpFormulation = "Inhaled LNP: DOTAP-based cationic lipid for lung delivery. Nebulizer-compatible particle size 100-300nm.",
        DosingNotes = "Intranasal or nebulized delivery. Administer within 24h of symptom onset for best efficacy. Single dose expected sufficient due to short illness duration.",
        Status = "Research",
        References = ["Moderna mRNA-encoded Ab platform", "IFN-lambda antiviral studies"],
    };

    public static readonly TherapyOption Influenza = new()
    {
        Id = "influenza",
        Name = "Influenza Treatment",
        Category = "Antiviral",
        Description = "mRNA vaccine or therapeutic encoding hemagglutinin (HA) stem domain for broad protection against influenza A and B.",
        Mechanism = "mRNA encodes the conserved HA stem domain, eliciting antibodies that neutralize across influenza subtypes (H1, H3, B). As treatment: mRNA encoding anti-HA stem broadly neutralizing antibody (bnAb) for passive immunization when already sick.",
        Constructs =
        [
            new() { Name = "HA stem antigen", Gene = "HA2 (consensus)", LengthNt = 1200, Function = "Universal flu antigen — conserved stalk", Source = "GenBank CY121680 (H1N1 consensus)" },
            new() { Name = "Anti-HA bnAb CR9114", Gene = "CR9114 IgG", LengthNt = 3000, Function = "Broadly neutralizing Ab vs influenza A+B", Source = "PDB 4FQI" },
            new() { Name = "Neuraminidase inhibitor peptide", Gene = "Custom", LengthNt = 300, Function = "Blocks viral release from infected cells", Source = "Custom design" },
        ],
        RequiredInputs = ["Symptom onset (hours)", "Temperature", "Influenza type (A/B)", "Subtype if known (H1N1, H3N2)"],
        LnpFormulation = "Standard IM injection LNP: SM-102:DSPC:Chol:PEG-DMG = 50:10:38.5:1.5. Same as COVID mRNA vaccines.",
        DosingNotes = "Prophylactic: 50ug IM single dose (like flu mRNA vaccine). Therapeutic: 100ug IM within 48h of symptom onset. Moderna mRNA-1010 (seasonal flu) is Phase 3.",
        Status = "Clinical (Moderna mRNA-1010 Phase 3)",
        References = ["Moderna mRNA-1010", "CR9114 bnAb (Dreyfus 2012)", "mRNA-1020 combo vaccine"],
    };

    public static readonly TherapyOption Covid19 = new()
    {
        Id = "covid19",
        Name = "COVID-19",
        Category = "Antiviral",
        Description = "Next-generation mRNA encoding updated spike protein or broadly neutralizing nanobody for SARS-CoV-2 variants.",
        Mechanism = "Encodes prefusion-stabilized spike protein (2P mutation) of current dominant variant, or a mosaic antigen covering multiple variant RBDs for pan-sarbecovirus protection.",
        Constructs =
        [
            new() { Name = "Spike 2P (current variant)", Gene = "S protein", LengthNt = 4000, Function = "Prefusion-stabilized spike antigen", Source = "GISAID latest consensus" },
            new() { Name = "Pan-sarbeco nanobody", Gene = "VHH-72-Fc", LengthNt = 1200, Function = "Broadly neutralizing nanobody vs all sarbecoviruses", Source = "PDB 6WAQ" },
        ],
        RequiredInputs = ["Vaccination status", "Days since symptom onset", "Variant (if known)", "Immunocompromised status"],
        LnpFormulation = "Standard IM: SM-102 formulation (identical to Moderna Spikevax).",
        DosingNotes = "Prophylactic: 50ug IM (approved). Therapeutic passive immunization: 200ug IM within 5 days of symptom onset.",
        Status = "Approved (vaccines), Research (therapeutic)",
        References = ["BNT162b2 (Pfizer)", "mRNA-1273 (Moderna)", "VHH-72 (Wrapp 2020)"],
    };

    public static readonly TherapyOption UniversalFlu = new()
    {
        Id = "universal-flu",
        Name = "Universal Flu Vaccine",
        Category = "Vaccine",
        Description = "Single mRNA encoding all 20 known influenza HA subtypes for lifelong pan-influenza protection.",
        Mechanism = "Multivalent mRNA encoding HA antigens from all 18 influenza A subtypes and 2 influenza B lineages. Each HA is nanoparticle-displayed. Proven in mice to elicit broad, subtype-specific antibody responses against all 20 subtypes simultaneously.",
        Constructs =
        [
            new() { Name = "20-valent HA mRNA", Gene = "20x HA", LengthNt = 24000, Function = "All 20 flu subtypes in single formulation", Source = "Arevalo 2022 (Penn/Bhatt lab)" },
        ],
        RequiredInputs = ["Age", "Prior flu vaccination history", "Immunocompromised status"],
        LnpFormulation = "Standard IM LNP. 20 separate mRNAs co-encapsulated or co-injected.",
        DosingNotes = "50ug total mRNA (2.5ug per subtype x 20). Single IM injection. Shown to protect against all subtypes in mice. Human trials pending.",
        Status = "Preclinical (mouse proof-of-concept published Science 2022)",
        References = ["Arevalo et al., Science 2022", "Penn/Bhatt lab 20-valent study"],
    };

    public static readonly TherapyOption CancerNeoantigen = new()
    {
        Id = "cancer-neoantigen",
        Name = "Cancer Neoantigen Vaccine",
        Category = "Oncology",
        Description = "Personalized mRNA vaccine encoding patient-specific tumor neoantigens to activate T-cell immune response against cancer.",
        Mechanism = "Tumor biopsy sequenced → somatic mutations identified → neoantigen peptides predicted (MHC-I binding) → up to 34 neoantigens encoded in single mRNA → injected IV → dendritic cells present neoantigens → cytotoxic T-cells kill tumor cells.",
        Constructs =
        [
            new() { Name = "Neoantigen poly-epitope", Gene = "Patient-specific", LengthNt = 3000, Function = "Up to 34 personalized neoantigens", Source = "Patient WES + RNA-seq" },
        ],
        RequiredInputs = ["Tumor type", "Tumor biopsy available", "Prior treatments", "HLA type"],
        LnpFormulation = "IV delivery LNP targeting dendritic cells. Proprietary lipid formulations (BioNTech Lipo-MERIT, Moderna lipid H).",
        DosingNotes = "IV infusion, 9 doses over ~6 months. Combined with anti-PD-1 checkpoint inhibitor. BioNTech autogene cevumeran (BNT122) in Phase 2 for pancreatic cancer + melanoma.",
        Status = "Clinical (Phase 2, BioNTech BNT122 + Moderna mRNA-4157/V940)",
        References = ["BNT122 (BioNTech)", "mRNA-4157/V940 KEYNOTE-942", "Sahin 2017 Nature"],
    };

    public static readonly TherapyOption WoundHealing = new()
    {
        Id = "wound-healing",
        Name = "Wound Healing",
        Category = "Regenerative",
        Description = "mRNA encoding growth factors (VEGF-A, FGF-2) to accelerate tissue repair in chronic or surgical wounds.",
        Mechanism = "Locally delivered mRNA encodes VEGF-A which promotes angiogenesis (new blood vessel formation) at wound site. Transient expression (24-48h) is ideal — sustained VEGF causes leaky vessels, but mRNA's short half-life provides perfect temporal window.",
        Constructs =
        [
            new() { Name = "VEGF-A 165", Gene = "VEGFA", LengthNt = 1200, Function = "Angiogenesis — new blood vessel formation", Source = "GenBank NM_001025366" },
            new() { Name = "FGF-2", Gene = "FGF2", LengthNt = 900, Function = "Fibroblast proliferation + collagen synthesis", Source = "GenBank NM_002006" },
        ],
        RequiredInputs = ["Wound type", "Wound area (cm2)", "Wound age (days)", "Diabetic status"],
        LnpFormulation = "Topical LNP in hydrogel carrier. Citrate-buffered, pH 6.5. Applied directly to wound bed.",
        DosingNotes = "Topical application to wound, 100ug per 10cm2 wound area. Repeat every 3-5 days. AstraZeneca AZD8601 (VEGF-A mRNA) in Phase 2 for diabetic wounds + heart failure.",
        Status = "Clinical (Phase 2, AstraZeneca AZD8601)",
        References = ["AZD8601 (AstraZeneca)", "Moderna mRNA-0184 (VEGF)", "Zangi 2013 Nature Biotech"],
    };

    public static readonly TherapyOption HairRegrowth = new()
    {
        Id = "hair-regrowth",
        Name = "Hair Regrowth",
        Category = "Regenerative",
        Description = "mRNA encoding Wnt pathway activators and sonic hedgehog (SHH) to reactivate dormant hair follicle stem cells.",
        Mechanism = "Dormant hair follicles in androgenetic alopecia have intact stem cells but suppressed Wnt/beta-catenin signaling. mRNA encoding stabilized Wnt3a + SHH transiently reactivates the stem cell niche, pushing follicles from telogen (rest) to anagen (growth) phase.",
        Constructs =
        [
            new() { Name = "WNT3A", Gene = "WNT3A", LengthNt = 1100, Function = "Activates hair follicle stem cells", Source = "GenBank NM_033131" },
            new() { Name = "SHH", Gene = "SHH", LengthNt = 1400, Function = "Dermal papilla morphogenesis", Source = "GenBank NM_000193" },
            new() { Name = "Noggin", Gene = "NOG", LengthNt = 700, Function = "BMP antagonist — permits hair cycling", Source = "GenBank NM_005450" },
        ],
        RequiredInputs = ["Hair loss pattern (Norwood/Ludwig scale)", "Duration of loss (years)", "Current treatments"],
        LnpFormulation = "Intradermal microneedle LNP. Small particle size (<100nm) to penetrate dermal papilla. Dissolving microneedle patch preferred.",
        DosingNotes = "Microneedle patch applied to scalp, 50ug per 5cm2 area. Weekly for 12 weeks, then monthly maintenance. Research stage.",
        Status = "Research",
        References = ["Plikus 2017 (Wnt + hair cycling)", "Hsu 2014 (hair follicle stem cells)"],
    };

    public static readonly TherapyOption MuscleLoss = new()
    {
        Id = "muscle-loss",
        Name = "Muscle Loss / Sarcopenia",
        Category = "Regenerative",
        Description = "mRNA encoding follistatin to block myostatin and promote muscle growth in age-related sarcopenia.",
        Mechanism = "Myostatin (GDF-8) is the primary negative regulator of muscle mass. Follistatin binds and neutralizes myostatin, removing the brake on muscle growth. mRNA delivery provides transient follistatin expression without permanent gene modification.",
        Constructs =
        [
            new() { Name = "Follistatin 344", Gene = "FST", LengthNt = 1200, Function = "Myostatin antagonist — promotes muscle growth", Source = "GenBank NM_013409" },
            new() { Name = "IGF-1 Ec (MGF)", Gene = "IGF1 splice variant", LengthNt = 400, Function = "Mechano-growth factor — muscle repair", Source = "GenBank NM_001111283" },
        ],
        RequiredInputs = ["Age", "Grip strength (kg)", "Muscle mass index", "Physical activity level"],
        LnpFormulation = "IM injection LNP. Standard SM-102 formulation targeting skeletal muscle.",
        DosingNotes = "IM injection into major muscle groups, 200ug per site. Biweekly for 8 weeks. Myostatin-targeting approaches validated in animal models. No mRNA trials yet but AAV-follistatin gene therapy in Phase 1.",
        Status = "Research (AAV-follistatin in Phase 1)",
        References = ["Lee & McPherron 2001 (myostatin)", "Mendell 2017 (AAV-follistatin Phase 1)"],
    };

    public static readonly TherapyOption Allergy = new()
    {
        Id = "allergy",
        Name = "Allergy Desensitization",
        Category = "Immunotherapy",
        Description = "mRNA encoding hypoallergenic variants of major allergens to retrain immune tolerance without risk of anaphylaxis.",
        Mechanism = "Standard allergy shots use whole allergen extract (anaphylaxis risk). mRNA approach: encode modified allergen with disrupted IgE-binding epitopes but intact T-cell epitopes. Immune system learns tolerance via T-regulatory cells without triggering mast cell degranulation.",
        Constructs =
        [
            new() { Name = "Hypo-Bet v 1", Gene = "Bet v 1 (modified)", LengthNt = 500, Function = "Birch pollen major allergen — disrupted IgE epitopes", Source = "Custom from UniProt P15494" },
            new() { Name = "Hypo-Der p 1", Gene = "Der p 1 (modified)", LengthNt = 700, Function = "Dust mite major allergen — disrupted IgE epitopes", Source = "Custom from UniProt P08176" },
            new() { Name = "Hypo-Fel d 1", Gene = "Fel d 1 (modified)", LengthNt = 600, Function = "Cat allergen — disrupted IgE epitopes", Source = "Custom from UniProt P30438" },
        ],
        RequiredInputs = ["Allergen(s)", "IgE levels", "Skin prick test results", "Severity score"],
        LnpFormulation = "Subcutaneous LNP with tolerogenic adjuvant. Mannose-conjugated LNP to target dendritic cells for tolerance induction.",
        DosingNotes = "SC injection, escalating doses over 8 weeks. BioNTech BNT162a2 (allergy mRNA) concept demonstrated. No clinical trials yet for allergy mRNA.",
        Status = "Research",
        References = ["Weiss 2012 (hypoallergenic recombinants)", "BioNTech allergy mRNA concept"],
    };
}
