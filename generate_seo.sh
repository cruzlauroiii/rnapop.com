#!/bin/bash
# Generate SEO-optimized HTML pages with unique titles, descriptions, and static content
BASE="C:/work/rna/publish/wwwroot"
SITE="https://cruzlauroiii.github.io/rnapop.com"

generate_page() {
    local path="$1" title="$2" desc="$3" h1="$4" content="$5"
    local dir="$BASE/$path"
    mkdir -p "$dir"
    cat > "$dir/index.html" << HTMLEOF
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>$title — RNA Therapy Platform</title>
    <meta name="description" content="$desc" />
    <meta property="og:title" content="$title — RNA Therapy Platform" />
    <meta property="og:description" content="$desc" />
    <meta property="og:type" content="website" />
    <meta property="og:url" content="$SITE/$path/" />
    <meta name="twitter:card" content="summary" />
    <meta name="twitter:title" content="$title" />
    <meta name="twitter:description" content="$desc" />
    <link rel="canonical" href="$SITE/$path/" />
    <base href="/rnapop.com/" />
    <link rel="icon" type="image/svg+xml" href="favicon.svg" />
    <link href="css/app.css" rel="stylesheet" />
</head>
<body>
    <div id="app">
        <div style="max-width:800px;margin:2rem auto;padding:0 1rem;font-family:system-ui,sans-serif;color:#e0e0e8;background:#0a0a0f;">
            <h1 style="color:#6366f1;">$h1</h1>
            $content
            <p style="color:#888;font-size:0.85rem;margin-top:2rem;">Loading interactive app...</p>
        </div>
    </div>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
</html>
HTMLEOF
}

# Root / Home
cat > "$BASE/index.html" << 'HTMLEOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>RNA Therapy Platform — Make mRNA Medicine at Home</title>
    <meta name="description" content="Open-source platform for synthesizing mRNA therapies. 10 therapies (cold, flu, COVID, cancer, wound healing, hair, muscle, allergy, rejuvenation), ESP32 lab control, step-by-step wizard. $2,000 total cost." />
    <meta property="og:title" content="RNA Therapy Platform — Make mRNA Medicine at Home" />
    <meta property="og:description" content="Open-source mRNA synthesis platform with 10 therapies, ESP32 lab equipment control, and step-by-step guided wizard. Total cost ~$2,000." />
    <meta property="og:type" content="website" />
    <meta property="og:url" content="https://cruzlauroiii.github.io/rnapop.com/" />
    <meta name="twitter:card" content="summary" />
    <link rel="canonical" href="https://cruzlauroiii.github.io/rnapop.com/" />
    <base href="/rnapop.com/" />
    <link rel="icon" type="image/svg+xml" href="favicon.svg" />
    <link href="css/app.css" rel="stylesheet" />
</head>
<body>
    <div id="app">
        <div style="max-width:800px;margin:2rem auto;padding:0 1rem;font-family:system-ui,sans-serif;color:#e0e0e8;background:#0a0a0f;">
            <h1 style="color:#6366f1;">RNA Therapy Platform</h1>
            <p>Make mRNA medicine at home using the same technology as COVID vaccines. This open-source platform guides you through the entire process — from choosing a therapy to synthesizing the mRNA and packaging it for delivery.</p>
            <h2>10 RNA Therapies</h2>
            <ul>
                <li><strong>Common Cold</strong> — mRNA-encoded antibodies against rhinovirus</li>
                <li><strong>Influenza</strong> — broadly neutralizing antibodies for flu treatment</li>
                <li><strong>COVID-19</strong> — pan-sarbecovirus nanobody therapy</li>
                <li><strong>Universal Flu Vaccine</strong> — 20-valent mRNA for all flu strains</li>
                <li><strong>Cancer Neoantigen Vaccine</strong> — personalized tumor-specific mRNA</li>
                <li><strong>Wound Healing</strong> — VEGF-A mRNA for angiogenesis</li>
                <li><strong>Hair Regrowth</strong> — WNT3A/SHH mRNA via microneedle patches</li>
                <li><strong>Muscle Loss</strong> — follistatin mRNA to block myostatin</li>
                <li><strong>Allergy Desensitization</strong> — hypoallergenic mRNA immunotherapy</li>
                <li><strong>Rejuvenation</strong> — OSK Yamanaka factors for epigenetic reprogramming</li>
            </ul>
            <h2>Features</h2>
            <ul>
                <li>Step-by-step wizard for non-technical users</li>
                <li>Biological age assessment using Levine PhenoAge algorithm</li>
                <li>ESP32 lab equipment control (17 instruments, $250 total hardware)</li>
                <li>Complete IVT protocol with auto-timers</li>
                <li>Simulation mode — practice without hardware</li>
                <li>100% client-side — no data leaves your browser</li>
            </ul>
            <p style="color:#888;font-size:0.85rem;margin-top:2rem;">Loading interactive app...</p>
        </div>
    </div>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
