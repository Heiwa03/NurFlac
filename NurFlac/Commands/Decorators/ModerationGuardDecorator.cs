// ConcreteDecorator — blocks command execution for banned/timed-out senders.
using NurFlac.Commands.Abstractions;
using NurFlac.Users;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;

namespace NurFlac.Commands.Decorators;

public sealed class ModerationGuardDecorator(
    IBotCommand        inner,
    IUserService       userService,
    ITelegramBotClient botClient,
    ILogger<ModerationGuardDecorator> logger) : CommandDecorator(inner)
{
    public override async Task ExecuteAsync(Message message, CancellationToken ct = default)
    {
        var telegramId = message.From?.Id ?? 0L;
        var user       = await userService.GetOrCreateAsync(telegramId, ct);

        logger.LogDebug("[MOD-GUARD] /{Token} — user={UserId} status={Status} strikes={Strikes}",
            Inner.CommandToken, telegramId, user.Status, user.StrikeCount);

        if (user.IsBanned())
        {
            logger.LogInformation("[MOD-GUARD] Blocked /{Token} — user {UserId} is banned",
                Inner.CommandToken, telegramId);
            await botClient.SendMessage(message.Chat.Id,
                "You are permanently banned from using this bot.", cancellationToken: ct);
            return;
        }

        if (user.IsTimedOut())
        {
            var remaining = user.TimeoutUntil!.Value - DateTime.UtcNow;
            logger.LogInformation("[MOD-GUARD] Blocked /{Token} — user {UserId} timed out ({Min:F0} min remaining)",
                Inner.CommandToken, telegramId, remaining.TotalMinutes);
            await botClient.SendMessage(message.Chat.Id,
                $"You are timed out. Try again in {remaining.TotalMinutes:F0} minutes.",
                cancellationToken: ct);
            return;
        }

        // Auto-lift timeout that has expired since the DB was last written.
        if (user.Status == Users.Entities.UserStatus.TimedOut && !user.IsTimedOut())
        {
            logger.LogInformation("[MOD-GUARD] Timeout expired for user {UserId} — lifting automatically", telegramId);
            await userService.UnbanAsync(telegramId, ct);
        }

        logger.LogDebug("[MOD-GUARD] PASS /{Token} for user {UserId}", Inner.CommandToken, telegramId);
        await Inner.ExecuteAsync(message, ct);
    }
}
