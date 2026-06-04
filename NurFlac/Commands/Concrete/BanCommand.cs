using NurFlac.Commands.Abstractions;
using NurFlac.Users;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

// Usage: /ban <telegramId>
public sealed class BanCommand(ITelegramBotClient botClient, IUserService userService) : IBotCommand
{
    public string CommandToken => "ban";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var parts = (message.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !long.TryParse(parts[1], out var targetId))
        {
            await botClient.SendMessage(message.Chat.Id, "Usage: /ban <telegramId>", cancellationToken: ct);
            return;
        }

        await userService.BanAsync(targetId, ct);
        await botClient.SendMessage(message.Chat.Id, $"User {targetId} has been banned.", cancellationToken: ct);
    }
}
