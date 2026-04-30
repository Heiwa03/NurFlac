using NurFlac.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using NurFlacUser = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Entry;

public class UpdateHandler
{
    private readonly CommandRouter _commandRouter;
    private readonly IUploadSessionQueue _uploadQueue;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(
        CommandRouter commandRouter,
        IUploadSessionQueue uploadQueue,
        ILogger<UpdateHandler> logger)
    {
        _commandRouter = commandRouter;
        _uploadQueue = uploadQueue;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        var telegramId = message.From?.Id ?? 0;

        // Audio or document file message → Queue for processing
        if (message.Audio is not null || message.Document is not null)
        {
            _logger.LogInformation("Received audio upload from {UserId}. Queuing...", telegramId);
            await _uploadQueue.EnqueueAsync(message, telegramId);
            await botClient.SendMessage(message.Chat.Id, "File received and queued for processing.");
            return;
        }

        // Text command
        _logger.LogInformation("Received message from {UserId}: {Text}",
            telegramId, message.Text ?? "(non-text)");

        await _commandRouter.RouteMessageAsync(message);
    }
}
