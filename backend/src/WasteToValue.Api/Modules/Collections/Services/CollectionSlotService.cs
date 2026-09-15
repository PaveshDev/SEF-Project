using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Entities;
using WasteToValue.Api.Modules.Collections.Interfaces;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// EF Core implementation of <see cref="ICollectionSlotService"/>.
/// Persists collection slot data to the PostgreSQL database via <see cref="AppDbContext"/>.
/// </summary>
public sealed class CollectionSlotService : ICollectionSlotService
{
    private readonly AppDbContext _db;

    public CollectionSlotService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CollectionSlotReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _db.CollectionSlots
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<CollectionSlotReadDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.CollectionSlots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<CollectionSlotReadDto> CreateAsync(CreateCollectionSlotRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new CollectionSlot
        {
            Id = Guid.NewGuid(),
            CollectorId = request.CollectorId,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            ServiceArea = request.ServiceArea,
            Capacity = request.Capacity,
            ReservedCount = 0,
            VehicleClass = request.VehicleClass,
            Status = "AVAILABLE",
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
        };

        _db.CollectionSlots.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public async Task<CollectionSlotReadDto?> UpdateAsync(Guid id, UpdateCollectionSlotRequest request, CancellationToken ct = default)
    {
        var entity = await _db.CollectionSlots.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return null;

        if (request.CollectorId is not null) entity.CollectorId = request.CollectorId;
        if (request.StartsAt is not null) entity.StartsAt = request.StartsAt.Value;
        if (request.EndsAt is not null) entity.EndsAt = request.EndsAt.Value;
        if (request.ServiceArea is not null) entity.ServiceArea = request.ServiceArea;
        if (request.Capacity is not null) entity.Capacity = request.Capacity.Value;
        if (request.VehicleClass is not null) entity.VehicleClass = request.VehicleClass;
        if (request.Status is not null) entity.Status = request.Status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.CollectionSlots.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return false;

        _db.CollectionSlots.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static CollectionSlotReadDto MapToDto(CollectionSlot s) => new(
        s.Id, s.CollectorId, s.StartsAt, s.EndsAt, s.ServiceArea,
        s.Capacity, s.ReservedCount, s.VehicleClass, s.Status,
        s.CreatedAt, s.UpdatedAt);
}
