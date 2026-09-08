using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;
using WasteToValue.Api.Modules.Recovery.Interfaces;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class UnavailableRecoveryReasoningProvider : IRecoveryReasoningProvider
{
    public Task<GatewayResult<RecoveryReasoningResponse>> ReasonAsync(
        RecoveryReasoningRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GatewayResult<RecoveryReasoningResponse>.Failure(GatewayOutcome.Unavailable,
            "IntegrationUnavailable", "Recovery reasoning is disabled or not configured."));
    }
}
