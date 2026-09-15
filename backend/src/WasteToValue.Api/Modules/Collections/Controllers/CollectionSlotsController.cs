using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;

namespace WasteToValue.Api.Modules.Collections.Controllers;

[ApiController]
[Route("api/collections/slots")]
public sealed class CollectionSlotsController : ControllerBase
{
    private readonly ICollectionSlotService _service;

    public CollectionSlotsController(ICollectionSlotService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var slots = await _service.GetAllAsync(ct);
        return Ok(slots);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var slot = await _service.GetByIdAsync(id, ct);
        return slot is null ? NotFound() : Ok(slot);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCollectionSlotRequest request, CancellationToken ct)
    {
        if (request.EndsAt <= request.StartsAt)
            return BadRequest(new { error = "EndsAt must be after StartsAt." });

        if (request.Capacity < 1)
            return BadRequest(new { error = "Capacity must be at least 1." });

        if (string.IsNullOrWhiteSpace(request.ServiceArea))
            return BadRequest(new { error = "ServiceArea is required." });

        var slot = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = slot.Id }, slot);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCollectionSlotRequest request, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
