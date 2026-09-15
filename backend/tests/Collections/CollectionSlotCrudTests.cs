using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Services;
using Xunit;

namespace WasteToValue.Tests.Collections;

public sealed class CollectionSlotCrudTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CollectionSlot_CreateReadUpdateDelete_Succeeds()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new CollectionSlotService(db);

        var collectorId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var createReq = new CreateCollectionSlotRequest(
            collectorId, now, now.AddHours(4), "Colombo", 3, "Small van");

        // Act 1: Create
        var created = await service.CreateAsync(createReq);

        // Assert 1: Create
        Assert.NotNull(created);
        Assert.Equal("Colombo", created.ServiceArea);
        Assert.Equal(3, created.Capacity);
        Assert.Equal("AVAILABLE", created.Status);

        // Act 2: Read GetById
        var fetched = await service.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);

        // Act 3: Read GetAll
        var all = await service.GetAllAsync();
        Assert.Single(all);

        // Act 4: Update
        var updateReq = new UpdateCollectionSlotRequest(null, null, null, "Kandy", 5, "Large truck", null);
        var updated = await service.UpdateAsync(created.Id, updateReq);

        // Assert 4: Update
        Assert.NotNull(updated);
        Assert.Equal("Kandy", updated.ServiceArea);
        Assert.Equal(5, updated.Capacity);
        Assert.Equal("Large truck", updated.VehicleClass);

        // Act 5: Delete
        var deleted = await service.DeleteAsync(created.Id);

        // Assert 5: Delete
        Assert.True(deleted);
        var deletedSlot = await service.GetByIdAsync(created.Id);
        Assert.Null(deletedSlot);
    }
}