</html>
HTMLEOF

# Each page with unique SEO content
generate_page "wizard" \
    "Step-by-Step mRNA Synthesis Guide" \
    "Guided wizard to make mRNA medicine at home. Pick a therapy, enter symptoms, check eligibility, connect lab equipment, synthesize mRNA step by step. No science background needed." \
    "Step-by-Step Guide — Make mRNA Medicine" \
    "<p>This wizard walks you through the entire process of making mRNA medicine, from choosing what to treat to holding the finished product. No science background needed — every step is explained in plain English.</p><h2>5 Simple Steps</h2><ol><li>Pick your therapy (cold, flu, COVID, cancer, wound, hair, muscle, allergy, rejuvenation)</li><li>Enter your information (symptoms, age, medical history)</li><li>Get your eligibility result and personalized dose</li><li>Connect lab equipment (or use simulation mode to practice)</li><li>Follow the guided protocol — each step tells you exactly what to do</li></ol>"

generate_page "therapies" \
    "10 RNA Therapies — Complete Catalog" \
    "Browse 10 mRNA therapies with mechanisms, gene constructs, LNP formulations, dosing protocols, delivery methods, and clinical trial status. From common cold to cancer." \
    "RNA Therapy Catalog" \
    "<p>10 mRNA therapies spanning antiviral, oncology, regenerative medicine, immunotherapy, and rejuvenation. Each includes scientific mechanism, mRNA construct details (gene, length, Addgene/GenBank source), lipid nanoparticle formulation, dosing protocol, and delivery method.</p><h2>Therapies</h2><ul><li>Common Cold — Anti-rhinovirus bispecific antibody + IFN-lambda (intranasal)</li><li>Influenza — CR9114 broadly neutralizing antibody (IM injection)</li><li>COVID-19 — Pan-sarbecovirus nanobody VHH-72 (IM injection)</li><li>Universal Flu Vaccine — 20-valent hemagglutinin (IM injection)</li><li>Cancer Neoantigen — Patient-specific tumor mutations (IV infusion)</li><li>Wound Healing — VEGF-A + FGF-2 growth factors (topical hydrogel)</li><li>Hair Regrowth — WNT3A + SHH + Noggin (dissolving microneedle patch)</li><li>Muscle Loss — Follistatin + IGF-1 (IM injection)</li><li>Allergy — Hypoallergenic Bet v 1, Der p 1, Fel d 1 (SC injection)</li><li>Rejuvenation — OCT4 + SOX2 + KLF4 Yamanaka factors (IV/IM)</li></ul>"

generate_page "assess" \
    "Biological Age Assessment — PhenoAge Calculator" \
    "Calculate your biological age using the Levine PhenoAge algorithm with 9 blood biomarkers. 6-pillar scoring: PhenoAge, piRNA, miRNA, senescence, NAD+/SIRT/AMPK pathways, telomere length." \
    "Biological Age Assessment" \
    "<p>Calculate your biological age using the Levine PhenoAge algorithm (2018), calibrated on NHANES III mortality data with 23 years of follow-up. Enter 9 standard blood biomarkers from a routine lab panel.</p><h2>9 Required Biomarkers</h2><ul><li>Albumin (g/dL)</li><li>Alkaline Phosphatase (U/L)</li><li>Creatinine (mg/dL)</li><li>C-Reactive Protein (mg/dL)</li><li>Fasting Glucose (mg/dL)</li><li>White Blood Cell Count (10³/µL)</li><li>Lymphocyte %</li><li>Mean Cell Volume (fL)</li><li>Red Cell Distribution Width (%)</li></ul><h2>Optional Panels</h2><ul><li>piRNA Longevity Panel (Duke 2026 — 6 piRNAs, 86% 2-year survival prediction)</li><li>miRNA Aging Signature (miR-34a, miR-21, miR-155)</li><li>Senescence/SASP Markers (p16, GDF15, IL-6, TNF-α)</li><li>NAD+/SIRT/AMPK Pathway Health</li><li>Telomere Length + Telomerase</li></ul>"

