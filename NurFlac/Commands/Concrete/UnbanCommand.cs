using NurFlac.Commands.Abstractions;
using NurFlac.Users;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

// Usage: /unban <telegramId>
public sealed class UnbanCommand(ITelegramBotClient botClient, IUserService userService) : IBotCommand
{
    public string CommandToken => "unban";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var parts = (message.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !long.TryParse(parts[1], out var targetId))
        {
            await botClient.SendMessage(message.Chat.Id, "Usage: /unban <telegramId>", cancellationToken: ct);
            return;
        }

        await userService.UnbanAsync(targetId, ct);
        await botClient.SendMessage(message.Chat.Id,
            $"User {targetId} has been unbanned.", cancellationToken: ct);
    }
}
