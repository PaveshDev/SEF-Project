using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Recovery.Controllers;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Contracts;

public sealed class RecoveryPhase2ApiTests
{
    [Fact]
    public async Task Case_listing_supports_search_filter_sort_and_pagination()
    {
        var fixture = new Fixture();
        await fixture.Planning.CreateAsync(new(fixture.Assessments.Value.ItemId,
            fixture.Inputs with { Objective = "Reuse laptop" }), "case-one", default);

        var result = await fixture.Planning.ListAsync(new RecoveryCaseQuery("laptop", RecoveryCaseStatus.Draft,
            "createdAt", "asc", 1, 1), default);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(1, result.PageSize);
    }

    [Fact]
    public async Task Unverified_value_reference_supports_update_then_delete()
    {
        var fixture = new Fixture();
        fixture.Actors.Actor = fixture.Actors.Actor with { CanManageValueReferences = true };
        var service = new ValueReferenceService(new(fixture.Actors, fixture.Repo), fixture.Repo, fixture.Commands, fixture.Clock);
        var request = new CreateValueReferenceRequest(Guid.NewGuid(), ConditionGrade.Good, RecoveryRoute.Reuse,
            100, 200, "LKR", "Initial source", null, fixture.Clock.Now);

        var created = await service.CreateAsync(request, "reference-create", default);
        var updated = await service.UpdateAsync(created.Id, new(created.Version, ConditionGrade.Fair, RecoveryRoute.Resell,
            80, 180, "LKR", "Updated source", null, fixture.Clock.Now), "reference-update", default);

        Assert.Equal(ConditionGrade.Fair, updated.Condition);
        Assert.Equal("Updated source", updated.SourceName);
        await service.DeleteAsync(updated.Id, "reference-delete", default);
        await Assert.ThrowsAsync<RecoveryException>(() => service.GetAsync(updated.Id, default));
    }

    [Fact]
    public async Task Draft_case_can_be_deleted_but_approved_case_cannot()
    {
        var fixture = new Fixture();
        var created = await fixture.Planning.CreateAsync(new(fixture.Assessments.Value.ItemId, fixture.Inputs), "delete-case", default);

        await fixture.Planning.DeleteAsync(created.Id, "delete-case-command", default);
        Assert.Empty(fixture.Repo.Cases);

        var second = new Fixture();
        var approved = await second.SubmitAsync();
        var decision = await second.Proposals.DecideAsync(approved.Id,
            new(approved.Version, approved.Revision, ProposalDecisionKind.Approved, null), "approve-case", default);

        var exception = await Assert.ThrowsAsync<RecoveryException>(() =>
            second.Planning.DeleteAsync(second.Repo.Cases.Single().Id, "delete-approved", default));
        Assert.Equal("case_not_deletable", exception.Code);
        Assert.Equal(RecoveryProposalStatus.Approved, decision.Status);
    }

    [Fact]
    public void Controllers_use_the_requested_resource_route_templates()
    {
        Assert.Equal("api/recovery-cases", typeof(RecoveryCasesController).GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>().Single().Template);
        Assert.Equal("api/recovery-proposals", typeof(RecoveryProposalsController).GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>().Single().Template);
        Assert.Equal("api/value-references", typeof(ValueReferencesController).GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>().Single().Template);
    }
}