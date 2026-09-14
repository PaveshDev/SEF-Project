namespace WasteToValue.Api.Modules.Collections.Interfaces;

/// <summary>
/// Abstraction for the external routing/maps API.
/// Returns estimated travel distance and time so the Collection Agent
/// can compare feasible pickup arrangements.
/// If the service fails, returns null — the system must never invent a travel estimate.
/// </summary>
public interface ITravelEstimateService
{
    Task<TravelEstimateResult?> GetEstimateAsync(string origin, string destination, CancellationToken ct = default);
}

public sealed record TravelEstimateResult(
    string DistanceText,
    double DistanceMeters,
    string DurationText,
    double DurationSeconds);
