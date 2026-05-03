using System;
using System.Linq;
using Xunit;
using NurFlac.UserManagement.Entities;
using NurFlac.UserModeration.States;
using NurFlac.UserModeration.Violations;
using NurFlac.UserModeration.Visitors;
using NurFlac.UserModeration.Chain;
using NurFlac.UserModeration.Mediator;
using NurFlac.UserModeration.Observers;

namespace NurFlac.Tests;

public class UserModerationTests
{
    [Fact]
    public void State_ActiveUser_CanUpload()
    {
        IUserState state = new ActiveState();
        Assert.True(state.CanUpload());
        Assert.Contains("active", state.GetStatusMessage());
    }

    [Fact]
    public void State_BannedUser_CannotUpload()
    {
        IUserState state = new BannedState();
        Assert.False(state.CanUpload());
        Assert.Contains("banned", state.GetStatusMessage());
    }

    [Fact]
    public void State_TimedOutUser_CanUploadOnlyAfterExpiry()
    {
        var future = DateTime.UtcNow.AddMinutes(5);
        IUserState state = new TimedOutState(future);

        Assert.False(state.CanUpload());
        Assert.Contains("timed out", state.GetStatusMessage());

        var past = DateTime.UtcNow.AddMinutes(-5);
        IUserState stateExpired = new TimedOutState(past);
        Assert.True(stateExpired.CanUpload());
    }

    [Fact]
    public void Visitor_PenaltyCalculator_ReturnsCorrectScores()
    {
        var visitor = new PenaltyCalculatorVisitor();
        var fakeLossless = new FakeLosslessViolation();
        var forbiddenFormat = new ForbiddenFormatViolation("MP3");

        fakeLossless.Accept(visitor);
        Assert.Equal(2, visitor.PenaltyScore);

        forbiddenFormat.Accept(visitor);
        Assert.Equal(1, visitor.PenaltyScore);
    }

    [Fact]
    public void Chain_StrikesLeadToTimeoutAndBan()
    {
        var user = new User { TelegramId = 123, StrikeCount = 0, Status = UserStatus.Whitelisted };
        var strikes = new StrikeHandler();
        var timeouts = new TimeoutHandler();
        var bans = new BanHandler();
        strikes.SetSuccessor(timeouts);
        timeouts.SetSuccessor(bans);

        // First violation: 2 strikes
        strikes.HandlePenalty(user, 2);
        Assert.Equal(2, user.StrikeCount);
        Assert.Equal(UserStatus.Whitelisted, user.Status);

        // Second violation: +2 = 4 strikes -> Timeout
        strikes.HandlePenalty(user, 2);
        Assert.Equal(4, user.StrikeCount);
        Assert.Equal(UserStatus.TimedOut, user.Status);

        // Third violation: +2 = 6 strikes -> Blacklisted
        strikes.HandlePenalty(user, 2);
        Assert.Equal(6, user.StrikeCount);
        Assert.Equal(UserStatus.Blacklisted, user.Status);
    }

    [Fact]
    public void Mediator_CoordinatesViolationProcessing()
    {
        var user = new User { TelegramId = 999, StrikeCount = 0, Status = UserStatus.Whitelisted };
        var mediator = new ModerationMediator();
        var observer = new TestModerationObserver();
        mediator.AddObserver(observer);

        var violation = new FakeLosslessViolation();
        mediator.ProcessViolation(user, violation);

        Assert.Equal(2, user.StrikeCount);
        Assert.True(observer.WasCalled);
        Assert.Equal(violation, observer.LastViolation);
    }

    private class TestModerationObserver : IModerationObserver
    {
        public bool WasCalled { get; private set; }
        public IViolation? LastViolation { get; private set; }

        public void OnViolationProcessed(User user, IViolation violation)
        {
            WasCalled = true;
            LastViolation = violation;
        }
    }
}
