using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Collections.Entities;

namespace WasteToValue.Api.Modules.Collections.Configurations;

public sealed class PickupRequestConfiguration : IEntityTypeConfiguration<PickupRequest>
{
    public void Configure(EntityTypeBuilder<PickupRequest> builder)
    {
        builder.ToTable("pickup_requests");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.RecoveryProposalId).HasColumnName("recovery_proposal_id");
        builder.Property(e => e.CollectionSlotId).HasColumnName("collection_slot_id");
        builder.Property(e => e.CollectorId).HasColumnName("collector_id");
        builder.Property(e => e.OwnerId).HasColumnName("owner_id");
        builder.Property(e => e.PickupAddressEncrypted).HasColumnName("pickup_address_encrypted").IsRequired();
        builder.Property(e => e.ScheduledStart).HasColumnName("scheduled_start");
        builder.Property(e => e.ScheduledEnd).HasColumnName("scheduled_end");
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("CONFIRMED");
        builder.Property(e => e.VerificationCode).HasColumnName("verification_code").HasMaxLength(50);
        builder.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        builder.Property(e => e.Version).HasColumnName("version").HasDefaultValue(1).IsConcurrencyToken();

        builder.HasIndex(e => e.RecoveryProposalId).IsUnique();
        builder.HasIndex(e => e.IdempotencyKey).IsUnique();
        builder.HasIndex(e => new { e.OwnerId, e.Status })
            .HasDatabaseName("ix_pickup_requests_owner_status");

        builder.HasOne(e => e.CollectionSlot)
            .WithMany()
            .HasForeignKey(e => e.CollectionSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Events)
            .WithOne(e => e.PickupRequest)
            .HasForeignKey(e => e.PickupRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.HandoverProofs)
            .WithOne(e => e.PickupRequest)
            .HasForeignKey(e => e.PickupRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
