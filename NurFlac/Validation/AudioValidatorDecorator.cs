namespace NurFlac.Validation;

/// <summary>
/// Abstract decorator base — wraps an inner <see cref="ILosslessAudioValidator"/>
/// and delegates to it after adding its own validation step.
/// </summary>
public abstract class AudioValidatorDecorator : ILosslessAudioValidator
{
    protected readonly ILosslessAudioValidator Inner;

    protected AudioValidatorDecorator(ILosslessAudioValidator inner)
    {
        Inner = inner;
    }

    public abstract Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken cancellationToken = default);
}
