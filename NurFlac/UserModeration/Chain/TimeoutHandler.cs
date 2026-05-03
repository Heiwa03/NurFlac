using NurFlac.UserManagement.Entities;

namespace NurFlac.UserModeration.Chain;

public class TimeoutHandler : PenaltyHandler
{
    public override void HandlePenalty(User user, int score)
    {
        if (user.StrikeCount >= 3 && user.StrikeCount < 5)
        {
            user.Status = UserStatus.TimedOut;
            // Set timeout for 24 hours
            user.TimeoutUntil = DateTime.UtcNow.AddHours(24);
        }
        else if (user.StrikeCount >= 5 && Successor != null)
        {
            Successor.HandlePenalty(user, score);
        }
    }
}
