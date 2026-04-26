using NurFlac.DuplicateChecking.Models;

namespace NurFlac.DuplicateChecking;

public interface IAudioFingerprintProvider
{
    Task<AudioFingerprint> CreateFingerprintAsync(string filePath, CancellationToken cancellationToken = default);
}
