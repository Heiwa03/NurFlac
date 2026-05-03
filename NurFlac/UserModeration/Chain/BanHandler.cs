using NurFlac.UserManagement.Entities;

namespace NurFlac.UserModeration.Chain;

public class BanHandler : PenaltyHandler
{
    public override void HandlePenalty(User user, int score)
    {
        if (user.StrikeCount >= 5)
        {
            user.Status = UserStatus.Blacklisted;
        }
    }
}
