// ConcreteDecorator — blocks admin commands for non-admin senders.
// Admin commands intentionally bypass the ModerationGuardDecorator.
using NurFlac.Commands.Abstractions;
using NurFlac.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;

namespace NurFlac.Commands.Decorators;

public sealed class AdminGuardDecorator(
    IBotCommand        inner,
    IBotConfiguration  config,
    ITelegramBotClient botClient,
    ILogger<AdminGuardDecorator> logger) : CommandDecorator(inner)
{
    public override async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var telegramId = message.From?.Id ?? 0L;
        var isAdmin    = config.IsAdmin(telegramId);

        logger.LogDebug("[ADMIN-GUARD] /{Token} — user={UserId} isAdmin={IsAdmin}",
            Inner.CommandToken, telegramId, isAdmin);

        if (!isAdmin)
        {
            // Silently ignore — no response sent so the command's existence stays hidden.
            logger.LogDebug("[ADMIN-GUARD] Silently dropped /{Token} from non-admin {UserId}",
                Inner.CommandToken, telegramId);
            return;
        }

        logger.LogDebug("[ADMIN-GUARD] PASS /{Token} for admin {UserId}", Inner.CommandToken, telegramId);
        await Inner.ExecuteAsync(message, ct);
    }
}
