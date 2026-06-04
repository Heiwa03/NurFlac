// ============================================================
// PATTERN: Command (Behavioral)
// Role   : Command interface — all bot commands implement this.
//          Encapsulates an action as an invocable object.
// ============================================================
using Telegram.Bot.Types;

namespace NurFlac.Commands.Abstractions;

public interface IBotCommand
{
    string CommandToken { get; }
    Task ExecuteAsync(Message message, CancellationToken ct = default);
}
