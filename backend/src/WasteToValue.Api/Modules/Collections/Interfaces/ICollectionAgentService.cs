using WasteToValue.Api.Modules.Collections.DTOs;

namespace WasteToValue.Api.Modules.Collections.Interfaces;

/// <summary>
/// Service interface for the Collection Agent orchestrator.
/// The agent plans the most feasible pickup arrangement for an approved
/// recovery proposal by evaluating multiple operational constraints.
/// </summary>
public interface ICollectionAgentService
{
    Task<CollectionProposalDto> PrepareCollectionPlanAsync(Guid pickupRequestId, CancellationToken ct = default);
    Task<CollectionProposalDto> RescheduleAsync(RescheduleRequestDto request, CancellationToken ct = default);
    Task<CollectionProposalDto?> GetProposalAsync(Guid proposalId, CancellationToken ct = default);
    Task<CollectionProposalDto?> ApproveProposalAsync(ApprovalDecisionDto decision, CancellationToken ct = default);
}
