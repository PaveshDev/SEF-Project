using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Validators;
using WasteToValue.Recovery.Tests.Fakes;
using Xunit;

namespace WasteToValue.Recovery.Tests.Contracts;

public sealed class RecoveryGatewayFakeTests
{
    [Fact]
    public async Task Assessment_fake_covers_current_missing_unconfirmed_stale_timeout_and_unavailable()
    {
        var current = Assessment();
        var currentResult = await FakeAssessmentGateway.Current(current).GetCurrentAsync(current.ItemId, default);
        var missingResult = await FakeAssessmentGateway.Missing().GetCurrentAsync(current.ItemId, default);
        var unconfirmedResult = await FakeAssessmentGateway.Unconfirmed(current).GetCurrentAsync(current.ItemId, default);
        var staleResult = await FakeAssessmentGateway.Stale().GetCurrentAsync(current.ItemId, default);
        var timeoutResult = await FakeAssessmentGateway.Timeout().GetCurrentAsync(current.ItemId, default);
        var unavailableResult = await FakeAssessmentGateway.Unavailable().GetCurrentAsync(current.ItemId, default);

        Assert.Equal(GatewayOutcome.Success, currentResult.Outcome);
        Assert.Equal(current, currentResult.Value);
        Assert.Equal(GatewayOutcome.NotFound, missingResult.Outcome);
        Assert.Equal(GatewayOutcome.Success, unconfirmedResult.Outcome);
        Assert.False(unconfirmedResult.Value!.IsCurrent);
        Assert.Equal(AssessmentStatus.Draft, unconfirmedResult.Value.Status);
        Assert.Equal(GatewayOutcome.Stale, staleResult.Outcome);
        Assert.Equal(GatewayOutcome.Unavailable, timeoutResult.Outcome);
        Assert.Equal("assessment_timeout", timeoutResult.Code);
        Assert.True(timeoutResult.Retryable);
        Assert.Equal(GatewayOutcome.Unavailable, unavailableResult.Outcome);
        Assert.All(new[] { missingResult, staleResult, timeoutResult, unavailableResult }, result => Assert.Null(result.Value));
    }

