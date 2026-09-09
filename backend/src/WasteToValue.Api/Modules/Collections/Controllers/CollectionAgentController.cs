using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;

namespace WasteToValue.Api.Modules.Collections.Controllers;

[ApiController]
[Route("api/collections/agent")]
public sealed class CollectionAgentController : ControllerBase
{
    private readonly ICollectionAgentService _agentService;

    public CollectionAgentController(ICollectionAgentService agentService)
    {
        _agentService = agentService;
    }

    /// <summary>
    /// Trigger the Collection Agent to prepare a collection plan for a pickup request.
    /// The agent evaluates slots, vehicle capacity, handling, travel, and availability
    /// constraints, then produces a structured proposal with a recommended and fallback option.
    /// </summary>
    [HttpPost("plan")]
    public async Task<IActionResult> PrepareCollectionPlan([FromBody] PrepareCollectionPlanRequest request, CancellationToken ct)
    {
        if (request.PickupRequestId == Guid.Empty)
            return BadRequest(new { error = "PickupRequestId is required." });

        var proposal = await _agentService.PrepareCollectionPlanAsync(request.PickupRequestId, ct);
        return Ok(proposal);
    }

    /// <summary>
    /// Trigger the Collection Agent to reschedule a failed collection.
    /// The agent detects the changed constraint, searches alternative slots,
    /// rechecks capacity and handling, and proposes a revised plan.
    /// </summary>
    [HttpPost("reschedule")]
    public async Task<IActionResult> Reschedule([FromBody] RescheduleRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "Reason is required for rescheduling." });

        var proposal = await _agentService.RescheduleAsync(request, ct);
        return Ok(proposal);
    }

    /// <summary>
    /// Retrieve a previously generated collection proposal by its ID.
    /// </summary>
    [HttpGet("proposals/{proposalId:guid}")]
    public async Task<IActionResult> GetProposal(Guid proposalId, CancellationToken ct)
    {
        var proposal = await _agentService.GetProposalAsync(proposalId, ct);
        return proposal is null ? NotFound() : Ok(proposal);
    }

    /// <summary>
    /// Submit an approval decision (Approve / Reject / Request Revision) on a collection proposal.
    /// The Collection Agent cannot directly confirm a pickup — only after an authorized
    /// staff member approves does the backend confirm the booking.
    /// </summary>
    [HttpPost("proposals/{proposalId:guid}/approve")]
    public async Task<IActionResult> ApproveProposal(Guid proposalId, [FromBody] ApprovalDecisionDto decision, CancellationToken ct)
    {
        var validDecisions = new[] { "APPROVED", "REJECTED", "REVISION_REQUESTED" };
        if (!validDecisions.Contains(decision.Decision, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "Decision must be APPROVED, REJECTED, or REVISION_REQUESTED." });

        if (decision.DecidedBy == Guid.Empty)
            return BadRequest(new { error = "DecidedBy is required." });

        var result = await _agentService.ApproveProposalAsync(
            decision with { ProposalId = proposalId }, ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public sealed record PrepareCollectionPlanRequest(Guid PickupRequestId);
