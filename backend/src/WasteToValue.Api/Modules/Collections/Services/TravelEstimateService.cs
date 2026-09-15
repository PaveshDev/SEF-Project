using WasteToValue.Api.Modules.Collections.Interfaces;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// Placeholder implementation that always returns null (travel estimate unavailable).
/// The real implementation will call an external Maps/Routing API through ASP.NET Core.
/// The system must never invent a travel estimate.
/// </summary>
public sealed class TravelEstimateService : ITravelEstimateService
{
    public Task<TravelEstimateResult?> GetEstimateAsync(string origin, string destination, CancellationToken ct = default)
    {
        // Travel estimate unavailable — manual review required.
        // External-service failures, invalid responses and timeouts are handled safely
        // by returning null rather than a fabricated estimate.
        return Task.FromResult<TravelEstimateResult?>(null);
    }
}
