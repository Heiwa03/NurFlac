using System.Security.Cryptography;

namespace NurFlac.Ledger.Hashing;

// ConcreteStrategy B
public sealed class Sha256HashStrategy : IHashStrategy
{
    public string AlgorithmName => "SHA256";

    public async Task<string> ComputeAsync(Stream data, CancellationToken ct = default)
    {
        var hash = await SHA256.HashDataAsync(data, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
