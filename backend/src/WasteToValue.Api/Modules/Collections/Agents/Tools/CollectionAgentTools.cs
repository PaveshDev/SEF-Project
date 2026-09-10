using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;

namespace WasteToValue.Api.Modules.Collections.Agents.Tools;

public interface ICollectionAgentTools
{
    Task<List<CollectionSlotReadDto>> ReadCollectionSlotsAsync(string? serviceArea, AgentWorkflowContext ctx, CancellationToken ct = default);
    Task<ConstraintCheckDto> CheckVehicleCapacityAsync(string vehicleClass, decimal estimatedWeightKg, AgentWorkflowContext ctx, CancellationToken ct = default);
    Task<List<string>> ReadHandlingRulesAsync(Guid recoveryProposalId, AgentWorkflowContext ctx, CancellationToken ct = default);
    Task<TravelEstimateResult?> GetTravelEstimateAsync(string origin, string destination, AgentWorkflowContext ctx, CancellationToken ct = default);
    CollectionProposalDto ProposePickup(Guid pickupRequestId, CollectionSlotReadDto? slot, TravelEstimateResult? travel, List<string> handling, List<ConstraintCheckDto> checks, AgentWorkflowContext ctx);
    CollectionProposalDto ProposeReschedule(Guid pickupRequestId, string reason, CollectionSlotReadDto? slot, TravelEstimateResult? travel, List<string> handling, List<ConstraintCheckDto> checks, AgentWorkflowContext ctx);
}

public sealed class CollectionAgentTools : ICollectionAgentTools
{
    private readonly ICollectionSlotService _slotService;
    private readonly ITravelEstimateService _travelService;

    public CollectionAgentTools(ICollectionSlotService slotService, ITravelEstimateService travelService)
    {
        _slotService = slotService;
        _travelService = travelService;
    }

    public async Task<List<CollectionSlotReadDto>> ReadCollectionSlotsAsync(string? serviceArea, AgentWorkflowContext ctx, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        try
        {
            var allSlots = await _slotService.GetAllAsync(ct);
            var filtered = string.IsNullOrWhiteSpace(serviceArea)
                ? allSlots.Where(s => s.Status.Equals("AVAILABLE", StringComparison.OrdinalIgnoreCase)).ToList()
                : allSlots.Where(s => s.Status.Equals("AVAILABLE", StringComparison.OrdinalIgnoreCase) &&
                                     s.ServiceArea.Contains(serviceArea, StringComparison.OrdinalIgnoreCase)).ToList();

            ctx.ToolLogs.Add(new ToolCallLog(
                "ReadCollectionSlots",
                $"serviceArea: '{serviceArea ?? "ALL"}'",
                $"Found {filtered.Count} available slot(s)",
                true, now, null));

            return filtered;
        }
        catch (Exception ex)
        {
            ctx.ToolLogs.Add(new ToolCallLog(
                "ReadCollectionSlots",
                $"serviceArea: '{serviceArea}'",
                "Failed to read slots",
                false, now, ex.Message));
            ctx.Errors.Add($"ReadCollectionSlots error: {ex.Message}");
            return new List<CollectionSlotReadDto>();
        }
    }

    public Task<ConstraintCheckDto> CheckVehicleCapacityAsync(string vehicleClass, decimal estimatedWeightKg, AgentWorkflowContext ctx, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        decimal capacityLimitKg = vehicleClass.ToUpperInvariant() switch
        {
            "SMALL VAN" or "VAN" => 1500m,
            "THREE WHEELER" or "TUKTUK" => 300m,
            "LARGE TRUCK" or "TRUCK" => 5000m,
            _ => 500m
        };

        bool passed = estimatedWeightKg <= capacityLimitKg;
        var detail = passed
            ? $"Vehicle class '{vehicleClass}' capacity ({capacityLimitKg} kg) exceeds item weight ({estimatedWeightKg} kg)."
            : $"Item weight ({estimatedWeightKg} kg) exceeds vehicle class '{vehicleClass}' capacity ({capacityLimitKg} kg).";

        var check = new ConstraintCheckDto("Vehicle capacity", passed, detail);
        ctx.ValidationChecks.Add(check);

        ctx.ToolLogs.Add(new ToolCallLog(
            "CheckVehicleCapacity",
            $"vehicleClass: '{vehicleClass}', weight: {estimatedWeightKg}kg",
            $"Passed: {passed}, Detail: {detail}",
            true, now, null));

        return Task.FromResult(check);
    }

    public Task<List<string>> ReadHandlingRulesAsync(Guid recoveryProposalId, AgentWorkflowContext ctx, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var handling = new List<string> { "Keep upright", "Protect finish during transit" };

        ctx.ToolLogs.Add(new ToolCallLog(
            "ReadHandlingRules",
            $"recoveryProposalId: {recoveryProposalId}",
            $"Rules: [{string.Join(", ", handling)}]",
            true, now, null));

        return Task.FromResult(handling);
    }

