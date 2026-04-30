using NurFlac.Extensions;
using NurFlac.Entry;

var builder = Host.CreateApplicationBuilder(args);

// Register services using extension methods
builder.Services
    .AddTelegramBot(builder.Configuration)
    .AddAudioProcessing()
    .AddStorageServices(builder.Configuration)
    .AddDuplicateChecking(builder.Configuration)
    .AddCommandTracking(builder.Configuration)
    .AddValidationPipeline()
    .AddTelegramCommands();

// Entry point
builder.Services.AddHostedService<TelegramBotWorker>();

var host = builder.Build();
host.Run();
