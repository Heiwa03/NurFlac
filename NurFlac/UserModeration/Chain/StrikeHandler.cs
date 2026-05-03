using NurFlac.UserManagement.Entities;

namespace NurFlac.UserModeration.Chain;

public class StrikeHandler : PenaltyHandler
{
    public override void HandlePenalty(User user, int score)
    {
        user.StrikeCount += score;
        if (user.StrikeCount >= 3 && Successor != null)
        {
            Successor.HandlePenalty(user, score);
        }
    }
}
