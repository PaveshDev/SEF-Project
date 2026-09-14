using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Modules.Collections.Entities;

namespace WasteToValue.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PickupRequest> PickupRequests => Set<PickupRequest>();
    public DbSet<CollectionSlot> CollectionSlots => Set<CollectionSlot>();
    public DbSet<PickupPlan> PickupPlans => Set<PickupPlan>();
    public DbSet<PickupEvent> PickupEvents => Set<PickupEvent>();
    public DbSet<HandoverProof> HandoverProofs => Set<HandoverProof>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
