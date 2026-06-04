// ============================================================
// PATTERN: Decorator (Structural)
// Role   : Abstract Decorator — wraps an IBotCommand and
//          delegates to the inner command after guard checks.
//          Subclasses inject Timeout/Ban/Admin validations.
// ============================================================
using NurFlac.Commands.Abstractions;
using Telegram.Bot.Types;

namespace NurFlac.Commands.Decorators;

public abstract class CommandDecorator(IBotCommand inner) : IBotCommand
{
    protected readonly IBotCommand Inner = inner;
    public string CommandToken => Inner.CommandToken;

    public abstract Task ExecuteAsync(Message message, CancellationToken ct = default);
}
