namespace Core.Models;

/// <summary>
/// Therapy-specific IVT protocols. Each therapy has different constructs,
/// formulations, and QC requirements.
/// </summary>
public static class TherapyProtocols
{
    public static IvtProtocol GetProtocol(string therapyId)
    {
        var therapy = TherapyCatalog.All.FirstOrDefault(t => t.Id == therapyId)
            ?? TherapyCatalog.Rejuvenation;

        return new IvtProtocol
        {
            Name = $"{therapy.Name} mRNA Synthesis",
            Steps = BuildSteps(therapy),
        };
    }

    private static IvtStep[] BuildSteps(TherapyOption therapy)
    {
        var steps = new List<IvtStep>();
        int order = 1;

        // Per-construct: template prep + IVT
        foreach (var construct in therapy.Constructs)
        {
            steps.Add(new()
            {
                Order = order++,
                Name = $"Template Prep: {construct.Name}",
                Description = $"Linearize plasmid or PCR-amplify template for {construct.Gene} ({construct.LengthNt} nt)",
                Equipment = EquipmentType.HeatBlock,
                Temperature = 37,
                DurationMinutes = 60,
                Notes = $"Source: {construct.Source}. Digest with restriction enzyme downstream of poly-A signal, or PCR with T7-promoter forward primer. Use 1-2ug template DNA in 20uL reaction.",
            });

            steps.Add(new()
            {
                Order = order++,
                Name = $"Purify Template: {construct.Name}",
                Description = $"Column-purify linearized {construct.Gene} template",
                Equipment = EquipmentType.Centrifuge,
                TargetRpm = 8000,
                DurationMinutes = 5,
                Notes = "Monarch PCR & DNA Cleanup Kit. Elute in 20uL RNase-free water. Target: 50-200 ng/uL, A260/280 > 1.8.",
            });

            steps.Add(new()
            {
                Order = order++,
                Name = $"IVT: {construct.Name}",
                Description = $"T7 in vitro transcription of {construct.Gene} mRNA with CleanCap + m1Ψ-UTP",
                Equipment = EquipmentType.HeatBlock,
                Temperature = 37,
                DurationMinutes = construct.LengthNt > 2000 ? 180 : 120, // longer mRNA = longer IVT
                Notes = $"HiScribe T7 kit: 10x buffer, {(construct.LengthNt > 2000 ? "2ug" : "1ug")} template, ATP/GTP/CTP + m1Ψ-UTP (replace UTP), CleanCap AG, T7 polymerase. Total 20uL. Expected yield: {(construct.LengthNt > 2000 ? "80-120" : "100-180")} ug.",
            });

            steps.Add(new()
            {
                Order = order++,
                Name = $"DNase: {construct.Name}",
                Description = $"Remove DNA template from {construct.Gene} IVT reaction",
                Equipment = EquipmentType.HeatBlock,
                Temperature = 37,
                DurationMinutes = 15,
                Notes = "Add 1uL DNase I (RNase-free grade), incubate 15 min at 37C. Critical: DNA contamination affects LNP encapsulation efficiency.",
            });

            steps.Add(new()
            {
                Order = order++,
                Name = $"Purify mRNA: {construct.Name}",
                Description = $"Column-purify {construct.Gene} mRNA, remove enzymes + unincorporated NTPs",
                Equipment = EquipmentType.Centrifuge,
                TargetRpm = 8000,
                DurationMinutes = 5,
                Notes = "Monarch RNA Cleanup Kit. Elute in 50uL RNase-free water. Handle on ice from this point.",
            });

            steps.Add(new()
            {
                Order = order++,
                Name = $"QC Yield: {construct.Name}",
                Description = $"Measure {construct.Gene} mRNA concentration and purity",
                Equipment = EquipmentType.Spectrophotometer,
                Notes = $"Target: A260/280 = 1.9-2.1 (pure RNA). Expected concentration: {(construct.LengthNt > 2000 ? "1500-2500" : "2000-3600")} ng/uL in 50uL = {(construct.LengthNt > 2000 ? "75-125" : "100-180")} ug total.",
            });

            steps.Add(new()
            {
                Order = order++,
                Name = $"QC Gel: {construct.Name}",
                Description = $"Verify {construct.Gene} mRNA size ({construct.LengthNt} nt) and integrity",
                Equipment = EquipmentType.GelElectrophoresis,
                Voltage = 100,
                DurationMinutes = 30,
                Notes = $"1% agarose/TAE + GelRed. Load 200ng mRNA + RNA ladder. Run 100V 30min. Expected: single sharp band at ~{construct.LengthNt / 1000.0:F1} kb. Smearing = degradation (remake). Double band = incomplete template digest.",
            });
        }

        // Combine mRNAs (if multiple constructs)
        if (therapy.Constructs.Length > 1)
        {
            steps.Add(new()
            {
                Order = order++,
                Name = "Combine mRNA Constructs",
                Description = $"Mix {therapy.Constructs.Length} purified mRNAs at equimolar ratio",
                Equipment = EquipmentType.None,
                Notes = $"Calculate molarity: conc (ng/uL) / (nt_length * 330 g/mol) = pmol/uL. Mix to equimolar. Constructs: {string.Join(", ", therapy.Constructs.Select(c => $"{c.Name} ({c.LengthNt}nt)"))}. Keep on ice.",
            });
        }

        // LNP formulation — therapy-specific
        string lnpNotes = therapy.Id switch
        {
            "common-cold" => "INTRANASAL LNP: Use DOTAP-based cationic lipid (not SM-102). DOTAP:DOPE:Chol:PEG = 40:30:28.5:1.5 molar. Particle size 100-300nm for lung deposition. Pump A: mRNA in citrate buffer pH 4.0 (3 parts). Pump B: lipids in ethanol (1 part). Total flow 8 mL/min.",
            "wound-healing" => "TOPICAL LNP in HYDROGEL: SM-102:DSPC:Chol:PEG = 50:10:38.5:1.5 molar. After LNP formation, mix 1:1 with 2% hyaluronic acid hydrogel. Final product: spreadable gel for wound application.",
            "hair-regrowth" => "MICRONEEDLE LNP: SM-102 formulation but small particle size (<100nm critical). Use higher flow rate (16 mL/min) for smaller particles. After LNP formation, concentrate via centrifugal filter, then load into dissolving microneedle mold (PVP/PVA polymer).",
            "cancer-neoantigen" => "IV DELIVERY LNP targeting dendritic cells: Use ionizable lipid with DC-targeting moiety. DLin-MC3:DSPC:Chol:PEG-DMG = 50:10:38.5:1.5. Must be sterile-filtered (0.22um) for IV use. Higher QC bar — endotoxin testing required.",
            "allergy" => "TOLEROGENIC LNP: SM-102 base + mannose-conjugated PEG-lipid (replaces standard PEG-DMG) for dendritic cell targeting. Mannose-PEG promotes tolerance over immunity. SC injection formulation.",
            _ => "STANDARD IM LNP: SM-102:DSPC:Chol:PEG-DMG = 50:10:38.5:1.5 molar. Pump A: mRNA in citrate buffer pH 4.0 (3 parts). Pump B: lipids in ethanol (1 part). Total flow 12 mL/min.",
        };

        steps.Add(new()
        {
            Order = order++,
            Name = "LNP Encapsulation",
            Description = "Microfluidic mixing of mRNA with lipid nanoparticle components",
            Equipment = EquipmentType.SyringePump,
            Notes = lnpNotes,
        });

        steps.Add(new()
        {
            Order = order++,
            Name = "Buffer Exchange",
            Description = "Remove ethanol by dialysis or centrifugal filtration",
            Equipment = EquipmentType.None,
            DurationMinutes = therapy.Id == "cancer-neoantigen" ? 240 : 120,
            Notes = therapy.Id == "cancer-neoantigen"
                ? "10kDa MWCO dialysis cassette in sterile PBS (endotoxin-free). Change PBS at 60min and 120min. Total 4 hours. Sterile-filter (0.22um) after dialysis."
                : "10kDa MWCO dialysis cassette in PBS, 2 hours. Change PBS at 60min. Or use 100kDa centrifugal filter (Amicon Ultra, 3000g x 30min x 3 washes).",
        });

        steps.Add(new()
        {
            Order = order++,
            Name = "QC: LNP-mRNA Final",
            Description = "Measure encapsulation efficiency, particle size, final concentration",
            Equipment = EquipmentType.Spectrophotometer,
            Notes = "1) Disrupt LNPs: add 1% Triton X-100, vortex 30s. 2) Measure A260 for total mRNA. 3) Compare to pre-encapsulation concentration. Target: >80% encapsulation. 4) For particle size: DLS if available, or correlate with flow rate (faster = smaller). Target: 60-100nm for IM/SC/IV, 100-300nm for intranasal.",
        });

        // Therapy-specific final formulation
        if (therapy.Id == "wound-healing")
        {
            steps.Add(new()
            {
                Order = order++,
                Name = "Hydrogel Formulation",
                Description = "Mix LNP-mRNA with hyaluronic acid hydrogel for topical application",
                Equipment = EquipmentType.None,
                Notes = "Mix LNP-mRNA 1:1 (v/v) with sterile 2% hyaluronic acid (HA) in PBS. Gently stir (do not vortex — shear damages LNPs). Final product: viscous gel, store at 2-8°C, use within 24 hours.",
            });
        }

        if (therapy.Id == "hair-regrowth")
        {
            steps.Add(new()
            {
                Order = order++,
                Name = "Microneedle Loading",
                Description = "Concentrate LNP-mRNA and load into dissolving microneedle arrays",
                Equipment = EquipmentType.Centrifuge,
                TargetRpm = 3000,
                DurationMinutes = 30,
                Notes = "Concentrate LNP-mRNA 10x using 100kDa Amicon Ultra centrifugal filter. Cast into PVP/PVA microneedle molds (600um needle height, 11x11 array). Dry at room temperature 24h in desiccator. Store at -20°C with desiccant.",
            });
        }

        if (therapy.Id == "common-cold")
        {
            steps.Add(new()
            {
                Order = order++,
                Name = "Nebulizer Loading",
                Description = "Load LNP-mRNA into nebulizer device for intranasal delivery",
                Equipment = EquipmentType.None,
                Notes = "Use vibrating mesh nebulizer (not jet nebulizer — shear forces destroy LNPs). Load 0.5-1mL LNP-mRNA solution. Particle output: 3-5 um MMAD for upper airway deposition. Administer via facemask, 5-10 minutes inhalation.",
            });
        }

        steps.Add(new()
        {
            Order = order++,
            Name = "Storage",
            Description = "Aliquot and store final product",
            Equipment = EquipmentType.None,
            Notes = therapy.Id switch
            {
                "wound-healing" => "Hydrogel format: store 2-8°C, use within 24h. LNP concentrate (pre-hydrogel): store -80°C, stable 6+ months.",
                "hair-regrowth" => "Microneedle patches: store -20°C with desiccant. Stable 3+ months. Bring to room temp 30min before application.",
                "common-cold" => "Nebulizer-ready solution: store 2-8°C, use within 4h. LNP concentrate: store -80°C, stable 6+ months.",
                "cancer-neoantigen" => "Store -80°C in single-dose vials (sterile). Stable 6+ months. Thaw at 2-8°C for 2h before IV infusion. Do NOT refreeze.",
                _ => "Aliquot into single-use volumes at -80°C. Stable 6+ months. Thaw at room temperature 15min before use. Do NOT freeze-thaw repeatedly.",
            },
        });

        return steps.ToArray();
    }
}
