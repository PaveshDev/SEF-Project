using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Collections.Entities;

namespace WasteToValue.Api.Modules.Collections.Configurations;

public sealed class CollectionSlotConfiguration : IEntityTypeConfiguration<CollectionSlot>
{
    public void Configure(EntityTypeBuilder<CollectionSlot> builder)
    {
        builder.ToTable("collection_slots");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CollectorId).HasColumnName("collector_id");
        builder.Property(e => e.StartsAt).HasColumnName("starts_at");
        builder.Property(e => e.EndsAt).HasColumnName("ends_at");
        builder.Property(e => e.ServiceArea).HasColumnName("service_area").HasMaxLength(300).IsRequired();
        builder.Property(e => e.Capacity).HasColumnName("capacity").HasDefaultValue(1);
        builder.Property(e => e.ReservedCount).HasColumnName("reserved_count").HasDefaultValue(0);
        builder.Property(e => e.VehicleClass).HasColumnName("vehicle_class").HasMaxLength(40);
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("AVAILABLE");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        builder.Property(e => e.Version).HasColumnName("version").HasDefaultValue(1).IsConcurrencyToken();

        builder.HasIndex(e => new { e.Status, e.StartsAt, e.EndsAt })
            .HasDatabaseName("ix_collection_slots_availability");
    }
}
