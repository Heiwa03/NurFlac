// RealSubject — direct SQLite access, no caching.
using Microsoft.Data.Sqlite;
using NurFlac.Users.Entities;

namespace NurFlac.Users;

public sealed class SqliteUserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _init = new(1, 1);
    private bool _ready;

    public SqliteUserRepository(string dbPath)
    {
        var full = Path.GetFullPath(dbPath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        _connectionString = $"Data Source={full}";
    }

    public async Task<User> GetOrCreateAsync(long telegramId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var sel = conn.CreateCommand();
        sel.CommandText = "SELECT Status, StrikeCount, TimeoutUntil FROM Users WHERE TelegramId = $id";
        sel.Parameters.AddWithValue("$id", telegramId);
        await using var reader = await sel.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return new User
            {
                TelegramId   = telegramId,
                Status       = Enum.Parse<UserStatus>(reader.GetString(0)),
                StrikeCount  = reader.GetInt32(1),
                TimeoutUntil = reader.IsDBNull(2) ? null : DateTime.Parse(reader.GetString(2))
            };
        }

        var newUser = new User { TelegramId = telegramId };
        await using var ins = conn.CreateCommand();
        ins.CommandText = "INSERT INTO Users (TelegramId, Status, StrikeCount) VALUES ($id, $s, 0)";
        ins.Parameters.AddWithValue("$id", telegramId);
        ins.Parameters.AddWithValue("$s",  newUser.Status.ToString());
        await ins.ExecuteNonQueryAsync(ct);
        return newUser;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "UPDATE Users SET Status=$s, StrikeCount=$sc, TimeoutUntil=$tu WHERE TelegramId=$id";
        cmd.Parameters.AddWithValue("$id", user.TelegramId);
        cmd.Parameters.AddWithValue("$s",  user.Status.ToString());
        cmd.Parameters.AddWithValue("$sc", user.StrikeCount);
        cmd.Parameters.AddWithValue("$tu", (object?)user.TimeoutUntil?.ToString("O") ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<int> ClearAllAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users";
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task ResetAsync(long telegramId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "UPDATE Users SET Status='Active', StrikeCount=0, TimeoutUntil=NULL WHERE TelegramId=$id";
        cmd.Parameters.AddWithValue("$id", telegramId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task EnsureSchemaAsync(CancellationToken ct)
    {
        if (_ready) return;
        await _init.WaitAsync(ct);
        try
        {
            if (_ready) return;
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Users (
                    TelegramId   INTEGER PRIMARY KEY,
                    Status       TEXT    NOT NULL DEFAULT 'Active',
                    StrikeCount  INTEGER NOT NULL DEFAULT 0,
                    TimeoutUntil TEXT
                );
                """;
            await cmd.ExecuteNonQueryAsync(ct);
            _ready = true;
        }
        finally { _init.Release(); }
    }
}
