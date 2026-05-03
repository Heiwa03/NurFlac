using System;
using NurFlac.UserManagement.Entities;
using NurFlac.UserModeration.Violations;

namespace NurFlac.UserModeration.Observers;

public class LoggingModerationObserver : IModerationObserver
{
    public void OnViolationProcessed(User user, IViolation violation)
    {
        Console.WriteLine($"[MODERATION] User {user.TelegramId} committed violation: {violation.Description}. Current strikes: {user.StrikeCount}. Status: {user.Status}");
    }
}
