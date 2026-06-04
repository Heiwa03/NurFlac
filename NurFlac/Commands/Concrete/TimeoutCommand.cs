using NurFlac.Commands.Abstractions;
using NurFlac.Users;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

// Usage: /timeout <telegramId> <hours>
public sealed class TimeoutCommand(ITelegramBotClient botClient, IUserService userService) : IBotCommand
{
    public string CommandToken => "timeout";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var parts = (message.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || !long.TryParse(parts[1], out var targetId) ||
            !double.TryParse(parts[2], out var hours))
        {
            await botClient.SendMessage(message.Chat.Id,
                "Usage: /timeout <telegramId> <hours>", cancellationToken: ct);
            return;
        }

        await userService.TimeoutAsync(targetId, TimeSpan.FromHours(hours), ct);
        await botClient.SendMessage(message.Chat.Id,
            $"User {targetId} timed out for {hours} hour(s).", cancellationToken: ct);
    }
}
