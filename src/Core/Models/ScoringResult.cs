namespace Core.Models;

public enum Verdict { Pass, Fail }
public enum ProtocolEligibility { Eligible, Borderline, NotEligible }

public sealed class PillarScore
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public double Score { get; init; }       // 0-100  (100 = youthful)
    public double Weight { get; init; }      // original weight
    public double EffectiveWeight { get; init; } // after redistribution
    public bool HasData { get; init; }
    public string[] Flags { get; init; } = [];
}

public sealed class RejuvenationProtocol
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public ProtocolEligibility Eligibility { get; init; }
    public string[] Blockers { get; init; } = [];
}

public sealed class ScoringResult
{
    public double CompositeScore { get; init; }      // 0-100
    public double BiologicalAge { get; init; }       // PhenoAge years
    public int ChronologicalAge { get; init; }
    public double AgeDelta { get; init; }            // Bio - Chrono (negative = younger)
    public Verdict Verdict { get; init; }
    public string VerdictReason { get; init; } = "";
    public PillarScore[] Pillars { get; init; } = [];
    public RejuvenationProtocol[] Protocols { get; init; } = [];
    public string[] Recommendations { get; init; } = [];
}
