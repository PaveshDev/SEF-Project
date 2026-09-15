using WasteToValue.Api.Modules.Collections.DTOs;

namespace WasteToValue.Api.Modules.Collections.Interfaces;

public interface IPickupRequestService
{
    Task<IReadOnlyList<PickupRequestReadDto>> GetAllAsync(string? statusFilter = null, CancellationToken ct = default);
    Task<PickupRequestReadDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PickupRequestReadDto> CreateAsync(CreatePickupRequestRequest request, CancellationToken ct = default);
    Task<PickupRequestReadDto?> UpdateAsync(Guid id, UpdatePickupRequestRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PickupEventReadDto>> GetEventsAsync(Guid pickupRequestId, CancellationToken ct = default);
    Task<PickupRequestReadDto?> RescheduleAsync(Guid id, RescheduleRequestDto request, CancellationToken ct = default);
}
