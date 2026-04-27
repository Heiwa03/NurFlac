namespace NurFlac.Validation;

public sealed record ValidationResult(bool IsValid, string? RejectionReason)
{
    public static ValidationResult Valid() => new(true, null);
    public static ValidationResult Reject(string reason) => new(false, reason);
}
