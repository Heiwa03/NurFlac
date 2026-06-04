// ConcreteCreator — maps incoming text tokens to concrete IBotCommand products.
using NurFlac.Album;
using NurFlac.Audio.Models;
using NurFlac.Commands.Abstractions;
using NurFlac.Commands.Concrete;
using NurFlac.Configuration;
using NurFlac.Ledger;
using NurFlac.Users;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace NurFlac.Commands.Factory;

public sealed class StandardCommandFactory : CommandFactory
{
    private readonly AudioFormatRegistry _registry;
    private readonly AlbumSessionManager _sessions;
    private readonly LedgerService       _ledger;

    public StandardCommandFactory(
        IBotConfiguration   config,
        IUserService        userService,
        ITelegramBotClient  botClient,
        ILoggerFactory      loggerFactory,
        AudioFormatRegistry registry,
        AlbumSessionManager sessions,
        LedgerService       ledger)
        : base(config, userService, botClient, loggerFactory)
    {
        _registry = registry;
        _sessions = sessions;
        _ledger   = ledger;
    }

    protected override IBotCommand? CreateCommand(string token) => token switch
    {
        "start"        => new StartCommand(BotClient),
        "help"         => new HelpCommand(BotClient, Config),
        "formats"      => new FormatsCommand(BotClient, _registry),
        "album-upload" => new AlbumUploadCommand(BotClient, _sessions),
        "album-done"   => new AlbumDoneCommand(BotClient, _sessions),
        "ban"          => new BanCommand(BotClient, UserService),
        "timeout"      => new TimeoutCommand(BotClient, UserService),
        "unban"        => new UnbanCommand(BotClient, UserService),
        "resetuser"    => new ResetUserCommand(BotClient, UserService),
        "clearusers"   => new ClearUsersCommand(BotClient, UserService),
        "clearledger"  => new ClearLedgerCommand(BotClient, _ledger),
        _              => null
    };
}
