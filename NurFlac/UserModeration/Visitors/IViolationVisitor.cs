using NurFlac.UserModeration.Violations;

namespace NurFlac.UserModeration.Visitors;

public interface IViolationVisitor
{
    void Visit(FakeLosslessViolation violation);
    void Visit(ForbiddenFormatViolation violation);
}
