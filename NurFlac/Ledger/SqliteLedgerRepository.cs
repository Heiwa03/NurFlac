using Microsoft.Data.Sqlite;

namespace NurFlac.Ledger;

public sealed class SqliteLedgerRepository : ILedgerRepository
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public SqliteLedgerRepository(string dbPath)
    {
        var full = Path.GetFullPath(dbPath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        _connectionString = $"Data Source={full}";
    }

    public async Task RecordAsync(LedgerEntry entry, CancellationToken ct = default)
    {
        await EnsureInitAsync(ct);
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT OR IGNORE INTO Ledger (FileName, FileHash, HashAlgorithm, TelegramId, UploadedAtUtc)
            VALUES ($fn, $hash, $algo, $tid, $ts)
            """;
        cmd.Parameters.AddWithValue("$fn",   entry.FileName);
        cmd.Parameters.AddWithValue("$hash", entry.FileHash);
        cmd.Parameters.AddWithValue("$algo", entry.HashAlgorithm);
        cmd.Parameters.AddWithValue("$tid",  entry.UploadedByTelegramId);
        cmd.Parameters.AddWithValue("$ts",   entry.UploadedAtUtc.ToString("O"));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> ExistsAsync(string fileHash, CancellationToken ct = default)
    {
        await EnsureInitAsync(ct);
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Ledger WHERE FileHash = $hash";
        cmd.Parameters.AddWithValue("$hash", fileHash);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(result) > 0;
    }

    public async Task<int> ClearAsync(CancellationToken ct = default)
    {
        await EnsureInitAsync(ct);
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Ledger";
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task EnsureInitAsync(CancellationToken ct)
    {
        if (_initialized) return;
        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized) return;
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Ledger (
                    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    FileName      TEXT NOT NULL,
                    FileHash      TEXT NOT NULL UNIQUE,
                    HashAlgorithm TEXT NOT NULL,
                    TelegramId    INTEGER NOT NULL,
                    UploadedAtUtc TEXT NOT NULL
                );
                """;
            await cmd.ExecuteNonQueryAsync(ct);
            _initialized = true;
        }
        finally { _initLock.Release(); }
    }
}
