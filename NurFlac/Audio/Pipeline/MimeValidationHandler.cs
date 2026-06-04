// ConcreteHandler 2 — cross-checks Telegram MIME type against known lossless formats.
using NurFlac.Audio.Models;
using Microsoft.Extensions.Logging;

namespace NurFlac.Audio.Pipeline;

public sealed class MimeValidationHandler(
    AudioFormatRegistry registry,
    ILogger<MimeValidationHandler> logger) : AudioValidationHandler
{
    public override async Task<ValidationResult> HandleAsync(AudioFileContext context, CancellationToken ct = default)
    {
        if (context.MimeType is null)
        {
            logger.LogDebug("[MIME] No MIME type present for '{File}' — skipping MIME check", context.FileName);
            return await base.HandleAsync(context, ct);
        }

        logger.LogDebug("[MIME] Checking mime='{Mime}' for '{File}'", context.MimeType, context.FileName);

        var format = registry.GetByMimeType(context.MimeType);

        if (format is null)
        {
            logger.LogInformation("[MIME] REJECT — unrecognised MIME '{Mime}' for '{File}'",
                context.MimeType, context.FileName);
            return ValidationResult.Reject(
                $"Unrecognized MIME type '{context.MimeType}'. Cannot verify lossless integrity.");
        }

        if (!format.IsLossless)
        {
            logger.LogInformation("[MIME] REJECT — lossy MIME '{Mime}' ({Name}) for '{File}'",
                context.MimeType, format.DisplayName, context.FileName);
            return ValidationResult.Reject(
                $"MIME type '{context.MimeType}' belongs to the lossy format '{format.DisplayName}'.");
        }

        logger.LogDebug("[MIME] PASS — '{Mime}' is lossless '{Name}'", context.MimeType, format.DisplayName);
        return await base.HandleAsync(context, ct);
    }
}
