using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

public interface IAssessmentGateway
{
    Task<GatewayResult<AssessmentSummary>> GetCurrentAsync(Guid itemId, CancellationToken cancellationToken);
}
