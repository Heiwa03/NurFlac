using Microsoft.Extensions.Caching.Memory;
using NurFlac.Album;
using NurFlac.Audio.Abstractions;
using NurFlac.Audio.Adapters;
using NurFlac.Audio.Facade;
using NurFlac.Audio.Factories;
using NurFlac.Audio.Models;
using NurFlac.Commands;
using NurFlac.Commands.Factory;
using NurFlac.Configuration;
using NurFlac.Infrastructure.Telegram;
using NurFlac.Ledger;
using NurFlac.Ledger.Hashing;
using NurFlac.Storage;
using NurFlac.Users;
using Telegram.Bot;

namespace NurFlac.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNurFlac(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        // ── Singleton: BotConfigurationManager ───────────────────────────
        services.AddSingleton<IBotConfiguration>(_ => BotConfigurationManager.Instance);

        // ── Telegram Bot Client ───────────────────────────────────────────
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var cfg  = sp.GetRequiredService<IBotConfiguration>();
            return string.IsNullOrWhiteSpace(cfg.LocalApiBaseUrl)
                ? new TelegramBotClient(cfg.BotToken)
                : new TelegramBotClient(new TelegramBotClientOptions(cfg.BotToken, cfg.LocalApiBaseUrl));
        });

        // ── Strategy: Hash strategy selected from configuration ───────────
        services.AddSingleton<IHashStrategy>(sp =>
        {
            var cfg = sp.GetRequiredService<IBotConfiguration>();
            return cfg.HashStrategy.ToUpperInvariant() == "MD5"
                ? (IHashStrategy)new Md5HashStrategy()
                : new Sha256HashStrategy();
        });

        // ── Ledger ────────────────────────────────────────────────────────
        services.AddSingleton<ILedgerRepository>(sp =>
            new SqliteLedgerRepository(sp.GetRequiredService<IBotConfiguration>().LedgerDbPath));
        services.AddSingleton<LedgerService>();

        // ── Proxy: CachingUserRepositoryProxy wraps SqliteUserRepository ──
        services.AddMemoryCache();
        services.AddSingleton<IUserRepository>(sp =>
        {
            var cfg   = sp.GetRequiredService<IBotConfiguration>();
            var real  = new SqliteUserRepository(cfg.UsersDbPath);
            var cache = sp.GetRequiredService<IMemoryCache>();
            return new CachingUserRepositoryProxy(real, cache);
        });
        services.AddSingleton<IUserService, UserService>();

        // ── Adapter: FfmpegAdapter is the IFfmpegTool Target ─────────────
        services.AddSingleton<IFfmpegTool, FfmpegAdapter>();

        // ── Abstract Factory: analyzer factory family ─────────────────────
        services.AddSingleton<IAudioAnalyzerFactory, LosslessAnalyzerFactory>();

        // ── Audio domain ──────────────────────────────────────────────────
        services.AddSingleton<AudioFormatRegistry>();

        // ── Facade: AudioPipelineFacade wraps the CoR chain ───────────────
        services.AddSingleton<AudioPipelineFacade>();

        // ── Storage: AudioLibraryStorage backed by the configured provider ─
        services.AddSingleton<AudioLibraryStorage>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var provider      = configuration["Storage:Provider"] ?? "WebDav";
            var org           = configuration["Storage:AudioLibrary:Organization"] ?? "flat";

            IStorageService storage = provider.ToUpperInvariant() switch
            {
                "WEBDAV" => new WebDavStorageFactory(
                    configuration["Storage:WebDav:BaseUrl"]
                        ?? throw new InvalidOperationException("Storage:WebDav:BaseUrl is not configured."),
                    configuration["Storage:WebDav:Username"]
                        ?? throw new InvalidOperationException("Storage:WebDav:Username is not configured."),
                    configuration["Storage:WebDav:Password"]
                        ?? throw new InvalidOperationException("Storage:WebDav:Password is not configured."),
                    loggerFactory).CreateStorageService(),
                _ => throw new InvalidOperationException($"Unknown storage provider: '{provider}'.")
            };

            var registry = sp.GetRequiredService<AudioFormatRegistry>();
            return org.ToLowerInvariant() == "organized"
                ? (AudioLibraryStorage)new OrganizedAudioLibraryStorage(storage, registry)
                : new FlatAudioLibraryStorage(storage);
        });

        // ── State + Builder: album session manager ────────────────────────
        services.AddSingleton<AlbumSessionManager>();

        // ── Factory Method: StandardCommandFactory ────────────────────────
        services.AddSingleton<CommandFactory, StandardCommandFactory>();
        services.AddSingleton<CommandDispatcher>();

        // ── Infrastructure ────────────────────────────────────────────────
        services.AddSingleton<UpdateRouter>();
        services.AddHostedService<TelegramBotWorker>();

        return services;
    }
}
