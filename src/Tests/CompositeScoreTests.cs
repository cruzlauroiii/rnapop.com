using Core.Models;
using Core.Scoring;
using Xunit;

namespace Tests;

public class CompositeScoreTests
{
    [Fact]
    public void HealthyInput_ReturnsPass()
    {
        var input = new BiomarkerInput
        {
            Demographics = new() { ChronologicalAge = 45, Sex = Sex.Female },
            Blood = new()
            {
                Albumin = 4.5, AlkalinePhosphatase = 60, Creatinine = 0.9,
                CReactiveProtein = 0.3, FastingGlucose = 85, WhiteBloodCellCount = 5.5,
                LymphocytePercent = 35, MeanCellVolume = 88, RedCellDistWidth = 12.5,
            },
            PiRna = new() { Values = [30, 25, 35, 20, 28, 32] },
            MiRna = new() { MiR34a = 0.8, MiR21 = 0.7, MiR155 = 0.9, MiR15b5p = 1.5, MiR3735p = 1.8 },
            Senescence = new() { P16Ink4a = 0.8, Gdf15 = 400, Il6 = 1.0 },
            Pathway = new() { NadLevel = 45, Sirt1Expression = 1.8, AmpkActivity = 1.9 },
            Telomere = new() { LeukocyteTelomereLength = 8.5, TertExpression = 1.5 },
        };

        var result = CompositeScorer.Score(input);

        Assert.Equal(Verdict.Pass, result.Verdict);
        Assert.True(result.CompositeScore >= 60);
        Assert.Equal(6, result.Pillars.Length);
        Assert.Equal(3, result.Protocols.Length);
        Assert.True(result.BiologicalAge < 45);
    }

    [Fact]
    public void UnhealthyInput_ReturnsFail()
    {
        var input = new BiomarkerInput
        {
            Demographics = new() { ChronologicalAge = 55, Sex = Sex.Male, SmokingPackYears = 20 },
            Blood = new()
            {
                Albumin = 3.2, AlkalinePhosphatase = 130, Creatinine = 1.4,
                CReactiveProtein = 4.5, FastingGlucose = 125, WhiteBloodCellCount = 9.5,
                LymphocytePercent = 18, MeanCellVolume = 98, RedCellDistWidth = 16,
            },
            Senescence = new() { P16Ink4a = 3.5, Gdf15 = 2000, Il6 = 8.0, Serpine1 = 65 },
            Pathway = new() { NadLevel = 12, Sirt1Expression = 0.4, AmpkActivity = 0.3 },
        };

        var result = CompositeScorer.Score(input);

        Assert.Equal(Verdict.Fail, result.Verdict);
        Assert.True(result.BiologicalAge > 55);
        Assert.NotEmpty(result.Recommendations);
    }

    [Fact]
    public void BloodOnly_StillScores()
    {
        var input = new BiomarkerInput
        {
            Demographics = new() { ChronologicalAge = 40 },
            Blood = new()
            {
                Albumin = 4.2, AlkalinePhosphatase = 70, Creatinine = 1.0,
                CReactiveProtein = 0.5, FastingGlucose = 90, WhiteBloodCellCount = 6,
                LymphocytePercent = 30, MeanCellVolume = 90, RedCellDistWidth = 13,
            },
        };

        var result = CompositeScorer.Score(input);

        Assert.True(result.CompositeScore > 0);
        // PhenoAge pillar should have data, others should not
        Assert.True(result.Pillars[0].HasData);
        Assert.False(result.Pillars[1].HasData);
        // PhenoAge should carry 100% effective weight
        Assert.Equal(1.0, result.Pillars[0].EffectiveWeight, 2);
    }

    [Fact]
    public void Protocols_HealthyInput_OskEligible()
    {
        var input = new BiomarkerInput
        {
            Demographics = new() { ChronologicalAge = 45 },
            Blood = new()
            {
                Albumin = 4.5, AlkalinePhosphatase = 60, Creatinine = 0.9,
                CReactiveProtein = 0.3, FastingGlucose = 85, WhiteBloodCellCount = 5.5,
                LymphocytePercent = 35, MeanCellVolume = 88, RedCellDistWidth = 12.5,
            },
            Senescence = new() { P16Ink4a = 0.8, Il6 = 1.0 },
            Pathway = new() { NadLevel = 40, Sirt1Expression = 1.5, AmpkActivity = 1.5 },
        };

        var result = CompositeScorer.Score(input);
        var osk = result.Protocols[0]; // OSK
        var sb000 = result.Protocols[2]; // SB000

        Assert.Equal(ProtocolEligibility.Eligible, osk.Eligibility);
        Assert.Equal(ProtocolEligibility.Eligible, sb000.Eligibility);
    }

    [Fact]
    public void AgeDelta_Negative_MeansYounger()
    {
        var input = new BiomarkerInput
        {
            Demographics = new() { ChronologicalAge = 50 },
            Blood = new()
            {
                Albumin = 4.5, AlkalinePhosphatase = 55, Creatinine = 0.8,
                CReactiveProtein = 0.2, FastingGlucose = 80, WhiteBloodCellCount = 5,
                LymphocytePercent = 38, MeanCellVolume = 85, RedCellDistWidth = 12,
            },
        };

        var result = CompositeScorer.Score(input);
        Assert.True(result.AgeDelta < 0, $"Expected negative AgeDelta for healthy 50yo, got {result.AgeDelta}");
    }
}
