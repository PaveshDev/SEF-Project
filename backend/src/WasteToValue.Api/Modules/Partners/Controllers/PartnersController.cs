using Microsoft.AspNetCore.Mvc;

namespace WasteToValue.Api.Modules.Partners.Controllers;

[ApiController]
[Route("api/partners")]
public class PartnersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(new string[] { });

    [HttpGet("{id}")]
    public IActionResult Get(Guid id) => Ok();

    [HttpPost]
    public IActionResult Create() => Ok();

    [HttpPut("{id}")]
    public IActionResult Update(Guid id) => Ok();

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id) => Ok();
}
