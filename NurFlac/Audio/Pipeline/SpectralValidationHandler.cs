// ConcreteHandler 3 — performs FFT spectral analysis to reject transcodes.
using NurFlac.Audio.Factories;
using NurFlac.Audio.Models;
using Microsoft.Extensions.Logging;

namespace NurFlac.Audio.Pipeline;

public sealed class SpectralValidationHandler(
    IAudioAnalyzerFactory analyzerFactory,
    ILogger<SpectralValidationHandler> logger) : AudioValidationHandler
{
    public override async Task<ValidationResult> HandleAsync(AudioFileContext context, CancellationToken ct = default)
    {
        // Spectral analysis requires the file to be locally present.
        if (context.LocalFilePath is null)
            return await base.HandleAsync(context, ct);

        var analyzer = analyzerFactory.CreateForExtension(context.Extension);
        if (analyzer is null)
            return ValidationResult.Reject($"No spectral analyzer available for '{context.Extension}'.");

        logger.LogInformation("[SPECTRAL] Scanning {File}", context.FileName);

        var result = await analyzer.AnalyzeAsync(context.LocalFilePath, ct);

        logger.LogInformation(
            "[SPECTRAL] {File}: lossless={L}, cutoff={C:F0}Hz, ratio={R:P2} — {Note}",
            context.FileName, result.IsTrueLossless, result.DetectedCutoffHz,
            result.HighFreqEnergyRatio, result.AnalysisNote);

        if (!result.IsTrueLossless)
            return ValidationResult.Reject(
                $"Spectral analysis rejected the file. Cutoff: {result.DetectedCutoffHz:F0} Hz, " +
                $"Energy ratio: {result.HighFreqEnergyRatio:P2}. {result.AnalysisNote}");

        return await base.HandleAsync(context, ct);
    }
}
