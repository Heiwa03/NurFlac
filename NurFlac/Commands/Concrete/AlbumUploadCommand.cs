using NurFlac.Album;
using NurFlac.Commands.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

public sealed class AlbumUploadCommand(ITelegramBotClient botClient, AlbumSessionManager sessions) : IBotCommand
{
    public string CommandToken => "album-upload";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var userId = message.From?.Id ?? 0L;
        var reply  = await sessions.StartAlbumAsync(userId, ct);
        await botClient.SendMessage(message.Chat.Id, reply, cancellationToken: ct);
    }
}
