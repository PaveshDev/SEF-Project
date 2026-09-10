using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Recovery.Interfaces;

namespace WasteToValue.Api.Modules.Recovery.Controllers;

[ApiController]
[Route("api/recovery/access")]
[ServiceFilter(typeof(RecoveryExceptionFilter))]
public sealed class RecoveryAccessController(IRecoveryActorAccessor actors) : ControllerBase
{
    [HttpGet]
    public async Task<object> Get(CancellationToken ct)
    {
        var actor = await actors.GetAsync(ct);
        return new { actor.IsHuman, actor.CanManageValueReferences };
    }
}