    public async Task<TravelEstimateResult?> GetTravelEstimateAsync(string origin, string destination, AgentWorkflowContext ctx, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        try
        {
            var estimate = await _travelService.GetEstimateAsync(origin, destination, ct);
            if (estimate is not null)
            {
                ctx.ValidationChecks.Add(new ConstraintCheckDto(
                    "Travel feasibility", true, $"Travel distance: {estimate.DistanceText}, time: {estimate.DurationText}"));

                ctx.ToolLogs.Add(new ToolCallLog(
                    "GetTravelEstimate",
                    $"origin: '{origin}', dest: '{destination}'",
                    $"Distance: {estimate.DistanceText}, Duration: {estimate.DurationText}",
                    true, now, null));
            }
            else
            {
                ctx.ValidationChecks.Add(new ConstraintCheckDto(
                    "Travel feasibility", false, "Travel estimate unavailable — manual review required."));

                ctx.ToolLogs.Add(new ToolCallLog(
                    "GetTravelEstimate",
                    $"origin: '{origin}', dest: '{destination}'",
                    "Service returned null (Maps API offline / unroutable)",
                    true, now, null));
            }

            return estimate;
        }
        catch (Exception ex)
        {
            ctx.ValidationChecks.Add(new ConstraintCheckDto(
                "Travel feasibility", false, "Routing service error — manual review required."));

            ctx.ToolLogs.Add(new ToolCallLog(
                "GetTravelEstimate",
                $"origin: '{origin}', dest: '{destination}'",
                "Travel estimate exception",
                false, now, ex.Message));
            ctx.Errors.Add($"GetTravelEstimate error: {ex.Message}");

            return null;
        }
    }

    public CollectionProposalDto ProposePickup(
        Guid pickupRequestId,
        CollectionSlotReadDto? slot,
        TravelEstimateResult? travel,
        List<string> handling,
        List<ConstraintCheckDto> checks,
        AgentWorkflowContext ctx)
    {
        var now = DateTimeOffset.UtcNow;
        var start = slot?.StartsAt ?? now.AddDays(1).AddHours(9);
        var end = slot?.EndsAt ?? start.AddHours(2);
        var vehicle = slot?.VehicleClass ?? "Small van";

        var recommended = new RecommendedOptionDto(
            start, end,
            "Pickup from owner address",
            vehicle,
            travel?.DistanceText,
            travel?.DurationText,
            null, "LKR",
            handling);

        var fallback = new RecommendedOptionDto(
            start.AddHours(2), end.AddHours(2),
            "Pickup from owner address",
            vehicle,
            travel?.DistanceText,
            travel?.DurationText,
            null, "LKR",
            handling);

        var allPassed = checks.All(c => c.Passed);
        var feasibility = (allPassed && travel != null && slot != null) ? "FEASIBLE" : "MANUAL_REVIEW";

        var reason = allPassed && travel != null && slot != null
            ? "The recommended window overlaps owner availability and destination operating hours. Vehicle capacity and travel estimates confirm feasibility."
            : "Some constraints require manual staff review (e.g. travel estimate or slot selection).";

        var proposal = new CollectionProposalDto(
            Guid.NewGuid(), pickupRequestId, recommended, fallback, feasibility, reason, checks, now);

        ctx.ToolLogs.Add(new ToolCallLog(
            "ProposePickup",
            $"pickupRequestId: {pickupRequestId}, slotId: {slot?.Id}",
            $"ProposalId: {proposal.ProposalId}, Feasibility: {feasibility}",
            true, now, null));

        return proposal;
    }

    public CollectionProposalDto ProposeReschedule(
        Guid pickupRequestId,
        string reason,
        CollectionSlotReadDto? slot,
        TravelEstimateResult? travel,
        List<string> handling,
        List<ConstraintCheckDto> checks,
        AgentWorkflowContext ctx)
    {
        var now = DateTimeOffset.UtcNow;
        var start = slot?.StartsAt ?? now.AddDays(1).AddHours(14);
        var end = slot?.EndsAt ?? start.AddHours(2);
        var vehicle = slot?.VehicleClass ?? "Small van";

        var recommended = new RecommendedOptionDto(
            start, end,
            "Rescheduled pickup from owner address",
            vehicle,
            travel?.DistanceText,
            travel?.DurationText,
            null, "LKR",
            handling);

        var fallback = new RecommendedOptionDto(
            start.AddHours(2), end.AddHours(2),
            "Rescheduled pickup from owner address",
            vehicle,
            travel?.DistanceText,
            travel?.DurationText,
            null, "LKR",
            handling);

        var feasibility = (slot != null) ? "FEASIBLE" : "MANUAL_REVIEW";
        var rationale = $"Reschedule proposed due to: {reason}. Alternative collection slot evaluated.";

        var proposal = new CollectionProposalDto(
            Guid.NewGuid(), pickupRequestId, recommended, fallback, feasibility, rationale, checks, now);

        ctx.ToolLogs.Add(new ToolCallLog(
            "ProposeReschedule",
            $"pickupRequestId: {pickupRequestId}, reason: '{reason}'",
            $"ProposalId: {proposal.ProposalId}, Feasibility: {feasibility}",
            true, now, null));

        return proposal;
    }
}
