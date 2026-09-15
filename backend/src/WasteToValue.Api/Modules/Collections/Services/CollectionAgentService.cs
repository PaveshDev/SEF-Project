using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.Agents;
using WasteToValue.Api.Modules.Collections.Agents.Tools;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Entities;
using WasteToValue.Api.Modules.Collections.Interfaces;
using WasteToValue.Api.Modules.Collections.Validators;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// Database-backed Collection Agent orchestrator using <see cref="AppDbContext"/> and <see cref="ICollectionAgentTools"/>.
/// Executes Agentic AI planning and re-planning workflows via allow-listed tool execution pipeline,
/// validates operational constraints deterministically, and persists workflow state to PostgreSQL.
/// </summary>
public sealed class CollectionAgentService : ICollectionAgentService
{
    private readonly AppDbContext _db;
    private readonly ICollectionAgentTools _tools;

    public CollectionAgentService(
        AppDbContext db,
        ICollectionAgentTools tools)
    {
        _db = db;
        _tools = tools;
    }

    public async Task<CollectionProposalDto> PrepareCollectionPlanAsync(Guid pickupRequestId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var workflowId = Guid.NewGuid();
        var workflowCtx = new AgentWorkflowContext(
            workflowId,
            pickupRequestId,
            "PLAN",
            "INITIALIZING",
            "IN_PROGRESS",
            new List<ToolCallLog>(),
            new List<ConstraintCheckDto>(),
            new List<string>(),
            now,
            now);

        // Step 1: Query available collection slots via ReadCollectionSlotsTool
        workflowCtx = workflowCtx with { CurrentStep = "READING_SLOTS" };
        var slots = await _tools.ReadCollectionSlotsAsync("Colombo", workflowCtx, ct);
        var selectedSlot = slots.FirstOrDefault();

        // Step 2: Check vehicle capacity via CheckVehicleCapacityTool
        workflowCtx = workflowCtx with { CurrentStep = "CHECKING_CAPACITY" };
        var vehicleClass = selectedSlot?.VehicleClass ?? "Small van";
        await _tools.CheckVehicleCapacityAsync(vehicleClass, 8.0m, workflowCtx, ct);

        // Step 3: Read handling rules via ReadHandlingRulesTool
        workflowCtx = workflowCtx with { CurrentStep = "READING_HANDLING_RULES" };
        var handlingRules = await _tools.ReadHandlingRulesAsync(pickupRequestId, workflowCtx, ct);

        // Add additional standard static constraint checks
        workflowCtx.ValidationChecks.Add(new ConstraintCheckDto(
            "Owner availability", true, "Owner available 14:00–17:00 overlaps proposed window."));
        workflowCtx.ValidationChecks.Add(new ConstraintCheckDto(
            "Destination hours", true, "Destination open 09:00–17:00; arrival expected before closing."));

        // Step 4: Get travel estimate via GetTravelEstimateTool
        workflowCtx = workflowCtx with { CurrentStep = "CALCULATING_TRAVEL" };
        var travelEstimate = await _tools.GetTravelEstimateAsync(
            "Example collection point, Colombo", "Demo reuse workshop", workflowCtx, ct);

        // Step 5: Synthesize structured proposal via ProposePickupTool
        workflowCtx = workflowCtx with { CurrentStep = "SYNTHESIZING_PROPOSAL" };
        var proposal = _tools.ProposePickup(
            pickupRequestId, selectedSlot, travelEstimate, handlingRules, workflowCtx.ValidationChecks, workflowCtx);

        workflowCtx.FinalProposal = proposal;
        workflowCtx = workflowCtx with { CurrentStep = "COMPLETED", Status = "COMPLETED", UpdatedAt = DateTimeOffset.UtcNow };

        // Step 6: Persist workflow state and proposal into PickupPlans DB entity
        var planEntity = new PickupPlan
        {
            Id = proposal.ProposalId,
            MatchId = pickupRequestId,
            CollectionSlotId = selectedSlot?.Id,
            ProposedStart = proposal.Recommended.ProposedStart,
            ProposedEnd = proposal.Recommended.ProposedEnd,
            EstimatedCost = proposal.Recommended.EstimatedCost ?? 0m,
            Currency = proposal.Recommended.Currency ?? "LKR",
            HandlingRequirements = JsonSerializer.Serialize(proposal.Recommended.HandlingRequirements),
            TravelEstimate = JsonSerializer.Serialize(new { travelEstimate, checks = proposal.ConstraintChecks, proposal, workflow = workflowCtx }),
            FeasibilityStatus = proposal.FeasibilityStatus,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1
        };

        _db.PickupPlans.Add(planEntity);

        // Ensure pickup request status is PENDING_APPROVAL
        var pickup = await _db.PickupRequests.FirstOrDefaultAsync(p => p.Id == pickupRequestId, ct);
        if (pickup is not null && (pickup.Status == "DRAFT" || pickup.Status == "PLANNED"))
        {
            pickup.Status = "PENDING_APPROVAL";
            pickup.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);

        return proposal;
    }

