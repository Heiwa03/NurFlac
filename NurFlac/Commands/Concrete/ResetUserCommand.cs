using NurFlac.Commands.Abstractions;
using NurFlac.Users;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

public sealed class ResetUserCommand(ITelegramBotClient botClient, IUserService userService) : IBotCommand
{
    public string CommandToken => "resetuser";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var parts = (message.Text ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2 || !long.TryParse(parts[1], out var targetId))
        {
            await botClient.SendMessage(message.Chat.Id,
                "Usage: /resetuser <telegram_id>", cancellationToken: ct);
            return;
        }

        await userService.ResetUserAsync(targetId, ct);
        await botClient.SendMessage(message.Chat.Id,
            $"✅ User {targetId} has been reset: strikes cleared, status set to Active.",
            cancellationToken: ct);
    }
}
