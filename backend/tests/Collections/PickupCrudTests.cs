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
        var now = DateTimeOffset.UtcNow;

        var createReq = new CreatePickupRequestRequest(
            proposalId, Guid.Empty, ownerId, now, now.AddHours(2));

        // Act 1: Create
        var created = await service.CreateAsync(createReq);

        // Assert 1: Create
        Assert.NotNull(created);
        Assert.Equal(ownerId, created.OwnerId);
        Assert.Equal("CONFIRMED", created.Status);

        // Act 2: Read GetById
        var fetched = await service.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);

        // Act 3: Read GetAll
        var all = await service.GetAllAsync();
        Assert.Single(all);

        // Act 4: Update
        var newCollectorId = Guid.NewGuid();
        var updateReq = new UpdatePickupRequestRequest(newCollectorId, null, null, null);
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
    }
}
