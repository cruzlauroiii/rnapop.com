namespace Core.Models;

/// <summary>
/// In Vitro Transcription (IVT) protocol steps for mRNA synthesis.
/// Automates the full pipeline: template prep → IVT → capping → poly-A → purification → LNP.
/// </summary>
public sealed class IvtProtocol
{
    public string Name { get; set; } = "OSK mRNA Synthesis";
    public IvtStep[] Steps { get; set; } = DefaultOskProtocol();

    public static IvtStep[] DefaultOskProtocol() =>
    [
        new()
        {
            Order = 1,
            Name = "Template Linearization",
            Description = "Digest plasmid with restriction enzyme downstream of poly-A signal",
            Equipment = EquipmentType.HeatBlock,
            Temperature = 37,
            DurationMinutes = 60,
            Notes = "Use 1ug plasmid + appropriate RE in 20uL reaction",
        },
        new()
        {
            Order = 2,
            Name = "Template Purification",
            Description = "Column-purify linearized template, elute in RNase-free water",
            Equipment = EquipmentType.Centrifuge,
            TargetRpm = 8000,
            DurationMinutes = 5,
            Notes = "Use Monarch PCR & DNA Cleanup Kit or equivalent",
        },
        new()
        {
            Order = 3,
            Name = "QC: Template Concentration",
            Description = "Measure A260 to confirm template concentration",
            Equipment = EquipmentType.Spectrophotometer,
            Notes = "Target: 50-200 ng/uL, A260/280 > 1.8",
        },
        new()
        {
            Order = 4,
            Name = "In Vitro Transcription",
            Description = "T7 IVT with CleanCap AG and N1-methylpseudouridine-5'-TP",
            Equipment = EquipmentType.HeatBlock,
            Temperature = 37,
            DurationMinutes = 120,
            Notes = "HiScribe T7 kit: 2uL 10x buffer, 1ug template, NTPs (replace UTP with m1Ψ-UTP), T7 polymerase, CleanCap AG, RNase-free H2O to 20uL",
        },
        new()
        {
            Order = 5,
            Name = "DNase Treatment",
            Description = "Remove DNA template with DNase I",
            Equipment = EquipmentType.HeatBlock,
            Temperature = 37,
            DurationMinutes = 15,
            Notes = "Add 1uL DNase I, incubate 15 min at 37C",
        },
        new()
        {
            Order = 6,
            Name = "mRNA Purification",
            Description = "Column-purify mRNA, remove unincorporated NTPs and enzymes",
            Equipment = EquipmentType.Centrifuge,
            TargetRpm = 8000,
            DurationMinutes = 5,
            Notes = "Use Monarch RNA Cleanup Kit. Elute in 50uL RNase-free water.",
        },
        new()
        {
            Order = 7,
            Name = "QC: mRNA Yield & Purity",
            Description = "Measure RNA concentration and purity by UV spectrophotometry",
            Equipment = EquipmentType.Spectrophotometer,
            Notes = "Target: 100-180 ug per reaction. A260/280 ~2.0 = pure RNA.",
        },
        new()
        {
            Order = 8,
            Name = "QC: Gel Electrophoresis",
            Description = "Verify mRNA size and integrity on agarose gel",
            Equipment = EquipmentType.GelElectrophoresis,
            Voltage = 100,
            DurationMinutes = 30,
            Notes = "1% agarose/TAE, GelRed stain, 100V 30min. Expected band: OCT4 ~1.1kb, SOX2 ~0.96kb, KLF4 ~1.4kb",
        },
        new()
        {
            Order = 9,
            Name = "LNP Encapsulation",
            Description = "Microfluidic mixing of mRNA with lipid nanoparticle components",
            Equipment = EquipmentType.SyringePump,
            Notes = "Pump A: mRNA in citrate buffer pH 4.0 (3 parts). Pump B: lipids in ethanol (1 part). SM-102:DSPC:Chol:PEG-lipid = 50:10:38.5:1.5 molar. Flow rate: 12 mL/min total. Collect in PBS.",
        },
        new()
        {
            Order = 10,
            Name = "Buffer Exchange",
            Description = "Dialyze LNP-mRNA against PBS to remove ethanol",
            Equipment = EquipmentType.None,
            DurationMinutes = 120,
            Notes = "Use 10kDa MWCO dialysis cassette in 1L PBS, change PBS at 60min. Or use centrifugal filter.",
        },
        new()
        {
            Order = 11,
            Name = "QC: Final LNP-mRNA",
            Description = "Measure final concentration, encapsulation efficiency",
            Equipment = EquipmentType.Spectrophotometer,
            Notes = "Disrupt LNPs with 1% Triton X-100, measure A260. Compare to pre-encapsulation to calculate efficiency. Target >80%.",
        },
        new()
        {
            Order = 12,
            Name = "Storage",
            Description = "Aliquot and store at -80C",
            Equipment = EquipmentType.None,
            Notes = "Aliquot into single-use volumes. Stable 6+ months at -80C. Do not freeze-thaw.",
        },
    ];
}

public sealed class IvtStep
{
    public int Order { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public EquipmentType Equipment { get; set; }
    public double Temperature { get; set; }
    public int DurationMinutes { get; set; }
    public int TargetRpm { get; set; }
    public double Voltage { get; set; }
    public string Notes { get; set; } = "";
    public bool Completed { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum EquipmentType
{
    None,
    HeatBlock,
    Centrifuge,
    Spectrophotometer,
    GelElectrophoresis,
    SyringePump,
}
