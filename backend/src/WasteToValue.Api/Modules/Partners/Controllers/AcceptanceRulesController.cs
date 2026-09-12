using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Partners.DTOs;
using WasteToValue.Api.Modules.Partners.Interfaces;

namespace WasteToValue.Api.Modules.Partners.Controllers;

[ApiController]
[Route("api/partners/acceptance-rules")]
public class AcceptanceRulesController(IAcceptanceRulesService acceptanceRulesService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) => 
        Ok(await acceptanceRulesService.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await acceptanceRulesService.GetByIdAsync(id, cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAcceptanceRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await acceptanceRulesService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAcceptanceRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await acceptanceRulesService.UpdateAsync(id, request, cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await acceptanceRulesService.DeleteAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}
