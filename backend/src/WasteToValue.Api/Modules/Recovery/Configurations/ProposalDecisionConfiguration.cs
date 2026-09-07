using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;

namespace WasteToValue.Api.Modules.Recovery.Configurations;

public sealed class ProposalDecisionConfiguration : IEntityTypeConfiguration<ProposalDecision>
{
    public void Configure(EntityTypeBuilder<ProposalDecision> builder)
    {
        RecoveryMapping.Common(builder, "proposal_decisions", mutable: false);
        RecoveryMapping.Enum<ProposalDecision, ProposalDecisionKind>(builder, nameof(ProposalDecision.Decision), "decision", 25);
        builder.Property(x => x.DecisionType).HasMaxLength(30);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100);
        builder.HasOne<RecoveryProposal>().WithMany()
            .HasForeignKey(x => new { x.RecoveryProposalId, x.ProposalRevision })
            .HasPrincipalKey(x => new { x.Id, x.Revision }).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.DecidedBy, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => new { x.RecoveryProposalId, x.ProposalRevision, x.DecidedBy, x.DecisionType }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("ck_proposal_decisions_type", "proposal_revision > 0 AND decision_type = 'OWNER_DECISION'"));
    }
}
