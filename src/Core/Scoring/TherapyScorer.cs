namespace Core.Scoring;

using Models;

/// <summary>
/// Scores all 10 RNA therapies based on patient-specific inputs.
/// Each therapy has its own eligibility logic, dosing, contraindications.
/// Supports both humans and pets — dose scales by body weight.
/// </summary>
public static class TherapyScorer
{
    /// <summary>Scale dose from human reference (70kg) to actual patient weight.</summary>
    public static double ScaleDose(double humanDoseUg, double weightKg, Species species)
    {
        // Allometric scaling: dose = humanDose × (weight / 70)^0.75
        // The 0.75 exponent accounts for metabolic rate differences across species
        double scaleFactor = Math.Pow(weightKg / 70.0, 0.75);
        // Minimum dose floor: 10% of human dose (safety)
        return Math.Max(humanDoseUg * 0.1, humanDoseUg * scaleFactor);
    }

    /// <summary>Get species-specific notes.</summary>
    public static string[] GetSpeciesWarnings(Species species) => species switch
    {
        Species.Dog => [
            "Veterinary use: mRNA vaccines are USDA-approved for dogs (Nobivac NXT by Merck — canine flu H3N2, rabies)",
            "Dose scaled allometrically by body weight (0.75 exponent)",
            "Dog-specific: monitor for injection site swelling 24-48h — more common in small breeds",
            "Consult veterinarian before administering any experimental therapy",
        ],
        Species.Cat => [
            "Veterinary use: mRNA vaccines are USDA-approved for cats (Nobivac NXT FeLV by Merck — feline leukemia virus)",
            "Cats are more sensitive to adjuvants — mRNA-LNP is adjuvant-free (advantage over traditional vaccines)",
            "Cat-specific: avoid scruff injection site (risk of injection-site sarcoma) — use lateral thorax instead",
            "Dose scaled allometrically by body weight",
            "Consult veterinarian before administering any experimental therapy",
        ],
        Species.Horse => [
            "Equine mRNA vaccines under development for influenza and EHV-1",
            "Large body weight requires proportionally scaled dose",
            "IM injection in neck muscle (jugular groove area)",
            "Consult equine veterinarian",
        ],
        Species.Other => ["Consult veterinarian for species-specific dosing and administration"],
        _ => [], // Human — no extra warnings
    };

    public static TherapyResult Score(TherapyInput input)
    {
        var therapy = TherapyCatalog.All.FirstOrDefault(t => t.Id == input.TherapyId)
            ?? TherapyCatalog.Rejuvenation;

        return input.TherapyId switch
        {
            "rejuvenation" => ScoreRejuvenation(input, therapy),
            "common-cold" => ScoreCold(input, therapy),
            "influenza" => ScoreInfluenza(input, therapy),
            "covid19" => ScoreCovid(input, therapy),
            "universal-flu" => ScoreUniversalFlu(input, therapy),
            "cancer-neoantigen" => ScoreCancer(input, therapy),
            "wound-healing" => ScoreWound(input, therapy),
            "hair-regrowth" => ScoreHair(input, therapy),
            "muscle-loss" => ScoreMuscle(input, therapy),
            "allergy" => ScoreAllergy(input, therapy),
            _ => ScoreRejuvenation(input, therapy),
        };
    }

