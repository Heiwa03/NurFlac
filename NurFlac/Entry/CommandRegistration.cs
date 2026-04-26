using NurFlac.Handlers;

namespace NurFlac.Entry;

public sealed class CommandRegistration
{
    public required string Name { get; init; }
    public string[] Aliases { get; init; } = [];
    public required ICommand Handler { get; init; }
    public string Category { get; init; } = "General";
}
