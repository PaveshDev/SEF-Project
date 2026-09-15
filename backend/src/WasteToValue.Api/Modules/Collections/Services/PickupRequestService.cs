using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Entities;
using WasteToValue.Api.Modules.Collections.Interfaces;
using WasteToValue.Api.Modules.Collections.Validators;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// EF Core database-backed implementation of <see cref="IPickupRequestService"/>.
/// Handles pickup request lifecycle, state machine transitions, and database persistence.
/// </summary>
public sealed class PickupRequestService : IPickupRequestService
{
    private readonly AppDbContext _db;

    public PickupRequestService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PickupRequestReadDto>> GetAllAsync(string? statusFilter = null, CancellationToken ct = default)
    {
        var query = _db.PickupRequests
            .AsNoTracking()
            .Include(p => p.CollectionSlot)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(p => p.Status == statusFilter.ToUpperInvariant());
        }

        var entities = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
        return entities.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<PickupRequestReadDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PickupRequests
            .AsNoTracking()
            .Include(p => p.CollectionSlot)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<PickupRequestReadDto> CreateAsync(CreatePickupRequestRequest request, CancellationToken ct = default)
    {
        // 1. Verify collection slot availability if slot ID is provided
        CollectionSlot? slot = null;
        if (request.CollectionSlotId != Guid.Empty)
        {
            slot = await _db.CollectionSlots.FirstOrDefaultAsync(s => s.Id == request.CollectionSlotId, ct);
            if (slot is null)
            {
                throw new InvalidOperationException($"Collection slot {request.CollectionSlotId} not found.");
            }

            if (!slot.Status.Equals("AVAILABLE", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Collection slot {request.CollectionSlotId} is unavailable (status: {slot.Status}).");
            }

            if (slot.ReservedCount >= slot.Capacity)
            {
                throw new InvalidOperationException($"Collection slot {request.CollectionSlotId} is fully booked (capacity: {slot.Capacity}).");
            }
        }

        var now = DateTimeOffset.UtcNow;
        var pickupId = Guid.NewGuid();
        var plainCode = HandoverCodeHasher.GeneratePlainCode();
        var verificationString = HandoverCodeHasher.FormatVerificationString(pickupId, plainCode);

        var entity = new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = request.RecoveryProposalId,
            CollectionSlotId = request.CollectionSlotId,
            CollectorId = null,
            OwnerId = request.OwnerId,
            PickupAddressEncrypted = "EncryptedAddressPlaceholder",
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Status = "CONFIRMED",
            VerificationCode = verificationString,
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
            CollectionSlot = slot
        };

        if (slot is not null)
        {
            slot.ReservedCount += 1;
            if (slot.ReservedCount >= slot.Capacity)
            {
                slot.Status = "BOOKED";
            }
        }

        var initialEvent = new PickupEvent
        {
            Id = Guid.NewGuid(),
            PickupRequestId = pickupId,
            EventType = "CONFIRMED",
            ActorId = request.OwnerId,
            EventAt = now,
            Notes = "Pickup request created and confirmed.",
            IdempotencyKey = Guid.NewGuid().ToString("N")
        };

        _db.PickupRequests.Add(entity);
        _db.PickupEvents.Add(initialEvent);

        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public async Task<PickupRequestReadDto?> UpdateAsync(Guid id, UpdatePickupRequestRequest request, CancellationToken ct = default)
    {
        var entity = await _db.PickupRequests
            .Include(p => p.CollectionSlot)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entity is null) return null;

        var now = DateTimeOffset.UtcNow;

        // If status transition is requested, validate state machine logic
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            !request.Status.Equals(entity.Status, StringComparison.OrdinalIgnoreCase))
        {
            var targetStatus = request.Status.ToUpperInvariant();

            if (!IsValidStatusTransition(entity.Status, targetStatus))
            {
                throw new InvalidOperationException($"Invalid status transition from {entity.Status} to {targetStatus}.");
            }

            // Specific business rule validations:
            if (targetStatus == "DELIVERED")
            {
                var hasProof = await _db.HandoverProofs.AnyAsync(h => h.PickupRequestId == id, ct);
                if (!hasProof)
                {
                    throw new InvalidOperationException("Cannot transition pickup to DELIVERED without recorded handover proof.");
                }
            }

            if (targetStatus == "CONFIRMED" && entity.Status != "PENDING_APPROVAL")
            {
                throw new InvalidOperationException("Cannot confirm pickup request prior to staff approval.");
            }

            // Log event for status change
            _db.PickupEvents.Add(new PickupEvent
            {
                Id = Guid.NewGuid(),
                PickupRequestId = id,
                EventType = targetStatus,
                ActorId = request.CollectorId ?? entity.CollectorId ?? entity.OwnerId,
                EventAt = now,
                Notes = $"Status changed from {entity.Status} to {targetStatus}.",
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });

            entity.Status = targetStatus;
        }

