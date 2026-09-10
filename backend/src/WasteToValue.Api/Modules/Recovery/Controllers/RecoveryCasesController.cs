using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Interfaces;

namespace WasteToValue.Api.Modules.Recovery.Controllers;

[ApiController]
[Route("api/recovery-cases")]
[ServiceFilter(typeof(RecoveryExceptionFilter))]
public sealed class RecoveryCasesController(IRecoveryPlanningService planning, IProposalDecisionService proposals) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecoveryCaseResponse>> Create(CreateRecoveryCaseRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
    {
        var result = await planning.CreateAsync(request, key, ct);
        return CreatedAtAction(nameof(Get), new { caseId = result.Id }, result);
    }

    [HttpGet]
    public Task<PagedResponse<RecoveryCaseResponse>> List([FromQuery] RecoveryCaseQuery query, CancellationToken ct) => planning.ListAsync(query, ct);
    [HttpGet("{caseId:guid}")]
    public Task<RecoveryCaseResponse> Get(Guid caseId, CancellationToken ct) => planning.GetAsync(caseId, ct);
    [HttpPut("{caseId:guid}")]
    public Task<RecoveryCaseResponse> Update(Guid caseId, UpdateRecoveryInputsRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => planning.UpdateInputsAsync(caseId, request, key, ct);
    [HttpDelete("{caseId:guid}")]
    public async Task<IActionResult> Delete(Guid caseId, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
    {
        await planning.DeleteAsync(caseId, key, ct);
        return NoContent();
    }
    [HttpPost("{caseId:guid}/plan")]
    public Task<PlanningResponse> Plan(Guid caseId, StartPlanningRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => planning.PlanAsync(caseId, request, key, ct);
    [HttpPost("{caseId:guid}/replan")]
    public Task<PlanningResponse> Replan(Guid caseId, StartPlanningRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => planning.ReplanAsync(caseId, request, key, ct);
    [HttpGet("{caseId:guid}/options")]
    public Task<IReadOnlyList<RecoveryOptionResponse>> Options(Guid caseId, CancellationToken ct) => planning.OptionsAsync(caseId, ct);
    [HttpPost("{caseId:guid}/proposals")]
    public async Task<ActionResult<RecoveryProposalResponse>> Submit(Guid caseId, SubmitProposalRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
    {
        var result = await proposals.SubmitAsync(caseId, request, key, ct);
        return CreatedAtAction(nameof(RecoveryProposalsController.Get), "RecoveryProposals", new { proposalId = result.Id }, result);
    }
    [HttpPost("{caseId:guid}/cancel")]
    public Task<RecoveryCaseResponse> Cancel(Guid caseId, CancelRecoveryCaseRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => planning.CancelAsync(caseId, request, key, ct);

    [HttpGet("{caseId:guid}/proposals")]
    public Task<IReadOnlyList<RecoveryProposalResponse>> Proposals(Guid caseId, CancellationToken ct)
        => proposals.ListAsync(caseId, ct);
}
