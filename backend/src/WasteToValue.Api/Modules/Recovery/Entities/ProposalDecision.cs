using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Entities;

public sealed class ProposalDecision
{
    public Guid Id { get; private set; }
    public Guid RecoveryProposalId { get; private set; }
    public int ProposalRevision { get; private set; }
    public Guid DecidedBy { get; private set; }
    public string DecisionType { get; private set; } = "OWNER_DECISION";
    public ProposalDecisionKind Decision { get; private set; }
    public string? Comment { get; private set; }
    public string IdempotencyKey { get; private set; } = "";
    public DateTimeOffset DecidedAt { get; private set; }
    private ProposalDecision() { }

    public static ProposalDecision Create(RecoveryProposal proposal, Guid ownerId, RecoveryActor actor,
        ProposalDecisionKind decision, string? comment, string idempotencyKey, DateTimeOffset now)
    {
        if (!actor.IsHuman || actor.UserId != ownerId)
            throw new RecoveryException(403, "human_owner_required", "Only the human item owner may decide a proposal.");
        RecoveryRequestValidator.Defined(decision);
        if (decision == ProposalDecisionKind.RevisionRequested)
            RecoveryRequestValidator.Text(comment, "Comment", 2000);
        RecoveryRequestValidator.Text(idempotencyKey, "Idempotency-Key", 100);
        if (comment?.Length > 2000) throw RecoveryException.Invalid("Comment must not exceed 2000 characters.");
        return new ProposalDecision { RecoveryProposalId = proposal.Id, ProposalRevision = proposal.Revision,
            DecidedBy = actor.UserId, Decision = decision, Comment = comment,
            IdempotencyKey = idempotencyKey, DecidedAt = now };
    }
}
