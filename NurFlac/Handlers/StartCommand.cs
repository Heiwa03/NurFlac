using Telegram.Bot;
using Telegram.Bot.Types;
using User = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class StartCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;

    public StartCommand(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task ExecuteAsync(Message message, User user)
    {
        await _botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Welcome to NurFlac! Send me a lossless audio file and I'll verify it.");
    }
}