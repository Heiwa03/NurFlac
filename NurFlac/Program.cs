using NurFlac.Entry;
using NurFlac.Handlers;
using NurFlac.AudioProcessing;
using NurFlac.Storage;
using Telegram.Bot;
using NurFlac.AudioProcessing.Interfaces;
using NurFlac.DuplicateChecking;
using NurFlac.DuplicateChecking.ExternalApi;

var builder = Host.CreateApplicationBuilder(args);

// Telegram bot client
var botToken = builder.Configuration["TelegramBot:Token"]
    ?? throw new InvalidOperationException("TelegramBot:Token is not configured.");

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

// Audio processing (Factory Method pattern)
builder.Services.AddSingleton<SpectralAnalyzerFactory>();
builder.Services.AddSingleton<IAudioProcessor, FFmpegAudioProcessor>();

// Storage (Abstract Factory pattern — provider is selected from configuration)
var storageProvider = builder.Configuration["Storage:Provider"]
    ?? throw new InvalidOperationException("Storage:Provider is not configured.");

builder.Services.AddSingleton<IStorageServiceFactory>(sp =>
{
    var config = builder.Configuration;
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

builder.Services.AddSingleton<IStorageService>(sp => sp.GetRequiredService<IStorageServiceFactory>().CreateStorageService());

// Duplicate checking (Adapter + Facade patterns)
var duplicateDbPath = builder.Configuration["DuplicateCheck:SqlitePath"] ?? "Data/nurflac-duplicates.db";
var externalApiEnabled = builder.Configuration.GetValue<bool>("DuplicateCheck:ExternalApi:Enabled");

builder.Services.AddSingleton<IDuplicateFingerprintRepository>(_ => new SqliteDuplicateFingerprintRepository(duplicateDbPath));
builder.Services.AddSingleton<IAudioFingerprintProvider, FfmpegFingerprintProvider>();

if (externalApiEnabled)
{
    builder.Services.AddSingleton<IExternalFingerprintApi, ExternalFingerprintHttpApi>();
    builder.Services.AddSingleton<IAudioFingerprintProvider, ExternalFingerprintApiAdapter>();
}

builder.Services.AddSingleton<IDuplicateCheckFacade, DuplicateCheckFacade>();

// Command tracking
var commandTrackingDbPath = builder.Configuration["CommandTracking:SqlitePath"] ?? "Data/nurflac-command-tracking.db";
builder.Services.AddSingleton<ICommandExecutionTracker>(_ => new SqliteCommandExecutionTracker(commandTrackingDbPath));

// Commands
builder.Services.AddSingleton<StartCommand>();
builder.Services.AddSingleton<HelpCommand>();
builder.Services.AddSingleton<TestUploadCommand>();
builder.Services.AddSingleton<IEnumerable<CommandRegistration>>(sp =>
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
        Name = "testupload",
        Aliases = ["tupload"],
        Category = "Admin",
        Handler = sp.GetRequiredService<TestUploadCommand>()
    }
]);
builder.Services.AddSingleton<ICommandCatalog, CommandCatalog>();

// Entry point & routing
builder.Services.AddSingleton<CommandRouter>();
builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddHostedService<TelegramBotWorker>();

var host = builder.Build();
host.Run();
