using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Domain;

public sealed class RecoveryValidationTests
{
    [Fact]
    public void Recovery_case_rejects_invalid_inputs()
    {
        var fixture = new Fixture();

        AssertInvalid(() => RecoveryCase.Create(fixture.Actors.Actor.UserId, fixture.Assessments.Value.ItemId,
            fixture.Assessments.Value, fixture.Inputs with { Objective = " " }, fixture.Clock.Now));
        AssertInvalid(() => RecoveryCase.Create(fixture.Actors.Actor.UserId, fixture.Assessments.Value.ItemId,
            fixture.Assessments.Value, fixture.Inputs with { MaximumPickupCost = -1 }, fixture.Clock.Now));
        AssertInvalid(() => RecoveryCase.Create(fixture.Actors.Actor.UserId, fixture.Assessments.Value.ItemId,
            fixture.Assessments.Value, fixture.Inputs with { PreferredRoutes = new[] { (RecoveryRoute)99 } }, fixture.Clock.Now));
    }

    [Fact]
    public void Recovery_case_rejects_invalid_transition_and_stale_assessment()
    {
        var fixture = new Fixture();
        var value = RecoveryCase.Create(fixture.Actors.Actor.UserId, fixture.Assessments.Value.ItemId,
            fixture.Assessments.Value, fixture.Inputs, fixture.Clock.Now);
        Check.AssignId(value);

        var transition = Assert.Throws<RecoveryException>(() => value.Submit(fixture.Clock.Now));
        Assert.Equal("invalid_transition", transition.Code);

        var stale = fixture.Assessments.Value with { AssessmentVersion = 2 };
        var assessment = Assert.Throws<RecoveryException>(() => value.RequireAssessment(stale));
        Assert.Equal("stale_assessment", assessment.Code);
    }

    [Fact]
    public void Value_reference_rejects_invalid_ranges_and_detects_stale_version()
    {
        var fixture = new Fixture();
        var request = new CreateValueReferenceRequest(Guid.NewGuid(), ConditionGrade.Good, RecoveryRoute.Reuse,
            200, 100, "LKR", "Source", null, fixture.Clock.Now);

        var range = Assert.Throws<RecoveryException>(() => ValueReference.Create(request, fixture.Actors.Actor.UserId, fixture.Clock.Now));
        Assert.Equal("invalid_input", range.Code);

        var reference = ValueReference.Create(request with { ValueLow = 100 }, fixture.Actors.Actor.UserId, fixture.Clock.Now);
        var stale = Assert.Throws<RecoveryException>(() => reference.Verify(2, fixture.Clock.Now));
        Assert.Equal("stale_version", stale.Code);

        var referenceSnapshot = reference.Snapshot();
        reference.Verify(1, fixture.Clock.Now);
        Assert.NotEqual(referenceSnapshot, reference.Snapshot());
        Assert.Equal(1, referenceSnapshot.Version);
        Assert.Equal(2, reference.Snapshot().Version);
    }

    private static void AssertInvalid(Action action)
        => Assert.Equal("invalid_input", Assert.Throws<RecoveryException>(action).Code);
}