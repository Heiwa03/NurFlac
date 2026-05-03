using NurFlac.UserModeration.Visitors;

namespace NurFlac.UserModeration.Violations;

public class ForbiddenFormatViolation : IViolation
{
    public string Format { get; }
    public ForbiddenFormatViolation(string format) => Format = format;
    public string Description => $"The format {Format} is forbidden. Only lossless formats are allowed.";
    public void Accept(IViolationVisitor visitor) => visitor.Visit(this);
}
