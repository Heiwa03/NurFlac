using NurFlac.DuplicateChecking.Models;

namespace NurFlac.DuplicateChecking;

/// <summary>
/// Facade pattern: single entry point for duplicate checking and persistence.
/// </summary>
public sealed class DuplicateCheckFacade : IDuplicateCheckFacade
{
    private readonly IReadOnlyList<IAudioFingerprintProvider> _fingerprintProviders;
    private readonly IDuplicateFingerprintRepository _repository;

    public DuplicateCheckFacade(IEnumerable<IAudioFingerprintProvider> fingerprintProviders, IDuplicateFingerprintRepository repository)
    {
        _fingerprintProviders = fingerprintProviders.ToList();
        _repository = repository;
    }

    public async Task<DuplicateCheckResult> CheckAsync(string filePath, CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        foreach (var provider in _fingerprintProviders)
        {
            try
            {
                var fingerprint = await provider.CreateFingerprintAsync(filePath, cancellationToken);
                var exists = await _repository.ExistsAsync(fingerprint.Value, cancellationToken);
                return new DuplicateCheckResult(IsDuplicate: exists, Fingerprint: fingerprint);
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        throw new InvalidOperationException("No fingerprint provider could generate a fingerprint.", lastException);
    }

    public Task RegisterUploadedAsync(DuplicateCheckResult checkResult, string fileName, long uploadedByTelegramId, CancellationToken cancellationToken = default)
    {
        return _repository.AddUploadedEntryAsync(
            checkResult.Fingerprint.Value,
            fileName,
            checkResult.Fingerprint.ProviderName,
            uploadedByTelegramId,
            cancellationToken);
    }
}
