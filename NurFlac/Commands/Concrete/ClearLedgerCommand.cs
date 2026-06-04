using NurFlac.Commands.Abstractions;
using NurFlac.Ledger;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Concrete;

public sealed class ClearLedgerCommand(ITelegramBotClient botClient, LedgerService ledger) : IBotCommand
{
    public string CommandToken => "clearledger";

    public async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var deleted = await ledger.ClearAsync(ct);
        await botClient.SendMessage(message.Chat.Id,
            $"✅ Ledger cleared. {deleted} record(s) removed. Files can now be re-uploaded.",
            cancellationToken: ct);
    }
}
