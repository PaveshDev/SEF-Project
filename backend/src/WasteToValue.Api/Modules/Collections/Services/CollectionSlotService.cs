using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// In-memory demo implementation of <see cref="ICollectionSlotService"/>.
/// Serves deterministic sample data so the API is functional without a database.
/// Replace with an EF-backed implementation when the migration is coordinated.
/// </summary>
public sealed class CollectionSlotService : ICollectionSlotService
{
    private readonly List<CollectionSlotReadDto> _slots;

    public CollectionSlotService()
    {
        var baseDate = new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.FromHours(5.5));
        _slots = new List<CollectionSlotReadDto>
        {
            new(Guid.Parse("a1000000-0000-0000-0000-000000000201"), null,
                baseDate.AddHours(9), baseDate.AddHours(11), "Colombo", 100, 0, "Cargo van", "AVAILABLE",
                baseDate, baseDate),
            new(Guid.Parse("a1000000-0000-0000-0000-000000000202"), null,
                baseDate.AddHours(14), baseDate.AddHours(16), "Colombo", 50, 0, "Small van", "AVAILABLE",
                baseDate, baseDate),
            new(Guid.Parse("a1000000-0000-0000-0000-000000000203"), null,
                baseDate.AddHours(16), baseDate.AddHours(18), "Colombo", 50, 0, "Small van", "AVAILABLE",
                baseDate, baseDate),
        };
    }

    public Task<IReadOnlyList<CollectionSlotReadDto>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CollectionSlotReadDto>>(_slots.AsReadOnly());

    public Task<CollectionSlotReadDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_slots.Find(s => s.Id == id));

    public Task<CollectionSlotReadDto> CreateAsync(CreateCollectionSlotRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var dto = new CollectionSlotReadDto(
            Guid.NewGuid(), request.CollectorId, request.StartsAt, request.EndsAt,
            request.ServiceArea, request.Capacity, 0, request.VehicleClass, "AVAILABLE", now, now);
        _slots.Add(dto);
        return Task.FromResult(dto);
    }

    public Task<CollectionSlotReadDto?> UpdateAsync(Guid id, UpdateCollectionSlotRequest request, CancellationToken ct = default)
    {
        var index = _slots.FindIndex(s => s.Id == id);
        if (index < 0) return Task.FromResult<CollectionSlotReadDto?>(null);

        var existing = _slots[index];
        var updated = new CollectionSlotReadDto(
            existing.Id, request.CollectorId ?? existing.CollectorId,
            request.StartsAt ?? existing.StartsAt, request.EndsAt ?? existing.EndsAt,
            request.ServiceArea ?? existing.ServiceArea, request.Capacity ?? existing.Capacity,
            existing.ReservedCount, request.VehicleClass ?? existing.VehicleClass,
            request.Status ?? existing.Status, existing.CreatedAt, DateTimeOffset.UtcNow);
        _slots[index] = updated;
        return Task.FromResult<CollectionSlotReadDto?>(updated);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var removed = _slots.RemoveAll(s => s.Id == id);
        return Task.FromResult(removed > 0);
    }
}
