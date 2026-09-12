using WasteToValue.Api.Modules.Partners.DTOs;

namespace WasteToValue.Api.Modules.Partners.Interfaces;

public interface IAcceptanceRulesService
{
    Task<IEnumerable<AcceptanceRuleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AcceptanceRuleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AcceptanceRuleResponse> CreateAsync(CreateAcceptanceRuleRequest request, CancellationToken cancellationToken = default);
    Task<AcceptanceRuleResponse?> UpdateAsync(Guid id, UpdateAcceptanceRuleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
