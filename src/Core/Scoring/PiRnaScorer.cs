namespace Core.Scoring;

using Models;

/// <summary>
/// Scores the 6-piRNA longevity panel from the Duke 2026 study.
/// Lower piRNA levels correlate with longer survival (86% accuracy for 2-year prediction).
/// Reference: Duke Health, Aging Cell, Feb 2026.
/// </summary>
public static class PiRnaScorer
{
    // Equal weights for the 6 piRNAs (model weights not yet public; using uniform)
    private static readonly double[] Weights = [1.0, 1.0, 1.0, 1.0, 1.0, 1.0];

    // Reference medians (RPM, approximate from healthy older adults)
    private static readonly double[] Medians = [50, 40, 60, 35, 45, 55];

    // Upper bounds for worst-case (high expression = bad prognosis)
    private static readonly double[] WorstCase = [200, 160, 240, 140, 180, 220];

    /// <summary>Score piRNA panel: 0-100 (100 = longevity-favorable, all piRNAs low).</summary>
    public static double Score(PiRnaPanel panel)
    {
        if (!panel.HasData) return -1;

        double totalWeight = 0;
        double weightedScore = 0;

        for (int i = 0; i < 6; i++)
        {
            if (!panel.Values[i].HasValue) continue;

            double val = panel.Values[i]!.Value;
            double median = Medians[i];
            double worst = WorstCase[i];

            // Lower = better: score 100 when at/below median, 0 at worst
            double score;
            if (val <= median)
                score = 100;
            else if (val >= worst)
                score = 0;
            else
                score = 100.0 * (worst - val) / (worst - median);

            weightedScore += score * Weights[i];
            totalWeight += Weights[i];
        }

        return totalWeight > 0 ? weightedScore / totalWeight : -1;
    }

    /// <summary>Generate flags for out-of-range piRNAs.</summary>
    public static string[] GetFlags(PiRnaPanel panel)
    {
        var flags = new List<string>();
        for (int i = 0; i < 6; i++)
        {
            if (panel.Values[i].HasValue && panel.Values[i]!.Value > Medians[i] * 2)
                flags.Add($"piRNA-{i + 1} elevated ({panel.Values[i]:F0} RPM, median {Medians[i]})");
        }
        return flags.ToArray();
    }
}