    [Fact]
    public async Task Assessment_fake_propagates_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            FakeAssessmentGateway.Current(Assessment()).GetCurrentAsync(Guid.NewGuid(), cancellation.Token));
    }

    [Fact]
    public async Task Matching_fake_covers_eligible_multiple_empty_rejected_timeout_unavailable_and_invalid()
    {
        var first = Match();
        var second = first with { MatchId = Guid.NewGuid(), PartnerId = Guid.NewGuid() };
        var request = MatchRequest();
        var one = await FakeMatchingGateway.OneEligible(first).FindMatchesAsync(request, default);
        var multiple = await FakeMatchingGateway.MultipleEligible(new[] { first, second }).FindMatchesAsync(request, default);
        var none = await FakeMatchingGateway.NoEligible().FindMatchesAsync(request, default);
        var rejected = await FakeMatchingGateway.PartnerRejected(first).FindMatchesAsync(request, default);
        var timeout = await FakeMatchingGateway.Timeout().FindMatchesAsync(request, default);
        var unavailable = await FakeMatchingGateway.Unavailable().FindMatchesAsync(request, default);
        var invalid = await FakeMatchingGateway.Invalid().FindMatchesAsync(request, default);

        Assert.Single(one.Value!);
        Assert.Equal(2, multiple.Value!.Count);
        Assert.Equal(GatewayOutcome.Success, none.Outcome);
        Assert.Empty(none.Value!);
        Assert.Equal(PartnerResponse.Rejected, rejected.Value!.Single().Response);
        Assert.Equal(GatewayOutcome.Unavailable, timeout.Outcome);
        Assert.Equal(GatewayOutcome.Unavailable, unavailable.Outcome);
        Assert.Equal(GatewayOutcome.Invalid, invalid.Outcome);
        Assert.All(new[] { timeout, unavailable, invalid }, result => Assert.Null(result.Value));
    }

    [Fact]
    public async Task Matching_failure_is_not_converted_to_a_successful_match()
    {
        var result = await FakeMatchingGateway.Timeout().FindMatchesAsync(MatchRequest(), default);

        Assert.NotEqual(GatewayOutcome.Success, result.Outcome);
        Assert.Null(result.Value);
        Assert.Throws<RecoveryException>(() => RecoveryRequestValidator.Require(result));
    }

    [Fact]
    public async Task Pickup_fake_covers_feasible_multiple_empty_manual_timeout_unavailable_and_invalid()
    {
        var first = Pickup();
        var second = first with { PickupPlanId = Guid.NewGuid(), EstimatedCost = 25m };
        var request = PickupRequest(first);
        var feasible = await FakePickupPlanningGateway.Feasible(first).PlanAsync(request, default);
        var multiple = await FakePickupPlanningGateway.MultipleFeasible(new[] { first, second }).PlanAsync(request, default);
        var none = await FakePickupPlanningGateway.NoFeasible().PlanAsync(request, default);
        var manual = await FakePickupPlanningGateway.ManualReview(first).PlanAsync(request, default);
        var timeout = await FakePickupPlanningGateway.Timeout().PlanAsync(request, default);
        var unavailable = await FakePickupPlanningGateway.Unavailable().PlanAsync(request, default);
        var invalid = await FakePickupPlanningGateway.Invalid().PlanAsync(request, default);
        var multipleGateway = FakePickupPlanningGateway.MultipleFeasible(new[] { first, second });

        Assert.Equal(PickupFeasibility.Feasible, feasible.Value!.Feasibility);
        Assert.Equal(first.PickupPlanId, multiple.Value!.PickupPlanId);
        Assert.Equal(2, multipleGateway.AvailablePlans.Count);
        Assert.Equal(GatewayOutcome.NotFound, none.Outcome);
        Assert.Equal(PickupFeasibility.ManualReview, manual.Value!.Feasibility);
        Assert.Equal(GatewayOutcome.Unavailable, timeout.Outcome);
        Assert.Equal(GatewayOutcome.Unavailable, unavailable.Outcome);
        Assert.Equal(GatewayOutcome.Invalid, invalid.Outcome);
        Assert.All(new[] { none, timeout, unavailable, invalid }, result => Assert.Null(result.Value));
    }

    [Fact]
    public async Task Pickup_failure_is_not_converted_to_a_successful_plan()
    {
        var result = await FakePickupPlanningGateway.Invalid().PlanAsync(PickupRequest(Pickup()), default);

        Assert.NotEqual(GatewayOutcome.Success, result.Outcome);
        Assert.Null(result.Value);
        Assert.Throws<RecoveryException>(() => RecoveryRequestValidator.Require(result));
    }

    private static AssessmentSummary Assessment() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        1, 1, true, AssessmentStatus.Confirmed, ConditionGrade.Good, FunctionalStatus.Working,
        DateTimeOffset.UtcNow, "General service area", Array.Empty<string>());

    private static MatchSummary Match() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, 1,
        MatchEligibility.Eligible, PartnerResponse.Accepted, "match-token", DateTimeOffset.UtcNow, Array.Empty<string>());

    private static MatchRequest MatchRequest() => new(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), 1,
        Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), ConditionGrade.Good, FunctionalStatus.Working,
        RecoveryRoute.Donate, "General service area", DateTimeOffset.UtcNow.AddHours(2));

    private static PickupPlanSummary Pickup() => new(Guid.NewGuid(), Guid.NewGuid(), null, 1,
        DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2), 20m, "LKR",
        PickupFeasibility.Feasible, "pickup-token", DateTimeOffset.UtcNow, Array.Empty<string>());

    private static PickupPlanningRequest PickupRequest(PickupPlanSummary pickup) => new(Guid.NewGuid(), Guid.NewGuid(), 1,
        pickup.MatchId, pickup.Version, pickup.FreshnessToken, "General service area", RecoveryRoute.Donate,
        Array.Empty<string>(), DateTimeOffset.UtcNow.AddHours(3), 50m, "LKR");
}
