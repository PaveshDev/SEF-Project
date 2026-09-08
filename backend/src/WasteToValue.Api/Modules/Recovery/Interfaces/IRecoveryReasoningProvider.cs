using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;

namespace WasteToValue.Api.Modules.Recovery.Interfaces;

public interface IRecoveryReasoningProvider
{
    Task<GatewayResult<RecoveryReasoningResponse>> ReasonAsync(
        RecoveryReasoningRequest request, CancellationToken cancellationToken);
}
