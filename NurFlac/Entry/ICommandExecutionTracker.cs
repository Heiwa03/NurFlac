namespace NurFlac.Entry;

public interface ICommandExecutionTracker
{
    Task TrackAsync(
        string commandKey,
        long telegramId,
        CommandExecutionOutcome outcome,
        long durationMs,
        string? error = null,
        CancellationToken cancellationToken = default);
}
