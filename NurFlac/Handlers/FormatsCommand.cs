using NurFlac.AudioProcessing;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class FormatsCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly AudioFormatRegistry _registry;

    public FormatsCommand(ITelegramBotClient botClient, AudioFormatRegistry registry)
    {
        _botClient = botClient;
        _registry = registry;
    }

    public async Task ExecuteAsync(Message message, User user)
    {
        var lossless = _registry.GetAllLossless();

        var lines = new List<string> { "Accepted lossless audio formats:\n" };
        foreach (var fmt in lossless)
        {
            var exts = string.Join(", ", fmt.Extensions);
            lines.Add($"  {fmt.DisplayName} — {exts}");
        }

        lines.Add(string.Empty);
        lines.Add("Send any audio file in one of these formats and I will verify and upload it.");

        await _botClient.SendMessage(message.Chat.Id, string.Join("\n", lines));
    }
}
