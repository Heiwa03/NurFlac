using Microsoft.Data.Sqlite;
using NurFlac.Handlers.Models;

namespace NurFlac.Handlers;

public class SqliteUploadSessionCaretaker : IUploadSessionCaretaker
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public SqliteUploadSessionCaretaker(IConfiguration configuration)
    {
        var dbPath = configuration["UploadRecovery:SqlitePath"] ?? "Data/nurflac-recovery.db";
        var fullPath = Path.GetFullPath(dbPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
        _connectionString = $"Data Source={fullPath}";
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS UploadSessions (
    SessionId TEXT PRIMARY KEY,
    TelegramId INTEGER NOT NULL,
    ChatId INTEGER NOT NULL,
    FileId TEXT NOT NULL,
    FileName TEXT NOT NULL,
    LocalFilePath TEXT,
    Status TEXT NOT NULL,
    CreatedAt TEXT NOT NULL
);";
            await command.ExecuteNonQueryAsync();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task SaveMementoAsync(UploadSessionMemento memento)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT OR REPLACE INTO UploadSessions 
(SessionId, TelegramId, ChatId, FileId, FileName, LocalFilePath, Status, CreatedAt)
VALUES ($id, $tgId, $chatId, $fileId, $fileName, $localPath, $status, $createdAt);";
        
        command.Parameters.AddWithValue("$id", memento.SessionId);
        command.Parameters.AddWithValue("$tgId", memento.TelegramId);
        command.Parameters.AddWithValue("$chatId", memento.ChatId);
        command.Parameters.AddWithValue("$fileId", memento.FileId);
        command.Parameters.AddWithValue("$fileName", memento.FileName);
        command.Parameters.AddWithValue("$localPath", (object?)memento.LocalFilePath ?? DBNull.Value);
        command.Parameters.AddWithValue("$status", memento.Status.ToString());
        command.Parameters.AddWithValue("$createdAt", memento.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<UploadSessionMemento>> GetPendingSessionsAsync()
    {
        await EnsureInitializedAsync();
        var sessions = new List<UploadSessionMemento>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM UploadSessions WHERE Status != 'Completed' AND Status != 'Failed';";
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            sessions.Add(new UploadSessionMemento(
                reader.GetString(0),
                reader.GetInt64(1),
                reader.GetInt64(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                Enum.Parse<UploadStatus>(reader.GetString(6)),
                DateTime.Parse(reader.GetString(7))
            ));
        }
        return sessions;
    }

    public async Task RemoveMementoAsync(string sessionId)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM UploadSessions WHERE SessionId = $id;";
        command.Parameters.AddWithValue("$id", sessionId);
        await command.ExecuteNonQueryAsync();
    }
}
