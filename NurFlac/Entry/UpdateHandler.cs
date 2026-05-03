using NurFlac.Handlers;
using NurFlac.UserManagement;
using NurFlac.UserModeration.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NurFlac.Entry;

public class UpdateHandler
{
    private readonly CommandRouter _commandRouter;
    private readonly IUploadSessionQueue _uploadQueue;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(
        CommandRouter commandRouter,
        IUploadSessionQueue uploadQueue,
        IUserService userService,
        ILogger<UpdateHandler> logger)
    {
        _commandRouter = commandRouter;
        _uploadQueue = uploadQueue;
        _userService = userService;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        var telegramId = message.From?.Id ?? 0;
        var user = await _userService.GetOrCreateUserAsync(telegramId);

        // STATE PATTERN: Early exit if user is not allowed to interact/upload
        IUserState state = user.Status switch
        {
            UserManagement.Entities.UserStatus.TimedOut => new TimedOutState(user.TimeoutUntil ?? DateTime.MinValue),
            UserManagement.Entities.UserStatus.Blacklisted => new BannedState(),
            _ => new ActiveState()
        };

        if (!state.CanUpload() && (message.Audio is not null || message.Document is not null))
        {
            _logger.LogWarning("Rejected upload attempt from {Status} user {UserId}", user.Status, telegramId);
            await botClient.SendMessage(message.Chat.Id, state.GetStatusMessage());
            return;
        }

        // Audio or document file message → Queue for processing
        if (message.Audio is not null || message.Document is not null)
        {
            _logger.LogInformation("Received audio upload from {UserId}. Queuing...", telegramId);
            await _uploadQueue.EnqueueAsync(message, telegramId);
            await botClient.SendMessage(message.Chat.Id, "File received and queued for processing.");
            return;
        }

        // Text command
        _logger.LogInformation("Received message from {UserId}: {Text}",
            telegramId, message.Text ?? "(non-text)");

        await _commandRouter.RouteMessageAsync(message);
    }
}
