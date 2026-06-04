using NurFlac.Commands.Abstractions;
using NurFlac.Users;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

public sealed class ClearUsersCommand(ITelegramBotClient botClient, IUserService userService) : IBotCommand
{
    public string CommandToken => "clearusers";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var deleted = await userService.ClearAllUsersAsync(ct);
        await botClient.SendMessage(message.Chat.Id,
            $"✅ Users database cleared. {deleted} record(s) removed. All strikes and bans have been reset.",
            cancellationToken: ct);
    }
}
