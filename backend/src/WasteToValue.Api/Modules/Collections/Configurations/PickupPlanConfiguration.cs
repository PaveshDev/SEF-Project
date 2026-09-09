using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Collections.Entities;

namespace WasteToValue.Api.Modules.Collections.Configurations;

public sealed class PickupPlanConfiguration : IEntityTypeConfiguration<PickupPlan>
{
    public void Configure(EntityTypeBuilder<PickupPlan> builder)
    {
        builder.ToTable("pickup_plans");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.MatchId).HasColumnName("match_id");
        builder.Property(e => e.CollectionSlotId).HasColumnName("collection_slot_id");
        builder.Property(e => e.ProposedStart).HasColumnName("proposed_start");
        builder.Property(e => e.ProposedEnd).HasColumnName("proposed_end");
        builder.Property(e => e.EstimatedCost).HasColumnName("estimated_cost").HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("LKR");
        builder.Property(e => e.HandlingRequirements).HasColumnName("handling_requirements").HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(e => e.TravelEstimate).HasColumnName("travel_estimate").HasColumnType("jsonb");
        builder.Property(e => e.FeasibilityStatus).HasColumnName("feasibility_status").HasMaxLength(25).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        builder.Property(e => e.Version).HasColumnName("version").HasDefaultValue(1).IsConcurrencyToken();

        builder.HasOne(e => e.CollectionSlot)
            .WithMany()
            .HasForeignKey(e => e.CollectionSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
