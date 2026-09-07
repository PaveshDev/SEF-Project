using WasteToValue.Api.Modules.Recovery.DTOs;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

public interface IRecoveryPlanningService
{
    Task<RecoveryCaseResponse> CreateAsync(CreateRecoveryCaseRequest request, string key, CancellationToken ct);
    Task<IReadOnlyList<RecoveryCaseResponse>> ListAsync(CancellationToken ct);
    Task<RecoveryCaseResponse> GetAsync(Guid id, CancellationToken ct);
    Task<RecoveryCaseResponse> UpdateInputsAsync(Guid id, UpdateRecoveryInputsRequest request, string key, CancellationToken ct);
    Task<PlanningResponse> PlanAsync(Guid id, StartPlanningRequest request, string key, CancellationToken ct);
    Task<IReadOnlyList<RecoveryOptionResponse>> OptionsAsync(Guid id, CancellationToken ct);
    Task<RecoveryCaseResponse> CancelAsync(Guid id, CancelRecoveryCaseRequest request, string key, CancellationToken ct);
}
