using Core.Models;
using Core.Scoring;
using Xunit;

namespace Tests;

public class TherapyScorerTests
{
    [Fact]
    public void Cold_EarlyOnset_Recommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "common-cold",
            Age = 35,
            SymptomOnsetHours = 12,
            TemperatureCelsius = 37.2,
            HasCongestion = true,
            HasSoreThroat = true,
            SeverityScore = 5,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Recommended, result.Verdict);
        Assert.Equal("Intranasal (nebulizer)", result.DeliveryRoute);
        Assert.True(result.Constructs.Length == 2); // Both constructs for early
    }

    [Fact]
    public void Cold_LateOnset_NotRecommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "common-cold",
            SymptomOnsetHours = 72,
            TemperatureCelsius = 37.0,
            SeverityScore = 3,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.NotRecommended, result.Verdict);
    }

    [Fact]
    public void Cold_WithFever_Conditional()
    {
        var input = new TherapyInput
        {
            TherapyId = "common-cold",
            SymptomOnsetHours = 8,
            TemperatureCelsius = 38.5,
            SeverityScore = 7,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Conditional, result.Verdict);
        Assert.Contains(result.Warnings, w => w.Contains("Fever"));
    }

    [Fact]
    public void Flu_Early_Recommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "influenza",
            SymptomOnsetHours = 24,
            TemperatureCelsius = 39.0,
            SeverityScore = 7,
            VirusSubtype = "H1N1",
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Recommended, result.Verdict);
        Assert.Equal(Urgency.Immediate, result.Urgency); // Severe
    }

    [Fact]
    public void Covid_HighRisk_Recommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "covid19",
            Age = 70,
            SymptomOnsetHours = 48,
            VaccinationDoses = 3,
            DaysSinceLastVaccine = 300,
            Immunocompromised = false,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Recommended, result.Verdict); // Age >= 65
        Assert.Equal(Urgency.Immediate, result.Urgency);
    }

    [Fact]
    public void Cancer_NoBiopsy_NotRecommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "cancer-neoantigen",
            TumorType = "Melanoma",
            TumorBiopsyAvailable = false,
            CancerStage = "III",
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.NotRecommended, result.Verdict);
        Assert.Contains(result.Contraindications, c => c.Contains("biopsy"));
    }

    [Fact]
    public void Cancer_WithBiopsyAndHla_Recommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "cancer-neoantigen",
            TumorType = "Melanoma",
            TumorBiopsyAvailable = true,
            HlaType = "HLA-A*02:01",
            CancerStage = "II",
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Recommended, result.Verdict);
        Assert.Equal(9, result.Dose.Doses);
    }

    [Fact]
    public void Wound_ChronicDiabetic_Recommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "wound-healing",
            WoundType = "Diabetic ulcer",
            WoundAreaCm2 = 15,
            WoundAgeDays = 60,
            IsDiabetic = true,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Recommended, result.Verdict);
        Assert.Equal(150, result.Dose.AmountUg); // 10ug/cm2 * 15cm2
        Assert.Equal(2, result.Constructs.Length); // VEGF + FGF for chronic
    }

    [Fact]
    public void Wound_AcuteSmall_Conditional()
    {
        var input = new TherapyInput
        {
            TherapyId = "wound-healing",
            WoundType = "Surgical",
            WoundAreaCm2 = 5,
            WoundAgeDays = 10,
            IsDiabetic = false,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Conditional, result.Verdict);
        Assert.Single(result.Constructs); // VEGF only for acute
    }

    [Fact]
    public void Hair_EarlyLoss_Conditional()
    {
        var input = new TherapyInput
        {
            TherapyId = "hair-regrowth",
            Sex = Sex.Male,
            HairLossScale = 2,
            HairLossYears = 2,
            FamilyHistoryBaldness = true,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Conditional, result.Verdict);
        Assert.Equal(3, result.Constructs.Length); // WNT3A + SHH + Noggin
    }

    [Fact]
    public void Muscle_Sarcopenia_Recommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "muscle-loss",
            Age = 75,
            Sex = Sex.Male,
            GripStrengthKg = 20,  // below 27 threshold
            MuscleMassIndex = 6.5, // below 7.0 threshold
            PhysicalActivityLevel = 2,
            HasFalls = true,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Recommended, result.Verdict);
        Assert.Equal(Urgency.Within48Hours, result.Urgency); // Falls history
    }

    [Fact]
    public void Muscle_Normal_NotRecommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "muscle-loss",
            Sex = Sex.Male,
            GripStrengthKg = 40,
            MuscleMassIndex = 8.5,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.NotRecommended, result.Verdict);
    }

    [Fact]
    public void Allergy_WithAllergens_Conditional()
    {
        var input = new TherapyInput
        {
            TherapyId = "allergy",
            Allergens = ["Birch pollen", "Dust mite"],
            TotalIgE = 350,
            AllergySeverity = 6,
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.Conditional, result.Verdict);
        Assert.Equal(2, result.Constructs.Length); // Bet v 1 + Der p 1
    }

    [Fact]
    public void Allergy_NoAllergens_NotRecommended()
    {
        var input = new TherapyInput
        {
            TherapyId = "allergy",
            Allergens = [],
        };
        var result = TherapyScorer.Score(input);
        Assert.Equal(TherapyVerdict.NotRecommended, result.Verdict);
    }

    [Fact]
    public void AllTherapies_ScoreWithoutCrashing()
    {
        foreach (var therapy in TherapyCatalog.All)
        {
            var input = new TherapyInput { TherapyId = therapy.Id, Age = 50 };
            var result = TherapyScorer.Score(input);
            Assert.NotNull(result);
            Assert.NotEmpty(result.TherapyName);
            Assert.NotNull(result.Constructs);
            Assert.NotEmpty(result.DeliveryRoute);
        }
    }
}
