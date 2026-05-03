namespace NurFlac.UserModeration.States;

public interface IUserState
{
    bool CanUpload();
    string GetStatusMessage();
    void ApplyPenalty(int score);
}
