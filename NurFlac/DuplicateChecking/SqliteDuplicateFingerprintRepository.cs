using Microsoft.Data.Sqlite;

namespace NurFlac.DuplicateChecking;

public sealed class SqliteDuplicateFingerprintRepository : IDuplicateFingerprintRepository
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public SqliteDuplicateFingerprintRepository(string dbPath)
    {
        var fullPath = Path.GetFullPath(dbPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = $"Data Source={fullPath}";
    }

    public async Task<bool> ExistsAsync(string fingerprint, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT 1 FROM UploadedAudioEntries WHERE Fingerprint = $fingerprint LIMIT 1);";
        command.Parameters.AddWithValue("$fingerprint", fingerprint);

        var result = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return result == 1;
    }

    public async Task AddUploadedEntryAsync(string fingerprint, string fileName, string providerName, long uploadedByTelegramId, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT OR IGNORE INTO UploadedAudioEntries
(Fingerprint, FileName, ProviderName, UploadedByTelegramId, UploadedAtUtc)
VALUES ($fingerprint, $fileName, $providerName, $uploadedByTelegramId, $uploadedAtUtc);";
        command.Parameters.AddWithValue("$fingerprint", fingerprint);
        command.Parameters.AddWithValue("$fileName", fileName);
        command.Parameters.AddWithValue("$providerName", providerName);
        command.Parameters.AddWithValue("$uploadedByTelegramId", uploadedByTelegramId);
        command.Parameters.AddWithValue("$uploadedAtUtc", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS UploadedAudioEntries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Fingerprint TEXT NOT NULL UNIQUE,
    FileName TEXT NOT NULL,
    ProviderName TEXT NOT NULL,
    UploadedByTelegramId INTEGER NOT NULL,
    UploadedAtUtc TEXT NOT NULL
);";
            await command.ExecuteNonQueryAsync(cancellationToken);
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
