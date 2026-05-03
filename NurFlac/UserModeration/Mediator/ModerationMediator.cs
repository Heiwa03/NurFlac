using NurFlac.UserManagement.Entities;
using NurFlac.UserModeration.Violations;
using NurFlac.UserModeration.Visitors;
using NurFlac.UserModeration.Chain;
using NurFlac.UserModeration.Observers;
using System.Collections.Generic;

namespace NurFlac.UserModeration.Mediator;

public class ModerationMediator : IModerationMediator
{
    private readonly List<IModerationObserver> _observers = new();
    private readonly PenaltyHandler _penaltyChain;

    public ModerationMediator()
    {
        var strikes = new StrikeHandler();
        var timeouts = new TimeoutHandler();
        var bans = new BanHandler();

        strikes.SetSuccessor(timeouts);
        timeouts.SetSuccessor(bans);

        _penaltyChain = strikes;
    }

    public void AddObserver(IModerationObserver observer) => _observers.Add(observer);

    public void ProcessViolation(User user, IViolation violation)
    {
        var visitor = new PenaltyCalculatorVisitor();
        violation.Accept(visitor);

        _penaltyChain.HandlePenalty(user, visitor.PenaltyScore);

        foreach (var observer in _observers)
        {
            observer.OnViolationProcessed(user, violation);
        }
    }
}
