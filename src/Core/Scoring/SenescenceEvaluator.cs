namespace Core.Scoring;

using Models;
using Reference;

/// <summary>
/// Evaluates cellular senescence burden via SASP (senescence-associated secretory phenotype) markers.
/// Higher SASP = more senescent cells = worse aging profile.
/// Key markers: p16^INK4a, GDF15, IL-6, TNF-a, SERPINE1, Activin A, FGF21, TIMP-1.
/// </summary>
public static class SenescenceEvaluator
{
    private static readonly (Func<SenescencePanel, double?> Get, double Median, double Weight)[] Markers =
    [
        (p => p.P16Ink4a,  ReferenceRanges.P16Median,       0.20),
        (p => p.Gdf15,     ReferenceRanges.Gdf15Median,     0.15),
        (p => p.Il6,       ReferenceRanges.Il6Median,        0.15),
        (p => p.TnfAlpha,  ReferenceRanges.TnfAlphaMedian,  0.10),
        (p => p.Serpine1,   ReferenceRanges.Serpine1Median,  0.15),
        (p => p.ActivinA,  ReferenceRanges.ActivinAMedian,  0.05),
        (p => p.Fgf21,     ReferenceRanges.Fgf21Median,     0.10),
        (p => p.Timp1,     ReferenceRanges.Timp1Median,      0.10),
    ];

    public static double Score(SenescencePanel panel)
    {
        if (!panel.HasData) return -1;

        double totalWeight = 0;
        double weighted = 0;

        foreach (var (get, median, weight) in Markers)
        {
            double? val = get(panel);
            if (!val.HasValue) continue;
            weighted += ReferenceRanges.NormalizeSasp(val.Value, median) * weight;
            totalWeight += weight;
        }

        return totalWeight > 0 ? weighted / totalWeight : -1;
    }

    public static string[] GetFlags(SenescencePanel panel)
    {
        var flags = new List<string>();
        if (panel.P16Ink4a.HasValue && panel.P16Ink4a.Value > 2.0)
            flags.Add($"p16^INK4a very high ({panel.P16Ink4a:F1}x) — significant senescent cell burden");
        if (panel.Il6.HasValue && panel.Il6.Value > 5.0)
            flags.Add($"IL-6 elevated ({panel.Il6:F1} pg/mL) — chronic inflammation");
        if (panel.Gdf15.HasValue && panel.Gdf15.Value > 1500)
            flags.Add($"GDF15 elevated ({panel.Gdf15:F0} pg/mL) — mitochondrial stress / senescence");
        if (panel.Serpine1.HasValue && panel.Serpine1.Value > 60)
            flags.Add($"PAI-1/SERPINE1 elevated ({panel.Serpine1:F0} ng/mL) — thrombotic & aging risk");
        return flags.ToArray();
    }

    /// <summary>Returns true if senescence burden is in critical zone (>80th percentile).</summary>
    public static bool IsCritical(SenescencePanel panel)
    {
        double score = Score(panel);
        return score >= 0 && score < 20; // below 20/100 = critical
    }
}
