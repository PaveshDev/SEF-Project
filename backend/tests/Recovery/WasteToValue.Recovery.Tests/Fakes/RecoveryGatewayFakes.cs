using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Interfaces;

namespace WasteToValue.Recovery.Tests.Fakes;

internal sealed class FakeAssessmentGateway : IAssessmentGateway
{
    private readonly GatewayResult<AssessmentSummary> result;

    private FakeAssessmentGateway(GatewayResult<AssessmentSummary> result) => this.result = result;

    public static FakeAssessmentGateway Current(AssessmentSummary assessment) =>
        new(GatewayResult<AssessmentSummary>.Success(assessment));

    public static FakeAssessmentGateway Missing() =>
        new(Failure(GatewayOutcome.NotFound, "assessment_not_found", "Assessment was not found."));

    public static FakeAssessmentGateway Unconfirmed(AssessmentSummary assessment) =>
        new(GatewayResult<AssessmentSummary>.Success(assessment with
        {
            IsCurrent = false,
            Status = AssessmentStatus.Draft,
            ConfirmedAt = null
        }));

    public static FakeAssessmentGateway Stale() =>
        new(Failure(GatewayOutcome.Stale, "assessment_stale", "Assessment is stale."));

    public static FakeAssessmentGateway Timeout() =>
        new(Failure(GatewayOutcome.Unavailable, "assessment_timeout", "Assessment provider timed out.", true));

    public static FakeAssessmentGateway Unavailable() =>
        new(Failure(GatewayOutcome.Unavailable, "assessment_unavailable", "Assessment provider is unavailable.", true));

    public Task<GatewayResult<AssessmentSummary>> GetCurrentAsync(Guid itemId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(result);
    }

    private static GatewayResult<AssessmentSummary> Failure(GatewayOutcome outcome, string code, string message, bool retryable = false) =>
        GatewayResult<AssessmentSummary>.Failure(outcome, code, message, retryable);
}

internal sealed class FakeMatchingGateway : IMatchingGateway
{
    private readonly GatewayResult<IReadOnlyList<MatchSummary>> findResult;
    private readonly GatewayResult<MatchSummary> revalidateResult;

    private FakeMatchingGateway(GatewayResult<IReadOnlyList<MatchSummary>> findResult,
        GatewayResult<MatchSummary>? revalidateResult = null)
    {
        this.findResult = findResult;
        this.revalidateResult = revalidateResult ?? Failure<MatchSummary>(GatewayOutcome.NotFound, "match_not_found", "Match was not found.");
    }

    public static FakeMatchingGateway OneEligible(MatchSummary match) => Matches(new[] { match });

    public static FakeMatchingGateway MultipleEligible(IReadOnlyList<MatchSummary> matches) => Matches(matches);

    public static FakeMatchingGateway NoEligible() => Matches(Array.Empty<MatchSummary>());

    public static FakeMatchingGateway PartnerRejected(MatchSummary match) => Matches(new[]
    {
        match with { Response = PartnerResponse.Rejected, Reasons = new[] { "Partner rejected the request." } }
    });

    public static FakeMatchingGateway Timeout() => Failed(GatewayOutcome.Unavailable, "matching_timeout", "Matching provider timed out.", true);

    public static FakeMatchingGateway Unavailable() => Failed(GatewayOutcome.Unavailable, "matching_unavailable", "Matching provider is unavailable.", true);

    public static FakeMatchingGateway Invalid() => Failed(GatewayOutcome.Invalid, "matching_invalid", "Matching provider returned an invalid response.");

    public static FakeMatchingGateway Matches(IReadOnlyList<MatchSummary> matches)
    {
        var result = GatewayResult<IReadOnlyList<MatchSummary>>.Success(matches);
        var first = matches.FirstOrDefault();
        return new(result, first is null ? null : GatewayResult<MatchSummary>.Success(first));
    }

    public Task<GatewayResult<IReadOnlyList<MatchSummary>>> FindMatchesAsync(MatchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(findResult);
    }

    public Task<GatewayResult<MatchSummary>> RevalidateAsync(Guid matchId, int expectedVersion,
        string expectedFreshnessToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(revalidateResult);
    }

    private static FakeMatchingGateway Failed(GatewayOutcome outcome, string code, string message, bool retryable = false) =>
        new(Failure<IReadOnlyList<MatchSummary>>(outcome, code, message, retryable));

    private static GatewayResult<T> Failure<T>(GatewayOutcome outcome, string code, string message, bool retryable = false) =>
        GatewayResult<T>.Failure(outcome, code, message, retryable);
}

internal sealed class FakePickupPlanningGateway : IPickupPlanningGateway
{
    private readonly GatewayResult<PickupPlanSummary> planResult;
    private readonly GatewayResult<PickupPlanSummary> revalidateResult;
    public IReadOnlyList<PickupPlanSummary> AvailablePlans { get; }

    private FakePickupPlanningGateway(GatewayResult<PickupPlanSummary> planResult,
        GatewayResult<PickupPlanSummary>? revalidateResult = null,
        IReadOnlyList<PickupPlanSummary>? availablePlans = null)
    {
        this.planResult = planResult;
        this.revalidateResult = revalidateResult ?? Failure(GatewayOutcome.NotFound, "pickup_not_found", "Pickup plan was not found.");
        AvailablePlans = availablePlans ?? Array.Empty<PickupPlanSummary>();
    }

    public static FakePickupPlanningGateway Feasible(PickupPlanSummary plan) => Plans(new[] { plan });

    public static FakePickupPlanningGateway MultipleFeasible(IReadOnlyList<PickupPlanSummary> plans) => Plans(plans);

    public static FakePickupPlanningGateway NoFeasible() => Plans(Array.Empty<PickupPlanSummary>());

    public static FakePickupPlanningGateway ManualReview(PickupPlanSummary plan) => Plans(new[]
    {
        plan with { Feasibility = PickupFeasibility.ManualReview, Reasons = new[] { "Manual review required." } }
    });

    public static FakePickupPlanningGateway Timeout() => Failed(GatewayOutcome.Unavailable, "pickup_timeout", "Pickup provider timed out.", true);

    public static FakePickupPlanningGateway Unavailable() => Failed(GatewayOutcome.Unavailable, "pickup_unavailable", "Pickup provider is unavailable.", true);

    public static FakePickupPlanningGateway Invalid() => Failed(GatewayOutcome.Invalid, "pickup_invalid", "Pickup provider returned an invalid response.");

    public static FakePickupPlanningGateway Plans(IReadOnlyList<PickupPlanSummary> plans)
    {
        var result = plans.Count == 0
            ? GatewayResult<PickupPlanSummary>.Failure(GatewayOutcome.NotFound, "pickup_not_found", "No feasible pickup plan was found.")
            : GatewayResult<PickupPlanSummary>.Success(plans[0]);
        return new(result, plans.Count == 0 ? null : GatewayResult<PickupPlanSummary>.Success(plans[0]), plans);
    }

    public Task<GatewayResult<PickupPlanSummary>> PlanAsync(PickupPlanningRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(planResult);
    }

    public Task<GatewayResult<PickupPlanSummary>> RevalidateAsync(Guid pickupPlanId, int expectedVersion,
        string expectedFreshnessToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(revalidateResult);
    }

    private static FakePickupPlanningGateway Failed(GatewayOutcome outcome, string code, string message, bool retryable = false) =>
        new(Failure(outcome, code, message, retryable));

    private static GatewayResult<PickupPlanSummary> Failure(GatewayOutcome outcome, string code, string message, bool retryable = false) =>
        GatewayResult<PickupPlanSummary>.Failure(outcome, code, message, retryable);
}
