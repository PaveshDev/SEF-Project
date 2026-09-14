using WasteToValue.Api.Modules.Collections.DTOs;

namespace WasteToValue.Api.Modules.Collections.Interfaces;

public interface ICollectionSlotService
{
    Task<IReadOnlyList<CollectionSlotReadDto>> GetAllAsync(CancellationToken ct = default);
    Task<CollectionSlotReadDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CollectionSlotReadDto> CreateAsync(CreateCollectionSlotRequest request, CancellationToken ct = default);
    Task<CollectionSlotReadDto?> UpdateAsync(Guid id, UpdateCollectionSlotRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
