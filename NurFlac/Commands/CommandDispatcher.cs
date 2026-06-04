using NurFlac.Commands.Factory;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;

namespace NurFlac.Commands;

public sealed class CommandDispatcher(
    CommandFactory        factory,
    ITelegramBotClient    botClient,
    ILogger<CommandDispatcher> logger)
{
    public async Task DispatchAsync(Message message, CancellationToken ct = default)
    {
        var text   = message.Text;
        var userId = message.From?.Id ?? 0L;

        if (string.IsNullOrWhiteSpace(text)) return;

        var token   = text.Split(' ')[0].TrimStart('/').ToLowerInvariant();
        var command = factory.Create(token);

        logger.LogDebug("[DISPATCH] token='{Token}' resolved={Resolved} user={UserId}",
            token, command is not null, userId);

        if (command is null)
        {
            logger.LogInformation("[DISPATCH] Unknown command '/{Token}' from {UserId}", token, userId);
            await botClient.SendMessage(message.Chat.Id,
                $"Unknown command: /{token}. Use /help for the command list.",
                cancellationToken: ct);
            return;
        }

        await command.ExecuteAsync(message, ct);
    }
}
