using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Entities;
using WasteToValue.Api.Modules.Collections.Interfaces;
using WasteToValue.Api.Modules.Collections.Validators;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// EF Core database-backed implementation of <see cref="IHandoverService"/>.
/// Validates secure OTP / QR codes, records handover proofs, updates pickup status, and logs audit events.
/// </summary>
public sealed class HandoverService : IHandoverService
{
    private readonly AppDbContext _db;

    public HandoverService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HandoverProofReadDto?> VerifyCodeAsync(Guid pickupRequestId, SubmitHandoverCodeRequest request, CancellationToken ct = default)
    {
        var errors = HandoverCodeValidator.Validate(request.Code);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join("; ", errors));
        }

        var pickup = await _db.PickupRequests.FirstOrDefaultAsync(p => p.Id == pickupRequestId, ct);
        if (pickup is null)
        {
            return null;
        }

        // Verify eligible pickup status for handover verification
        var eligibleStatuses = new[] { "CONFIRMED", "ASSIGNED", "COLLECTED" };
        if (!eligibleStatuses.Contains(pickup.Status, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Handover verification is not allowed for pickup with status {pickup.Status}.");
        }

        var now = DateTimeOffset.UtcNow;
        var verifyResult = HandoverCodeHasher.Verify(pickupRequestId, pickup.VerificationCode, request.Code, now);

        if (verifyResult.UpdatedVerificationString is not null)
        {
            pickup.VerificationCode = verifyResult.UpdatedVerificationString;
        }

        if (!verifyResult.IsSuccess)
        {
            // Log failed attempt audit event
            _db.PickupEvents.Add(new PickupEvent
            {
                Id = Guid.NewGuid(),
                PickupRequestId = pickupRequestId,
                EventType = "HANDOVER_FAILED",
                ActorId = request.ActorId,
                EventAt = now,
                Notes = verifyResult.ErrorMessage ?? "Verification failed.",
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });

            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException(verifyResult.ErrorMessage ?? "Code verification failed.");
        }

        // Log successful verification audit event
        var eventId = Guid.NewGuid();
        _db.PickupEvents.Add(new PickupEvent
        {
            Id = eventId,
            PickupRequestId = pickupRequestId,
            EventType = "HANDOVER_VERIFIED",
            ActorId = request.ActorId,
            EventAt = now,
            Notes = "One-time handover verification code verified successfully.",
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        // Record HandoverProof entity
        var proof = new HandoverProof
        {
            Id = Guid.NewGuid(),
            PickupRequestId = pickupRequestId,
            PickupEventId = eventId,
            ProofType = "ONE_TIME_CODE",
            StorageKey = null,
            VerificationHash = request.Code,
            VerifiedBy = request.ActorId,
            VerifiedAt = now,
            CreatedAt = now
        };

        _db.HandoverProofs.Add(proof);

        // Update pickup status to COLLECTED or DELIVERED if appropriate
        if (pickup.Status == "ASSIGNED" || pickup.Status == "CONFIRMED")
        {
            pickup.Status = "COLLECTED";
        }
        else if (pickup.Status == "COLLECTED")
        {
            pickup.Status = "DELIVERED";
        }
        pickup.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        return new HandoverProofReadDto(
            proof.Id,
            proof.PickupRequestId,
            proof.PickupEventId,
            proof.ProofType,
            proof.VerifiedBy ?? request.ActorId,
            proof.VerifiedAt ?? now,
            proof.CreatedAt);
    }

    public async Task<HandoverProofReadDto?> SubmitProofAsync(Guid pickupRequestId, SubmitHandoverProofRequest request, CancellationToken ct = default)
    {
        var pickup = await _db.PickupRequests.FirstOrDefaultAsync(p => p.Id == pickupRequestId, ct);
        if (pickup is null) return null;

        var now = DateTimeOffset.UtcNow;
        var eventId = Guid.NewGuid();

        _db.PickupEvents.Add(new PickupEvent
        {
            Id = eventId,
            PickupRequestId = pickupRequestId,
            EventType = "PROOF_SUBMITTED",
            ActorId = request.ActorId,
            EventAt = now,
            Notes = $"Proof submitted: {request.ProofType}",
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var proof = new HandoverProof
        {
            Id = Guid.NewGuid(),
            PickupRequestId = pickupRequestId,
            PickupEventId = eventId,
            ProofType = request.ProofType,
            StorageKey = request.StorageKey,
            VerificationHash = null,
            VerifiedBy = request.ActorId,
            VerifiedAt = now,
            CreatedAt = now
        };

        _db.HandoverProofs.Add(proof);
        await _db.SaveChangesAsync(ct);

        return new HandoverProofReadDto(
            proof.Id,
            proof.PickupRequestId,
            proof.PickupEventId,
            proof.ProofType,
            proof.VerifiedBy ?? request.ActorId,
            proof.VerifiedAt ?? now,
            proof.CreatedAt);
    }

    public async Task<IReadOnlyList<HandoverProofReadDto>> GetProofsAsync(Guid pickupRequestId, CancellationToken ct = default)
    {
        var proofs = await _db.HandoverProofs
            .AsNoTracking()
            .Where(p => p.PickupRequestId == pickupRequestId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return proofs.Select(p => new HandoverProofReadDto(
            p.Id,
            p.PickupRequestId,
            p.PickupEventId,
            p.ProofType,
            p.VerifiedBy ?? Guid.Empty,
            p.VerifiedAt ?? p.CreatedAt,
            p.CreatedAt
        )).ToList().AsReadOnly();
    }
}
