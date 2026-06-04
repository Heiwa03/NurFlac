namespace NurFlac.Ledger;

public sealed record LedgerEntry(
    string   FileName,
    string   FileHash,
    string   HashAlgorithm,
    long     UploadedByTelegramId,
    DateTime UploadedAtUtc);
