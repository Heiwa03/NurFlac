using System.Security.Cryptography;

namespace NurFlac.DuplicateChecking.ExternalApi;

/// <summary>
/// Simulates an external fingerprint API call. Replace internals with real HTTP call when endpoint is available.
/// </summary>
public sealed class ExternalFingerprintHttpApi : IExternalFingerprintApi
{
    public async Task<string> GenerateFingerprintAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        var hashBytes = await SHA1.HashDataAsync(stream, cancellationToken);
        return $"external:{Convert.ToHexString(hashBytes)}";
    }
}
