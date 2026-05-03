using NurFlac.UserManagement.Entities;
using NurFlac.UserModeration.Violations;

namespace NurFlac.UserModeration.Observers;

public interface IModerationObserver
{
    void OnViolationProcessed(User user, IViolation violation);
}
