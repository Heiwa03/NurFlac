using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace NurFlac.Infrastructure.Telegram;

public sealed class TelegramBotWorker(
    ITelegramBotClient botClient,
    UpdateRouter       updateRouter,
    ILogger<TelegramBotWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new ReceiverOptions { AllowedUpdates = [UpdateType.Message] };

        botClient.StartReceiving(
            updateHandler:  (_, update, ct) => updateRouter.RouteAsync(update, ct),
            errorHandler:   (_, ex, _)       => { logger.LogError(ex, "Polling error"); return Task.CompletedTask; },
            receiverOptions: options,
            cancellationToken: stoppingToken);

        var me = await botClient.GetMe(stoppingToken);
        logger.LogInformation("NurFlac bot @{Username} started", me.Username);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
