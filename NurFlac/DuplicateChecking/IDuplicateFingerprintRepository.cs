namespace NurFlac.DuplicateChecking;

public interface IDuplicateFingerprintRepository
{
    Task<bool> ExistsAsync(string fingerprint, CancellationToken cancellationToken = default);
    Task AddUploadedEntryAsync(string fingerprint, string fileName, string providerName, long uploadedByTelegramId, CancellationToken cancellationToken = default);
}
