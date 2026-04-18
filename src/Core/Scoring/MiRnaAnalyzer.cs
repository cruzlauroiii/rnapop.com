namespace Core.Scoring;

using Models;

/// <summary>
/// miRNA aging signature scorer.
/// miR-34a: senescence/brain aging (higher = worse)
/// miR-21: inflammaging (higher = worse)
/// miR-155: metabolic dysregulation (higher = worse)
/// miR-15b-5p: downregulated in long-lived (lower = worse)
/// miR-373-5p: upregulated in long-lived (higher = better)
/// </summary>
public static class MiRnaAnalyzer
{
    // Reference medians (relative expression units, normalized to healthy 40-year-old)
    private const double Ref_MiR34a = 1.0;
    private const double Ref_MiR21 = 1.0;
    private const double Ref_MiR155 = 1.0;
    private const double Ref_MiR15b = 1.0;
    private const double Ref_MiR373 = 1.0;

    // Deviation thresholds for worst score
    private const double MaxDeviation = 5.0;

    public static double Score(MiRnaPanel panel)
    {
        if (!panel.HasData) return -1;

        double total = 0;
        int count = 0;

        // Aging markers: lower is better
        if (panel.MiR34a.HasValue)
        {
            total += ScoreLowerBetter(panel.MiR34a.Value, Ref_MiR34a);
            count++;
        }
        if (panel.MiR21.HasValue)
        {
            total += ScoreLowerBetter(panel.MiR21.Value, Ref_MiR21);
            count++;
        }
        if (panel.MiR155.HasValue)
        {
            total += ScoreLowerBetter(panel.MiR155.Value, Ref_MiR155);
            count++;
        }

        // Longevity markers: higher is better
        if (panel.MiR15b5p.HasValue)
        {
            total += ScoreHigherBetter(panel.MiR15b5p.Value, Ref_MiR15b);
            count++;
        }
        if (panel.MiR3735p.HasValue)
        {
            total += ScoreHigherBetter(panel.MiR3735p.Value, Ref_MiR373);
            count++;
        }

        return count > 0 ? total / count : -1;
    }

    public static string[] GetFlags(MiRnaPanel panel)
    {
        var flags = new List<string>();
        if (panel.MiR34a.HasValue && panel.MiR34a.Value > 2.0)
            flags.Add($"miR-34a elevated ({panel.MiR34a:F2}x) — senescence/brain aging marker");
        if (panel.MiR21.HasValue && panel.MiR21.Value > 2.0)
            flags.Add($"miR-21 elevated ({panel.MiR21:F2}x) — chronic inflammation marker");
        if (panel.MiR15b5p.HasValue && panel.MiR15b5p.Value < 0.5)
            flags.Add($"miR-15b-5p low ({panel.MiR15b5p:F2}x) — associated with shorter lifespan");
        return flags.ToArray();
    }

    private static double ScoreLowerBetter(double value, double reference)
    {
        double ratio = value / reference;
        if (ratio <= 1.0) return 100;
        if (ratio >= MaxDeviation) return 0;
        return 100.0 * (MaxDeviation - ratio) / (MaxDeviation - 1.0);
    }

    private static double ScoreHigherBetter(double value, double reference)
    {
        double ratio = value / reference;
        if (ratio >= 1.0) return 100;
        if (ratio <= 1.0 / MaxDeviation) return 0;
        return 100.0 * (ratio - 1.0 / MaxDeviation) / (1.0 - 1.0 / MaxDeviation);
    }
}
