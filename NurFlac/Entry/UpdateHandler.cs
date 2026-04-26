using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NurFlac.Entry;

public class UpdateHandler
{
    private readonly CommandRouter _commandRouter;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(CommandRouter commandRouter, ILogger<UpdateHandler> logger)
    {
        _commandRouter = commandRouter;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        _logger.LogInformation("Received message from {UserId}: {Text}",
            message.From?.Id, message.Text ?? "(non-text)");

        await _commandRouter.RouteMessageAsync(message);
    }
}
