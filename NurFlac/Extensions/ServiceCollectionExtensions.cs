using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using NurFlac.AudioProcessing;
using NurFlac.AudioProcessing.Interfaces;
using NurFlac.DuplicateChecking;
using NurFlac.DuplicateChecking.ExternalApi;
using NurFlac.Entry;
using NurFlac.Handlers;
using NurFlac.Storage;
using NurFlac.Validation;

namespace NurFlac.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        var botToken = configuration["TelegramBot:Token"]
            ?? throw new InvalidOperationException("TelegramBot:Token is not configured.");

        var localApiBaseUrl = configuration["TelegramBot:LocalApiBaseUrl"];

        var botClient = string.IsNullOrWhiteSpace(localApiBaseUrl)
            ? new TelegramBotClient(botToken)
            : new TelegramBotClient(new TelegramBotClientOptions(botToken, localApiBaseUrl));

        services.AddSingleton<ITelegramBotClient>(botClient);
        return services;
    }

    public static IServiceCollection AddAudioProcessing(this IServiceCollection services)
    {
        services.AddSingleton<SpectralAnalyzerFactory>();
        services.AddSingleton<IAudioProcessor, FFmpegAudioProcessor>();
        services.AddSingleton<AudioFormatRegistry>();
        return services;
    }

    public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        var storageProvider = configuration["Storage:Provider"]
            ?? throw new InvalidOperationException("Storage:Provider is not configured.");

        services.AddSingleton<IStorageServiceFactory>(sp =>
        {
            var config = configuration;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            return storageProvider.ToLowerInvariant() switch
            {
                "webdav" => new WebDavStorageFactory(
                    config["Storage:WebDav:BaseUrl"]
                        ?? throw new InvalidOperationException("Storage:WebDav:BaseUrl is not configured."),
                    config["Storage:WebDav:Username"]
                        ?? throw new InvalidOperationException("Storage:WebDav:Username is not configured."),
                    config["Storage:WebDav:Password"]
                        ?? throw new InvalidOperationException("Storage:WebDav:Password is not configured."),
                    loggerFactory),
                "sftp" => new SftpStorageFactory(
                    config["Storage:Sftp:Host"]
                        ?? throw new InvalidOperationException("Storage:Sftp:Host is not configured."),
                    config["Storage:Sftp:Username"]
                        ?? throw new InvalidOperationException("Storage:Sftp:Username is not configured.")),
                "samba" => new SambaStorageFactory(
                    config["Storage:Samba:SharePath"]
                        ?? throw new InvalidOperationException("Storage:Samba:SharePath is not configured.")),
                _ => throw new InvalidOperationException($"Unknown storage provider: '{storageProvider}'.")
            };
        });

        services.AddSingleton<IStorageService>(sp =>
            new StorageServiceProxy(
                sp.GetRequiredService<IStorageServiceFactory>().CreateStorageService(),
                sp.GetRequiredService<ILogger<StorageServiceProxy>>()));

        var audioLibraryOrganization = configuration["Storage:AudioLibrary:Organization"] ?? "flat";
        services.AddSingleton<AudioLibraryStorage>(sp =>
        {
            var storage = sp.GetRequiredService<IStorageService>();
            var registry = sp.GetRequiredService<AudioFormatRegistry>();
            return audioLibraryOrganization.ToLowerInvariant() switch
            {
                "organized" => (AudioLibraryStorage)new OrganizedAudioLibraryStorage(storage, registry),
                _ => new FlatAudioLibraryStorage(storage)
            };
        });

        return services;
    }

    public static IServiceCollection AddDuplicateChecking(this IServiceCollection services, IConfiguration configuration)
    {
        var duplicateDbPath = configuration["DuplicateCheck:SqlitePath"] ?? "Data/nurflac-duplicates.db";
        var externalApiEnabled = configuration.GetValue<bool>("DuplicateCheck:ExternalApi:Enabled");

        services.AddSingleton<IDuplicateFingerprintRepository>(_ => new SqliteDuplicateFingerprintRepository(duplicateDbPath));
        services.AddSingleton<IAudioFingerprintProvider, FfmpegFingerprintProvider>();

        if (externalApiEnabled)
        {
            services.AddSingleton<IExternalFingerprintApi, ExternalFingerprintHttpApi>();
            services.AddSingleton<IAudioFingerprintProvider, ExternalFingerprintApiAdapter>();
        }

        services.AddSingleton<IDuplicateCheckFacade, DuplicateCheckFacade>();
        return services;
    }

    public static IServiceCollection AddCommandTracking(this IServiceCollection services, IConfiguration configuration)
    {
        var commandTrackingDbPath = configuration["CommandTracking:SqlitePath"] ?? "Data/nurflac-command-tracking.db";
        services.AddSingleton<ICommandExecutionTracker>(_ => new SqliteCommandExecutionTracker(commandTrackingDbPath));
        return services;
    }

    public static IServiceCollection AddValidationPipeline(this IServiceCollection services)
    {
        services.AddSingleton<ILosslessAudioValidator>(sp =>
            new SpectralValidatorDecorator(
                new MimeValidatorDecorator(
                    new ExtensionValidatorDecorator(
                        new PassthroughValidator(),
                        sp.GetRequiredService<AudioFormatRegistry>()),
                    sp.GetRequiredService<AudioFormatRegistry>()),
                sp.GetRequiredService<IAudioProcessor>()));
        return services;
    }

    public static IServiceCollection AddTelegramCommands(this IServiceCollection services)
    {
        services.AddSingleton<StartCommand>();
        services.AddSingleton<HelpCommand>();
        services.AddSingleton<FormatsCommand>();
        services.AddSingleton<TestUploadCommand>();
        services.AddSingleton<SingleAudioUploadCommand>();
        
        services.AddSingleton<IEnumerable<CommandRegistration>>(sp =>
        [
            new CommandRegistration
            {
                Name = "start",
                Aliases = ["hello", "begin"],
                Category = "General",
                Handler = sp.GetRequiredService<StartCommand>()
            },
            new CommandRegistration
            {
                Name = "help",
                Aliases = ["h", "commands"],
                Category = "General",
                Handler = sp.GetRequiredService<HelpCommand>()
            },
            new CommandRegistration
            {
                Name = "formats",
                Aliases = ["supported", "accept"],
                Category = "General",
                Handler = sp.GetRequiredService<FormatsCommand>()
            },
            new CommandRegistration
            {
                Name = "testupload",
                Aliases = ["tupload"],
                Category = "Admin",
                Handler = sp.GetRequiredService<TestUploadCommand>()
            }
        ]);
        
        services.AddSingleton<ICommandCatalog, CommandCatalog>();
        services.AddSingleton<CommandRouter>();
        services.AddSingleton<UpdateHandler>();
        
        return services;
    }
}
