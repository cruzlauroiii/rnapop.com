namespace Core.Scoring;

using Models;

/// <summary>
/// Combines all 6 pillar scores into a composite score and renders PASS/FAIL verdict.
/// Weights are redistributed when optional panels are missing.
/// </summary>
public static class CompositeScorer
{
    private const double PassThreshold = 60.0;
    private const double PillarFloor = 30.0;

    private static readonly (string Name, string Desc, double Weight)[] PillarDefs =
    [
        ("PhenoAge",    "Biological age clock (Levine 2018)",          0.30),
        ("piRNA",       "Longevity piRNA panel (Duke 2026)",           0.15),
        ("miRNA",       "Aging miRNA signature",                       0.15),
        ("Senescence",  "Cellular senescence / SASP burden",           0.15),
        ("Pathways",    "NAD+/SIRT/AMPK longevity pathways",           0.15),
        ("Telomere",    "Telomere length & telomerase health",         0.10),
    ];

    public static ScoringResult Score(BiomarkerInput input)
    {
        int age = input.Demographics.ChronologicalAge;
        var sex = input.Demographics.Sex;

        // Calculate each pillar
        var (phenoAge, mortality, _) = PhenoAgeCalculator.Calculate(input.Blood, age);
        double phenoScore = PhenoAgeCalculator.Score(phenoAge, age);

        double pirnaScore = PiRnaScorer.Score(input.PiRna);
        double mirnaScore = MiRnaAnalyzer.Score(input.MiRna);
        double saspScore = SenescenceEvaluator.Score(input.Senescence);
        double pathwayScore = PathwayAnalyzer.Score(input.Pathway);
        double telomereScore = TelomereAssessor.Score(input.Telomere, age, sex);

        double[] rawScores = [phenoScore, pirnaScore, mirnaScore, saspScore, pathwayScore, telomereScore];

        // Build pillars with weight redistribution
        double availableWeight = 0;
        for (int i = 0; i < 6; i++)
        {
            if (rawScores[i] >= 0) availableWeight += PillarDefs[i].Weight;
        }

        var pillars = new PillarScore[6];
        double composite = 0;

        for (int i = 0; i < 6; i++)
        {
            bool has = rawScores[i] >= 0;
            double effectiveWeight = has && availableWeight > 0 ? PillarDefs[i].Weight / availableWeight : 0;

            string[] flags = i switch
            {
                0 => phenoAge > age + 5 ? [$"Biological age {phenoAge:F1} vs chronological {age} (+{phenoAge - age:F1} years)"] : [],
                1 => PiRnaScorer.GetFlags(input.PiRna),
                2 => MiRnaAnalyzer.GetFlags(input.MiRna),
                3 => SenescenceEvaluator.GetFlags(input.Senescence),
                4 => PathwayAnalyzer.GetFlags(input.Pathway),
                5 => TelomereAssessor.GetFlags(input.Telomere, age, sex),
                _ => []
            };

            double score = has ? rawScores[i] : 0;
            pillars[i] = new PillarScore
            {
                Name = PillarDefs[i].Name,
                Description = PillarDefs[i].Desc,
                Score = score,
                Weight = PillarDefs[i].Weight,
                EffectiveWeight = effectiveWeight,
                HasData = has,
                Flags = flags,
            };

            if (has) composite += score * effectiveWeight;
        }

        // Check pass conditions
        bool anyBelowFloor = pillars.Any(p => p.HasData && p.Score < PillarFloor);
        bool saspCritical = SenescenceEvaluator.IsCritical(input.Senescence);
        bool pass = composite >= PassThreshold && !anyBelowFloor && !saspCritical;

        string reason = pass
            ? "All criteria met for rejuvenation candidacy."
            : BuildFailReason(composite, pillars, saspCritical);

        // Recommendations
        var recs = BuildRecommendations(pillars, input);

        // Rejuvenation protocol eligibility
        var protocols = EvaluateProtocols(pillars, input);

        return new ScoringResult
        {
            CompositeScore = Math.Round(composite, 1),
            BiologicalAge = Math.Round(phenoAge, 1),
            ChronologicalAge = age,
            AgeDelta = Math.Round(phenoAge - age, 1),
            Verdict = pass ? Verdict.Pass : Verdict.Fail,
            VerdictReason = reason,
            Pillars = pillars,
            Protocols = protocols,
            Recommendations = recs.ToArray(),
        };
    }

    private static string BuildFailReason(double composite, PillarScore[] pillars, bool saspCritical)
    {
        var parts = new List<string>();
        if (composite < PassThreshold)
            parts.Add($"Composite score {composite:F1} below threshold ({PassThreshold})");
        foreach (var p in pillars.Where(p => p.HasData && p.Score < PillarFloor))
            parts.Add($"{p.Name} score critically low ({p.Score:F1}/100)");
        if (saspCritical)
            parts.Add("Senescence burden in critical zone");
        return string.Join(". ", parts) + ".";
    }