generate_page "hardware" \
    "Build Your mRNA Lab — Complete Hardware Guide" \
    "Build a complete mRNA synthesis lab for \$250. 4 ESP32 chips controlling 17 instruments. Shopping list with exact search terms, wiring diagrams, assembly instructions for non-technical users." \
    "Build Your Lab — Complete Hardware Guide" \
    "<p>Everything you need to build a complete mRNA synthesis laboratory for about \$250 in electronics and sensors, plus \$1,600 in chemical reagents. Total: ~\$2,000.</p><h2>4 ESP32 Controllers (\$16 total)</h2><ul><li>ESP32 #1 Controller — Heat block, UV spectrophotometer, centrifuge, gel electrophoresis</li><li>ESP32 #2 Stepper — Dedicated syringe pump timing for precise LNP mixing</li><li>ESP32 #3 Environment — pH meter, stirrer, vortex, UV-C sterilizer, fume hood, safety systems</li><li>ESP32 #4 QC — Gel camera, DLS particle sizer, turbidity sensor</li></ul><h2>17 Instruments</h2><p>Heat block, 2 syringe pumps, UV spectrophotometer, centrifuge, gel electrophoresis, pH meter, magnetic stirrer, vortex mixer, UV-C sterilizer, fume hood, DLS particle sizer, turbidity sensor, gel imager, room/freezer/fridge monitors, emergency stop, gas detector.</p>"

generate_page "lab" \
    "Lab Dashboard — Control 17 Instruments via WebSocket" \
    "Real-time lab equipment dashboard. Control heat block, syringe pumps, UV spectrophotometer, centrifuge, gel electrophoresis, pH meter, stirrer, vortex, UV-C, and more via WebSocket." \
    "Lab Dashboard" \
    "<p>Control all 17 lab instruments from your browser via WebSocket (WSS) connection to ESP32 microcontrollers. Real-time status updates every 500ms.</p><h2>Instruments</h2><ul><li>Heat Block — PID temperature control at 37°C for in vitro transcription</li><li>Syringe Pumps A+B — Precise 3:1 ratio mixing for LNP encapsulation</li><li>UV Spectrophotometer — A260/A280 measurement for RNA concentration and purity</li><li>Centrifuge — 8000 RPM for purification</li><li>Gel Electrophoresis — 100V for RNA size verification</li><li>pH Meter — Citrate buffer must be pH 4.0 for LNP formation</li><li>Magnetic Stirrer — Dissolve lipids in ethanol</li><li>UV-C Sterilizer — Workspace decontamination</li><li>DLS Particle Sizer — Verify LNP diameter 60-100nm</li><li>Safety Systems — Emergency stop, gas sensor, freezer alarm</li></ul>"

generate_page "protocol" \
    "mRNA Synthesis Protocol — IVT Step-by-Step" \
    "Complete in vitro transcription protocol for mRNA synthesis. Template prep, T7 IVT with CleanCap + modified nucleotides, purification, gel QC, LNP encapsulation, buffer exchange, storage." \
    "mRNA Synthesis Protocol" \
    "<p>Step-by-step in vitro transcription (IVT) protocol customized per therapy. Covers the full pipeline from DNA template to LNP-encapsulated mRNA ready for storage at -80°C.</p><h2>Protocol Steps</h2><ol><li>Template linearization — Cut plasmid DNA with restriction enzyme</li><li>Template purification — Column cleanup</li><li>QC template — UV spectrophotometer A260 measurement</li><li>In vitro transcription — T7 polymerase + CleanCap + m1Ψ-UTP at 37°C for 2 hours</li><li>DNase treatment — Destroy DNA template</li><li>mRNA purification — Column cleanup, keep on ice</li><li>QC yield — A260/A280 ratio, concentration</li><li>Gel electrophoresis — Verify mRNA size and integrity</li><li>LNP encapsulation — Microfluidic mixing via syringe pumps</li><li>Buffer exchange — Dialysis to remove ethanol</li><li>Final QC — Encapsulation efficiency measurement</li><li>Storage — Aliquot at -80°C</li></ol>"

