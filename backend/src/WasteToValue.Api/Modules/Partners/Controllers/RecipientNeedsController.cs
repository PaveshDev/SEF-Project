using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Partners.DTOs;
using WasteToValue.Api.Modules.Partners.Interfaces;

namespace WasteToValue.Api.Modules.Partners.Controllers;

[ApiController]
[Route("api/partners/recipient-needs")]
public class RecipientNeedsController(IRecipientNeedsService recipientNeedsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) => 
        Ok(await recipientNeedsService.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await recipientNeedsService.GetByIdAsync(id, cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipientNeedRequest request, CancellationToken cancellationToken)
    {
        var result = await recipientNeedsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecipientNeedRequest request, CancellationToken cancellationToken)
    {
        var result = await recipientNeedsService.UpdateAsync(id, request, cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await recipientNeedsService.DeleteAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}
