using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Recovery.Tests.Services;

public class RecoveryReadinessRegressionTests
{
    [Fact]
    public async Task Editing_a_pending_case_creates_revision_and_stales_proposal()
    {
        var f = new Fixture();
        var proposal = await f.SubmitAsync();
        var item = await f.Planning.GetAsync(proposal.CaseId, default);
        var updated = await f.Planning.UpdateInputsAsync(item.Id,
            new(item.Version, f.Inputs with { Objective = "Updated recovery intent", PreferredRoutes = new[] { RecoveryRoute.Reuse, RecoveryRoute.Donate } }), "edit", default);
        Assert.Equal(RecoveryCaseStatus.RevisionRequested, updated.Status);
        Assert.Equal(item.Revision + 1, updated.Revision);
        Assert.Equal(2, updated.PreferredRoutes.Count);
        Assert.Equal(RecoveryProposalStatus.Stale, (await f.Proposals.GetAsync(proposal.Id, default)).Status);
        Assert.All(f.Repo.Options, option => Assert.Equal(RecoveryOptionStatus.Stale, option.Status));
    }

    [Fact]
    public async Task Reopening_returns_owned_proposal_history_and_replan_invalidates_old_revision()
    {
        var f = new Fixture();
        var first = await f.SubmitAsync();
        Assert.Equal(first.Id, Assert.Single(await f.Proposals.ListAsync(first.CaseId, default)).Id);
        var current = await f.Planning.GetAsync(first.CaseId, default);
        var next = await f.Planning.ReplanAsync(current.Id, new(current.Version), "replan", default);
        Assert.Equal(current.Revision + 1, next.Case.Revision);
        Assert.Equal(RecoveryProposalStatus.Stale, (await f.Proposals.GetAsync(first.Id, default)).Status);
        var option = Assert.Single(next.Options);
        var second = await f.Proposals.SubmitAsync(current.Id, new(next.Case.Version, option.Id, option.Version, null, null, f.Clock.Now.AddHours(1), "Updated proposal"), "second", default);
        Assert.Equal(new[] { second.Id, first.Id }, (await f.Proposals.ListAsync(current.Id, default)).Select(p => p.Id));
        f.Actors.Actor = f.Actors.Actor with { UserId = Guid.NewGuid() };
        await Assert.ThrowsAsync<RecoveryException>(() => f.Proposals.ListAsync(current.Id, default));
    }

    [Fact]
    public async Task Reference_filter_runs_before_pagination()
    {
        var f = new Fixture();
        for (var i = 0; i < 30; i++)
        {
            var reference = ValueReference.Create(new(Guid.NewGuid(), ConditionGrade.Good, RecoveryRoute.Reuse,
                5, 10, "LKR", "Unrelated category", null, f.Clock.Now), f.Actors.Actor.UserId, f.Clock.Now);
            reference.Verify(1, f.Clock.Now); f.Repo.Add(reference);
        }
        var (_, option) = await f.PlanReuseAsync();
        Assert.Equal(RecoveryOptionStatus.Validated, option.Status);
        Assert.Equal(100, option.Estimate!.NetValue);
    }

    [Fact]
    public async Task Stale_evidence_is_unavailable_during_planning_and_approval()
    {
        var f = new Fixture();
        var proposal = await f.SubmitAsync();
        f.Clock.Now = f.Clock.Now.AddDays(31);
        var refreshed = await f.Proposals.RefreshAsync(proposal.Id, "refresh", default);
        Assert.Contains(refreshed.Status, new[] { RecoveryProposalStatus.Expired, RecoveryProposalStatus.Stale });
    }

    [Fact]
    public async Task Cancelled_case_with_options_cannot_be_deleted()
    {
        var f = new Fixture();
        var (item, _) = await f.PlanReuseAsync();
        await f.Planning.CancelAsync(item.Id, new(item.Version), "cancel", default);
        var error = await Assert.ThrowsAsync<RecoveryException>(() => f.Planning.DeleteAsync(item.Id, "delete", default));
        Assert.Equal("case_history_exists", error.Code);
        Assert.Single(f.Repo.Cases);
    }

    [Fact]
    public async Task Revision_request_requires_a_comment()
    {
        var f = new Fixture();
        var proposal = await f.SubmitAsync();
        await Assert.ThrowsAsync<RecoveryException>(() => f.Proposals.DecideAsync(proposal.Id,
            new(proposal.Version, proposal.Revision, ProposalDecisionKind.RevisionRequested, "  "), "revise", default));
        Assert.Empty(f.Repo.Decisions);
    }
}
