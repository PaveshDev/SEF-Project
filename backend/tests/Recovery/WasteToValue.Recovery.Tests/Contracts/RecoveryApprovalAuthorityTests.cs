using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using WasteToValue.Recovery.Agent;

namespace WasteToValue.Recovery.Tests.Contracts;

public sealed partial class RecoveryPlannerAgentTests
{
    [Fact]
    public async Task Resume_without_authoritative_approval_verification_is_unavailable()
    {
        var agent = new RecoveryPlannerAgent(new RecoveryPlannerToolset(new PlannerTools(), new ValueEstimationService()),
            workflows: new Fakes.InMemoryRecoveryWorkflowStore(), actors: new TestActor(), commands: new TestCommands());
        var result = await agent.ResumeAsync(Guid.NewGuid(), new(Guid.NewGuid(), 1, ProposalDecisionKind.Approved, null), default);
        Assert.Equal("approval_authorization_unavailable", result.Error!.Code);
    }

    [Fact]
    public async Task Resume_authority_requires_an_owned_recorded_decision()
    {
        var f = new Fixture();
        var proposal = await f.SubmitAsync();
        var state = StoredWorkflow(Guid.NewGuid(), proposal.Id, proposal.Revision, proposal.ExpiresAt) with { RecoveryCaseId = proposal.CaseId };
        var decision = new RecoveryApprovalDecision(proposal.Id, proposal.Revision, ProposalDecisionKind.Approved, null);
        var authorizer = new RecoveryWorkflowApprovalAuthorizer(f.Proposals);
        var missing = await Assert.ThrowsAsync<RecoveryException>(() => authorizer.AuthorizeAsync(state, decision, default));
        Assert.Equal("approval_not_recorded", missing.Code);
        await f.Proposals.DecideAsync(proposal.Id, new(proposal.Version, proposal.Revision, ProposalDecisionKind.Approved, null), "approve", default);
        await authorizer.AuthorizeAsync(state, decision, default);
        f.Actors.Actor = f.Actors.Actor with { UserId = Guid.NewGuid() };
        await Assert.ThrowsAsync<RecoveryException>(() => authorizer.AuthorizeAsync(state, decision, default));
    }
}
