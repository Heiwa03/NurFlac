namespace NurFlac.Validation;

/// <summary>
/// Component interface for the Decorator pattern.
/// Both the base component and all decorators implement this interface.
/// </summary>
public interface ILosslessAudioValidator
{
    Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken cancellationToken = default);
}
