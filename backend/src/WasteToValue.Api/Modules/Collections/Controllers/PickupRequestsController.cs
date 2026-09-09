using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;
using WasteToValue.Api.Modules.Collections.Validators;

namespace WasteToValue.Api.Modules.Collections.Controllers;

[ApiController]
[Route("api/collections/pickups")]
public sealed class PickupRequestsController : ControllerBase
{
    private readonly IPickupRequestService _service;

    public PickupRequestsController(IPickupRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
    {
        var pickups = await _service.GetAllAsync(status, ct);
        return Ok(pickups);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var pickup = await _service.GetByIdAsync(id, ct);
        return pickup is null ? NotFound() : Ok(pickup);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePickupRequestRequest request, CancellationToken ct)
    {
        var errors = CreatePickupRequestValidator.Validate(request);
        if (errors.Count > 0)
            return BadRequest(new { errors });

        var pickup = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = pickup.Id }, pickup);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePickupRequestRequest request, CancellationToken ct)
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

    [HttpGet("{id:guid}/events")]
    public async Task<IActionResult> GetEvents(Guid id, CancellationToken ct)
    {
        var events = await _service.GetEventsAsync(id, ct);
        return Ok(events);
    }

    [HttpPost("{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "Reason is required for rescheduling." });

        var updated = await _service.RescheduleAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }
}
