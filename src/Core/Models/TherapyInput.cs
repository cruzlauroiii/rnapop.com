namespace Core.Models;

/// <summary>Universal therapy input — each therapy uses relevant fields.</summary>
public sealed class TherapyInput
{
    public string TherapyId { get; set; } = "rejuvenation";

    // Shared
    public Species Species { get; set; } = Species.Human;
    public string? PetBreed { get; set; }
    public int Age { get; set; } = 40;
    public Sex Sex { get; set; } = Sex.Male;
    public double WeightKg { get; set; } = 70;

    // Rejuvenation (existing)
    public BiomarkerInput? Biomarkers { get; set; }

    // Antiviral (Cold / Flu / COVID)
    public int SymptomOnsetHours { get; set; }
    public double TemperatureCelsius { get; set; } = 37.0;
    public string? VirusType { get; set; }          // "Rhinovirus", "Influenza A", "SARS-CoV-2"
    public string? VirusSubtype { get; set; }       // "H1N1", "H3N2", "Omicron XBB"
    public bool HasCough { get; set; }
    public bool HasSoreThroat { get; set; }
    public bool HasCongestion { get; set; }
    public bool HasBodyAches { get; set; }
    public bool HasFatigue { get; set; }
    public bool HasHeadache { get; set; }
    public int SeverityScore { get; set; } = 5;     // 1-10

    // COVID-specific
    public int VaccinationDoses { get; set; }
    public int DaysSinceLastVaccine { get; set; }
    public bool Immunocompromised { get; set; }

    // Flu vaccine
    public int PriorFluVaccinations { get; set; }

    // Cancer
    public string? TumorType { get; set; }
    public bool TumorBiopsyAvailable { get; set; }
    public string? HlaType { get; set; }
    public string[]? PriorTreatments { get; set; }
    public double TumorSizeCm { get; set; }
    public string? CancerStage { get; set; }

    // Wound healing
    public string? WoundType { get; set; }          // "Surgical", "Diabetic ulcer", "Burns", "Chronic"
    public double WoundAreaCm2 { get; set; }
    public int WoundAgeDays { get; set; }
    public bool IsDiabetic { get; set; }
    public bool OnAnticoagulants { get; set; }

    // Hair
    public int HairLossScale { get; set; }          // Norwood 1-7 (M) or Ludwig I-III (F)
    public int HairLossYears { get; set; }
    public string[]? CurrentHairTreatments { get; set; }
    public bool FamilyHistoryBaldness { get; set; }

    // Muscle / Sarcopenia
    public double GripStrengthKg { get; set; }
    public double MuscleMassIndex { get; set; }     // kg/m2
    public int PhysicalActivityLevel { get; set; }  // 1-5
    public bool HasFalls { get; set; }

    // Allergy
    public string[]? Allergens { get; set; }        // "Birch pollen", "Dust mite", "Cat", etc.
    public double TotalIgE { get; set; }            // IU/mL
    public int[]? SkinPrickResults { get; set; }    // wheal size mm per allergen
    public int AllergySeverity { get; set; }         // 1-10
    public bool HasAnaphylaxisHistory { get; set; }
}
