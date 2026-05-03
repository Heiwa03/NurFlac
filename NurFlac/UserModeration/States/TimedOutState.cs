using System;

namespace NurFlac.UserModeration.States;

public class TimedOutState : IUserState
{
    private readonly DateTime _until;

    public TimedOutState(DateTime until)
    {
        _until = until;
    }

    public bool CanUpload() => DateTime.UtcNow > _until;

    public string GetStatusMessage()
    {
        if (CanUpload()) return "Your timeout has expired. You can upload again.";
        var remaining = _until - DateTime.UtcNow;
        // Logic to handle negative remaining time if DateTime.UtcNow > _until
        if (remaining.TotalSeconds < 0) return "Your timeout has expired.";
        
        return $"You are currently timed out. Remaining time: {remaining.Hours}h {remaining.Minutes}m {remaining.Seconds}s.";
    }
    
    public void ApplyPenalty(int score) { /* handled by mediator */ }
}
