namespace NurFlac.UserManagement.Entities;

public class User
{
    public Guid Id { get; set; }
    public long TelegramId { get; set; }
    public UserStatus Status { get; set; }
    public int StrikeCount { get; set; }
    public DateTime? TimeoutUntil { get; set; }
}
