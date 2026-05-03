using NurFlac.UserManagement.Entities;

namespace NurFlac.UserModeration.Chain;

public abstract class PenaltyHandler
{
    protected PenaltyHandler? Successor;

    public void SetSuccessor(PenaltyHandler successor)
    {
        Successor = successor;
    }

    public abstract void HandlePenalty(User user, int score);
}
