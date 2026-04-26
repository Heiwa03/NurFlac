using NurFlac.Handlers;
using Telegram.Bot.Types;
using System.Diagnostics;

namespace NurFlac.Entry;

public class CommandRouter
{
    private readonly ICommandCatalog _catalog;
    private readonly ICommandExecutionTracker _tracker;
    private readonly ILogger<CommandRouter> _logger;

    public CommandRouter(
        ICommandCatalog catalog,
        ICommandExecutionTracker tracker,
        ILogger<CommandRouter> logger)
    {
        _catalog = catalog;
        _tracker = tracker;
        _logger = logger;
    }

    public async Task RouteMessageAsync(Message message)
    {
        var text = message.Text;
        if (string.IsNullOrWhiteSpace(text))
            return;

        var commandKey = text.Split(' ')[0].TrimStart('/');
        var telegramId = message.From?.Id ?? 0;
        var stopwatch = Stopwatch.StartNew();

        if (!_catalog.TryResolve(commandKey, out var registration))
        {
            _logger.LogWarning("Unknown command: {Command}", commandKey);
            await _tracker.TrackAsync(commandKey, telegramId, CommandExecutionOutcome.UnknownCommand, stopwatch.ElapsedMilliseconds);
            return;
        }

        try
        {
            // TODO: resolve User from IUserService once implemented
            var user = new UserManagement.Entities.User
            {
                TelegramId = telegramId
            };

            await registration.Handler.ExecuteAsync(message, user);
            await _tracker.TrackAsync(registration.Name, telegramId, CommandExecutionOutcome.Success, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command execution failed: {Command}", registration.Name);
            await _tracker.TrackAsync(registration.Name, telegramId, CommandExecutionOutcome.Failed, stopwatch.ElapsedMilliseconds, ex.Message);
        }
    }
}
