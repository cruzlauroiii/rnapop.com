namespace Core.Scoring;

using Models;
using Reference;

/// <summary>
/// Evaluates longevity pathway health: NAD+/SIRT1/SIRT6/AMPK/mTOR axis.
/// AMPK + SIRT activate each other, both inhibit mTOR.
/// Higher NAD+, SIRT1, SIRT6, AMPK = better. Overactive mTOR = worse.
/// </summary>
public static class PathwayAnalyzer
{
    public static double Score(PathwayPanel panel)
    {
        if (!panel.HasData) return -1;

        double total = 0;
        double weightSum = 0;

        // NAD+ level: optimal ~40uM, low <10, high is okay
        if (panel.NadLevel.HasValue)
        {
            double s = panel.NadLevel.Value >= ReferenceRanges.NadRange.Optimal ? 100
                : ReferenceRanges.NormalizeToOptimal(panel.NadLevel.Value,
                    ReferenceRanges.NadRange.Optimal, ReferenceRanges.NadRange.Optimal - ReferenceRanges.NadRange.Low);
            total += s * 0.20;
            weightSum += 0.20;
        }

        // NAD+/NADH ratio: optimal >= 3.0
        if (panel.NadNadhRatio.HasValue)
        {
            double s = panel.NadNadhRatio.Value >= ReferenceRanges.NadRatio.Optimal ? 100
                : ReferenceRanges.NormalizeToOptimal(panel.NadNadhRatio.Value,
                    ReferenceRanges.NadRatio.Optimal, ReferenceRanges.NadRatio.Optimal - ReferenceRanges.NadRatio.Low);
            total += s * 0.15;
            weightSum += 0.15;
        }

        // SIRT1: higher = better (relative expression, 1.0 = normal)
        if (panel.Sirt1Expression.HasValue)
        {
            double s = Math.Min(100, panel.Sirt1Expression.Value * 50); // 2.0x = 100
            total += s * 0.20;
            weightSum += 0.20;
        }

        // SIRT6: higher = better
        if (panel.Sirt6Expression.HasValue)
        {
            double s = Math.Min(100, panel.Sirt6Expression.Value * 50);
            total += s * 0.20;
            weightSum += 0.20;
        }

        // AMPK: higher = better
        if (panel.AmpkActivity.HasValue)
        {
            double s = Math.Min(100, panel.AmpkActivity.Value * 50);
            total += s * 0.25;
            weightSum += 0.25;
        }

        return weightSum > 0 ? total / weightSum : -1;
    }

    public static string[] GetFlags(PathwayPanel panel)
    {
        var flags = new List<string>();
        if (panel.NadLevel.HasValue && panel.NadLevel.Value < 15)
            flags.Add($"NAD+ critically low ({panel.NadLevel:F0} uM) — consider NMN/NR supplementation");
        if (panel.Sirt1Expression.HasValue && panel.Sirt1Expression.Value < 0.5)
            flags.Add($"SIRT1 expression low ({panel.Sirt1Expression:F2}x) — impaired DNA repair / metabolism");
        if (panel.AmpkActivity.HasValue && panel.AmpkActivity.Value < 0.5)
            flags.Add($"AMPK activity low ({panel.AmpkActivity:F2}x) — energy sensing impaired");
        return flags.ToArray();
    }
}
