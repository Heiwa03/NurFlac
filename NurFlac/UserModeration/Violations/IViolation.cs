using NurFlac.UserModeration.Visitors;

namespace NurFlac.UserModeration.Violations;

public interface IViolation
{
    string Description { get; }
    void Accept(IViolationVisitor visitor);
}
