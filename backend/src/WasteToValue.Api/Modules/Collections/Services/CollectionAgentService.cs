using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// Placeholder Collection Agent orchestrator. Simulates the multi-step agent workflow:
/// read slots → check capacity → check handling → get travel estimate → rank options →
/// produce structured proposal. Returns deterministic demo data.
///
/// The real implementation will use an AI framework to drive the workflow with
/// allow-listed tools (ReadCollectionSlots, CheckVehicleCapacity, ReadHandlingRules,
/// GetTravelEstimate, ProposePickup, ProposeReschedule).
/// </summary>
public sealed class CollectionAgentService : ICollectionAgentService
{
    private readonly ICollectionSlotService _slotService;
    private readonly ITravelEstimateService _travelService;
    private readonly Dictionary<Guid, CollectionProposalDto> _proposals = new();

    public CollectionAgentService(ICollectionSlotService slotService, ITravelEstimateService travelService)
    {
        _slotService = slotService;
        _travelService = travelService;
    }

    public async Task<CollectionProposalDto> PrepareCollectionPlanAsync(Guid pickupRequestId, CancellationToken ct = default)
    {
        // Step 1: Read available collection slots
        var slots = await _slotService.GetAllAsync(ct);

        // Step 2: Get travel estimate (will return null — no routing service connected)
        var travelEstimate = await _travelService.GetEstimateAsync(
            "Example collection point, Colombo", "Demo reuse workshop", ct);

        // Step 3: Build constraint checks
        var checks = new List<ConstraintCheckDto>
        {
            new("Vehicle capacity", true, "Sample vehicle capacity (50 kg) exceeds the item weight (8 kg)."),
            new("Owner availability", true, "Owner available 14:00–17:00 overlaps the proposed window."),
            new("Destination hours", true, "Destination open 09:00–17:00; arrival expected before closing."),
            new("Travel feasibility", travelEstimate != null, travelEstimate != null
                ? $"Travel: {travelEstimate.DistanceText}, {travelEstimate.DurationText}"
                : "Travel estimate unavailable — manual review required."),
        };

        var now = DateTimeOffset.UtcNow;
        var baseDate = new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.FromHours(5.5));

        // Step 4: Build recommended option
        var recommended = new RecommendedOptionDto(
            baseDate.AddHours(14), baseDate.AddHours(16),
            "Pickup from owner address",
            "Small van",
            travelEstimate?.DistanceText,
            travelEstimate?.DurationText,
            null, "LKR",
            new List<string> { "Keep upright", "Protect wooden finish" });

        // Step 5: Build fallback option
        var fallback = new RecommendedOptionDto(
            baseDate.AddHours(16), baseDate.AddHours(18),
            "Pickup from owner address",
            "Small van",
            travelEstimate?.DistanceText,
            travelEstimate?.DurationText,
            null, "LKR",
            new List<string> { "Keep upright", "Protect wooden finish" });

        var feasibility = travelEstimate != null ? "FEASIBLE" : "MANUAL_REVIEW";

        var proposal = new CollectionProposalDto(
            Guid.NewGuid(), pickupRequestId, recommended, fallback, feasibility,
            "The recommended window overlaps owner availability and destination hours. "
            + "Vehicle capacity is sufficient. "
            + (travelEstimate != null ? "Travel estimate confirms feasibility." : "Travel estimate unavailable — manual review required."),
            checks, now);

        _proposals[proposal.ProposalId] = proposal;
        return proposal;
    }

    public async Task<CollectionProposalDto> RescheduleAsync(RescheduleRequestDto request, CancellationToken ct = default)
    {
        // Re-planning: detect failure → identify changed constraint → search alternative slots →
        // check vehicle capacity → calculate travel estimate → compare alternatives →
        // propose a revised collection plan → request human approval.
        return await PrepareCollectionPlanAsync(request.PickupRequestId, ct);
    }

    public Task<CollectionProposalDto?> GetProposalAsync(Guid proposalId, CancellationToken ct = default)
    {
        _proposals.TryGetValue(proposalId, out var proposal);
        return Task.FromResult(proposal);
    }

    public Task<CollectionProposalDto?> ApproveProposalAsync(ApprovalDecisionDto decision, CancellationToken ct = default)
    {
        if (!_proposals.TryGetValue(decision.ProposalId, out var proposal))
            return Task.FromResult<CollectionProposalDto?>(null);

        // The Collection Agent cannot directly confirm a pickup.
        // Only after approval does the ASP.NET Core backend confirm the booking.
        // In a real implementation, this would update the workflow status,
        // persist the decision for auditing, and confirm the pickup request.
        return Task.FromResult<CollectionProposalDto?>(proposal);
    }
}
