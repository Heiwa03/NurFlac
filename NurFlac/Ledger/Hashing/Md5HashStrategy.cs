using System.Security.Cryptography;

namespace NurFlac.Ledger.Hashing;

// ConcreteStrategy A
public sealed class Md5HashStrategy : IHashStrategy
{
    public string AlgorithmName => "MD5";

    public async Task<string> ComputeAsync(Stream data, CancellationToken ct = default)
    {
        var hash = await MD5.HashDataAsync(data, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
