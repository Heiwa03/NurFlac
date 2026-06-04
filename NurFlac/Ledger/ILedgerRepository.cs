namespace NurFlac.Ledger;

public interface ILedgerRepository
{
    Task RecordAsync(LedgerEntry entry, CancellationToken ct = default);
    Task<bool> ExistsAsync(string fileHash, CancellationToken ct = default);
    Task<int> ClearAsync(CancellationToken ct = default);
}
