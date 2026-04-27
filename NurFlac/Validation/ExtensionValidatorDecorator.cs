using NurFlac.AudioProcessing;

namespace NurFlac.Validation;

/// <summary>
/// Step 1 decorator — rejects the file if the extension is unknown or belongs to a lossy format.
/// Uses the <see cref="AudioFormatRegistry"/> flyweight to look up format metadata.
/// </summary>
public sealed class ExtensionValidatorDecorator : AudioValidatorDecorator
{
    private readonly AudioFormatRegistry _registry;

    public ExtensionValidatorDecorator(ILosslessAudioValidator inner, AudioFormatRegistry registry)
        : base(inner)
    {
        _registry = registry;
    }

    public override async Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken cancellationToken = default)
    {
        var format = _registry.GetByExtension(context.Extension);

        if (format is null)
            return ValidationResult.Reject(
                $"Unsupported file format '{context.Extension}'. Only lossless audio files are accepted (FLAC, WAV, ALAC, AIFF).");

        if (!format.IsLossless)
            return ValidationResult.Reject(
                $"{format.DisplayName} is a lossy format and cannot be accepted. Please send a lossless audio file (FLAC, WAV, ALAC, AIFF).");

        return await Inner.ValidateAsync(context, cancellationToken);
    }
}
