using WasteToValue.Api.Modules.Partners.DTOs;

namespace WasteToValue.Api.Modules.Partners.Interfaces;

public interface IPartnersService
{
    Task<IEnumerable<PartnerResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PartnerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PartnerResponse> CreateAsync(CreatePartnerRequest request, CancellationToken cancellationToken = default);
    Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
