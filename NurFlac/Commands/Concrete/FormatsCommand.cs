using NurFlac.Audio.Models;
using NurFlac.Commands.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

public sealed class FormatsCommand(ITelegramBotClient botClient, AudioFormatRegistry registry) : IBotCommand
{
    public string CommandToken => "formats";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var list = string.Join(", ", registry.GetLosslessFormats().Select(f => f.DisplayName).Distinct());
        await botClient.SendMessage(message.Chat.Id,
            $"Accepted lossless formats: {list}", cancellationToken: ct);
    }
}
