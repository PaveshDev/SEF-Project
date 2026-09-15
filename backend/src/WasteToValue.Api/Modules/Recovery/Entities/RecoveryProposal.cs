using System.Text.Json;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Entities;

public sealed class RecoveryProposal
{
    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; } = 1;

    public void RequireVersion(int expected)
    {
        RecoveryRequestValidator.Version(expected);
        if (Version != expected) throw RecoveryException.Conflict("stale_version", "The resource changed. Reload before retrying.");
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version = checked(Version + 1);
    }

    public Guid RecoveryCaseId { get; private set; }
    public int CaseRevision { get; private set; }
    public Guid RecoveryOptionId { get; private set; }
    public int OptionVersion { get; private set; }
    public Guid? MatchId { get; private set; }
    public int? MatchVersion { get; private set; }
    public string? MatchFreshnessToken { get; private set; }
    public Guid? PickupPlanId { get; private set; }
    public int? PickupPlanVersion { get; private set; }
    public string? PickupFreshnessToken { get; private set; }
    public int Revision { get; private set; }
    public string InputSnapshot { get; private set; } = "{}";
    public string EstimateSnapshot { get; private set; } = "{}";
    public string Explanation { get; private set; } = "";
    public DateTimeOffset ExpiresAt { get; private set; }
    public RecoveryProposalStatus Status { get; private set; } = RecoveryProposalStatus.AwaitingApproval;
    public RecommendationOrigin RecommendationOrigin { get; private set; }
    public Guid? AgentRunId { get; private set; }
    private RecoveryProposal() { }

    public static RecoveryProposal Submit(RecoveryCase recoveryCase, RecoveryOption option, int revision,
        AssessmentSummary assessment, MatchSummary? match, PickupPlanSummary? pickup,
        string explanation, DateTimeOffset expiresAt, RecommendationOrigin origin, Guid? agentRunId, DateTimeOffset now)
    {
        RecoveryRequestValidator.Version(revision);
        RecoveryRequestValidator.Text(explanation, "Explanation", 4000);
        RecoveryRequestValidator.Defined(origin);
        if ((origin == RecommendationOrigin.Agent) != agentRunId.HasValue || agentRunId == Guid.Empty)
            throw RecoveryException.Invalid("Agent recommendations require an agent run ID; human recommendations must not provide one.");
        if (expiresAt <= now || (recoveryCase.Deadline is { } deadline && expiresAt > deadline))
            throw RecoveryException.Invalid("Proposal expiry must be in the future and no later than the case deadline.");
        if (recoveryCase.Status != RecoveryCaseStatus.Planning || option.RecoveryCaseId != recoveryCase.Id ||
            option.CaseRevision != recoveryCase.Revision || option.Status != RecoveryOptionStatus.Validated)
            throw RecoveryException.Conflict("invalid_option", "A current validated option from this case is required.");
        recoveryCase.RequireAssessment(assessment);
        RecoveryProposalValidator.Dependencies(recoveryCase, option, match, pickup, now);
        return new RecoveryProposal
        {
            RecoveryCaseId = recoveryCase.Id, CaseRevision = recoveryCase.Revision,
            RecoveryOptionId = option.Id, OptionVersion = option.Version, Revision = revision,
            MatchId = match?.MatchId, MatchVersion = match?.Version, MatchFreshnessToken = match?.FreshnessToken,
            PickupPlanId = pickup?.PickupPlanId, PickupPlanVersion = pickup?.Version, PickupFreshnessToken = pickup?.FreshnessToken,
            InputSnapshot = JsonSerializer.Serialize(new { assessment.AssessmentId, assessment.ItemRevision,
                assessment.AssessmentVersion, option.RequiresPartner, option.RequiresPickup, option.EvidenceJson }),
            EstimateSnapshot = JsonSerializer.Serialize(option.Estimate()), Explanation = explanation.Trim(),
            ExpiresAt = expiresAt.ToUniversalTime(), RecommendationOrigin = origin, AgentRunId = agentRunId, CreatedAt = now, UpdatedAt = now
        };
    }

    public void RequireDecidable(int expectedVersion, int revision, DateTimeOffset now)
    {
        RequireVersion(expectedVersion);
        if (revision != Revision) throw RecoveryException.Conflict("stale_proposal", "The decision targets another proposal revision.");
        if (now >= ExpiresAt || Status == RecoveryProposalStatus.Expired)
            throw RecoveryException.Conflict("proposal_expired", "The proposal has expired.");
        if (Status != RecoveryProposalStatus.AwaitingApproval)
            throw RecoveryException.Conflict("proposal_not_decidable", "The proposal is stale or already decided.");
    }

    public void Decide(ProposalDecisionKind decision, int expectedVersion, int revision, DateTimeOffset now)
    {
        RequireDecidable(expectedVersion, revision, now);
        RecoveryRequestValidator.Defined(decision);
        Status = decision switch
        {
            ProposalDecisionKind.Approved => RecoveryProposalStatus.Approved,
            ProposalDecisionKind.Rejected => RecoveryProposalStatus.Rejected,
            _ => RecoveryProposalStatus.RevisionRequested
        };
        Touch(now);
    }

    public void MarkStale(DateTimeOffset now)
    {
        if (Status != RecoveryProposalStatus.AwaitingApproval) throw RecoveryException.Conflict("invalid_transition", "Only a pending proposal can become stale.");
        Status = now >= ExpiresAt ? RecoveryProposalStatus.Expired : RecoveryProposalStatus.Stale;
        Touch(now);
    }

    public void Execute(DateTimeOffset now)
    {
        if (Status != RecoveryProposalStatus.Approved) throw RecoveryException.Conflict("invalid_transition", "Only an approved proposal can be fulfilled.");
        Status = RecoveryProposalStatus.Executed; Touch(now);
    }
}
