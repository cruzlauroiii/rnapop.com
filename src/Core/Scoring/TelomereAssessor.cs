namespace Core.Scoring;

using Models;
using Reference;

/// <summary>
/// Telomere health assessment: leukocyte telomere length (LTL), TERT expression, TERC level.
/// Longer telomeres for age = better replicative capacity.
/// </summary>
public static class TelomereAssessor
{
    public static double Score(TelomerePanel panel, int age, Sex sex)
    {
        if (!panel.HasData) return -1;

        double total = 0;
        double weightSum = 0;

        // LTL: compare to expected for age/sex
        if (panel.LeukocyteTelomereLength.HasValue)
        {
            double expected = ReferenceRanges.ExpectedTelomereLength(age, sex);
            double ratio = panel.LeukocyteTelomereLength.Value / expected;
            // ratio > 1.0 = longer than expected = good
            double s = Math.Clamp(ratio * 50, 0, 100); // 2.0x = 100
            total += s * 0.60;
            weightSum += 0.60;
        }

        // TERT expression: higher = better (telomerase activity)
        if (panel.TertExpression.HasValue)
        {
            double s = Math.Min(100, panel.TertExpression.Value * 50);
            total += s * 0.20;
            weightSum += 0.20;
        }

        // TERC level: higher = better (telomerase RNA component)
        if (panel.TercLevel.HasValue)
        {
            double s = Math.Min(100, panel.TercLevel.Value * 50);
            total += s * 0.20;
            weightSum += 0.20;
        }

        return weightSum > 0 ? total / weightSum : -1;
    }

    public static string[] GetFlags(TelomerePanel panel, int age, Sex sex)
    {
        var flags = new List<string>();
        if (panel.LeukocyteTelomereLength.HasValue)
        {
            double expected = ReferenceRanges.ExpectedTelomereLength(age, sex);
            if (panel.LeukocyteTelomereLength.Value < expected * 0.7)
                flags.Add($"LTL critically short ({panel.LeukocyteTelomereLength:F1} kb vs {expected:F1} kb expected)");
        }
        if (panel.TertExpression.HasValue && panel.TertExpression.Value < 0.3)
            flags.Add($"TERT expression very low ({panel.TertExpression:F2}x) — telomerase activity impaired");
        return flags.ToArray();
    }
}
