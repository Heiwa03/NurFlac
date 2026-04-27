using NurFlac.AudioProcessing;

namespace NurFlac.Validation;

/// <summary>
/// Step 2 decorator — validates the MIME type reported by Telegram against known lossless formats.
/// Passes through if MimeType is null (Telegram did not provide one).
/// </summary>
public sealed class MimeValidatorDecorator : AudioValidatorDecorator
{
    private readonly AudioFormatRegistry _registry;

    public MimeValidatorDecorator(ILosslessAudioValidator inner, AudioFormatRegistry registry)
        : base(inner)
    {
        _registry = registry;
    }

    public override async Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken cancellationToken = default)
    {
        if (context.MimeType is null)
            return await Inner.ValidateAsync(context, cancellationToken);

        var format = _registry.GetByMimeType(context.MimeType);

        if (format is null)
            return ValidationResult.Reject(
                $"Unrecognized MIME type '{context.MimeType}'. Cannot verify the file is lossless.");

        if (!format.IsLossless)
            return ValidationResult.Reject(
                $"MIME type '{context.MimeType}' belongs to the lossy format {format.DisplayName}.");

        return await Inner.ValidateAsync(context, cancellationToken);
    }
}