    private static List<string> BuildRecommendations(PillarScore[] pillars, BiomarkerInput input)
    {
        var recs = new List<string>();

        foreach (var p in pillars.Where(p => p.HasData && p.Score < 60))
        {
            switch (p.Name)
            {
                case "PhenoAge":
                    if (input.Blood.CReactiveProtein > 1)
                        recs.Add("Reduce systemic inflammation (CRP elevated) — anti-inflammatory diet, omega-3");
                    if (input.Blood.FastingGlucose > 100)
                        recs.Add("Improve glycemic control (glucose elevated) — exercise, reduce refined carbs");
                    if (input.Blood.Albumin < 3.8)
                        recs.Add("Increase protein intake (albumin low)");
                    break;
                case "piRNA":
                    recs.Add("piRNA levels elevated — monitor via repeat testing; lifestyle interventions may help");
                    break;
                case "miRNA":
                    if (input.MiRna.MiR34a > 2) recs.Add("miR-34a high — senolytics (quercetin + dasatinib) may reduce senescent burden");
                    if (input.MiRna.MiR21 > 2) recs.Add("miR-21 high — address chronic inflammation sources");
                    break;
                case "Senescence":
                    recs.Add("High senescent cell burden — consider senolytics (dasatinib + quercetin, fisetin)");
                    if (input.Senescence.P16Ink4a > 2) recs.Add("p16 very elevated — strong candidate for senolytic intervention");
                    break;
                case "Pathways":
                    if (input.Pathway.NadLevel < 20) recs.Add("NAD+ depleted — NMN (500-1000mg/day) or NR supplementation");
                    if (input.Pathway.Sirt1Expression < 0.7) recs.Add("SIRT1 low — resveratrol, caloric restriction, or exercise may help");
                    if (input.Pathway.AmpkActivity < 0.7) recs.Add("AMPK low — metformin, berberine, or fasting may activate");
                    break;
                case "Telomere":
                    recs.Add("Telomere health suboptimal — exercise, stress reduction, and adequate sleep support telomere maintenance");
                    break;
            }
        }

        if (!recs.Any()) recs.Add("All pillars in good range. Maintain current lifestyle.");
        return recs;
    }

    private static RejuvenationProtocol[] EvaluateProtocols(PillarScore[] pillars, BiomarkerInput input)
    {
        double saspScore = pillars[3].HasData ? pillars[3].Score : 100;
        double pathScore = pillars[4].HasData ? pillars[4].Score : 50;
        double phenoScore = pillars[0].Score;
        bool saspCritical = SenescenceEvaluator.IsCritical(input.Senescence);

        // OSK mRNA: safest, requires moderate baseline health
        var oskBlockers = new List<string>();
        if (saspCritical) oskBlockers.Add("Senescence burden too high — clear senescent cells first");
        if (pathScore < 30) oskBlockers.Add("Longevity pathways severely impaired — optimize NAD+/SIRT first");

        var osk = new RejuvenationProtocol
        {
            Name = "OSK mRNA",
            Description = "Oct4/Sox2/Klf4 partial reprogramming — no c-Myc, lower cancer risk",
            Eligibility = oskBlockers.Count == 0 ? ProtocolEligibility.Eligible
                : oskBlockers.Count == 1 ? ProtocolEligibility.Borderline
                : ProtocolEligibility.NotEligible,
            Blockers = oskBlockers.ToArray(),
        };

        // OSKM mRNA: more potent, stricter requirements
        var oskmBlockers = new List<string>(oskBlockers);
        if (phenoScore < 40) oskmBlockers.Add("Biological age too advanced for safe OSKM");
        if (saspScore < 50) oskmBlockers.Add("Moderate senescence — OSKM c-Myc adds oncogenic risk");

        var oskm = new RejuvenationProtocol
        {
            Name = "OSKM mRNA",
            Description = "Oct4/Sox2/Klf4/c-Myc full Yamanaka — maximum potency, higher risk",
            Eligibility = oskmBlockers.Count == 0 ? ProtocolEligibility.Eligible
                : oskmBlockers.Count <= 2 ? ProtocolEligibility.Borderline
                : ProtocolEligibility.NotEligible,
            Blockers = oskmBlockers.ToArray(),
        };

        // SB000 single-factor: safest, broadest eligibility
        var sb000Blockers = new List<string>();
        if (saspCritical && pathScore < 20) sb000Blockers.Add("Multiple systems critically impaired");

        var sb000 = new RejuvenationProtocol
        {
            Name = "SB000 Single-Factor",
            Description = "Novel single gene — comparable to OSKM potency, minimal identity disruption",
            Eligibility = sb000Blockers.Count == 0 ? ProtocolEligibility.Eligible
                : ProtocolEligibility.Borderline,
            Blockers = sb000Blockers.ToArray(),
        };

        return [osk, oskm, sb000];
    }
}
