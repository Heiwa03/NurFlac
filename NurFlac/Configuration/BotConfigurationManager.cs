// ============================================================
// PATTERN: Singleton (Creational)
// Role   : Single authoritative source for all bot configuration.
//          Merges appsettings.json with NURFLAC_* env-var overrides.
// ============================================================
using Microsoft.Extensions.Configuration;

namespace NurFlac.Configuration;

public sealed class BotConfigurationManager : IBotConfiguration
{
    // Thread-safe lazy initialisation — the canonical Singleton approach in .NET.
    private static readonly Lazy<BotConfigurationManager> _instance =
        new(static () => new BotConfigurationManager(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static IBotConfiguration Instance => _instance.Value;

    // ── Private fields ────────────────────────────────────────
    private readonly IConfiguration _config;

    private BotConfigurationManager()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables("NURFLAC_")
            .Build();
    }

    // ── IBotConfiguration ─────────────────────────────────────
    public string BotToken =>
        _config["TelegramBot:Token"]
        ?? throw new InvalidOperationException("TelegramBot:Token is not configured.");

    public IReadOnlyList<long> AdminIds =>
        _config.GetSection("TelegramBot:AdminIds").Get<long[]>() ?? [];

    public string? LocalApiBaseUrl => _config["TelegramBot:LocalApiBaseUrl"];
    public string? LocalApiPath    => _config["TelegramBot:LocalApi:LocalPath"];
    public string? NginxUrl        => _config["TelegramBot:LocalApi:NginxUrl"];

    public string LedgerDbPath =>
        _config["Ledger:SqlitePath"] ?? "Data/nurflac-ledger.db";

    public string UsersDbPath =>
        _config["UserManagement:SqlitePath"] ?? "Data/nurflac-users.db";

    public string HashStrategy =>
        _config["Ledger:HashStrategy"] ?? "SHA256";

    public bool IsAdmin(long telegramId) =>
        AdminIds.Contains(telegramId);
}
