using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Services;

// Deliberately fail closed until real module adapters and shared infrastructure are installed.
public sealed class UnavailableRecoveryIntegrations : IAssessmentGateway, IMatchingGateway,
    IPickupPlanningGateway, IRecoveryActorAccessor, IRecoveryCommandExecutor
{
    private static Task<GatewayResult<T>> Missing<T>(string name, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(GatewayResult<T>.Failure(GatewayOutcome.Unavailable,
            $"{name}_unavailable", $"The {name} integration has not been configured.", true));
    }

    public Task<GatewayResult<AssessmentSummary>> GetCurrentAsync(Guid itemId, CancellationToken cancellationToken)
        => Missing<AssessmentSummary>("assessment", cancellationToken);
    public Task<GatewayResult<IReadOnlyList<MatchSummary>>> FindMatchesAsync(MatchRequest request, CancellationToken cancellationToken)
        => Missing<IReadOnlyList<MatchSummary>>("matching", cancellationToken);
    Task<GatewayResult<MatchSummary>> IMatchingGateway.RevalidateAsync(Guid id, int version, string token, CancellationToken ct)
        => Missing<MatchSummary>("matching", ct);
    public Task<GatewayResult<PickupPlanSummary>> PlanAsync(PickupPlanningRequest request, CancellationToken cancellationToken)
        => Missing<PickupPlanSummary>("pickup_planning", cancellationToken);
    Task<GatewayResult<PickupPlanSummary>> IPickupPlanningGateway.RevalidateAsync(Guid id, int version, string token, CancellationToken ct)
        => Missing<PickupPlanSummary>("pickup_planning", ct);

    public Task<RecoveryActor> GetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw RecoveryException.Unavailable("identity_unavailable", "Recovery requires the shared verified-identity integration.");
    }

    public Task<T> ExecuteAsync<T>(RecoveryCommand command, Func<CancellationToken, Task> authorize,
        Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw RecoveryException.Unavailable("idempotency_unavailable",
            "Recovery writes require durable idempotency and coordinated transaction infrastructure.");
    }
}
