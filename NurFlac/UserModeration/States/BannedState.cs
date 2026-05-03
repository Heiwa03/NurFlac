namespace NurFlac.UserModeration.States;

public class BannedState : IUserState
{
    public bool CanUpload() => false;
    public string GetStatusMessage() => "You have been permanently banned for repeated violations.";
    public void ApplyPenalty(int score) { /* already banned */ }
}
