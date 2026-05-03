using Telegram.Bot;
using Telegram.Bot.Types;
using NurFlac.UserManagement;
using NurFlac.UserModeration.Mediator;
using NurFlac.UserModeration.Violations;
using NurFlacUser = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class ModTestCommand : AdminOnlyCommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly IModerationMediator _moderationMediator;

    public ModTestCommand(
        ITelegramBotClient botClient, 
        IUserService userService, 
        IModerationMediator moderationMediator, 
        IConfiguration configuration)
        : base(botClient, configuration)
    {
        _botClient = botClient;
        _userService = userService;
        _moderationMediator = moderationMediator;
    }

    protected override async Task ExecuteAdminAsync(Message message, NurFlacUser user)
    {
        var parts = message.Text?.Split(' ');
        if (parts?.Length < 2 || !long.TryParse(parts[1], out var targetId))
        {
            await _botClient.SendMessage(message.Chat.Id, "Usage: /modtest <telegramId>");
            return;
        }

        var targetUser = await _userService.GetOrCreateUserAsync(targetId);
        
        await _botClient.SendMessage(message.Chat.Id, $"Simulating violation for user {targetId}...");

        var violation = new FakeLosslessViolation();
        _moderationMediator.ProcessViolation(targetUser, violation);
        
        await _userService.UpdateUserAsync(targetUser);

        await _botClient.SendMessage(message.Chat.Id, 
            $"Violation processed. User status: {targetUser.Status}, Strikes: {targetUser.StrikeCount}");
    }
}
