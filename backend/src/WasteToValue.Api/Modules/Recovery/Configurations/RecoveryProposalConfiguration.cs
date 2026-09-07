using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;

namespace WasteToValue.Api.Modules.Recovery.Configurations;

public sealed class RecoveryProposalConfiguration : IEntityTypeConfiguration<RecoveryProposal>
{
    public void Configure(EntityTypeBuilder<RecoveryProposal> builder)
    {
        RecoveryMapping.Common(builder, "recovery_proposals");
        RecoveryMapping.Enum<RecoveryProposal, RecoveryProposalStatus>(builder, nameof(RecoveryProposal.Status), "status", 25);
        RecoveryMapping.Enum<RecoveryProposal, RecommendationOrigin>(builder, nameof(RecoveryProposal.RecommendationOrigin), "recommendation_origin");
        RecoveryMapping.Json(builder, nameof(RecoveryProposal.InputSnapshot), "input_snapshot");
        RecoveryMapping.Json(builder, nameof(RecoveryProposal.EstimateSnapshot), "estimate_snapshot");
        builder.Property(x => x.Explanation).HasMaxLength(4000);
        builder.Property(x => x.MatchFreshnessToken).HasMaxLength(500);
        builder.Property(x => x.PickupFreshnessToken).HasMaxLength(500);
        builder.HasOne<RecoveryCase>().WithMany().HasForeignKey(x => x.RecoveryCaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RecoveryOption>().WithMany().HasForeignKey(x => x.RecoveryOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.RecoveryCaseId, x.Revision }).IsUnique();
        builder.HasAlternateKey(x => new { x.Id, x.Revision });
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_recovery_proposals_revisions", "revision > 0 AND case_revision > 0 AND option_version > 0");
            t.HasCheckConstraint("ck_recovery_proposals_match", "(match_id IS NULL AND match_version IS NULL AND match_freshness_token IS NULL) OR (match_id IS NOT NULL AND match_version > 0 AND match_freshness_token IS NOT NULL)");
            t.HasCheckConstraint("ck_recovery_proposals_pickup", "(pickup_plan_id IS NULL AND pickup_plan_version IS NULL AND pickup_freshness_token IS NULL) OR (pickup_plan_id IS NOT NULL AND pickup_plan_version > 0 AND pickup_freshness_token IS NOT NULL AND match_id IS NOT NULL)");
            t.HasCheckConstraint("ck_recovery_proposals_origin", "(recommendation_origin = 'HUMAN' AND agent_run_id IS NULL) OR (recommendation_origin = 'AGENT' AND agent_run_id IS NOT NULL)");
            t.HasCheckConstraint("ck_recovery_proposals_expiry", "expires_at > created_at");
        });
    }
}
