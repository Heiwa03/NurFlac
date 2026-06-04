using NurFlac.Commands.Abstractions;
using NurFlac.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

public sealed class HelpCommand(ITelegramBotClient botClient, IBotConfiguration config) : IBotCommand
{
    public string CommandToken => "help";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var isAdmin = config.IsAdmin(message.From?.Id ?? 0L);

        var text = isAdmin
            ? """
              **NurFlac Commands**
              /start                — Welcome message
              /help                 — This list
              /formats              — Accepted lossless formats
              /album-upload         — Begin a multi-file album batch
              /album-done           — Finalize and validate the batch

              **Admin Commands**
              /ban <id>             — Permanently ban a user
              /timeout <id> <hours> — Temporarily timeout a user
              /unban <id>           — Lift a ban or timeout
              /resetuser <id>       — Clear a user's strikes and restore Active status
              /clearusers           — Wipe all user records (resets all strikes and bans)
              /clearledger          — Clear the deduplication ledger (allows re-uploads)
              """
            : """
              **NurFlac Commands**
              /start        — Welcome message
              /help         — This list
              /formats      — Accepted lossless formats
              /album-upload — Begin a multi-file album batch
              /album-done   — Finalize and validate the batch
              """;

        await botClient.SendMessage(message.Chat.Id, text, cancellationToken: ct);
    }
}
