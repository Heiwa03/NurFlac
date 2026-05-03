using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using NurFlac.UserManagement.Entities;

namespace NurFlac.UserManagement;

public class SqliteUserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public SqliteUserRepository(IConfiguration configuration)
    {
        var dbPath = configuration["UserManagement:SqlitePath"] ?? "Data/nurflac-users.db";
        var fullPath = Path.GetFullPath(dbPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
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
            
            // Migration: Create table if not exists with all columns
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
    TelegramId INTEGER PRIMARY KEY,
    Status TEXT NOT NULL,
    StrikeCount INTEGER NOT NULL,
    TimeoutUntil TEXT
);";
            await command.ExecuteNonQueryAsync();

            // Safety check: Ensure TimeoutUntil column exists if table already existed without it
            try
            {
                command.CommandText = "ALTER TABLE Users ADD COLUMN TimeoutUntil TEXT;";
                await command.ExecuteNonQueryAsync();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("duplicate column name"))
            {
                // Column already exists, ignore
            }

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<User> GetOrCreateByTelegramIdAsync(long telegramId)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Status, StrikeCount, TimeoutUntil FROM Users WHERE TelegramId = $id";
        selectCmd.Parameters.AddWithValue("$id", telegramId);
        
        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                TelegramId = telegramId,
                Status = Enum.Parse<UserStatus>(reader.GetString(0)),
                StrikeCount = reader.GetInt32(1),
                TimeoutUntil = reader.IsDBNull(2) ? null : DateTime.Parse(reader.GetString(2))
            };
        }

        var newUser = new User { TelegramId = telegramId, Status = UserStatus.Whitelisted, StrikeCount = 0 };
        await using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Users (TelegramId, Status, StrikeCount, TimeoutUntil) VALUES ($id, $status, $strikes, $timeout)";
        insertCmd.Parameters.AddWithValue("$id", telegramId);
        insertCmd.Parameters.AddWithValue("$status", newUser.Status.ToString());
        insertCmd.Parameters.AddWithValue("$strikes", newUser.StrikeCount);
        insertCmd.Parameters.AddWithValue("$timeout", DBNull.Value);
        await insertCmd.ExecuteNonQueryAsync();
        
        return newUser;
    }

    public async Task UpdateUserAsync(User user)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Users SET Status = $status, StrikeCount = $strikes, TimeoutUntil = $timeout WHERE TelegramId = $id";
        command.Parameters.AddWithValue("$id", user.TelegramId);
        command.Parameters.AddWithValue("$status", user.Status.ToString());
        command.Parameters.AddWithValue("$strikes", user.StrikeCount);
        command.Parameters.AddWithValue("$timeout", (object?)user.TimeoutUntil?.ToString("O") ?? DBNull.Value);
        
        await command.ExecuteNonQueryAsync();
    }
}
