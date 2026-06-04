// ============================================================
// PATTERN: Factory Method (Creational)
// Role   : Creator — declares the factory method CreateCommand()
//          that subclasses override to produce IBotCommand products.
//          The non-virtual Create() method applies the correct guard
//          decorator and forms the complete invocable command.
// ============================================================
using NurFlac.Commands.Abstractions;
using NurFlac.Commands.Decorators;
using NurFlac.Configuration;
using NurFlac.Users;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace NurFlac.Commands.Factory;

public abstract class CommandFactory
{
    protected readonly IBotConfiguration  Config;
    protected readonly IUserService       UserService;
    protected readonly ITelegramBotClient BotClient;
    private   readonly ILoggerFactory     _loggerFactory;

    protected CommandFactory(
        IBotConfiguration  config,
        IUserService       userService,
        ITelegramBotClient botClient,
        ILoggerFactory     loggerFactory)
    {
        Config         = config;
        UserService    = userService;
        BotClient      = botClient;
        _loggerFactory = loggerFactory;
    }

    // Non-virtual template: resolves the command, then wraps with the correct guard.
    // Admin commands  → AdminGuardDecorator only (bypasses moderation intentionally).
    //   A banned/timed-out admin can still run /ban, /timeout, /unban.
    // User commands   → ModerationGuardDecorator only.
    //   An admin who is banned cannot run /start, /help, etc.
    public IBotCommand? Create(string token)
    {
        var command = CreateCommand(token);
        if (command is null) return null;

        return IsAdminCommand(token)
            ? new AdminGuardDecorator(command, Config, BotClient,
                _loggerFactory.CreateLogger<AdminGuardDecorator>())
            : new ModerationGuardDecorator(command, UserService, BotClient,
                _loggerFactory.CreateLogger<ModerationGuardDecorator>());
    }

    // Factory method — ConcreteCreator subclasses instantiate the correct product.
    protected abstract IBotCommand? CreateCommand(string token);

    private static bool IsAdminCommand(string token) =>
        token is "ban" or "timeout" or "unban"
                 or "clearledger" or "clearusers" or "resetuser";
}
