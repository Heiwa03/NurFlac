using NurFlac.UserManagement.Entities;
using NurFlac.UserModeration.Violations;

namespace NurFlac.UserModeration.Mediator;

public interface IModerationMediator
{
    void ProcessViolation(User user, IViolation violation);
}
