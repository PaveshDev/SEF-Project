using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Services;

namespace WasteToValue.Api.Modules.Recovery.Controllers;

[ApiController]
[Route("api/recovery/value-references")]
[ServiceFilter(typeof(RecoveryExceptionFilter))]
public sealed class ValueReferencesController(ValueReferenceService references) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ValueReferenceResponse>> List(CancellationToken ct) => references.ListAsync(ct);
    [HttpPost]
    public async Task<ActionResult<ValueReferenceResponse>> Create(CreateValueReferenceRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
    {
        var result = await references.CreateAsync(request, key, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
    [HttpPost("{referenceId:guid}/verify")]
    public Task<ValueReferenceResponse> Verify(Guid referenceId, VerifyValueReferenceRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => references.VerifyAsync(referenceId, request, key, ct);
}
