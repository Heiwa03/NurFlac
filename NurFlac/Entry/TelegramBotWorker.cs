using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using NurFlac.Handlers;

namespace NurFlac.Entry;

public class TelegramBotWorker : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;
    private readonly IUploadSessionCaretaker _caretaker;
    private readonly SingleAudioUploadCommand _uploadCommand;
    private readonly ILogger<TelegramBotWorker> _logger;

    public TelegramBotWorker(
        ITelegramBotClient botClient,
        UpdateHandler updateHandler,
        IUploadSessionCaretaker caretaker,
        SingleAudioUploadCommand uploadCommand,
        ILogger<TelegramBotWorker> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _caretaker = caretaker;
        _uploadCommand = uploadCommand;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message]
        };

        _logger.LogInformation("Telegram bot starting polling...");

        _botClient.StartReceiving(
            updateHandler: (bot, update, ct) => _updateHandler.HandleUpdateAsync(bot, update, ct),
            errorHandler: (bot, exception, ct) =>
            {
                _logger.LogError(exception, "Telegram bot polling error");
                return Task.CompletedTask;
            },
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);

        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Bot started: @{BotUsername}", me.Username);

        // Resume pending sessions
        try
        {
            var pendingSessions = await _caretaker.GetPendingSessionsAsync();
            foreach (var session in pendingSessions)
            {
                // Fire and forget resumption
                _ = _uploadCommand.ResumeSessionAsync(session);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recover pending upload sessions");
        }

        // Keep alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
