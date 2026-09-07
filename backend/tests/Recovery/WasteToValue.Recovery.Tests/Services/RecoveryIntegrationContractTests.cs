using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Services;

public class RecoveryIntegrationContractTests
{
    [Fact]
    public void RecoveryProposalValidator_Handles_Missing_Matching_Integration()
    {
        var f = new Fixture();
        var value = RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId,
            f.Assessments.Value, f.Inputs with { PreferredRoutes = new[] { RecoveryRoute.Donate }, MaximumPickupCost = 50 }, f.Clock.Now);
        Check.AssignId(value);
        value.StartPlanning(1, f.Clock.Now);
        var option = RecoveryOption.Draft(value, RecoveryRoute.Donate, true, true, f.Clock.Now);

        var match = new MatchSummary(Guid.NewGuid(), option.Id, Guid.NewGuid(), null, 1,
            MatchEligibility.Eligible, PartnerResponse.Accepted, "match-token", f.Clock.Now, Array.Empty<string>());
        var pickup = new PickupPlanSummary(Guid.NewGuid(), match.MatchId, null, 1, f.Clock.Now.AddHours(3),
            f.Clock.Now.AddHours(4), 20, "LKR", PickupFeasibility.Feasible, "pickup-token", f.Clock.Now, Array.Empty<string>());

        // Should work with valid dependencies
        RecoveryProposalValidator.Dependencies(value, option, match, pickup, f.Clock.Now);