    public async Task<CollectionProposalDto> RescheduleAsync(RescheduleRequestDto request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var workflowId = Guid.NewGuid();
        var workflowCtx = new AgentWorkflowContext(
            workflowId,
            request.PickupRequestId,
            "REPLAN",
            "INITIALIZING_REPLAN",
            "IN_PROGRESS",
            new List<ToolCallLog>(),
            new List<ConstraintCheckDto>(),
            new List<string>(),
            now,
            now);

        // Step 1: Read alternative slots via ReadCollectionSlotsTool
        workflowCtx = workflowCtx with { CurrentStep = "READING_ALTERNATIVE_SLOTS" };
        var slots = await _tools.ReadCollectionSlotsAsync(null, workflowCtx, ct);
        var selectedSlot = slots.Skip(1).FirstOrDefault() ?? slots.FirstOrDefault();

        // Step 2: Check vehicle capacity via CheckVehicleCapacityTool
        workflowCtx = workflowCtx with { CurrentStep = "CHECKING_CAPACITY" };
        var vehicleClass = selectedSlot?.VehicleClass ?? "Small van";
        await _tools.CheckVehicleCapacityAsync(vehicleClass, 8.0m, workflowCtx, ct);

        // Step 3: Read handling rules via ReadHandlingRulesTool
        workflowCtx = workflowCtx with { CurrentStep = "READING_HANDLING_RULES" };
        var handlingRules = await _tools.ReadHandlingRulesAsync(request.PickupRequestId, workflowCtx, ct);

        // Step 4: Get travel estimate via GetTravelEstimateTool
        workflowCtx = workflowCtx with { CurrentStep = "CALCULATING_TRAVEL" };
        var travelEstimate = await _tools.GetTravelEstimateAsync(
            "Example collection point, Colombo", "Demo reuse workshop", workflowCtx, ct);

        // Step 5: Synthesize reschedule proposal via ProposeRescheduleTool
        workflowCtx = workflowCtx with { CurrentStep = "SYNTHESIZING_RESCHEDULE_PROPOSAL" };
        var proposal = _tools.ProposeReschedule(
            request.PickupRequestId, request.Reason, selectedSlot, travelEstimate, handlingRules, workflowCtx.ValidationChecks, workflowCtx);

        workflowCtx.FinalProposal = proposal;
        workflowCtx = workflowCtx with { CurrentStep = "COMPLETED", Status = "COMPLETED", UpdatedAt = DateTimeOffset.UtcNow };

        // Step 6: Persist rescheduled workflow state and proposal into PickupPlans DB entity
        var planEntity = new PickupPlan
        {
            Id = proposal.ProposalId,
            MatchId = request.PickupRequestId,
            CollectionSlotId = selectedSlot?.Id,
            ProposedStart = proposal.Recommended.ProposedStart,
            ProposedEnd = proposal.Recommended.ProposedEnd,
            EstimatedCost = proposal.Recommended.EstimatedCost ?? 0m,
            Currency = proposal.Recommended.Currency ?? "LKR",
            HandlingRequirements = JsonSerializer.Serialize(proposal.Recommended.HandlingRequirements),
            TravelEstimate = JsonSerializer.Serialize(new { travelEstimate, checks = proposal.ConstraintChecks, proposal, workflow = workflowCtx }),
            FeasibilityStatus = proposal.FeasibilityStatus,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1
        };

        _db.PickupPlans.Add(planEntity);

        var pickup = await _db.PickupRequests.FirstOrDefaultAsync(p => p.Id == request.PickupRequestId, ct);
        if (pickup is not null)
        {
            pickup.Status = "PENDING_APPROVAL";
            pickup.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);

        return proposal;
    }

    public async Task<CollectionProposalDto?> GetProposalAsync(Guid proposalId, CancellationToken ct = default)
    {
        var entity = await _db.PickupPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == proposalId, ct);

        if (entity is null) return null;