# Therapy assessment pages
for therapy in \
    "common-cold|Common Cold Treatment|mRNA therapy assessment for common cold. Enter symptoms, check eligibility, get dose recommendation for intranasal anti-rhinovirus antibody + IFN-lambda mRNA.|Common Cold — Therapy Assessment|mRNA-encoded antibodies against all 160+ rhinovirus serotypes. Intranasal delivery via nebulizer. Best within 24 hours of symptom onset." \
    "influenza|Influenza Treatment|mRNA therapy for flu. Broadly neutralizing antibody CR9114 effective against all influenza A and B subtypes. IM injection within 48 hours of onset.|Influenza — Therapy Assessment|mRNA-encoded CR9114 broadly neutralizing antibody against all influenza subtypes. IM injection. Clinical trials: Moderna mRNA-1010 Phase 3." \
    "covid19|COVID-19 Treatment|mRNA therapy for COVID-19. Pan-sarbecovirus nanobody VHH-72 for immediate passive immunization. Works against all variants.|COVID-19 — Therapy Assessment|mRNA-encoded pan-sarbecovirus nanobody providing immediate passive immunity against all SARS-CoV-2 variants." \
    "universal-flu|Universal Flu Vaccine|20-valent mRNA vaccine encoding all influenza subtypes. One shot for lifetime pan-influenza protection. Preclinical — proven in mice (Science 2022).|Universal Flu Vaccine — Assessment|Single mRNA injection encoding all 20 influenza hemagglutinin subtypes for lifelong pan-influenza protection." \
    "cancer-neoantigen|Cancer Neoantigen Vaccine|Personalized cancer vaccine from tumor DNA. Up to 34 patient-specific neoantigens encoded in mRNA. IV infusion with anti-PD-1. BioNTech BNT122 Phase 2.|Cancer Neoantigen Vaccine — Assessment|Personalized mRNA cancer vaccine. Requires tumor biopsy + HLA typing. Up to 34 neoantigens. BioNTech BNT122 in Phase 2 for melanoma and pancreatic cancer." \
    "wound-healing|Wound Healing mRNA Therapy|VEGF-A mRNA for chronic wound healing. Topical hydrogel application. Promotes angiogenesis. AstraZeneca AZD8601 Phase 2 clinical trial.|Wound Healing — Therapy Assessment|VEGF-A + FGF-2 mRNA in topical hydrogel for chronic and diabetic wound healing. Promotes blood vessel formation at wound site." \
    "hair-regrowth|Hair Regrowth mRNA Therapy|WNT3A + SHH mRNA microneedle patches to reactivate dormant hair follicles. Targets Wnt/beta-catenin pathway in androgenetic alopecia.|Hair Regrowth — Therapy Assessment|WNT3A + Sonic Hedgehog + Noggin mRNA delivered via dissolving microneedle patches to reactivate dormant hair follicle stem cells." \
    "muscle-loss|Muscle Loss Sarcopenia Treatment|Follistatin mRNA to block myostatin and reverse age-related muscle loss. EWGSOP2 sarcopenia criteria. IM injection with resistance exercise.|Muscle Loss (Sarcopenia) — Assessment|Follistatin mRNA blocks myostatin, the protein that limits muscle growth. Combined with resistance exercise for age-related sarcopenia." \
    "allergy|Allergy Desensitization mRNA|Hypoallergenic mRNA immunotherapy. Modified allergens retrain immune tolerance without anaphylaxis risk. Safer than traditional allergy shots.|Allergy Desensitization — Assessment|mRNA-encoded hypoallergenic proteins (modified Bet v 1, Der p 1, Fel d 1) for immune tolerance induction. Safer than traditional allergy shots." \
    "rejuvenation|Rejuvenation Epigenetic Reprogramming|OSK Yamanaka factors mRNA for cellular rejuvenation. PhenoAge biological age assessment. First human trial 2026 (Life Biosciences ER-100).|Rejuvenation — Therapy Assessment|OCT4/SOX2/KLF4 mRNA for partial epigenetic reprogramming. Reverses biological aging markers. 6-pillar scoring with PhenoAge algorithm."
do
    IFS='|' read -r slug title desc h1 content <<< "$therapy"
    generate_page "therapy-assess/$slug" "$title" "$desc" "$h1" "<p>$content</p>"
done

echo "SEO pages generated"
