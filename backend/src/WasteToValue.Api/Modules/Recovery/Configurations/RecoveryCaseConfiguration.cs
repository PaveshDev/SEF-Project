using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;

namespace WasteToValue.Api.Modules.Recovery.Configurations;

public sealed class RecoveryCaseConfiguration : IEntityTypeConfiguration<RecoveryCase>
{
    public void Configure(EntityTypeBuilder<RecoveryCase> builder)
    {
        RecoveryMapping.Common(builder, "recovery_cases");
        RecoveryMapping.Enum<RecoveryCase, RecoveryCaseStatus>(builder, nameof(RecoveryCase.Status), "status");
        RecoveryMapping.Json(builder, nameof(RecoveryCase.PreferredRoutesJson), "preferred_routes");
        RecoveryMapping.Currency(builder);
        RecoveryMapping.Money(builder, nameof(RecoveryCase.MaximumPickupCost));
        builder.Property(x => x.Objective).HasMaxLength(1000);
        builder.HasIndex(x => x.ItemId).IsUnique().HasDatabaseName("uq_recovery_cases_active_item")
            .HasFilter("status NOT IN ('REJECTED','COMPLETED','FAILED','CANCELLED')");
        builder.HasIndex(x => new { x.OwnerId, x.Status });
        builder.ToTable(t => t.HasCheckConstraint("ck_recovery_cases_revisions", "revision > 0 AND item_revision > 0 AND assessment_version > 0"));
        // External item/assessment/user FKs require the integrator's actual entity mappings.
    }
}
