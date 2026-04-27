using NurFlac.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using NurFlacUser = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Entry;

public class UpdateHandler
{
    private readonly CommandRouter _commandRouter;
    private readonly SingleAudioUploadCommand _audioUploadCommand;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(
        CommandRouter commandRouter,
        SingleAudioUploadCommand audioUploadCommand,
        ILogger<UpdateHandler> logger)
    {
        _commandRouter = commandRouter;
        _audioUploadCommand = audioUploadCommand;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        var telegramId = message.From?.Id ?? 0;

        // Audio or document file message → audio upload pipeline
        if (message.Audio is not null || message.Document is not null)
        {
            _logger.LogInformation("Received audio upload from {UserId}", telegramId);
            var user = new NurFlacUser { TelegramId = telegramId };
            await _audioUploadCommand.ExecuteAsync(message, user);
            return;
        }

        // Text command
        _logger.LogInformation("Received message from {UserId}: {Text}",
            telegramId, message.Text ?? "(non-text)");

        await _commandRouter.RouteMessageAsync(message);
    }
}
