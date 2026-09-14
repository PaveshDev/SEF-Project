using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Services;

namespace WasteToValue.Api.Modules.Recovery.Controllers;

[ApiController]
[Route("api/value-references")]
[ServiceFilter(typeof(RecoveryExceptionFilter))]
public sealed class ValueReferencesController(ValueReferenceService references) : ControllerBase
{
    [HttpGet]
    public Task<PagedResponse<ValueReferenceResponse>> List([FromQuery] ValueReferenceQuery query, CancellationToken ct) => references.ListAsync(query, ct);
    [HttpPost]
    public async Task<ActionResult<ValueReferenceResponse>> Create(CreateValueReferenceRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
    {
        var result = await references.CreateAsync(request, key, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
    [HttpGet("{referenceId:guid}")]
    public Task<ValueReferenceResponse> Get(Guid referenceId, CancellationToken ct) => references.GetAsync(referenceId, ct);
    [HttpPut("{referenceId:guid}")]
    public Task<ValueReferenceResponse> Update(Guid referenceId, UpdateValueReferenceRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => references.UpdateAsync(referenceId, request, key, ct);
    [HttpDelete("{referenceId:guid}")]
    public async Task<IActionResult> Delete(Guid referenceId, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct)
    {
        await references.DeleteAsync(referenceId, key, ct);
        return NoContent();
    }
    [HttpPost("{referenceId:guid}/verify")]
    public Task<ValueReferenceResponse> Verify(Guid referenceId, VerifyValueReferenceRequest request,
        [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => references.VerifyAsync(referenceId, request, key, ct);
}
