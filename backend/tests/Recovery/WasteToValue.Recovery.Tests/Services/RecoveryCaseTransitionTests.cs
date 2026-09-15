using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Services;

public class RecoveryCaseTransitionTests
{
    [Fact]
    public void RecoveryCase_Creation_Requires_Owner()
    {
        var f = new Fixture();
        Assert.Throws<RecoveryException>(() =>
            RecoveryCase.Create(Guid.NewGuid(), f.Assessments.Value.ItemId, f.Assessments.Value, f.Inputs, f.Clock.Now));
    }

    [Fact]
    public void RecoveryCase_Creation_Requires_Confirmed_Assessment()
    {
        var f = new Fixture();
        Assert.Throws<RecoveryException>(() =>
            RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId,
                f.Assessments.Value with { IsCurrent = false }, f.Inputs, f.Clock.Now));

        Assert.Throws<RecoveryException>(() =>
            RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId,
                f.Assessments.Value with { Status = AssessmentStatus.Draft }, f.Inputs, f.Clock.Now));
    }

    [Fact]
    public void RecoveryCase_Transition_Workflow()
    {
        var f = new Fixture();
        var value = RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId, f.Assessments.Value, f.Inputs, f.Clock.Now);

        var offsetCase = RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId,
            f.Assessments.Value, f.Inputs with { Deadline = f.Clock.Now.AddDays(1).ToOffset(TimeSpan.FromHours(5.5)) }, f.Clock.Now);

        Assert.Equal(TimeSpan.Zero, offsetCase.Deadline.Value.Offset);

        Assert.Throws<RecoveryException>(() => value.StartPlanning(2, f.Clock.Now));
        value.StartPlanning(1, f.Clock.Now);

        Assert.Equal(RecoveryCaseStatus.Planning, value.Status);
        Assert.Equal(2, value.Version);

        value.Submit(f.Clock.Now);
        value.Decide(ProposalDecisionKind.Approved, f.Clock.Now);

        Assert.True(value.IsActive);

        Assert.Throws<RecoveryException>(() => value.Cancel(value.Version, f.Clock.Now));
        Assert.Throws<RecoveryException>(() => value.UpdateInputs(value.Version, f.Inputs, f.Assessments.Value, f.Clock.Now));

        value.Complete(f.Clock.Now);

        Assert.False(value.IsActive);
        Assert.Equal(RecoveryCaseStatus.Completed, value.Status);

        Assert.Throws<RecoveryException>(() => value.Cancel(value.Version, f.Clock.Now));
    }
}