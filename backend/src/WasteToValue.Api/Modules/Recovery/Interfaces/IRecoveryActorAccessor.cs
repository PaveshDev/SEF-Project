using WasteToValue.Api.Modules.Recovery.DTOs;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

// The integrator supplies verified identity/role information, never client request fields.
public interface IRecoveryActorAccessor
{
    Task<RecoveryActor> GetAsync(CancellationToken cancellationToken);
}
