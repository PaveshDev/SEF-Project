using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Partners.DTOs;
using WasteToValue.Api.Modules.Partners.Interfaces;

namespace WasteToValue.Api.Modules.Partners.Controllers;

[ApiController]
[Route("api/partners")]
public class PartnersController(IPartnersService partnersService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) => 
        Ok(await partnersService.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await partnersService.GetByIdAsync(id, cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        var result = await partnersService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerRequest request, CancellationToken cancellationToken)
    {
        var result = await partnersService.UpdateAsync(id, request, cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await partnersService.DeleteAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}