        if (request.CollectorId is not null) entity.CollectorId = request.CollectorId;
        if (request.ScheduledStart is not null) entity.ScheduledStart = request.ScheduledStart.Value;
        if (request.ScheduledEnd is not null) entity.ScheduledEnd = request.ScheduledEnd.Value;
        entity.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PickupRequests
            .Include(p => p.CollectionSlot)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entity is null) return false;

        // Mark as cancelled or remove
        if (entity.Status == "DELIVERED")
        {
            throw new InvalidOperationException("Cannot cancel or delete a DELIVERED pickup request.");
        }

        if (entity.CollectionSlot is not null && entity.CollectionSlot.ReservedCount > 0)
        {
            entity.CollectionSlot.ReservedCount -= 1;
            if (entity.CollectionSlot.Status == "BOOKED")
            {
                entity.CollectionSlot.Status = "AVAILABLE";
            }
        }

        entity.Status = "CANCELLED";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        _db.PickupEvents.Add(new PickupEvent
        {
            Id = Guid.NewGuid(),
            PickupRequestId = id,
            EventType = "CANCELLED",
            ActorId = entity.OwnerId,
            EventAt = DateTimeOffset.UtcNow,
            Notes = "Pickup request cancelled.",
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<PickupEventReadDto>> GetEventsAsync(Guid pickupRequestId, CancellationToken ct = default)
    {
        var events = await _db.PickupEvents
            .AsNoTracking()
            .Where(e => e.PickupRequestId == pickupRequestId)
            .OrderBy(e => e.EventAt)
            .ToListAsync(ct);

        return events.Select(e => new PickupEventReadDto(
            e.Id, e.PickupRequestId, e.EventType, e.ActorId, e.EventAt, e.Notes
        )).ToList().AsReadOnly();
    }

    public async Task<PickupRequestReadDto?> RescheduleAsync(Guid id, RescheduleRequestDto request, CancellationToken ct = default)
    {
        var entity = await _db.PickupRequests.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return null;

        if (entity.Status == "DELIVERED" || entity.Status == "CANCELLED")
        {
            throw new InvalidOperationException($"Cannot reschedule a pickup request with status {entity.Status}.");
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = "RESCHEDULE_PENDING";
        entity.UpdatedAt = now;

        _db.PickupEvents.Add(new PickupEvent
        {
            Id = Guid.NewGuid(),
            PickupRequestId = id,
            EventType = "RESCHEDULE_REQUESTED",
            ActorId = request.RequestedBy,
            EventAt = now,
            Notes = $"Reschedule requested: {request.Reason}",
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public static bool IsValidStatusTransition(string currentStatus, string targetStatus)
    {
        if (currentStatus.Equals(targetStatus, StringComparison.OrdinalIgnoreCase))
            return true;

        return (currentStatus.ToUpperInvariant(), targetStatus.ToUpperInvariant()) switch
        {
            ("DRAFT", "PLANNED") => true,
            ("DRAFT", "CANCELLED") => true,

            ("PLANNED", "PENDING_APPROVAL") => true,
            ("PLANNED", "CANCELLED") => true,

            ("PENDING_APPROVAL", "CONFIRMED") => true,
            ("PENDING_APPROVAL", "CANCELLED") => true,

            ("CONFIRMED", "ASSIGNED") => true,
            ("CONFIRMED", "CANCELLED") => true,

            ("ASSIGNED", "COLLECTED") => true,
            ("ASSIGNED", "FAILED") => true,
            ("ASSIGNED", "CANCELLED") => true,

            ("COLLECTED", "DELIVERED") => true,
            ("COLLECTED", "FAILED") => true,

            ("FAILED", "RESCHEDULE_PENDING") => true,
            ("FAILED", "CANCELLED") => true,

            ("RESCHEDULE_PENDING", "PENDING_APPROVAL") => true,
            ("RESCHEDULE_PENDING", "CANCELLED") => true,

            _ => false
        };
    }

    private static PickupRequestReadDto MapToDto(PickupRequest p) => new(
        p.Id,
        p.RecoveryProposalId,
        p.CollectionSlotId,
        p.CollectorId,
        p.OwnerId,
        p.ScheduledStart,
        p.ScheduledEnd,
        p.Status,
        p.CreatedAt,
        p.UpdatedAt,
        p.CollectionSlot is null ? null : new CollectionSlotReadDto(
            p.CollectionSlot.Id,
            p.CollectionSlot.CollectorId,
            p.CollectionSlot.StartsAt,
            p.CollectionSlot.EndsAt,
            p.CollectionSlot.ServiceArea,
            p.CollectionSlot.Capacity,
            p.CollectionSlot.ReservedCount,
            p.CollectionSlot.VehicleClass,
            p.CollectionSlot.Status,
            p.CollectionSlot.CreatedAt,
            p.CollectionSlot.UpdatedAt)
    );
}
