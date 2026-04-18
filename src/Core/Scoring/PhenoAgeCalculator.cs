namespace Core.Scoring;

using Models;

/// <summary>
/// Levine PhenoAge algorithm (2018).
/// Coefficients from Table S6, Levine et al. "An epigenetic biomarker of aging for lifespan and healthspan."
/// Aging (Albany NY). 2018;10(4):573-591.
/// </summary>
public static class PhenoAgeCalculator
{
    // Gompertz mortality model coefficients from NHANES III
    private const double Intercept = -19.9067;
    private const double B_Albumin = -0.0336;
    private const double B_Creatinine = 0.0095;
    private const double B_Glucose = 0.1953;
    private const double B_LnCrp = 0.0954;
    private const double B_Lymphocyte = -0.0120;
    private const double B_Mcv = 0.0268;
    private const double B_Rdw = 0.3306;
    private const double B_Alp = 0.00188;
    private const double B_Wbc = 0.0554;
    private const double B_Age = 0.0804;

    // Gompertz parameters
    private const double Gamma = 0.0076927;
    private const double ConversionConstant = 141.50225;
    private const double ScalingFactor = 0.090165;

    /// <summary>Calculate the linear predictor xb from blood biomarkers.</summary>
    public static double CalculateLinearPredictor(BloodPanel blood, int age)
    {
        double lnCrp = Math.Log(Math.Max(0.001, blood.CReactiveProtein));

        return Intercept
            + B_Albumin * blood.Albumin
            + B_Creatinine * blood.Creatinine
            + B_Glucose * blood.FastingGlucose
            + B_LnCrp * lnCrp
            + B_Lymphocyte * blood.LymphocytePercent
            + B_Mcv * blood.MeanCellVolume
            + B_Rdw * blood.RedCellDistWidth
            + B_Alp * blood.AlkalinePhosphatase
            + B_Wbc * blood.WhiteBloodCellCount
            + B_Age * age;
    }

    /// <summary>Calculate mortality score from linear predictor.</summary>
    public static double CalculateMortalityScore(double xb)
    {
        // 10-year mortality probability
        double hazard = Math.Exp(xb);
        double cumHazard = hazard * (Math.Exp(120 * Gamma) - 1) / Gamma;
        return 1.0 - Math.Exp(-cumHazard);
    }

    /// <summary>Convert mortality score to PhenoAge (biological age in years).</summary>
    public static double CalculatePhenoAge(double mortalityScore)
    {
        // Clamp to avoid log of zero/negative
        double clamped = Math.Clamp(mortalityScore, 1e-10, 1.0 - 1e-10);
        return ConversionConstant + Math.Log(-0.00553 * Math.Log(1.0 - clamped)) / ScalingFactor;
    }

    /// <summary>Full PhenoAge calculation from inputs.</summary>
    public static (double PhenoAge, double MortalityScore, double LinearPredictor) Calculate(BloodPanel blood, int age)
    {
        double xb = CalculateLinearPredictor(blood, age);
        double mortality = CalculateMortalityScore(xb);
        double phenoAge = CalculatePhenoAge(mortality);
        return (phenoAge, mortality, xb);
    }

    /// <summary>Score PhenoAge as 0-100 (100 = biologically younger than chronological age).</summary>
    public static double Score(double phenoAge, int chronologicalAge)
    {
        double delta = phenoAge - chronologicalAge;
        // delta of -20 = score 100, delta of +20 = score 0
        return Math.Clamp(100.0 * (1.0 - delta / 20.0) / 2.0 + 50.0, 0, 100);
    }
}
