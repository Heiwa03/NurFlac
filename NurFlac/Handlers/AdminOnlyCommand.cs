using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

/// <summary>
/// Base class that gates command execution behind a configured list of admin Telegram IDs.
/// </summary>
public abstract class AdminOnlyCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly long[] _adminTelegramIds;

    protected AdminOnlyCommand(ITelegramBotClient botClient, IConfiguration configuration)
    {
        _botClient = botClient;
        _adminTelegramIds = configuration.GetSection("TelegramBot:AdminIds")
            .Get<long[]>() ?? [];
    }

    public async Task ExecuteAsync(Message message, User user)
    {
        if (_adminTelegramIds.Length == 0 || !_adminTelegramIds.Contains(user.TelegramId))
        {
            await _botClient.SendMessage(message.Chat.Id, "You are not authorized to use this command.");
            return;
        }

        await ExecuteAdminAsync(message, user);
    }

    protected abstract Task ExecuteAdminAsync(Message message, User user);
}