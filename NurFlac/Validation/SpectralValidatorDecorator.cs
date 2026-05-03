using NurFlac.AudioProcessing.Interfaces;
using Microsoft.Extensions.Logging;

namespace NurFlac.Validation;

public sealed class SpectralValidatorDecorator : AudioValidatorDecorator
{
    private readonly IAudioProcessor _audioProcessor;
    private readonly ILogger<SpectralValidatorDecorator> _logger;

    public SpectralValidatorDecorator(
        ILosslessAudioValidator inner, 
        IAudioProcessor audioProcessor,
        ILogger<SpectralValidatorDecorator> logger)
        : base(inner)
    {
        _audioProcessor = audioProcessor;
        _logger = logger;
    }

    public override async Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken cancellationToken = default)
    {
        var innerResult = await Inner.ValidateAsync(context, cancellationToken);
        if (!innerResult.IsValid)
            return innerResult;

        if (context.LocalFilePath is null)
            return ValidationResult.Valid();

        _logger.LogInformation("[SPECTRAL] Triggering automated scan for: {FileName}", context.FileName);
        
        var result = await _audioProcessor.AnalyzeLosslessQualityAsync(context.LocalFilePath);

        _logger.LogInformation("[SPECTRAL] Result for {FileName}: {Status}. Detected Cutoff: {Cutoff}Hz. Energy Ratio: {Ratio:P2}. Note: {Note}", 
            context.FileName, 
            result.IsTrueLossless ? "SUCCESS" : "REJECTED", 
            result.DetectedCutoffHz, 
            result.HighFreqEnergyRatio,
            result.AnalysisNote);

        if (!result.IsTrueLossless)
            return ValidationResult.Reject(
                $"Spectral analysis indicates this file is a transcode. Detected Cutoff: {result.DetectedCutoffHz}Hz. Energy Ratio: {result.HighFreqEnergyRatio:P2}. {result.AnalysisNote}");

        return ValidationResult.Valid();
    }
}
