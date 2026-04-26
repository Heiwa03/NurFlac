using NurFlac.DuplicateChecking.Models;

namespace NurFlac.DuplicateChecking.ExternalApi;

/// <summary>
/// Adapter pattern: converts an external API contract to the app-wide <see cref="IAudioFingerprintProvider"/> contract.
/// </summary>
public sealed class ExternalFingerprintApiAdapter : IAudioFingerprintProvider
{
    private readonly IExternalFingerprintApi _externalApi;

    public ExternalFingerprintApiAdapter(IExternalFingerprintApi externalApi)
    {
        _externalApi = externalApi;
    }

    public async Task<AudioFingerprint> CreateFingerprintAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var externalFingerprint = await _externalApi.GenerateFingerprintAsync(filePath, cancellationToken);
        return new AudioFingerprint(externalFingerprint, ProviderName: "external-api");
    }
}