    // ===== 1. Rejuvenation =====
    private static TherapyResult ScoreRejuvenation(TherapyInput input, TherapyOption therapy)
    {
        var biomarkers = input.Biomarkers ?? new BiomarkerInput
        {
            Demographics = new() { ChronologicalAge = input.Age, Sex = input.Sex }
        };
        var rejuvResult = CompositeScorer.Score(biomarkers);

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = rejuvResult.Verdict == Verdict.Pass ? TherapyVerdict.Recommended : TherapyVerdict.Conditional,
            Urgency = Urgency.Elective,
            ConfidenceScore = rejuvResult.CompositeScore,
            Summary = rejuvResult.Verdict == Verdict.Pass
                ? $"Eligible for rejuvenation. Biological age {rejuvResult.BiologicalAge:F0} vs chronological {rejuvResult.ChronologicalAge}."
                : $"Not yet eligible. Biological age {rejuvResult.BiologicalAge:F0} ({rejuvResult.AgeDelta:+0.0;-0.0} years). Address flagged pillars first.",
            Rationale = rejuvResult.Pillars.Where(p => p.HasData).Select(p => $"{p.Name}: {p.Score:F0}/100").ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = 100,
                Route = "IV or IM",
                Doses = 6,
                Schedule = "Cyclic: 2 days on / 5 days off, 6 cycles",
                Notes = "No human protocol established. Based on mouse OSK studies. First human trial (Life Biosciences ER-100) starting 2026."
            },
            Warnings = ["No approved human dosing protocol exists", "Tumor risk with c-Myc — OSK (without c-Myc) recommended", "Requires monitoring for teratoma formation"],
            Contraindications = rejuvResult.Recommendations,
            Constructs = therapy.Constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "IV infusion or IM injection",
            DeliveryForm = DeliveryForm.IntramuscularInjection,
            RejuvenationScore = rejuvResult,
        };
    }

    // ===== 2. Common Cold =====
    private static TherapyResult ScoreCold(TherapyInput input, TherapyOption therapy)
    {
        var warnings = new List<string>();
        var contras = new List<string>();
        var rationale = new List<string>();

        // Timing is critical — within 24h of onset
        bool earlyEnough = input.SymptomOnsetHours <= 24;
        rationale.Add(earlyEnough
            ? $"Symptom onset {input.SymptomOnsetHours}h ago — within treatment window"
            : $"Symptom onset {input.SymptomOnsetHours}h ago — past optimal 24h window");

        // Severity assessment
        int symptomCount = (input.HasCough ? 1 : 0) + (input.HasSoreThroat ? 1 : 0) + (input.HasCongestion ? 1 : 0)
            + (input.HasBodyAches ? 1 : 0) + (input.HasFatigue ? 1 : 0) + (input.HasHeadache ? 1 : 0);
        rationale.Add($"Symptoms: {symptomCount}/6 present, severity {input.SeverityScore}/10");

        bool hasFever = input.TemperatureCelsius > 38.0;
        if (hasFever)
        {
            rationale.Add($"Fever present ({input.TemperatureCelsius:F1}°C) — unusual for common cold, consider flu/COVID");
            warnings.Add("Fever >38°C is uncommon in rhinovirus cold. Rule out influenza or COVID-19.");
        }

        double confidence = earlyEnough ? 75 : 45;
        confidence += symptomCount * 3;
        if (input.SeverityScore > 7) confidence -= 10; // Severe = might be flu

        var verdict = earlyEnough && !hasFever ? TherapyVerdict.Recommended
            : earlyEnough ? TherapyVerdict.Conditional
            : TherapyVerdict.NotRecommended;

        // Construct selection
        var constructs = earlyEnough
            ? therapy.Constructs  // Both: anti-RV Ab + IFN-lambda
            : [therapy.Constructs[1]]; // Late: IFN-lambda only (broad-spectrum)

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = verdict,
            Urgency = earlyEnough ? Urgency.Immediate : Urgency.Within24Hours,
            ConfidenceScore = Math.Clamp(confidence, 0, 100),
            Summary = verdict == TherapyVerdict.Recommended
                ? "Early-stage cold detected. Intranasal mRNA delivery recommended."
                : hasFever
                    ? "Fever suggests this may not be a simple cold. Test for flu/COVID first."
                    : $"Symptom onset {input.SymptomOnsetHours}h ago — past optimal treatment window. IFN-lambda may still provide symptom relief.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = earlyEnough ? 50 : 25,
                Route = "Intranasal (nebulizer or nasal spray)",
                Doses = 1,
                Schedule = "Single dose within 24h of symptom onset",
                Notes = "Nebulized LNP-mRNA delivered to upper respiratory tract. IFN-lambda mRNA provides 24-48h of local antiviral activity."
            },
            Warnings = warnings.ToArray(),
            Contraindications = contras.ToArray(),
            Constructs = constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "Intranasal (nebulizer)",
            DeliveryForm = DeliveryForm.IntranasalNebulizer,
        };
    }

    // ===== 3. Influenza =====
    private static TherapyResult ScoreInfluenza(TherapyInput input, TherapyOption therapy)
    {
        var rationale = new List<string>();
        var warnings = new List<string>();

        bool earlyEnough = input.SymptomOnsetHours <= 48;
        bool hasFever = input.TemperatureCelsius >= 38.0;
        bool severe = input.SeverityScore >= 7 || input.TemperatureCelsius >= 39.5;

        rationale.Add($"Onset: {input.SymptomOnsetHours}h ago ({(earlyEnough ? "within" : "past")} 48h window)");
        rationale.Add($"Temperature: {input.TemperatureCelsius:F1}°C {(hasFever ? "(febrile)" : "(afebrile)")}");
        rationale.Add($"Severity: {input.SeverityScore}/10");
        if (input.VirusSubtype is not null) rationale.Add($"Subtype: {input.VirusSubtype}");

        if (input.Age >= 65) warnings.Add("Age ≥65: high-risk group. Consider hospitalization if severe.");
        if (input.Immunocompromised) warnings.Add("Immunocompromised: may have reduced response to mRNA therapy.");
        if (severe) warnings.Add("Severe symptoms — antiviral (oseltamivir) should be co-administered.");

        double confidence = earlyEnough ? 80 : 40;
        if (hasFever) confidence += 10; // Fever confirms flu
        if (severe) confidence -= 10;

        // Use bnAb construct for treatment, HA stem for prophylaxis
        MrnaConstruct[] constructs = earlyEnough
            ? [therapy.Constructs[1], therapy.Constructs[2]] // CR9114 bnAb + NA inhibitor
            : [therapy.Constructs[0]]; // HA stem antigen (vaccine-like)

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = earlyEnough ? TherapyVerdict.Recommended : TherapyVerdict.Conditional,
            Urgency = severe ? Urgency.Immediate : Urgency.Within24Hours,
            ConfidenceScore = Math.Clamp(confidence, 0, 100),
            Summary = earlyEnough
                ? $"Influenza detected within treatment window. mRNA-encoded bnAb (CR9114) + neuraminidase inhibitor recommended."
                : "Past 48h window for therapeutic mRNA. Consider HA stem mRNA for immune boost.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = earlyEnough ? 100 : 50,
                Route = "IM injection",
                Doses = 1,
                Schedule = earlyEnough ? "Single dose ASAP (within 48h onset)" : "Single dose for immune boost",
                Notes = severe
                    ? "Co-administer with oseltamivir 75mg BID x 5 days. mRNA provides complementary bnAb passive immunity."
                    : "Standalone mRNA therapy. Monitor temperature every 6h."
            },
            Warnings = warnings.ToArray(),
            Contraindications = severe ? ["Combine with standard antiviral (oseltamivir)"] : [],
            Constructs = constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "IM injection (deltoid)",
            DeliveryForm = DeliveryForm.IntramuscularInjection,
        };
    }

    // ===== 4. COVID-19 =====
    private static TherapyResult ScoreCovid(TherapyInput input, TherapyOption therapy)
    {
        var rationale = new List<string>();
        var warnings = new List<string>();

        bool earlyEnough = input.SymptomOnsetHours <= 120; // 5 days
        bool vaccinated = input.VaccinationDoses >= 2;
        bool boosterCurrent = input.DaysSinceLastVaccine < 180;
        bool highRisk = input.Age >= 65 || input.Immunocompromised;

        rationale.Add($"Onset: {input.SymptomOnsetHours}h ago");
        rationale.Add($"Vaccination: {input.VaccinationDoses} doses, {input.DaysSinceLastVaccine} days since last");
        rationale.Add($"High risk: {(highRisk ? "Yes" : "No")}");
        if (input.VirusSubtype is not null) rationale.Add($"Variant: {input.VirusSubtype}");

        if (input.Immunocompromised) warnings.Add("Immunocompromised: mRNA passive immunization especially valuable.");
        if (!vaccinated) warnings.Add("Unvaccinated: higher risk of severe disease.");

        double confidence = 85;
        if (!earlyEnough) confidence -= 30;
        if (highRisk && !earlyEnough) confidence -= 15;

        // Therapeutic: nanobody for passive immunization
        var constructs = earlyEnough
            ? therapy.Constructs // Both spike + nanobody
            : [therapy.Constructs[0]]; // Spike only (vaccine boost)

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = highRisk || !vaccinated ? TherapyVerdict.Recommended : TherapyVerdict.Conditional,
            Urgency = highRisk ? Urgency.Immediate : Urgency.Within48Hours,
            ConfidenceScore = Math.Clamp(confidence, 0, 100),
            Summary = highRisk
                ? "High-risk patient. mRNA-encoded pan-sarbecovirus nanobody (VHH-72) for immediate passive immunization."
                : earlyEnough
                    ? "Within treatment window. mRNA therapeutic + spike booster recommended."
                    : "Past 5-day window. Spike mRNA booster for immune enhancement.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = highRisk ? 200 : 100,
                Route = "IM injection",
                Doses = 1,
                Schedule = "Single dose. Second dose at day 28 if immunocompromised.",
                Notes = "Co-administer with nirmatrelvir/ritonavir (Paxlovid) if within 5 days and high-risk."
            },
            Warnings = warnings.ToArray(),
            Contraindications = input.Immunocompromised ? ["Monitor for enhanced inflammatory response"] : [],
            Constructs = constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "IM injection (deltoid)",
            DeliveryForm = DeliveryForm.IntramuscularInjection,
        };
    }

    // ===== 5. Universal Flu Vaccine =====
    private static TherapyResult ScoreUniversalFlu(TherapyInput input, TherapyOption therapy)
    {
        var rationale = new List<string>();
        var warnings = new List<string>();

        bool youngAdult = input.Age >= 18 && input.Age <= 50;
        rationale.Add($"Age: {input.Age} — {(youngAdult ? "optimal" : input.Age < 18 ? "pediatric dosing needed" : "elderly — enhanced formulation")}");
        rationale.Add($"Prior flu vaccinations: {input.PriorFluVaccinations}");
        if (input.Immunocompromised) warnings.Add("Immunocompromised: may need double dose or booster.");

        double confidence = 70; // Preclinical — lower confidence
        if (youngAdult) confidence += 10;

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = TherapyVerdict.Conditional,
            Urgency = Urgency.Scheduled,
            ConfidenceScore = confidence,
            Summary = "Experimental 20-valent mRNA flu vaccine. Shown to protect against all influenza subtypes in mice. No human data yet.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = input.Age >= 65 ? 100 : 50,
                Route = "IM injection",
                Doses = 1,
                Schedule = "Single dose, annually before flu season (September-October)",
                Notes = "50ug total = 2.5ug per subtype x 20. Based on Arevalo et al. (Science 2022) mouse data."
            },
            Warnings = ["Preclinical only — not tested in humans", "May cause stronger reactogenicity than standard flu vaccines due to 20 antigens", ..warnings],
            Contraindications = input.HasAnaphylaxisHistory ? ["History of anaphylaxis — administer under observation"] : [],
            Constructs = therapy.Constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "IM injection (deltoid)",
            DeliveryForm = DeliveryForm.IntramuscularInjection,
        };
    }

    // ===== 6. Cancer Neoantigen =====
    private static TherapyResult ScoreCancer(TherapyInput input, TherapyOption therapy)
    {
        var rationale = new List<string>();
        var warnings = new List<string>();
        var contras = new List<string>();

        bool hasBiopsy = input.TumorBiopsyAvailable;
        bool hasHla = !string.IsNullOrEmpty(input.HlaType);

        rationale.Add($"Tumor type: {input.TumorType ?? "Not specified"}");
        rationale.Add($"Stage: {input.CancerStage ?? "Not specified"}");
        rationale.Add($"Biopsy available: {(hasBiopsy ? "Yes" : "No — REQUIRED")}");
        rationale.Add($"HLA typed: {(hasHla ? input.HlaType : "No — REQUIRED for neoantigen prediction")}");
        rationale.Add($"Tumor size: {input.TumorSizeCm:F1} cm");

        if (!hasBiopsy) contras.Add("Tumor biopsy required for WES + RNA-seq neoantigen identification");
        if (!hasHla) contras.Add("HLA typing required for MHC-I peptide binding prediction");

        if (input.Immunocompromised) warnings.Add("Immunocompromised: T-cell response may be blunted. Combine with IL-2 or checkpoint inhibitor.");

        double confidence = (hasBiopsy && hasHla) ? 75 : 20;

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = hasBiopsy && hasHla ? TherapyVerdict.Recommended : TherapyVerdict.NotRecommended,
            Urgency = Urgency.Scheduled,
            ConfidenceScore = confidence,
            Summary = hasBiopsy && hasHla
                ? $"Personalized neoantigen vaccine feasible. WES of {input.TumorType} biopsy will identify patient-specific mutations for mRNA encoding."
                : "Cannot proceed without tumor biopsy and HLA typing. These are required for neoantigen identification.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = 25,
                Route = "IV infusion",
                Doses = 9,
                Schedule = "Induction: weekly x 4, then biweekly x 2, then monthly x 3. Co-administer with anti-PD-1.",
                Notes = "BioNTech BNT122 protocol: up to 34 neoantigens per mRNA. 4-6 weeks from biopsy to first dose (sequencing + mRNA synthesis). Combine with pembrolizumab (anti-PD-1)."
            },
            Warnings = ["Requires 4-6 week manufacturing lead time", "Must combine with checkpoint inhibitor for efficacy", ..warnings],
            Contraindications = contras.ToArray(),
            Constructs = therapy.Constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "IV infusion",
            DeliveryForm = DeliveryForm.IntravenousInfusion,
        };
    }

    // ===== 7. Wound Healing =====
    private static TherapyResult ScoreWound(TherapyInput input, TherapyOption therapy)
    {
        var rationale = new List<string>();
        var warnings = new List<string>();

        bool chronic = input.WoundAgeDays > 30;
        bool large = input.WoundAreaCm2 > 20;
        bool diabetic = input.IsDiabetic;

        rationale.Add($"Wound type: {input.WoundType ?? "Not specified"}");
        rationale.Add($"Area: {input.WoundAreaCm2:F1} cm² ({(large ? "large" : "manageable")})");
        rationale.Add($"Age: {input.WoundAgeDays} days ({(chronic ? "chronic" : "acute")})");
        rationale.Add($"Diabetic: {(diabetic ? "Yes — impaired healing" : "No")}");

        if (diabetic) rationale.Add("Diabetic wounds: VEGF-A mRNA especially beneficial — restores angiogenesis impaired by hyperglycemia");
        if (input.OnAnticoagulants) warnings.Add("On anticoagulants: monitor for bleeding at application site");

        double confidence = 80;
        if (chronic && diabetic) confidence = 90; // Best use case — AZD8601 targets this
        if (input.WoundAgeDays < 3) confidence -= 20; // Too early, normal healing may suffice

        double dosePerCm2 = 10; // ug per cm2
        double totalDose = dosePerCm2 * input.WoundAreaCm2;

        // Use both VEGF + FGF for chronic, VEGF only for acute
        var constructs = chronic
            ? therapy.Constructs
            : [therapy.Constructs[0]]; // VEGF-A only

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = chronic || diabetic ? TherapyVerdict.Recommended : TherapyVerdict.Conditional,
            Urgency = chronic ? Urgency.Within24Hours : Urgency.Scheduled,
            ConfidenceScore = confidence,
            Summary = chronic
                ? $"Chronic wound ({input.WoundAgeDays} days). VEGF-A + FGF-2 mRNA topical application to restore angiogenesis and fibroblast activity."
                : $"Acute wound. VEGF-A mRNA may accelerate healing if standard care insufficient after 7+ days.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = totalDose,
                Route = "Topical (LNP in hydrogel)",
                Doses = chronic ? 6 : 3,
                Schedule = "Apply every 3-5 days until wound closure",
                Notes = $"Calculated: {dosePerCm2} ug/cm² x {input.WoundAreaCm2:F0} cm² = {totalDose:F0} ug per application. Apply directly to wound bed after debridement. Cover with occlusive dressing."
            },
            Warnings = ["Keep wound sterile during application", "mRNA hydrogel must be stored at 2-8°C", ..warnings],
            Contraindications = input.WoundType == "Burns" ? ["Burn wounds: wait until granulation tissue visible before mRNA application"] : [],
            Constructs = constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "Topical (hydrogel on wound bed)",
            DeliveryForm = DeliveryForm.TopicalGel,
        };
    }

    // ===== 8. Hair Regrowth =====
    private static TherapyResult ScoreHair(TherapyInput input, TherapyOption therapy)
    {
        var rationale = new List<string>();
        var warnings = new List<string>();

        bool earlyLoss = input.HairLossScale <= 3;
        bool longDuration = input.HairLossYears > 10;

        string scaleType = input.Sex == Sex.Male ? "Norwood" : "Ludwig";
        rationale.Add($"Hair loss: {scaleType} {input.HairLossScale} for {input.HairLossYears} years");
        rationale.Add($"Family history: {(input.FamilyHistoryBaldness ? "Yes (androgenetic)" : "No")}");
        if (input.CurrentHairTreatments?.Length > 0)
            rationale.Add($"Current treatments: {string.Join(", ", input.CurrentHairTreatments)}");

        if (longDuration && !earlyLoss) warnings.Add("Advanced loss (>10 years): follicle miniaturization may be irreversible in some areas. Best results in areas with vellus hair remaining.");

        double confidence = 55; // Research stage
        if (earlyLoss) confidence += 15; // Early = more follicles salvageable
        if (longDuration) confidence -= 15;

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = earlyLoss ? TherapyVerdict.Conditional : TherapyVerdict.Conditional,
            Urgency = Urgency.Elective,
            ConfidenceScore = Math.Clamp(confidence, 0, 100),
            Summary = earlyLoss
                ? $"Early-stage hair loss ({scaleType} {input.HairLossScale}). WNT3A + SHH mRNA microneedle patches may reactivate dormant follicles."
                : $"Advanced hair loss ({scaleType} {input.HairLossScale}). Some follicles may be salvageable. Best in areas with remaining vellus hair.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = 50,
                Route = "Intradermal (microneedle patch)",
                Doses = 12,
                Schedule = "Weekly for 12 weeks, then monthly maintenance",
                Notes = "50ug per 5cm² treatment area. Dissolving microneedle patch applied to affected scalp. Leave on 30 minutes. Research stage — no clinical data."
            },
            Warnings = ["Research stage only — no human trials", "Microneedle application may cause temporary redness/irritation", ..warnings],
            Contraindications = input.HasAnaphylaxisHistory ? ["History of severe allergic reactions — patch test first"] : [],
            Constructs = therapy.Constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "Intradermal (dissolving microneedle patch)",
            DeliveryForm = DeliveryForm.MicroneedlePatch,
        };
    }

    // ===== 9. Muscle Loss / Sarcopenia =====
    private static TherapyResult ScoreMuscle(TherapyInput input, TherapyOption therapy)
    {
        var rationale = new List<string>();
        var warnings = new List<string>();

        // Sarcopenia criteria (EWGSOP2): grip <27kg M / <16kg F, muscle mass index <7.0 M / <5.5 F
        double gripThreshold = input.Sex == Sex.Male ? 27 : 16;
        double mmiThreshold = input.Sex == Sex.Male ? 7.0 : 5.5;
        bool lowGrip = input.GripStrengthKg < gripThreshold;
        bool lowMass = input.MuscleMassIndex < mmiThreshold;
        bool sarcopenia = lowGrip && lowMass;
        bool preSarcopenia = lowGrip || lowMass;

        rationale.Add($"Grip strength: {input.GripStrengthKg:F1} kg (threshold: {gripThreshold} kg) — {(lowGrip ? "BELOW" : "normal")}");
        rationale.Add($"Muscle mass index: {input.MuscleMassIndex:F1} kg/m² (threshold: {mmiThreshold}) — {(lowMass ? "BELOW" : "normal")}");
        rationale.Add($"Physical activity: {input.PhysicalActivityLevel}/5");
        rationale.Add($"Diagnosis: {(sarcopenia ? "Sarcopenia confirmed" : preSarcopenia ? "Pre-sarcopenia" : "Not sarcopenic")}");

        if (input.HasFalls) warnings.Add("Fall history: sarcopenia increases fracture risk. Prioritize treatment.");
        if (input.Age >= 80) warnings.Add("Age ≥80: monitor for cardiac effects of follistatin (myostatin inhibition affects cardiac muscle).");

        double confidence = sarcopenia ? 70 : preSarcopenia ? 50 : 30;

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = sarcopenia ? TherapyVerdict.Recommended : preSarcopenia ? TherapyVerdict.Conditional : TherapyVerdict.NotRecommended,
            Urgency = input.HasFalls ? Urgency.Within48Hours : Urgency.Scheduled,
            ConfidenceScore = confidence,
            Summary = sarcopenia
                ? "Sarcopenia confirmed. Follistatin mRNA to block myostatin and promote muscle growth, combined with resistance exercise."
                : preSarcopenia
                    ? "Pre-sarcopenic. Resistance exercise first; follistatin mRNA if no improvement after 12 weeks."
                    : "Muscle metrics within normal range. Focus on exercise and protein intake.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = 200,
                Route = "IM injection (quadriceps, deltoid)",
                Doses = 8,
                Schedule = "Biweekly for 8 weeks (4 months), with progressive resistance training",
                Notes = "200ug per injection site. Rotate between major muscle groups. Must combine with resistance exercise (3x/week) and protein intake (1.2g/kg/day)."
            },
            Warnings = ["Research stage — AAV-follistatin in Phase 1, mRNA version not yet tested", "Must combine with exercise program", ..warnings],
            Contraindications = input.Age < 40 && !sarcopenia ? ["Not recommended for non-sarcopenic individuals under 40"] : [],
            Constructs = therapy.Constructs,
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "IM injection (rotating muscle groups)",
            DeliveryForm = DeliveryForm.IntramuscularInjection,
        };
    }

    // ===== 10. Allergy Desensitization =====
    private static TherapyResult ScoreAllergy(TherapyInput input, TherapyOption therapy)
    {
        var rationale = new List<string>();
        var warnings = new List<string>();
        var contras = new List<string>();

        int allergenCount = input.Allergens?.Length ?? 0;
        bool severeAllergy = input.AllergySeverity >= 7;
        bool highIgE = input.TotalIgE > 200;

        rationale.Add($"Allergens: {(allergenCount > 0 ? string.Join(", ", input.Allergens!) : "None specified")}");
        rationale.Add($"Total IgE: {input.TotalIgE:F0} IU/mL ({(highIgE ? "elevated" : "normal")})");
        rationale.Add($"Severity: {input.AllergySeverity}/10");

        if (input.HasAnaphylaxisHistory)
        {
            warnings.Add("ANAPHYLAXIS HISTORY: mRNA hypoallergenic approach is SAFER than traditional allergy shots (no intact IgE epitopes). However, administer first dose under medical supervision with epinephrine available.");
            contras.Add("First 3 doses must be administered under medical observation");
        }

        // Select constructs based on allergens
        var selectedConstructs = new List<MrnaConstruct>();
        if (input.Allergens?.Any(a => a.Contains("pollen", StringComparison.OrdinalIgnoreCase) || a.Contains("birch", StringComparison.OrdinalIgnoreCase)) == true)
            selectedConstructs.Add(therapy.Constructs[0]); // Bet v 1
        if (input.Allergens?.Any(a => a.Contains("dust", StringComparison.OrdinalIgnoreCase) || a.Contains("mite", StringComparison.OrdinalIgnoreCase)) == true)
            selectedConstructs.Add(therapy.Constructs[1]); // Der p 1
        if (input.Allergens?.Any(a => a.Contains("cat", StringComparison.OrdinalIgnoreCase)) == true)
            selectedConstructs.Add(therapy.Constructs[2]); // Fel d 1
        if (selectedConstructs.Count == 0)
            selectedConstructs.AddRange(therapy.Constructs); // All if not specified

        double confidence = allergenCount > 0 ? 60 : 30;
        if (highIgE) confidence += 10; // Clear allergic phenotype

        return new TherapyResult
        {
            TherapyId = therapy.Id,
            TherapyName = therapy.Name,
            Verdict = allergenCount > 0 ? TherapyVerdict.Conditional : TherapyVerdict.NotRecommended,
            Urgency = Urgency.Scheduled,
            ConfidenceScore = confidence,
            Summary = allergenCount > 0
                ? $"mRNA-encoded hypoallergenic {string.Join(" + ", selectedConstructs.Select(c => c.Name))} for immune tolerance induction. Safer than traditional allergy shots — no intact IgE-binding epitopes."
                : "Specify allergens to determine appropriate mRNA constructs.",
            Rationale = rationale.ToArray(),
            Dose = new DoseRecommendation
            {
                AmountUg = 10 * selectedConstructs.Count,
                Route = "SC injection",
                Doses = 12,
                Schedule = "Escalating: 1ug week 1, 2ug week 2, 5ug week 3-4, 10ug week 5-8, then monthly x4",
                Notes = $"Total constructs: {selectedConstructs.Count}. Escalating dose reduces risk of allergic reaction. T-regulatory cell induction takes 4-8 weeks. Full course: 6 months."
            },
            Warnings = ["Research stage — no clinical trials yet", "Escalating dose protocol required", ..warnings],
            Contraindications = contras.ToArray(),
            Constructs = selectedConstructs.ToArray(),
            LnpFormulation = therapy.LnpFormulation,
            DeliveryRoute = "SC injection (upper arm)",
            DeliveryForm = DeliveryForm.SubcutaneousInjection,
        };
    }
}
