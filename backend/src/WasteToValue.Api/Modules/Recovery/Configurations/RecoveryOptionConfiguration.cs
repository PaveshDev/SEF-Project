using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;

namespace WasteToValue.Api.Modules.Recovery.Configurations;

public sealed class RecoveryOptionConfiguration : IEntityTypeConfiguration<RecoveryOption>
{
    public void Configure(EntityTypeBuilder<RecoveryOption> builder)
    {
        RecoveryMapping.Common(builder, "recovery_options");
        RecoveryMapping.Enum<RecoveryOption, RecoveryRoute>(builder, nameof(RecoveryOption.Route), "route_type");
        RecoveryMapping.Enum<RecoveryOption, RecoveryOptionStatus>(builder, nameof(RecoveryOption.Status), "status", 25);
        RecoveryMapping.Currency(builder);
        RecoveryMapping.Money(builder, nameof(RecoveryOption.EstimatedValueLow), nameof(RecoveryOption.EstimatedValueHigh),
            nameof(RecoveryOption.EstimatedRepairCost), nameof(RecoveryOption.EstimatedPickupCost),
            nameof(RecoveryOption.EstimatedNetValue), nameof(RecoveryOption.EstimatedShortfall));
        RecoveryMapping.Json(builder, nameof(RecoveryOption.EvidenceJson), "evidence");
        RecoveryMapping.Json(builder, nameof(RecoveryOption.NonFinancialBenefitsJson), "non_financial_benefits");
        RecoveryMapping.Json(builder, nameof(RecoveryOption.IntegrationSnapshotJson), "integration_snapshot");
        builder.HasOne<RecoveryCase>().WithMany().HasForeignKey(x => x.RecoveryCaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.RecoveryCaseId, x.Status });
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_recovery_options_revisions", "case_revision > 0 AND assessment_version > 0");
            t.HasCheckConstraint("ck_recovery_options_bounds", "estimated_value_low <= estimated_value_high");
            t.HasCheckConstraint("ck_recovery_options_pickup", "NOT requires_pickup OR requires_partner");
            t.HasCheckConstraint("ck_recovery_options_complete", "status NOT IN ('VALIDATED','SELECTED') OR (estimated_value_low IS NOT NULL AND estimated_value_high IS NOT NULL AND estimated_repair_cost IS NOT NULL AND estimated_pickup_cost IS NOT NULL AND estimated_net_value IS NOT NULL AND estimated_shortfall IS NOT NULL)");
            t.HasCheckConstraint("ck_recovery_options_balance", "estimated_net_value = GREATEST(estimated_value_low - estimated_repair_cost - estimated_pickup_cost, 0) AND estimated_shortfall = GREATEST(estimated_repair_cost + estimated_pickup_cost - estimated_value_low, 0)");
        });
    }
}
