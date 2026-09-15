using WasteToValue.Api.Modules.Partners.DTOs;

namespace WasteToValue.Api.Modules.Partners.Interfaces;

public interface IRecipientNeedsService
{
    Task<IEnumerable<RecipientNeedResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RecipientNeedResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RecipientNeedResponse> CreateAsync(CreateRecipientNeedRequest request, CancellationToken cancellationToken = default);
    Task<RecipientNeedResponse?> UpdateAsync(Guid id, UpdateRecipientNeedRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
