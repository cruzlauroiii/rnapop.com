namespace Core.Validation;

using Models;

public sealed class ValidationError
{
    public required string Field { get; init; }
    public required string Message { get; init; }
}

public static class InputValidator
{
    public static List<ValidationError> Validate(BiomarkerInput input)
    {
        var errors = new List<ValidationError>();

        // Demographics
        if (input.Demographics.ChronologicalAge is < 1 or > 130)
            errors.Add(new() { Field = "Age", Message = "Age must be between 1 and 130." });
        if (input.Demographics.SmokingPackYears < 0)
            errors.Add(new() { Field = "SmokingPackYears", Message = "Cannot be negative." });

        // Blood panel (required)
        var b = input.Blood;
        ValidateRange(errors, b.Albumin, 0.5, 8.0, "Albumin");
        ValidateRange(errors, b.AlkalinePhosphatase, 5, 1000, "ALP");
        ValidateRange(errors, b.Creatinine, 0.1, 20, "Creatinine");
        ValidateRange(errors, b.CReactiveProtein, 0.001, 50, "CRP");
        ValidateRange(errors, b.FastingGlucose, 20, 600, "Glucose");
        ValidateRange(errors, b.WhiteBloodCellCount, 0.5, 50, "WBC");
        ValidateRange(errors, b.LymphocytePercent, 1, 80, "Lymphocyte%");
        ValidateRange(errors, b.MeanCellVolume, 50, 130, "MCV");
        ValidateRange(errors, b.RedCellDistWidth, 8, 30, "RDW");

        // Telomere
        if (input.Telomere.LeukocyteTelomereLength is < 0 or > 20)
            errors.Add(new() { Field = "LTL", Message = "Telomere length must be 0-20 kb." });

        return errors;
    }

    private static void ValidateRange(List<ValidationError> errors, double val, double min, double max, string name)
    {
        if (val < min || val > max)
            errors.Add(new() { Field = name, Message = $"{name} must be between {min} and {max}." });
    }
}