        // Should fail with missing match
        var exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.Dependencies(value, option, null, pickup, f.Clock.Now));

        Assert.Equal("inputs_unavailable", exception.Code);
    }

    [Fact]
    public void RecoveryProposalValidator_Detects_Invalid_Match()
    {
        var f = new Fixture();
        var value = RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId,
            f.Assessments.Value, f.Inputs with { PreferredRoutes = new[] { RecoveryRoute.Donate }, MaximumPickupCost = 50 }, f.Clock.Now);
        Check.AssignId(value);
        value.StartPlanning(1, f.Clock.Now);
        var option = RecoveryOption.Draft(value, RecoveryRoute.Donate, true, true, f.Clock.Now);

        var match = new MatchSummary(Guid.NewGuid(), option.Id, Guid.NewGuid(), null, 1,
            MatchEligibility.Eligible, PartnerResponse.Accepted, "match-token", f.Clock.Now, Array.Empty<string>());
        var pickup = new PickupPlanSummary(Guid.NewGuid(), match.MatchId, null, 1, f.Clock.Now.AddHours(3),
            f.Clock.Now.AddHours(4), 20, "LKR", PickupFeasibility.Feasible, "pickup-token", f.Clock.Now, Array.Empty<string>());

        // Should fail with wrong recovery option ID
        var exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.Dependencies(value, option, match with { RecoveryOptionId = Guid.NewGuid() }, pickup, f.Clock.Now));

        Assert.Equal("invalid_match", exception.Code);

        // Should fail with pending partner response
        exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.Dependencies(value, option, match with { Response = PartnerResponse.Pending }, pickup, f.Clock.Now));

        Assert.Equal("invalid_match", exception.Code);
    }

    [Fact]
    public void RecoveryProposalValidator_Detects_Invalid_Pickup()
    {
        var f = new Fixture();
        var value = RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId,
            f.Assessments.Value, f.Inputs with { PreferredRoutes = new[] { RecoveryRoute.Donate }, MaximumPickupCost = 50 }, f.Clock.Now);
        Check.AssignId(value);
        value.StartPlanning(1, f.Clock.Now);
        var option = RecoveryOption.Draft(value, RecoveryRoute.Donate, true, true, f.Clock.Now);

        var match = new MatchSummary(Guid.NewGuid(), option.Id, Guid.NewGuid(), null, 1,
            MatchEligibility.Eligible, PartnerResponse.Accepted, "match-token", f.Clock.Now, Array.Empty<string>());
        var pickup = new PickupPlanSummary(Guid.NewGuid(), match.MatchId, null, 1, f.Clock.Now.AddHours(3),
            f.Clock.Now.AddHours(4), 20, "LKR", PickupFeasibility.Feasible, "pickup-token", f.Clock.Now, Array.Empty<string>());

        // Should fail with currency mismatch
        var exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.Dependencies(value, option, match, pickup with { Currency = "USD" }, f.Clock.Now));

        Assert.Equal("invalid_pickup", exception.Code);

        // Should fail with cost exceeding maximum
        exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.Dependencies(value, option, match, pickup with { EstimatedCost = 51 }, f.Clock.Now));

        Assert.Equal("invalid_pickup", exception.Code);

        // Should fail with proposed start time in the past
        exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.Dependencies(value, option, match, pickup with { ProposedStart = f.Clock.Now }, f.Clock.Now));

        Assert.Equal("invalid_pickup", exception.Code);
    }

    [Fact]
    public void RecoveryProposalValidator_Detects_Stale_Estimate_After_Option_Integration()
    {
        var f = new Fixture();
        var value = RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId,
            f.Assessments.Value, f.Inputs with { PreferredRoutes = new[] { RecoveryRoute.Donate }, MaximumPickupCost = 50 }, f.Clock.Now);
        Check.AssignId(value);
        value.StartPlanning(1, f.Clock.Now);
        var option = RecoveryOption.Draft(value, RecoveryRoute.Donate, true, true, f.Clock.Now);

        var match = new MatchSummary(Guid.NewGuid(), option.Id, Guid.NewGuid(), null, 1,
            MatchEligibility.Eligible, PartnerResponse.Accepted, "match-token", f.Clock.Now, Array.Empty<string>());
        var pickup = new PickupPlanSummary(Guid.NewGuid(), match.MatchId, null, 1, f.Clock.Now.AddHours(3),
            f.Clock.Now.AddHours(4), 20, "LKR", PickupFeasibility.Feasible, "pickup-token", f.Clock.Now, Array.Empty<string>());

        option.RecordIntegration(match, pickup);
        option.ValidateEstimate(new ValueEstimationService().Calculate(0, 0, 0, 20, "LKR", "LKR"),
            new[] { new ValueEvidence(Guid.NewGuid(), 1, f.Clock.Now, 0, 0, "LKR", "Verified donation reference") }, f.Clock.Now);

        // Should fail with stale estimate (lower cost than validated)
        var exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.Dependencies(value, option, match, pickup with { EstimatedCost = 30 }, f.Clock.Now));

        Assert.Equal("stale_estimate", exception.Code);
    }

    [Fact]
    public void RecoveryProposalValidator_Detects_Stale_Match_And_Pickup()
    {
        var f = new Fixture();
        var value = RecoveryCase.Create(f.Actors.Actor.UserId, f.Assessments.Value.ItemId,
            f.Assessments.Value, f.Inputs with { PreferredRoutes = new[] { RecoveryRoute.Donate }, MaximumPickupCost = 50 }, f.Clock.Now);
        Check.AssignId(value);
        value.StartPlanning(1, f.Clock.Now);
        var option = RecoveryOption.Draft(value, RecoveryRoute.Donate, true, true, f.Clock.Now);

        var match = new MatchSummary(Guid.NewGuid(), option.Id, Guid.NewGuid(), null, 1,
            MatchEligibility.Eligible, PartnerResponse.Accepted, "match-token", f.Clock.Now, Array.Empty<string>());
        var pickup = new PickupPlanSummary(Guid.NewGuid(), match.MatchId, null, 1, f.Clock.Now.AddHours(3),
            f.Clock.Now.AddHours(4), 20, "LKR", PickupFeasibility.Feasible, "pickup-token", f.Clock.Now, Array.Empty<string>());

        // Should fail with stale match
        var exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.MatchIdentity(match, match.MatchId, 2, match.FreshnessToken));

        Assert.Equal("stale_match", exception.Code);

        // Should fail with stale pickup
        exception = Assert.Throws<RecoveryException>(() =>
            RecoveryProposalValidator.PickupIdentity(pickup, pickup.PickupPlanId, 1, "changed"));

        Assert.Equal("stale_pickup", exception.Code);
    }

    [Fact]
    public void Missing_Gateway_Returns_Unavailable_Outcome()
    {
        var unavailable = new UnavailableRecoveryIntegrations();

        // Should return unavailable for missing matching integration
        var matchResult = ((IMatchingGateway)unavailable).RevalidateAsync(Guid.NewGuid(), 1, "token", default).Result;
        Assert.Equal(GatewayOutcome.Unavailable, matchResult.Outcome);

        // Should return unavailable for missing pickup integration
        var pickupResult = ((IPickupPlanningGateway)unavailable).RevalidateAsync(Guid.NewGuid(), 1, "token", default).Result;
        Assert.Equal(GatewayOutcome.Unavailable, pickupResult.Outcome);
    }

    [Fact]
    public void Gateway_Propagation_Of_Cancellation_Token()
    {
        var unavailable = new UnavailableRecoveryIntegrations();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Should propagate cancellation token
        Assert.Throws<OperationCanceledException>(() =>
            unavailable.GetCurrentAsync(Guid.NewGuid(), cancellation.Token).GetAwaiter().GetResult());
    }

    [Fact]
    public void RecoveryRequestValidator_Handles_All_Failure_Outcomes()
    {
        foreach (var outcome in new[] { GatewayOutcome.NotFound, GatewayOutcome.Invalid, GatewayOutcome.Stale, GatewayOutcome.Unavailable })
        {
            var result = GatewayResult<AssessmentSummary>.Failure(outcome, "provider_code", "Safe failure");

            // Should throw validation exception for all failure outcomes
            var exception = Assert.Throws<RecoveryException>(() =>
                RecoveryRequestValidator.Require(result));

            Assert.Equal("provider_code", exception.Code);
        }
    }
}