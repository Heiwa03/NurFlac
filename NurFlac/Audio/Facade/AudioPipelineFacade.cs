// ============================================================
// PATTERN: Facade (Structural)
// Role   : Facade — provides a single simplified entry-point
//          over the 3-step validation Chain of Responsibility
//          (Extension → MIME → Spectral). Callers never deal
//          with handler wiring or chain traversal directly.
// ============================================================
using NurFlac.Audio.Factories;
using NurFlac.Audio.Models;
using NurFlac.Audio.Pipeline;
using Microsoft.Extensions.Logging;

namespace NurFlac.Audio.Facade;

public sealed class AudioPipelineFacade
{
    private readonly IAudioValidationHandler _chain;
    private readonly ILogger<AudioPipelineFacade> _logger;

    public AudioPipelineFacade(
        AudioFormatRegistry   registry,
        IAudioAnalyzerFactory analyzerFactory,
        ILoggerFactory        loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AudioPipelineFacade>();

        var extension = new ExtensionValidationHandler(registry,
            loggerFactory.CreateLogger<ExtensionValidationHandler>());
        var mime      = new MimeValidationHandler(registry,
            loggerFactory.CreateLogger<MimeValidationHandler>());
        var spectral  = new SpectralValidationHandler(analyzerFactory,
            loggerFactory.CreateLogger<SpectralValidationHandler>());

        extension.SetNext(mime).SetNext(spectral);
        _chain = extension;
    }

    public async Task<ValidationResult> ValidateAsync(AudioFileContext context, CancellationToken ct = default)
    {
        _logger.LogDebug("[FACADE] Pipeline entry — file='{File}' ext='{Ext}' mime='{Mime}' localPath='{Path}'",
            context.FileName, context.Extension, context.MimeType ?? "(none)",
            context.LocalFilePath ?? "(not downloaded yet)");

        var result = await _chain.HandleAsync(context, ct);

        _logger.LogDebug("[FACADE] Pipeline exit — file='{File}' valid={Valid} reason='{Reason}'",
            context.FileName, result.IsValid, result.RejectionReason ?? "—");

        return result;
    }
}
