using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Entities;
using WasteToValue.Api.Modules.Collections.Services;
using WasteToValue.Api.Modules.Collections.Validators;
using Xunit;

namespace WasteToValue.Tests.Collections;

public sealed class PickupStatusWorkflowTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreatePickupRequest_WithUnavailableSlot_ThrowsException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new PickupRequestService(db);
        var slotId = Guid.NewGuid();

        db.CollectionSlots.Add(new CollectionSlot
        {
            Id = slotId,
            StartsAt = DateTimeOffset.UtcNow,
            EndsAt = DateTimeOffset.UtcNow.AddHours(2),
            ServiceArea = "Colombo",
            Capacity = 1,
            ReservedCount = 1,
            Status = "BOOKED",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var scheduledStart = DateTimeOffset.UtcNow;
        var request = new CreatePickupRequestRequest(
            RecoveryProposalId: Guid.NewGuid(),
            CollectionSlotId: slotId,
            OwnerId: Guid.NewGuid(),
            PickupAddress: "25 Temple Road, Colombo",
            ScheduledStart: scheduledStart,
            ScheduledEnd: scheduledStart.AddHours(2));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
        Assert.Contains("unavailable", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TransitionToDelivered_WithoutHandoverProof_ThrowsException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new PickupRequestService(db);
        var pickupId = Guid.NewGuid();
        var slot = await CreateSlotAsync(db);

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = slot.Id,
            OwnerId = Guid.NewGuid(),
            PickupAddressEncrypted = "Test encrypted address",
            ScheduledStart = slot.StartsAt,
            ScheduledEnd = slot.EndsAt,
            Status = "COLLECTED",
            VerificationCode = "plain",
            IdempotencyKey = "key-wf1",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var updateRequest = new UpdatePickupRequestRequest(
            ScheduledStart: null,
            ScheduledEnd: null,
            PickupAddress: null,
            CollectorId: null,
            Status: "DELIVERED");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(pickupId, updateRequest));
        Assert.Contains("without recorded handover proof", ex.Message);
        db.ChangeTracker.Clear();
        Assert.Equal("COLLECTED", (await db.PickupRequests.SingleAsync()).Status);
        Assert.Empty(await db.PickupEvents.ToListAsync());
    }

    [Fact]
    public async Task InvalidStatusTransition_SkippingStates_ThrowsException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new PickupRequestService(db);
        var pickupId = Guid.NewGuid();
        var slot = await CreateSlotAsync(db);

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = slot.Id,
            OwnerId = Guid.NewGuid(),
            PickupAddressEncrypted = "Test encrypted address",
            ScheduledStart = slot.StartsAt,
            ScheduledEnd = slot.EndsAt,
            Status = "DRAFT",
            VerificationCode = "plain",
            IdempotencyKey = "key-wf2",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var updateRequest = new UpdatePickupRequestRequest(
            ScheduledStart: null,
            ScheduledEnd: null,
            PickupAddress: null,
            CollectorId: null,
            Status: "DELIVERED");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(pickupId, updateRequest));
        Assert.Contains("Invalid status transition from DRAFT to DELIVERED", ex.Message);
        db.ChangeTracker.Clear();
        Assert.Equal("DRAFT", (await db.PickupRequests.SingleAsync()).Status);
        Assert.Empty(await db.PickupEvents.ToListAsync());
    }

    [Fact]
    public async Task RescheduleDeliveredPickup_ThrowsException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new PickupRequestService(db);
        var pickupId = Guid.NewGuid();
        var slot = await CreateSlotAsync(db);

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = slot.Id,
            OwnerId = Guid.NewGuid(),
            PickupAddressEncrypted = "Test encrypted address",
            ScheduledStart = slot.StartsAt,
            ScheduledEnd = slot.EndsAt,
            Status = "DELIVERED",
            VerificationCode = "plain",
            IdempotencyKey = "key-wf3",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var rescheduleRequest = new RescheduleRequestDto(
            PickupRequestId: pickupId,
            Reason: "Change time",
            ChangedConstraint: "Owner available only after 14:00",
            RequestedBy: Guid.Parse("44444444-4444-4444-8444-444444444444"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RescheduleAsync(pickupId, rescheduleRequest));
        Assert.Contains("Cannot reschedule a pickup request with status DELIVERED", ex.Message);
    }

    [Theory]
    [InlineData("DRAFT", "PLANNED")]
    [InlineData("CONFIRMED", "ASSIGNED")]
    [InlineData("FAILED", "RESCHEDULE_PENDING")]
    [InlineData("RESCHEDULE_PENDING", "PENDING_APPROVAL")]
    public async Task UpdatePickup_WithValidTransition_PersistsStatusAndAuditEvent(string currentStatus, string targetStatus)
    {
        using var db = CreateInMemoryDbContext();
        var pickup = await SeedPickupAsync(db, currentStatus);
        var service = new PickupRequestService(db);

        var updated = await service.UpdateAsync(pickup.Id, new UpdatePickupRequestRequest(
            ScheduledStart: null,
            ScheduledEnd: null,
            PickupAddress: null,
            CollectorId: null,
            Status: targetStatus));

        Assert.NotNull(updated);
        Assert.Equal(targetStatus, updated.Status);
        db.ChangeTracker.Clear();
        Assert.Equal(targetStatus, (await db.PickupRequests.SingleAsync()).Status);
        var auditEvent = Assert.Single(await service.GetEventsAsync(pickup.Id));
        Assert.Equal(targetStatus, auditEvent.EventType);
        Assert.Equal(pickup.OwnerId, auditEvent.ActorId);
    }

    [Theory]
    [InlineData("CONFIRMED", "DELIVERED")]
    [InlineData("CANCELLED", "ASSIGNED")]
    [InlineData("DELIVERED", "COLLECTED")]
    [InlineData("ASSIGNED", "CLIENT_DEFINED_STATUS")]
    public async Task UpdatePickup_WithUnsupportedTransition_RejectsWithoutChangingState(string currentStatus, string targetStatus)
    {
        using var db = CreateInMemoryDbContext();
        var pickup = await SeedPickupAsync(db, currentStatus);
        var service = new PickupRequestService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(
            pickup.Id, new UpdatePickupRequestRequest(
                ScheduledStart: null,
                ScheduledEnd: null,
                PickupAddress: null,
                CollectorId: null,
                Status: targetStatus)));

        Assert.Contains($"Invalid status transition from {currentStatus} to {targetStatus}", ex.Message);
        db.ChangeTracker.Clear();
        Assert.Equal(currentStatus, (await db.PickupRequests.SingleAsync()).Status);
        Assert.Empty(await db.PickupEvents.ToListAsync());
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("00000000-0000-0000-0000-000000000000", true)]
    [InlineData("44444444-4444-4444-8444-444444444444", false)]
    public async Task TransitionToDelivered_WithUnverifiedProof_RejectsWithoutChangingState(string? verifiedBy, bool hasVerifiedAt)
    {
        using var db = CreateInMemoryDbContext();
        var pickup = await SeedPickupAsync(db, "COLLECTED");
        var service = new PickupRequestService(db);
        var proofEvent = new PickupEvent
        {
            Id = Guid.NewGuid(),
            PickupRequestId = pickup.Id,
            EventType = "PROOF_SUBMITTED",
            ActorId = pickup.OwnerId,
            EventAt = DateTimeOffset.UtcNow,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        };
        db.PickupEvents.Add(proofEvent);
        db.HandoverProofs.Add(new HandoverProof
        {
            Id = Guid.NewGuid(),
            PickupRequestId = pickup.Id,
            PickupEventId = proofEvent.Id,
            ProofType = "PHOTO",
            StorageKey = "test/handover.jpg",
            VerifiedBy = verifiedBy is null ? null : Guid.Parse(verifiedBy),
            VerifiedAt = hasVerifiedAt ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(
            pickup.Id, new UpdatePickupRequestRequest(
                ScheduledStart: null,
                ScheduledEnd: null,
                PickupAddress: null,
                CollectorId: null,
                Status: "DELIVERED")));

        Assert.Contains("without recorded handover proof", ex.Message);
        db.ChangeTracker.Clear();
        Assert.Equal("COLLECTED", (await db.PickupRequests.SingleAsync()).Status);
        Assert.Equal("PROOF_SUBMITTED", Assert.Single(await service.GetEventsAsync(pickup.Id)).EventType);
    }

    [Fact]
    public async Task TransitionToDelivered_AfterVerifiedHandover_PersistsDeliveryAndAuditEvent()
    {
        using var db = CreateInMemoryDbContext();
        var pickup = await SeedPickupAsync(db, "ASSIGNED");
        pickup.VerificationCode = HandoverCodeHasher.FormatVerificationString(pickup.Id, "123456", TimeSpan.FromHours(1));
        await db.SaveChangesAsync();
        var handover = new HandoverService(db);
        var service = new PickupRequestService(db);

        var proof = await handover.VerifyCodeAsync(pickup.Id, new SubmitHandoverCodeRequest(
            Code: "123456",
            ActorId: pickup.OwnerId));

        Assert.NotNull(proof);
        Assert.Equal(pickup.OwnerId, proof.VerifiedBy);
        Assert.NotNull(proof.VerifiedAt);
        Assert.Equal("COLLECTED", pickup.Status);
        db.ChangeTracker.Clear();

        var delivered = await service.UpdateAsync(pickup.Id, new UpdatePickupRequestRequest(
            ScheduledStart: null,
            ScheduledEnd: null,
            PickupAddress: null,
            CollectorId: null,
            Status: "DELIVERED"));

        Assert.NotNull(delivered);
        Assert.Equal("DELIVERED", delivered.Status);
        db.ChangeTracker.Clear();
        Assert.Equal("DELIVERED", (await db.PickupRequests.SingleAsync()).Status);
        var events = await service.GetEventsAsync(pickup.Id);
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e.EventType == "HANDOVER_VERIFIED");
        Assert.Contains(events, e => e.EventType == "DELIVERED");
    }

    private static Task<CollectionSlotReadDto> CreateSlotAsync(AppDbContext db)
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        return new CollectionSlotService(db).CreateAsync(new CreateCollectionSlotRequest(
            StartsAt: start,
            EndsAt: start.AddHours(2),
            ServiceArea: "Colombo",
            Capacity: 3,
            VehicleClass: "Small van",
            CollectorId: Guid.Parse("44444444-4444-4444-8444-444444444444")));
    }

    private static async Task<PickupRequest> SeedPickupAsync(AppDbContext db, string status)
    {
        var slot = await CreateSlotAsync(db);
        var pickup = new PickupRequest
        {
            Id = Guid.NewGuid(),
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = slot.Id,
            OwnerId = Guid.Parse("44444444-4444-4444-8444-444444444444"),
            PickupAddressEncrypted = "Test encrypted address",
            ScheduledStart = slot.StartsAt,
            ScheduledEnd = slot.EndsAt,
            Status = status,
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.PickupRequests.Add(pickup);
        await db.SaveChangesAsync();
        return pickup;
    }
}
