using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Entities;
using WasteToValue.Api.Modules.Collections.Services;
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

        var request = new CreatePickupRequestRequest(
            Guid.NewGuid(), slotId, Guid.NewGuid(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2));

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

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Status = "COLLECTED",
            VerificationCode = "plain",
            IdempotencyKey = "key-wf1",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var updateRequest = new UpdatePickupRequestRequest(null, null, null, "DELIVERED");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(pickupId, updateRequest));
        Assert.Contains("without recorded handover proof", ex.Message);
    }

    [Fact]
    public async Task InvalidStatusTransition_SkippingStates_ThrowsException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new PickupRequestService(db);
        var pickupId = Guid.NewGuid();

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Status = "DRAFT",
            VerificationCode = "plain",
            IdempotencyKey = "key-wf2",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var updateRequest = new UpdatePickupRequestRequest(null, null, null, "DELIVERED");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(pickupId, updateRequest));
        Assert.Contains("Invalid status transition from DRAFT to DELIVERED", ex.Message);
    }

    [Fact]
    public async Task RescheduleDeliveredPickup_ThrowsException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new PickupRequestService(db);
        var pickupId = Guid.NewGuid();

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Status = "DELIVERED",
            VerificationCode = "plain",
            IdempotencyKey = "key-wf3",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var rescheduleRequest = new RescheduleRequestDto(pickupId, Guid.NewGuid(), "Change time");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RescheduleAsync(pickupId, rescheduleRequest));
        Assert.Contains("Cannot reschedule a pickup request with status DELIVERED", ex.Message);
    }
}
