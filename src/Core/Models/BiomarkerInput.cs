namespace Core.Models;

public enum Sex { Male, Female }
public enum Species { Human, Dog, Cat, Horse, Other }

public sealed class Demographics
{
    public int ChronologicalAge { get; set; } = 40;
    public Sex Sex { get; set; } = Sex.Male;
    public double SmokingPackYears { get; set; }
}

/// <summary>PhenoAge 9 blood biomarkers (required).</summary>
public sealed class BloodPanel
{
    public double Albumin { get; set; } = 4.0;          // g/dL
    public double AlkalinePhosphatase { get; set; } = 70; // U/L
    public double Creatinine { get; set; } = 1.0;        // mg/dL
    public double CReactiveProtein { get; set; } = 0.5;  // mg/dL  (log-transformed in calc)
    public double FastingGlucose { get; set; } = 90;     // mg/dL
    public double WhiteBloodCellCount { get; set; } = 6;  // 10^3/uL
    public double LymphocytePercent { get; set; } = 30;   // %
    public double MeanCellVolume { get; set; } = 90;      // fL
    public double RedCellDistWidth { get; set; } = 13;    // %
}

/// <summary>6 piRNAs from Duke 2026 study — lower = better longevity.</summary>
public sealed class PiRnaPanel
{
    public double?[] Values { get; set; } = new double?[6];
    public bool HasData => Values.Any(v => v.HasValue);
}

/// <summary>miRNA aging signature panel.</summary>
public sealed class MiRnaPanel
{
    public double? MiR34a { get; set; }   // senescence / brain aging
    public double? MiR21 { get; set; }    // inflammaging
    public double? MiR155 { get; set; }   // metabolic regulation
    public double? MiR15b5p { get; set; } // downregulated in long-lived
    public double? MiR3735p { get; set; } // upregulated in long-lived
    public bool HasData => MiR34a.HasValue || MiR21.HasValue || MiR155.HasValue
                        || MiR15b5p.HasValue || MiR3735p.HasValue;
}

/// <summary>Senescence-associated secretory phenotype (SASP) markers.</summary>
public sealed class SenescencePanel
{
    public double? P16Ink4a { get; set; }   // T-cell expression (relative)
    public double? Gdf15 { get; set; }      // pg/mL
    public double? Il6 { get; set; }        // pg/mL
    public double? TnfAlpha { get; set; }   // pg/mL
    public double? Serpine1 { get; set; }   // ng/mL  (PAI-1)
    public double? ActivinA { get; set; }   // pg/mL
    public double? Fgf21 { get; set; }      // pg/mL
    public double? Timp1 { get; set; }      // ng/mL
    public bool HasData => P16Ink4a.HasValue || Gdf15.HasValue || Il6.HasValue
                        || TnfAlpha.HasValue || Serpine1.HasValue || ActivinA.HasValue
                        || Fgf21.HasValue || Timp1.HasValue;
}

/// <summary>Longevity pathway markers — NAD+/SIRT/AMPK/mTOR.</summary>
public sealed class PathwayPanel
{
    public double? NadLevel { get; set; }       // uM
    public double? NadNadhRatio { get; set; }   // ratio
    public double? Sirt1Expression { get; set; } // relative
    public double? Sirt6Expression { get; set; } // relative
    public double? AmpkActivity { get; set; }    // relative
    public bool HasData => NadLevel.HasValue || NadNadhRatio.HasValue
                        || Sirt1Expression.HasValue || Sirt6Expression.HasValue
                        || AmpkActivity.HasValue;
}

/// <summary>Telomere health panel.</summary>
public sealed class TelomerePanel
{
    public double? LeukocyteTelomereLength { get; set; } // kb
    public double? TertExpression { get; set; }           // relative
    public double? TercLevel { get; set; }                // relative
    public bool HasData => LeukocyteTelomereLength.HasValue || TertExpression.HasValue
                        || TercLevel.HasValue;
}

/// <summary>All biomarker inputs combined.</summary>
public sealed class BiomarkerInput
{
    public Demographics Demographics { get; set; } = new();
    public BloodPanel Blood { get; set; } = new();
    public PiRnaPanel PiRna { get; set; } = new();
    public MiRnaPanel MiRna { get; set; } = new();
    public SenescencePanel Senescence { get; set; } = new();
    public PathwayPanel Pathway { get; set; } = new();
    public TelomerePanel Telomere { get; set; } = new();
}
