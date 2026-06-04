// AbstractHandler — wires the chain and provides the pass-through default.
using NurFlac.Audio.Models;

namespace NurFlac.Audio.Pipeline;

public abstract class AudioValidationHandler : IAudioValidationHandler
{
    private IAudioValidationHandler? _next;

    public IAudioValidationHandler SetNext(IAudioValidationHandler next)
    {
        _next = next;
        return next;
    }

    public virtual async Task<ValidationResult> HandleAsync(AudioFileContext context, CancellationToken ct = default)
    {
        if (_next is not null)
            return await _next.HandleAsync(context, ct);

        return ValidationResult.Valid();
    }
}