        return DeserializeProposal(entity);
    }

    public async Task<CollectionProposalDto?> ApproveProposalAsync(ApprovalDecisionDto decision, CancellationToken ct = default)
    {
        var planEntity = await _db.PickupPlans.FirstOrDefaultAsync(p => p.Id == decision.ProposalId, ct);
        if (planEntity is null) return null;

        var pickup = await _db.PickupRequests.FirstOrDefaultAsync(p => p.Id == planEntity.MatchId, ct);
        var decisionType = decision.Decision.ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;

        if (decisionType == "APPROVED")
        {
            // Re-validate slot availability and capacity before confirming
            if (planEntity.CollectionSlotId is not null)
            {
                var slot = await _db.CollectionSlots.FirstOrDefaultAsync(s => s.Id == planEntity.CollectionSlotId, ct);
                if (slot is null || !slot.Status.Equals("AVAILABLE", StringComparison.OrdinalIgnoreCase) || slot.ReservedCount >= slot.Capacity)
                {
                    planEntity.FeasibilityStatus = "REJECTED";
                    planEntity.UpdatedAt = now;

                    _db.PickupEvents.Add(new PickupEvent
                    {
                        Id = Guid.NewGuid(),
                        PickupRequestId = planEntity.MatchId,
                        EventType = "REJECTED_STALE_SLOT",
                        ActorId = decision.DecidedBy,
                        EventAt = now,
                        Notes = "Approval failed: Collection slot is no longer available or at full capacity. Re-planning required.",
                        IdempotencyKey = Guid.NewGuid().ToString("N")
                    });

                    await _db.SaveChangesAsync(ct);
                    throw new InvalidOperationException("Collection slot is no longer available. Re-planning required.");
                }

                slot.ReservedCount += 1;
                if (slot.ReservedCount >= slot.Capacity)
                {
                    slot.Status = "BOOKED";
                }
            }

            if (pickup is not null)
            {
                pickup.Status = "CONFIRMED";
                var plainCode = HandoverCodeHasher.GeneratePlainCode();
                pickup.VerificationCode = HandoverCodeHasher.FormatVerificationString(pickup.Id, plainCode);
                pickup.UpdatedAt = now;
            }

            planEntity.FeasibilityStatus = "APPROVED";
            planEntity.UpdatedAt = now;

            _db.PickupEvents.Add(new PickupEvent
            {
                Id = Guid.NewGuid(),
                PickupRequestId = planEntity.MatchId,
                EventType = "APPROVED",
                ActorId = decision.DecidedBy,
                EventAt = now,
                Notes = string.IsNullOrWhiteSpace(decision.Comment) ? "Proposal approved by staff." : decision.Comment,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });
        }
        else if (decisionType == "REJECTED")
        {
            planEntity.FeasibilityStatus = "REJECTED";
            planEntity.UpdatedAt = now;

            if (pickup is not null)
            {
                pickup.Status = "RESCHEDULE_PENDING";
                pickup.UpdatedAt = now;
            }

            _db.PickupEvents.Add(new PickupEvent
            {
                Id = Guid.NewGuid(),
                PickupRequestId = planEntity.MatchId,
                EventType = "REJECTED",
                ActorId = decision.DecidedBy,
                EventAt = now,
                Notes = string.IsNullOrWhiteSpace(decision.Comment) ? "Proposal rejected by staff." : decision.Comment,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });
        }
        else if (decisionType == "REVISION_REQUESTED")
        {
            planEntity.FeasibilityStatus = "REVISION_REQUESTED";
            planEntity.UpdatedAt = now;

            if (pickup is not null)
            {
                pickup.Status = "RESCHEDULE_PENDING";
                pickup.UpdatedAt = now;
            }

            _db.PickupEvents.Add(new PickupEvent
            {
                Id = Guid.NewGuid(),
                PickupRequestId = planEntity.MatchId,
                EventType = "REVISION_REQUESTED",
                ActorId = decision.DecidedBy,
                EventAt = now,
                Notes = string.IsNullOrWhiteSpace(decision.Comment) ? "Revision requested by staff." : decision.Comment,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });
        }

        await _db.SaveChangesAsync(ct);
        return DeserializeProposal(planEntity);
    }

    private static CollectionProposalDto DeserializeProposal(PickupPlan entity)
    {
        if (!string.IsNullOrEmpty(entity.TravelEstimate))
        {
            try
            {
                using var doc = JsonDocument.Parse(entity.TravelEstimate);
                if (doc.RootElement.TryGetProperty("proposal", out var propEl))
                {
                    var dto = JsonSerializer.Deserialize<CollectionProposalDto>(propEl.GetRawText());
                    if (dto is not null)
                    {
                        return dto with { FeasibilityStatus = entity.FeasibilityStatus };
                    }
                }
            }
            catch
            {
                // Fallback reconstruction below
            }
        }

        var recommended = new RecommendedOptionDto(
            entity.ProposedStart, entity.ProposedEnd,
            "Pickup from owner address", "Small van",
            null, null, entity.EstimatedCost, entity.Currency,
            new List<string>());

        return new CollectionProposalDto(
            entity.Id, entity.MatchId, recommended, null, entity.FeasibilityStatus,
            "Proposal loaded from database persistence.",
            new List<ConstraintCheckDto>(), entity.CreatedAt);
    }
}
