using NurFlac.Commands.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

public sealed class StartCommand(ITelegramBotClient botClient) : IBotCommand
{
    public string CommandToken => "start";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default) =>
        await botClient.SendMessage(message.Chat.Id,
            "Welcome to NurFlac! Upload a lossless audio file or use /help for commands.",
            cancellationToken: ct);
}
