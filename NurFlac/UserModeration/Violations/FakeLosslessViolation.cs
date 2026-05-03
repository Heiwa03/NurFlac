using NurFlac.UserModeration.Visitors;

namespace NurFlac.UserModeration.Violations;

public class FakeLosslessViolation : IViolation
{
    public string Description => "Spectral analysis detected lossy content in a lossless container.";
    public void Accept(IViolationVisitor visitor) => visitor.Visit(this);
}
