namespace Core.Models;

public enum DeliveryForm
{
    IntramuscularInjection,     // IM — deltoid, thigh, glute
    IntravenousInfusion,        // IV — drip bag, slow push
    SubcutaneousInjection,      // SC — upper arm, abdomen
    IntranasalSpray,            // Nasal spray device
    IntranasalNebulizer,        // Nebulizer mask inhalation
    TopicalGel,                 // Hydrogel applied to skin/wound
    TopicalCream,               // Cream/lotion rubbed in
    MicroneedlePatch,           // Dissolving microneedle array on skin
    OralCapsule,                // Swallowed capsule (future — LNP in enteric coating)
    EyeDrops,                   // Ophthalmic drops
    Suppository,                // Rectal/vaginal (rare for mRNA)
}

/// <summary>Detailed administration instructions per delivery form.</summary>
public static class DeliveryInstructions
{
    public static DeliveryInfo Get(DeliveryForm form) => form switch
    {
        DeliveryForm.IntramuscularInjection => new()
        {
            Form = form,
            DisplayName = "Injection (Intramuscular)",
            Icon = "💉",
            ShortDescription = "IM injection into muscle",
            Preparation = "Thaw vial at room temperature 15 min. Do NOT shake — gently swirl to mix. Draw into 1mL syringe with 25G x 1\" needle.",
            Administration = "Clean injection site (deltoid preferred) with alcohol swab. Insert needle at 90° angle into muscle. Aspirate briefly — if blood, withdraw and retry. Inject slowly over 5-10 seconds. Withdraw needle, apply gentle pressure with gauze.",
            PostCare = "Apply bandage. Monitor injection site 15 min for allergic reaction. Arm soreness normal for 24-48h. Report fever >39°C, rash, or breathing difficulty.",
            StorageBefore = "Store at -80°C. Thaw at room temp 15 min before use. Use within 6h of thaw. Do NOT refreeze.",
            Equipment = ["1mL syringe", "25G x 1\" needle", "Alcohol swabs", "Gauze", "Bandage", "Sharps container"],
        },
        DeliveryForm.IntravenousInfusion => new()
        {
            Form = form,
            DisplayName = "IV Infusion (Intravenous)",
            Icon = "🩸",
            ShortDescription = "Slow IV drip into vein",
            Preparation = "Thaw vial at 2-8°C for 2h (do NOT microwave or warm rapidly). Dilute in 100mL sterile 0.9% NaCl. Use non-PVC IV bag and tubing (PVC adsorbs LNPs). Inline 0.2um filter NOT recommended (blocks LNPs).",
            Administration = "Establish IV access (20-22G catheter, antecubital vein). Infuse over 30-60 minutes via infusion pump. Monitor vitals every 15 min: BP, HR, SpO2, temperature. Have epinephrine and resuscitation equipment available.",
            PostCare = "Monitor patient 60 min post-infusion. Flush IV line with 20mL NaCl. Report: rigors, hypotension, rash, dyspnea, chest pain.",
            StorageBefore = "Store at -80°C. Thaw at 2-8°C for 2h. Once diluted in NaCl, use within 4h. Do NOT store diluted product.",
            Equipment = ["IV catheter (20-22G)", "Non-PVC IV bag + tubing", "100mL 0.9% NaCl", "Infusion pump", "Vital signs monitor", "Epinephrine 1:1000", "Resuscitation cart"],
        },
        DeliveryForm.SubcutaneousInjection => new()
        {
            Form = form,
            DisplayName = "Injection (Subcutaneous)",
            Icon = "💉",
            ShortDescription = "SC injection under skin",
            Preparation = "Thaw at room temperature 15 min. Draw into 1mL syringe with 27G x 0.5\" needle. Smaller gauge reduces pain.",
            Administration = "Clean injection site (upper arm, abdomen 2\" from navel, or thigh) with alcohol. Pinch skin fold. Insert needle at 45° angle into subcutaneous fat. Inject slowly over 5 seconds. Release skin, withdraw needle.",
            PostCare = "Apply bandage. Mild redness/swelling at site is normal. Rotate injection sites between doses. Report: large swelling, hives, fever >39°C.",
            StorageBefore = "Store at -80°C. Thaw at room temp 15 min. Use within 6h of thaw.",
            Equipment = ["1mL syringe", "27G x 0.5\" needle", "Alcohol swabs", "Gauze", "Bandage"],
        },
        DeliveryForm.IntranasalNebulizer => new()
        {
            Form = form,
            DisplayName = "Nebulizer (Intranasal/Inhaled)",
            Icon = "🫁",
            ShortDescription = "Inhaled mist via nebulizer mask",
            Preparation = "Use vibrating mesh nebulizer ONLY (e.g., Aerogen Solo, PARI eFlow). Do NOT use jet nebulizer — shear forces destroy LNPs. Load 0.5-1mL LNP-mRNA solution into nebulizer reservoir.",
            Administration = "Sit upright. Place nebulizer mask over nose and mouth. Breathe normally through nose for 5-10 minutes until reservoir is empty. For nasal-only targeting: use nasal cannula adapter instead of mask.",
            PostCare = "May experience mild nasal tingling or dryness. Rinse nebulizer with sterile water after use. Report: wheezing, difficulty breathing, throat swelling.",
            StorageBefore = "Store nebulizer-ready solution at 2-8°C, use within 4h. LNP concentrate at -80°C stable 6+ months.",
            Equipment = ["Vibrating mesh nebulizer", "Facemask or nasal cannula", "Sterile water for rinse"],
        },
        DeliveryForm.IntranasalSpray => new()
        {
            Form = form,
            DisplayName = "Nasal Spray",
            Icon = "👃",
            ShortDescription = "Spray into nostrils",
            Preparation = "Prime spray device: pump 3-4 times until fine mist appears. Each spray delivers ~100uL (0.1mL).",
            Administration = "Tilt head slightly forward. Insert nozzle into one nostril, angle toward outer wall (not septum). Close other nostril. Spray while inhaling gently. Repeat in other nostril. Do NOT blow nose for 15 minutes.",
            PostCare = "Mild sneezing or runny nose is normal for 30 min. Report: severe nasal pain, nosebleed that won't stop, throat swelling.",
            StorageBefore = "Store at 2-8°C. Shake gently before each use. Discard 30 days after first use.",
            Equipment = ["Nasal spray device (metered-dose pump)"],
        },
        DeliveryForm.TopicalGel => new()
        {
            Form = form,
            DisplayName = "Topical Gel / Hydrogel",
            Icon = "🧴",
            ShortDescription = "Gel applied directly to wound/skin",
            Preparation = "Warm gel to room temperature (15 min). Gently mix — do NOT shake or vortex (damages LNPs in hydrogel). If wound: debride necrotic tissue before application.",
            Administration = "Using sterile gloves, apply gel evenly to wound bed or target skin area. Layer thickness: ~2mm. Cover with sterile non-adherent dressing (e.g., Tegaderm, Mepilex). Secure with tape or bandage.",
            PostCare = "Leave dressing in place 24-48h. Do NOT wash treated area for 24h. Change dressing every 2-3 days or when saturated. Monitor for signs of infection: redness spreading, pus, fever.",
            StorageBefore = "Hydrogel format: store 2-8°C, use within 24h of preparation. Discard if discolored or separated.",
            Equipment = ["Sterile gloves", "Wound debridement kit (if needed)", "Non-adherent dressing", "Medical tape", "Sterile gauze"],
        },
        DeliveryForm.MicroneedlePatch => new()
        {
            Form = form,
            DisplayName = "Microneedle Patch",
            Icon = "🩹",
            ShortDescription = "Dissolving microneedle patch pressed onto skin",
            Preparation = "Remove patch from sealed foil pouch. Allow to reach room temperature (30 min from -20°C). Do NOT touch needle side. Clean target skin with alcohol and let dry completely.",
            Administration = "Press patch firmly onto clean, dry skin at target area (scalp for hair, arm for vaccine). Apply even pressure with thumb for 30 seconds. Needles (600um) penetrate painlessly into upper dermis. Leave patch in place 30-60 minutes while needles dissolve. Peel off backing.",
            PostCare = "Mild redness at application site is normal (resolves in 1-2h). Tiny dots where needles entered fade within 24h. Do NOT apply moisturizer to treated area for 6h. Report: severe redness, swelling, or blistering.",
            StorageBefore = "Store at -20°C with desiccant in sealed foil. Stable 3+ months. Once opened, use within 1h.",
            Equipment = ["Alcohol swab", "Timer", "Mirror (for scalp application)"],
        },
        _ => new()
        {
            Form = form,
            DisplayName = form.ToString(),
            Icon = "💊",
            ShortDescription = form.ToString(),
            Preparation = "Follow manufacturer instructions.",
            Administration = "Follow healthcare provider guidance.",
            PostCare = "Monitor for adverse reactions.",
            StorageBefore = "Store as directed.",
            Equipment = [],
        },
    };
}

public sealed class DeliveryInfo
{
    public DeliveryForm Form { get; init; }
    public required string DisplayName { get; init; }
    public required string Icon { get; init; }
    public required string ShortDescription { get; init; }
    public required string Preparation { get; init; }
    public required string Administration { get; init; }
    public required string PostCare { get; init; }
    public required string StorageBefore { get; init; }
    public string[] Equipment { get; init; } = [];
}
