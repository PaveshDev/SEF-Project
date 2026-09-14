using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Services;
using Xunit;

namespace WasteToValue.Tests.Collections;

public sealed class PickupCrudTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task PickupRequest_CreateReadUpdateDelete_Succeeds()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new PickupRequestService(db);

        var ownerId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow.AddHours(1);
        var slot = await new CollectionSlotService(db).CreateAsync(new CreateCollectionSlotRequest(
            StartsAt: now,
            EndsAt: now.AddHours(2),
            ServiceArea: "Colombo",
            Capacity: 1,
            VehicleClass: "Small van",
            CollectorId: Guid.Parse("44444444-4444-4444-8444-444444444444")));

        var createReq = new CreatePickupRequestRequest(
            RecoveryProposalId: proposalId,
            CollectionSlotId: slot.Id,
            OwnerId: ownerId,
            PickupAddress: "25 Temple Road, Colombo",
            ScheduledStart: now,
            ScheduledEnd: now.AddHours(2));

        // Act 1: Create
        var created = await service.CreateAsync(createReq);

        // Assert 1: Create
        Assert.NotNull(created);
        Assert.Equal(ownerId, created.OwnerId);
        Assert.Equal("CONFIRMED", created.Status);
        Assert.Equal(slot.Id, created.CollectionSlotId);
        db.ChangeTracker.Clear();

        // Act 2: Read GetById
        var fetched = await service.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.NotNull(fetched.CollectionSlot);
        Assert.Equal(slot.Id, fetched.CollectionSlot.Id);
        Assert.Equal(1, fetched.CollectionSlot.ReservedCount);

        // Act 3: Read GetAll
        var all = await service.GetAllAsync();
        Assert.Single(all);

        // Act 4: Update
        var newCollectorId = Guid.NewGuid();
        var updateReq = new UpdatePickupRequestRequest(
            ScheduledStart: null,
            ScheduledEnd: null,
            PickupAddress: null,
            CollectorId: newCollectorId,
            Status: null);
        var updated = await service.UpdateAsync(created.Id, updateReq);

        // Assert 4: Update
        Assert.NotNull(updated);
        Assert.Equal(newCollectorId, updated.CollectorId);

        // Act 5: Delete / Cancel
        var deleted = await service.DeleteAsync(created.Id);

        // Assert 5: Delete / Cancel
        Assert.True(deleted);
        var cancelledPickup = await service.GetByIdAsync(created.Id);
        Assert.NotNull(cancelledPickup);
        Assert.Equal("CANCELLED", cancelledPickup.Status);
        Assert.NotNull(cancelledPickup.CollectionSlot);
        Assert.Equal(0, cancelledPickup.CollectionSlot.ReservedCount);
        Assert.Equal("AVAILABLE", cancelledPickup.CollectionSlot.Status);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "CollectionSlotId is required.")]
    [InlineData("44444444-4444-4444-8444-444444444444", "not found")]
    public async Task CreatePickupRequest_WithMissingSlot_RejectsWithoutPersisting(string slotId, string expectedError)
    {
        using var db = CreateInMemoryDbContext();
        var service = new PickupRequestService(db);
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var request = new CreatePickupRequestRequest(
            RecoveryProposalId: Guid.NewGuid(),
            CollectionSlotId: Guid.Parse(slotId),
            OwnerId: Guid.NewGuid(),
            PickupAddress: "25 Temple Road, Colombo",
            ScheduledStart: start,
            ScheduledEnd: start.AddHours(2));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));

        Assert.Contains(expectedError, ex.Message);
        Assert.Empty(await db.PickupRequests.ToListAsync());
        Assert.Empty(await db.PickupEvents.ToListAsync());
    }
}
