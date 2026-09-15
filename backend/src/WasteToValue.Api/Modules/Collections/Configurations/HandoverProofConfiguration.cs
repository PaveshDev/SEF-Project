using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Collections.Entities;

namespace WasteToValue.Api.Modules.Collections.Configurations;

public sealed class HandoverProofConfiguration : IEntityTypeConfiguration<HandoverProof>
{
    public void Configure(EntityTypeBuilder<HandoverProof> builder)
    {
        builder.ToTable("handover_proofs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PickupRequestId).HasColumnName("pickup_request_id");
        builder.Property(e => e.PickupEventId).HasColumnName("pickup_event_id");
        builder.Property(e => e.ProofType).HasColumnName("proof_type").HasMaxLength(30).IsRequired();
        builder.Property(e => e.StorageKey).HasColumnName("storage_key").HasMaxLength(500);
        builder.Property(e => e.VerificationHash).HasColumnName("verification_hash").HasMaxLength(255);
        builder.Property(e => e.VerifiedBy).HasColumnName("verified_by");
        builder.Property(e => e.VerifiedAt).HasColumnName("verified_at");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        builder.HasOne(e => e.PickupEvent)
            .WithMany()
            .HasForeignKey(e => e.PickupEventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
