using Core.Models;
using Core.Scoring;
using Xunit;

namespace Tests;

public class PhenoAgeTests
{
    [Fact]
    public void HealthyYoungAdult_PhenoAge_BelowChronological()
    {
        var blood = new BloodPanel
        {
            Albumin = 4.5,
            AlkalinePhosphatase = 60,
            Creatinine = 0.9,
            CReactiveProtein = 0.3,
            FastingGlucose = 85,
            WhiteBloodCellCount = 5.5,
            LymphocytePercent = 35,
            MeanCellVolume = 88,
            RedCellDistWidth = 12.5,
        };

        var (phenoAge, mortality, _) = PhenoAgeCalculator.Calculate(blood, 35);

        Assert.InRange(phenoAge, 20, 40);
        Assert.InRange(mortality, 0, 0.5);
    }

    [Fact]
    public void UnhealthyOlder_PhenoAge_AboveChronological()
    {
        var blood = new BloodPanel
        {
            Albumin = 3.2,
            AlkalinePhosphatase = 130,
            Creatinine = 1.4,
            CReactiveProtein = 4.5,
            FastingGlucose = 125,
            WhiteBloodCellCount = 9.5,
            LymphocytePercent = 18,
            MeanCellVolume = 98,
            RedCellDistWidth = 16,
        };

        var (phenoAge, _, _) = PhenoAgeCalculator.Calculate(blood, 55);

        Assert.True(phenoAge > 55, $"Expected PhenoAge > 55, got {phenoAge:F1}");
    }

    [Fact]
    public void Score_YoungerBio_ReturnsHighScore()
    {
        double score = PhenoAgeCalculator.Score(30, 40); // 10 years younger
        Assert.True(score > 60, $"Expected score > 60 for -10yr delta, got {score:F1}");
    }

    [Fact]
    public void Score_OlderBio_ReturnsLowScore()
    {
        double score = PhenoAgeCalculator.Score(60, 40); // 20 years older
        Assert.True(score < 30, $"Expected score < 30 for +20yr delta, got {score:F1}");
    }

    [Fact]
    public void LinearPredictor_Deterministic()
    {
        var blood = new BloodPanel();
        double xb1 = PhenoAgeCalculator.CalculateLinearPredictor(blood, 50);
        double xb2 = PhenoAgeCalculator.CalculateLinearPredictor(blood, 50);
        Assert.Equal(xb1, xb2);
    }
}
