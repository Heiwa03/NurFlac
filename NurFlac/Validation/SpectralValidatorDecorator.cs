using NurFlac.AudioProcessing.Interfaces;

namespace NurFlac.Validation;

/// <summary>
/// Step 3 decorator — runs spectral analysis to detect lossy-to-lossless transcodes.
/// Skips if <see cref="AudioFileContext.LocalFilePath"/> is null (file not yet downloaded).
/// </summary>
public sealed class SpectralValidatorDecorator : AudioValidatorDecorator
{
    private readonly IAudioProcessor _audioProcessor;

    public SpectralValidatorDecorator(ILosslessAudioValidator inner, IAudioProcessor audioProcessor)
        : base(inner)
    {
        _audioProcessor = audioProcessor;
    }

    public override async Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken cancellationToken = default)
    {
        if (context.LocalFilePath is null)
            return await Inner.ValidateAsync(context, cancellationToken);

        var isLossless = await _audioProcessor.VerifyLosslessQualityAsync(context.LocalFilePath);

        if (!isLossless)
            return ValidationResult.Reject(
                "Spectral analysis indicates this file is a lossy-to-lossless transcode and cannot be accepted.");

        return await Inner.ValidateAsync(context, cancellationToken);
    }
}
