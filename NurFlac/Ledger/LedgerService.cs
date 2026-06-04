// Context class that holds and uses the IHashStrategy (Strategy pattern).
using NurFlac.Ledger.Hashing;

namespace NurFlac.Ledger;

public sealed class LedgerService(ILedgerRepository repository, IHashStrategy hashStrategy)
{
    public async Task<bool> IsDuplicateAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await hashStrategy.ComputeAsync(stream, ct);
        return await repository.ExistsAsync(hash, ct);
    }

    public async Task RecordAsync(string filePath, long telegramId, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await hashStrategy.ComputeAsync(stream, ct);
        var entry = new LedgerEntry(
            FileName:              Path.GetFileName(filePath),
            FileHash:              hash,
            HashAlgorithm:         hashStrategy.AlgorithmName,
            UploadedByTelegramId:  telegramId,
            UploadedAtUtc:         DateTime.UtcNow);
        await repository.RecordAsync(entry, ct);
    }

    public Task<int> ClearAsync(CancellationToken ct = default)
        => repository.ClearAsync(ct);
}
