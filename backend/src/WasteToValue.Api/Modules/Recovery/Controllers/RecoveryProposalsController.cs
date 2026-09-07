using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Interfaces;

namespace WasteToValue.Api.Modules.Recovery.Controllers;

[ApiController]
[Route("api/recovery-proposals")]
[ServiceFilter(typeof(RecoveryExceptionFilter))]
public sealed class RecoveryProposalsController(IProposalDecisionService proposals) : ControllerBase
{
    [HttpGet("{proposalId:guid}")]
    public Task<RecoveryProposalResponse> Get(Guid proposalId, CancellationToken ct) => proposals.GetAsync(proposalId, ct);
    [HttpPost("{proposalId:guid}/decisions")]
    public Task<RecoveryProposalResponse> Decide(Guid proposalId, ProposalDecisionRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => proposals.DecideAsync(proposalId, request, key, ct);
    [HttpPost("{proposalId:guid}/refresh")]
    public Task<RecoveryProposalResponse> Refresh(Guid proposalId,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => proposals.RefreshAsync(proposalId, key, ct);
}
