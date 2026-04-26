using Microsoft.Data.Sqlite;

namespace NurFlac.Entry;

public sealed class SqliteCommandExecutionTracker : ICommandExecutionTracker
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public SqliteCommandExecutionTracker(string dbPath)
    {
        var fullPath = Path.GetFullPath(dbPath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        _connectionString = $"Data Source={fullPath}";
    }

    public async Task TrackAsync(
        string commandKey,
        long telegramId,
        CommandExecutionOutcome outcome,
        long durationMs,
        string? error = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO CommandExecutionLog
(CommandKey, TelegramId, Outcome, DurationMs, Error, ExecutedAtUtc)
VALUES ($commandKey, $telegramId, $outcome, $durationMs, $error, $executedAtUtc);";

        cmd.Parameters.AddWithValue("$commandKey", commandKey);
        cmd.Parameters.AddWithValue("$telegramId", telegramId);
        cmd.Parameters.AddWithValue("$outcome", outcome.ToString());
        cmd.Parameters.AddWithValue("$durationMs", durationMs);
        cmd.Parameters.AddWithValue("$error", (object?)error ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$executedAtUtc", DateTimeOffset.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken);
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

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS CommandExecutionLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CommandKey TEXT NOT NULL,
    TelegramId INTEGER NOT NULL,
    Outcome TEXT NOT NULL,
    DurationMs INTEGER NOT NULL,
    Error TEXT NULL,
    ExecutedAtUtc TEXT NOT NULL
);";
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
