namespace Core.Models;

public enum TherapyVerdict { Recommended, Conditional, NotRecommended }
public enum Urgency { Immediate, Within24Hours, Within48Hours, Scheduled, Elective }

public sealed class TherapyResult
{
    public required string TherapyId { get; init; }
    public required string TherapyName { get; init; }
    public TherapyVerdict Verdict { get; init; }
    public Urgency Urgency { get; init; }
    public double ConfidenceScore { get; init; }     // 0-100
    public required string Summary { get; init; }
    public required string[] Rationale { get; init; }
    public required DoseRecommendation Dose { get; init; }
    public required string[] Warnings { get; init; }
    public required string[] Contraindications { get; init; }
    public required MrnaConstruct[] Constructs { get; init; }
    public required string LnpFormulation { get; init; }
    public required string DeliveryRoute { get; init; }
    public DeliveryForm DeliveryForm { get; init; }
    public DeliveryInfo DeliveryInfo => DeliveryInstructions.Get(DeliveryForm);

    // For rejuvenation only
    public ScoringResult? RejuvenationScore { get; init; }
}

public sealed class DoseRecommendation
{
    public double AmountUg { get; init; }
    public required string Route { get; init; }       // IM, IV, SC, intranasal, topical, microneedle
    public int Doses { get; init; } = 1;
    public required string Schedule { get; init; }     // "Single dose", "Day 1, 21", "Weekly x12"
    public required string Notes { get; init; }
}
