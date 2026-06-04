namespace NurFlac.Users.Entities;

public sealed class User
{
    public long TelegramId { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public int StrikeCount { get; set; }
    public DateTime? TimeoutUntil { get; set; }

    public bool IsActive() => Status == UserStatus.Active ||
        (Status == UserStatus.TimedOut && TimeoutUntil.HasValue && TimeoutUntil.Value <= DateTime.UtcNow);

    public bool IsBanned() => Status == UserStatus.Banned;

    public bool IsTimedOut() => Status == UserStatus.TimedOut &&
        TimeoutUntil.HasValue && TimeoutUntil.Value > DateTime.UtcNow;
}
