using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Services;

public class ProposalDecisionServiceTests
{
    [Fact]
    public async Task Owner_Approval_Persists_One_Decision()
    {
        var f = new Fixture();
        var proposal = await f.SubmitAsync();
        var approved = await f.Proposals.DecideAsync(proposal.Id,
            new(proposal.Version, proposal.Revision, ProposalDecisionKind.Approved, null), "approve", default);

        Assert.Equal(RecoveryProposalStatus.Approved, approved.Status);
        Assert.Single(f.Repo.Decisions);
        Assert.Equal(RecoveryCaseStatus.Approved, f.Repo.Cases.Single().Status);
    }

    [Theory]
    [InlineData(ProposalDecisionKind.Rejected, RecoveryProposalStatus.Rejected, RecoveryCaseStatus.Rejected)]
    [InlineData(ProposalDecisionKind.RevisionRequested, RecoveryProposalStatus.RevisionRequested, RecoveryCaseStatus.RevisionRequested)]
    public async Task Owner_decision_transitions_are_persisted(ProposalDecisionKind decision,
        RecoveryProposalStatus proposalStatus, RecoveryCaseStatus caseStatus)
    {
        var f = new Fixture();
        var proposal = await f.SubmitAsync();

        var result = await f.Proposals.DecideAsync(proposal.Id,
            new(proposal.Version, proposal.Revision, decision, "Owner decision"), "decision-key", default);

        Assert.Equal(proposalStatus, result.Status);
        Assert.Equal(caseStatus, f.Repo.Cases.Single().Status);
        Assert.Single(f.Repo.Decisions);
    }

    [Fact]
    public async Task Duplicate_decision_key_replays_without_another_decision()
    {
        var f = new Fixture();
        var proposal = await f.SubmitAsync();
        var request = new ProposalDecisionRequest(proposal.Version, proposal.Revision, ProposalDecisionKind.Approved, null);

        var first = await f.Proposals.DecideAsync(proposal.Id, request, "decision-key", default);
        var replay = await f.Proposals.DecideAsync(proposal.Id, request, "decision-key", default);

        Assert.Equal(first, replay);
        Assert.Single(f.Repo.Decisions);
    }

    [Fact]
    public async Task Agent_Cannot_Approve_Recommendation()
    {
        var agent = new Fixture();
        var agentProposal = await agent.SubmitAsync();
        agent.Actors.Actor = agent.Actors.Actor with { IsHuman = false };

        await Assert.ThrowsAsync<RecoveryException>(async () =>
            await agent.Proposals.DecideAsync(agentProposal.Id,
                new(agentProposal.Version, agentProposal.Revision, ProposalDecisionKind.Approved, null), "agent-decision", default));

        Assert.Equal("human_required", ((RecoveryException)await Record.ExceptionAsync(async () =>
            await agent.Proposals.DecideAsync(agentProposal.Id,
                new(agentProposal.Version, agentProposal.Revision, ProposalDecisionKind.Approved, null), "agent-decision", default))).Code);

        Assert.Empty(agent.Repo.Decisions);
    }

    [Fact]
    public async Task Stale_Proposal_Cannot_Be_Approved()
    {
        var stale = new Fixture();
        var staleProposal = await stale.SubmitAsync();

        await Assert.ThrowsAsync<RecoveryException>(async () =>
            await stale.Proposals.DecideAsync(staleProposal.Id,
                new(staleProposal.Version, staleProposal.Revision + 1, ProposalDecisionKind.Approved, null), "wrong-revision", default));

        Assert.Equal("stale_proposal", ((RecoveryException)await Record.ExceptionAsync(async () =>
            await stale.Proposals.DecideAsync(staleProposal.Id,
                new(staleProposal.Version, staleProposal.Revision + 1, ProposalDecisionKind.Approved, null), "wrong-revision", default))).Code);

        stale.Assessments.Value = stale.Assessments.Value with { AssessmentVersion = 2 };
        await Assert.ThrowsAsync<RecoveryException>(async () =>
            await stale.Proposals.DecideAsync(staleProposal.Id,
                new(staleProposal.Version, staleProposal.Revision, ProposalDecisionKind.Approved, null), "stale-input", default));

        Assert.Equal("stale_assessment", ((RecoveryException)await Record.ExceptionAsync(async () =>
            await stale.Proposals.DecideAsync(staleProposal.Id,
                new(staleProposal.Version, staleProposal.Revision, ProposalDecisionKind.Approved, null), "stale-input", default))).Code);

        var refreshed = await stale.Proposals.RefreshAsync(staleProposal.Id, "refresh", default);
        Assert.Equal(RecoveryProposalStatus.Stale, refreshed.Status);
        Assert.Equal(RecoveryCaseStatus.RevisionRequested, stale.Repo.Cases.Single().Status);
    }

    [Fact]
    public async Task Expired_Proposal_Cannot_Be_Approved()
    {
        var expired = new Fixture();
        var expiredProposal = await expired.SubmitAsync();
        expired.Clock.Now = expiredProposal.ExpiresAt;

        await Assert.ThrowsAsync<RecoveryException>(async () =>
            await expired.Proposals.DecideAsync(expiredProposal.Id,
                new(expiredProposal.Version, expiredProposal.Revision, ProposalDecisionKind.Approved, null), "expired", default));

        Assert.Equal("proposal_expired", ((RecoveryException)await Record.ExceptionAsync(async () =>
            await expired.Proposals.DecideAsync(expiredProposal.Id,
                new(expiredProposal.Version, expiredProposal.Revision, ProposalDecisionKind.Approved, null), "expired", default))).Code);

        var refreshed = await expired.Proposals.RefreshAsync(expiredProposal.Id, "refresh-expired", default);
        Assert.Equal(RecoveryProposalStatus.Expired, refreshed.Status);
    }
}