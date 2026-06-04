// ============================================================
// PATTERN: Chain of Responsibility (Behavioral)
// Role   : Handler — declares the interface for all pipeline
//          validation steps. Each handler either rejects the
//          context or passes it to the next handler in chain.
// ============================================================
using NurFlac.Audio.Models;

namespace NurFlac.Audio.Pipeline;

public interface IAudioValidationHandler
{
    IAudioValidationHandler SetNext(IAudioValidationHandler next);
    Task<ValidationResult>  HandleAsync(AudioFileContext context, CancellationToken ct = default);
}
