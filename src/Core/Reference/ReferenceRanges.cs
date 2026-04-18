namespace Core.Reference;

using Models;

/// <summary>Age/sex-stratified reference ranges for normalization.</summary>
public static class ReferenceRanges
{
    // PhenoAge biomarker optimal ranges
    public static readonly (double Min, double Max) Albumin = (3.5, 5.5);
    public static readonly (double Min, double Max) Alp = (44, 147);
    public static readonly (double Min, double Max) Creatinine = (0.7, 1.3);
    public static readonly (double Min, double Max) Crp = (0.0, 1.0);
    public static readonly (double Min, double Max) Glucose = (70, 100);
    public static readonly (double Min, double Max) Wbc = (4.5, 11.0);
    public static readonly (double Min, double Max) Lymphocyte = (20, 40);
    public static readonly (double Min, double Max) Mcv = (80, 100);
    public static readonly (double Min, double Max) Rdw = (11.5, 14.5);

    // SASP reference medians (healthy adults, approximate pg/mL or ng/mL)
    public static readonly double Gdf15Median = 500;       // pg/mL
    public static readonly double Il6Median = 1.5;          // pg/mL
    public static readonly double TnfAlphaMedian = 1.0;     // pg/mL
    public static readonly double Serpine1Median = 20;       // ng/mL
    public static readonly double ActivinAMedian = 300;      // pg/mL
    public static readonly double Fgf21Median = 200;         // pg/mL
    public static readonly double Timp1Median = 150;         // ng/mL
    public static readonly double P16Median = 1.0;           // relative expression

    // Telomere length by age (kb, approximate population medians)
    public static double ExpectedTelomereLength(int age, Sex sex)
    {
        // Approximate linear decline: ~11kb at birth, losing ~0.04kb/year
        double baseline = sex == Sex.Female ? 11.2 : 11.0;
        return Math.Max(3.0, baseline - 0.04 * age);
    }

    // NAD+ reference (uM in whole blood)
    public static readonly (double Low, double Optimal, double High) NadRange = (10, 40, 80);
    public static readonly (double Low, double Optimal) NadRatio = (1.0, 3.0);

    /// <summary>Normalize value to 0-100 where 100 = optimal.</summary>
    public static double NormalizeToRange(double value, double min, double max, bool lowerIsBetter = false)
    {
        if (lowerIsBetter)
        {
            if (value <= min) return 100;
            if (value >= max) return 0;
            return 100.0 * (max - value) / (max - min);
        }
        else
        {
            double clamped = Math.Clamp(value, min, max);
            double mid = (min + max) / 2.0;
            double dist = Math.Abs(clamped - mid);
            double halfRange = (max - min) / 2.0;
            return 100.0 * (1.0 - dist / halfRange);
        }
    }

    /// <summary>Normalize where closer to optimal value = 100.</summary>
    public static double NormalizeToOptimal(double value, double optimal, double worstDeviation)
    {
        double dist = Math.Abs(value - optimal);
        return Math.Max(0, 100.0 * (1.0 - dist / worstDeviation));
    }

    /// <summary>Normalize SASP marker: ratio vs median. Lower = better. Score = 100 when at/below median.</summary>
    public static double NormalizeSasp(double value, double median)
    {
        double ratio = value / median;
        if (ratio <= 1.0) return 100;
        if (ratio >= 5.0) return 0; // 5x median = worst
        return 100.0 * (5.0 - ratio) / 4.0;
    }
}
