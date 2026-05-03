namespace NurFlac.UserModeration.States;

public class ActiveState : IUserState
{
    public bool CanUpload() => true;
    public string GetStatusMessage() => "You are an active user.";
    public void ApplyPenalty(int score) { /* handled by mediator */ }
}
