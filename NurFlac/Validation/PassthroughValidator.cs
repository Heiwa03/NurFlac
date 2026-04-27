namespace NurFlac.Validation;

/// <summary>
/// Concrete component — the innermost element of the Decorator chain.
/// Always returns valid; decorators wrap this to add validation steps.
/// </summary>
public sealed class PassthroughValidator : ILosslessAudioValidator
{
    public Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(ValidationResult.Valid());
}
