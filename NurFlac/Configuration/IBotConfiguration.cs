namespace NurFlac.Configuration;

public interface IBotConfiguration
{
    string              BotToken        { get; }
    IReadOnlyList<long> AdminIds        { get; }
    string?             LocalApiBaseUrl { get; }
    string?             LocalApiPath    { get; }
    string?             NginxUrl        { get; }
    string              LedgerDbPath    { get; }
    string              UsersDbPath     { get; }
    string              HashStrategy    { get; }
    bool                IsAdmin(long telegramId);
}
