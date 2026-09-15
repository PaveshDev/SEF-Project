using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

public interface IPickupPlanningGateway
{
    Task<GatewayResult<PickupPlanSummary>> PlanAsync(PickupPlanningRequest request, CancellationToken cancellationToken);
    Task<GatewayResult<PickupPlanSummary>> RevalidateAsync(Guid pickupPlanId, int expectedVersion,
        string expectedFreshnessToken, CancellationToken cancellationToken);
}
