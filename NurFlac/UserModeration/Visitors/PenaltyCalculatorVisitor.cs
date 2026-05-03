using NurFlac.UserModeration.Violations;

namespace NurFlac.UserModeration.Visitors;

public class PenaltyCalculatorVisitor : IViolationVisitor
{
    public int PenaltyScore { get; private set; }

    public void Visit(FakeLosslessViolation violation)
    {
        // Fake lossless is a severe violation
        PenaltyScore = 2;
    }

    public void Visit(ForbiddenFormatViolation violation)
    {
        // MP3 upload is a minor violation (maybe user didn't know)
        PenaltyScore = 1;
    }
}
