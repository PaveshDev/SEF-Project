using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Collections.Entities;

namespace WasteToValue.Api.Modules.Collections.Configurations;

public sealed class PickupEventConfiguration : IEntityTypeConfiguration<PickupEvent>
{
    public void Configure(EntityTypeBuilder<PickupEvent> builder)
    {
        builder.ToTable("pickup_events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PickupRequestId).HasColumnName("pickup_request_id");
        builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(30).IsRequired();
        builder.Property(e => e.ActorId).HasColumnName("actor_id");
        builder.Property(e => e.EventAt).HasColumnName("event_at").HasDefaultValueSql("now()");
        builder.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100).IsRequired();

        builder.HasIndex(e => e.IdempotencyKey).IsUnique();
        builder.HasIndex(e => new { e.PickupRequestId, e.EventAt })
            .HasDatabaseName("ix_pickup_events_request_time");
    }
}
