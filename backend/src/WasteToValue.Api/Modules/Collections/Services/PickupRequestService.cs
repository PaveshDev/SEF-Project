using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// In-memory demo implementation of <see cref="IPickupRequestService"/>.
/// Provides sample pickup data matching the React/Flutter demo fixtures.
/// </summary>
public sealed class PickupRequestService : IPickupRequestService
{
    private readonly List<PickupRequestReadDto> _pickups;
    private readonly List<PickupEventReadDto> _events;

    public PickupRequestService()
    {
        var baseDate = new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.FromHours(5.5));
        var demoOwnerId = Guid.Parse("b1000000-0000-0000-0000-000000000001");
        var demoSlotId = Guid.Parse("a1000000-0000-0000-0000-000000000202");

        _pickups = new List<PickupRequestReadDto>
        {
            new(Guid.Parse("c1000000-0000-0000-0000-000000001042"), Guid.NewGuid(), demoSlotId, null, demoOwnerId,
                baseDate.AddHours(14), baseDate.AddHours(16), "PROPOSED", baseDate, baseDate, null),
            new(Guid.Parse("c1000000-0000-0000-0000-000000001041"), Guid.NewGuid(),
                Guid.Parse("a1000000-0000-0000-0000-000000000201"), null, demoOwnerId,
                baseDate.AddHours(9), baseDate.AddHours(11), "CONFIRMED", baseDate, baseDate, null),
            new(Guid.Parse("c1000000-0000-0000-0000-000000001040"), Guid.NewGuid(),
                Guid.Parse("a1000000-0000-0000-0000-000000000201"), null, demoOwnerId,
                baseDate.AddHours(11), baseDate.AddHours(13), "FAILED", baseDate, baseDate, null),
            new(Guid.Parse("c1000000-0000-0000-0000-000000001039"), Guid.NewGuid(),
                Guid.Parse("a1000000-0000-0000-0000-000000000201"), null, demoOwnerId,
                baseDate.AddHours(8), baseDate.AddHours(10), "ASSIGNED", baseDate, baseDate, null),
            new(Guid.Parse("c1000000-0000-0000-0000-000000001038"), Guid.NewGuid(),
                Guid.Parse("a1000000-0000-0000-0000-000000000202"), null, demoOwnerId,
                baseDate.AddHours(-11), baseDate.AddHours(-9), "DELIVERED", baseDate.AddDays(-1), baseDate, null),
        };

        _events = new List<PickupEventReadDto>
        {
            new(Guid.NewGuid(), _pickups[1].Id, "CONFIRMED", demoOwnerId, baseDate, "Sample assignment"),
            new(Guid.NewGuid(), _pickups[2].Id, "CONFIRMED", demoOwnerId, baseDate, "Sample assignment"),
            new(Guid.NewGuid(), _pickups[2].Id, "FAILED", demoOwnerId, baseDate.AddHours(1), "Assigned vehicle unavailable"),
            new(Guid.NewGuid(), _pickups[3].Id, "CONFIRMED", demoOwnerId, baseDate, "Sample assignment"),
            new(Guid.NewGuid(), _pickups[3].Id, "ASSIGNED", demoOwnerId, baseDate.AddHours(1), "En route"),
            new(Guid.NewGuid(), _pickups[4].Id, "CONFIRMED", demoOwnerId, baseDate.AddDays(-1), null),
            new(Guid.NewGuid(), _pickups[4].Id, "COLLECTED", demoOwnerId, baseDate.AddDays(-1).AddHours(2), null),
            new(Guid.NewGuid(), _pickups[4].Id, "DELIVERED", demoOwnerId, baseDate.AddDays(-1).AddHours(3), null),
        };
    }

    public Task<IReadOnlyList<PickupRequestReadDto>> GetAllAsync(string? statusFilter = null, CancellationToken ct = default)
    {
        var result = string.IsNullOrWhiteSpace(statusFilter)
            ? _pickups
            : _pickups.Where(p => p.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult<IReadOnlyList<PickupRequestReadDto>>(result.AsReadOnly());
    }

    public Task<PickupRequestReadDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_pickups.Find(p => p.Id == id));

    public Task<PickupRequestReadDto> CreateAsync(CreatePickupRequestRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var dto = new PickupRequestReadDto(
            Guid.NewGuid(), request.RecoveryProposalId, request.CollectionSlotId,
            null, request.OwnerId, request.ScheduledStart, request.ScheduledEnd,
            "DRAFT", now, now, null);
        _pickups.Add(dto);
        return Task.FromResult(dto);
    }

    public Task<PickupRequestReadDto?> UpdateAsync(Guid id, UpdatePickupRequestRequest request, CancellationToken ct = default)
    {
        var index = _pickups.FindIndex(p => p.Id == id);
        if (index < 0) return Task.FromResult<PickupRequestReadDto?>(null);

        var existing = _pickups[index];
        var updated = new PickupRequestReadDto(
            existing.Id, existing.RecoveryProposalId, existing.CollectionSlotId,
            request.CollectorId ?? existing.CollectorId, existing.OwnerId,
            request.ScheduledStart ?? existing.ScheduledStart,
            request.ScheduledEnd ?? existing.ScheduledEnd,
            request.Status ?? existing.Status,
            existing.CreatedAt, DateTimeOffset.UtcNow, existing.CollectionSlot);
        _pickups[index] = updated;
        return Task.FromResult<PickupRequestReadDto?>(updated);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var removed = _pickups.RemoveAll(p => p.Id == id);
        return Task.FromResult(removed > 0);
    }

    public Task<IReadOnlyList<PickupEventReadDto>> GetEventsAsync(Guid pickupRequestId, CancellationToken ct = default)
    {
        var events = _events.Where(e => e.PickupRequestId == pickupRequestId).ToList();
        return Task.FromResult<IReadOnlyList<PickupEventReadDto>>(events.AsReadOnly());
    }

    public Task<PickupRequestReadDto?> RescheduleAsync(Guid id, RescheduleRequestDto request, CancellationToken ct = default)
    {
        var index = _pickups.FindIndex(p => p.Id == id);
        if (index < 0) return Task.FromResult<PickupRequestReadDto?>(null);

        var existing = _pickups[index];
        var updated = new PickupRequestReadDto(
            existing.Id, existing.RecoveryProposalId, existing.CollectionSlotId,
            existing.CollectorId, existing.OwnerId, existing.ScheduledStart, existing.ScheduledEnd,
            "RESCHEDULE_REQUIRED", existing.CreatedAt, DateTimeOffset.UtcNow, existing.CollectionSlot);
        _pickups[index] = updated;

        _events.Add(new PickupEventReadDto(
            Guid.NewGuid(), id, "RESCHEDULED", request.RequestedBy,
            DateTimeOffset.UtcNow, $"Reschedule requested: {request.Reason}"));

        return Task.FromResult<PickupRequestReadDto?>(updated);
    }
}
