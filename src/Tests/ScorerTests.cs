using Core.Models;
using Core.Scoring;
using Xunit;

namespace Tests;

public class PiRnaScorerTests
{
    [Fact]
    public void AllLow_ReturnsHighScore()
    {
        var panel = new PiRnaPanel { Values = [20, 15, 25, 10, 18, 22] };
        double score = PiRnaScorer.Score(panel);
        Assert.Equal(100, score);
    }

    [Fact]
    public void AllHigh_ReturnsLowScore()
    {
        var panel = new PiRnaPanel { Values = [200, 160, 240, 140, 180, 220] };
        double score = PiRnaScorer.Score(panel);
        Assert.Equal(0, score);
    }

    [Fact]
    public void NoData_ReturnsNegative()
    {
        var panel = new PiRnaPanel();
        Assert.Equal(-1, PiRnaScorer.Score(panel));
    }
}

public class MiRnaScorerTests
{
    [Fact]
    public void AllOptimal_Returns100()
    {
        var panel = new MiRnaPanel { MiR34a = 0.5, MiR21 = 0.5, MiR155 = 0.5, MiR15b5p = 2.0, MiR3735p = 2.0 };
        double score = MiRnaAnalyzer.Score(panel);
        Assert.Equal(100, score);
    }

    [Fact]
    public void Elevated_ReturnsLower()
    {
        var panel = new MiRnaPanel { MiR34a = 3.0, MiR21 = 3.0 };
        double score = MiRnaAnalyzer.Score(panel);
        Assert.InRange(score, 30, 70);
    }
}

public class SenescenceTests
{
    [Fact]
    public void BelowMedian_Returns100()
    {
        var panel = new SenescencePanel { P16Ink4a = 0.5, Il6 = 1.0, Gdf15 = 400 };
        double score = SenescenceEvaluator.Score(panel);
        Assert.Equal(100, score);
    }

    [Fact]
    public void HighBurden_ReturnsLow()
    {
        var panel = new SenescencePanel { P16Ink4a = 4.0, Il6 = 7.0, Gdf15 = 2500 };
        double score = SenescenceEvaluator.Score(panel);
        Assert.InRange(score, 0, 40);
    }
}

public class PathwayTests
{
    [Fact]
    public void OptimalPathways_ReturnsHigh()
    {
        var panel = new PathwayPanel { NadLevel = 50, Sirt1Expression = 2.0, AmpkActivity = 2.0 };
        double score = PathwayAnalyzer.Score(panel);
        Assert.True(score >= 80, $"Expected >= 80, got {score:F1}");
    }
}

public class TelomereTests
{
    [Fact]
    public void LongTelomeres_ReturnsHigh()
    {
        var panel = new TelomerePanel { LeukocyteTelomereLength = 10.0, TertExpression = 2.0 };
        double score = TelomereAssessor.Score(panel, 40, Sex.Male);
        Assert.True(score >= 50, $"Expected >= 50, got {score:F1}");
    }

    [Fact]
    public void ShortTelomeres_ReturnsLow()
    {
        var panel = new TelomerePanel { LeukocyteTelomereLength = 4.0 };
        double score = TelomereAssessor.Score(panel, 40, Sex.Male);
        Assert.InRange(score, 0, 40);
    }
}

public class ValidationTests
{
    [Fact]
    public void ValidInput_NoErrors()
    {
        var input = new BiomarkerInput();
        var errors = InputValidator.Validate(input);
        Assert.Empty(errors);
    }

    [Fact]
    public void InvalidAge_ReturnsError()
    {
        var input = new BiomarkerInput { Demographics = new() { ChronologicalAge = 200 } };
        var errors = InputValidator.Validate(input);
        Assert.Contains(errors, e => e.Field == "Age");
    }

    [Fact]
    public void InvalidBlood_ReturnsErrors()
    {
        var input = new BiomarkerInput { Blood = new() { Albumin = 0, Creatinine = 100 } };
        var errors = InputValidator.Validate(input);
        Assert.True(errors.Count >= 2);
    }
}
