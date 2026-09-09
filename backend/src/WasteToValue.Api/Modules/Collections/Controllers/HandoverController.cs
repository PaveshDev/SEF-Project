using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;
using WasteToValue.Api.Modules.Collections.Validators;

namespace WasteToValue.Api.Modules.Collections.Controllers;

[ApiController]
[Route("api/collections/pickups/{pickupId:guid}/handover")]
public sealed class HandoverController : ControllerBase
{
    private readonly IHandoverService _service;

    public HandoverController(IHandoverService service)
    {
        _service = service;
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCode(Guid pickupId, [FromBody] SubmitHandoverCodeRequest request, CancellationToken ct)
    {
        var codeErrors = HandoverCodeValidator.Validate(request.Code);
        if (codeErrors.Count > 0)
            return BadRequest(new { errors = codeErrors });

        if (request.ActorId == Guid.Empty)
            return BadRequest(new { error = "ActorId is required." });

        var proof = await _service.VerifyCodeAsync(pickupId, request, ct);
        return proof is null
            ? UnprocessableEntity(new { error = "Code verification failed." })
            : Ok(proof);
    }

    [HttpPost("proof")]
    public async Task<IActionResult> SubmitProof(Guid pickupId, [FromBody] SubmitHandoverProofRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProofType))
            return BadRequest(new { error = "ProofType is required." });

        if (request.ActorId == Guid.Empty)
            return BadRequest(new { error = "ActorId is required." });

        var proof = await _service.SubmitProofAsync(pickupId, request, ct);
        return proof is null ? NotFound() : Ok(proof);
    }

    [HttpGet]
    public async Task<IActionResult> GetProofs(Guid pickupId, CancellationToken ct)
    {
        var proofs = await _service.GetProofsAsync(pickupId, ct);
        return Ok(proofs);
    }
}
