using NurFlac.Album;
using NurFlac.Commands.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

public sealed class AlbumDoneCommand(ITelegramBotClient botClient, AlbumSessionManager sessions) : IBotCommand
{
    public string CommandToken => "album-done";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var userId = message.From?.Id ?? 0L;
        var report = await sessions.FinalizeAlbumAsync(userId, ct);
        await botClient.SendMessage(message.Chat.Id, report, cancellationToken: ct);
    }
}
