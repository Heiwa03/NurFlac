// ConcreteHandler 1 — verifies file extension is a known lossless format.
using NurFlac.Audio.Models;
using Microsoft.Extensions.Logging;

namespace NurFlac.Audio.Pipeline;

public sealed class ExtensionValidationHandler(
    AudioFormatRegistry registry,
    ILogger<ExtensionValidationHandler> logger) : AudioValidationHandler
{
    public override async Task<ValidationResult> HandleAsync(AudioFileContext context, CancellationToken ct = default)
    {
        logger.LogDebug("[EXT] Checking extension='{Ext}' for file='{File}'",
            context.Extension, context.FileName);

        if (string.IsNullOrEmpty(context.Extension))
        {
            var msg = $"File '{context.FileName}' has no extension and no MIME type could be resolved. " +
                      "Cannot determine format.";
            logger.LogInformation("[EXT] REJECT — no extension: {File}", context.FileName);
            return ValidationResult.Reject(msg);
        }

        var format = registry.GetByExtension(context.Extension);

        if (format is null)
        {
            logger.LogInformation("[EXT] REJECT — unknown extension '{Ext}' for '{File}'",
                context.Extension, context.FileName);
            return ValidationResult.Reject(
                $"Unsupported file format '{context.Extension}'. Accepted formats: FLAC, WAV, ALAC (.m4a), AIFF.");
        }

        if (!format.IsLossless)
        {
            logger.LogInformation("[EXT] REJECT — lossy format '{Name}' ({Ext}) for '{File}'",
                format.DisplayName, context.Extension, context.FileName);
            return ValidationResult.Reject(
                $"'{format.DisplayName}' is a lossy format. Please upload a lossless file (FLAC, WAV, ALAC, AIFF).");
        }

        logger.LogDebug("[EXT] PASS — '{Ext}' recognised as lossless '{Name}'",
            context.Extension, format.DisplayName);
        return await base.HandleAsync(context, ct);
    }
}
