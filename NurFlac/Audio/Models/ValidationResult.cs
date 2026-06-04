namespace NurFlac.Audio.Models;

public sealed class ValidationResult
{
    private ValidationResult(bool isValid, string? reason)
    {
        IsValid          = isValid;
        RejectionReason  = reason;
    }

    public bool    IsValid         { get; }
    public string? RejectionReason { get; }

    public static ValidationResult Valid()               => new(true, null);
    public static ValidationResult Reject(string reason) => new(false, reason);
}
