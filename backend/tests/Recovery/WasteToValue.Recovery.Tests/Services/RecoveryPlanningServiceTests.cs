using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Services;

public class RecoveryPlanningServiceTests
{
    [Fact]
    public async Task Planning_Creates_Valid_Reuse_Plans()
    {
        var f = new Fixture();
        var (value, option) = await f.PlanReuseAsync();

        Assert.Equal(RecoveryCaseStatus.Planning, value.Status);
        Assert.Equal(RecoveryOptionStatus.Validated, option.Status);
        Assert.Null(option.Integration.Match);
        Assert.Null(option.Integration.Pickup);
    }

    [Fact]
    public async Task Planning_Prevents_Second_Active_Case()
    {
        var f = new Fixture();
        await f.Planning.CreateAsync(new(f.Assessments.Value.ItemId, f.Inputs), "first-case", default);

        var exception = await Assert.ThrowsAsync<RecoveryException>(async () =>
            await f.Planning.CreateAsync(new(f.Assessments.Value.ItemId, f.Inputs), "second-case", default));

        Assert.Equal("active_case_exists", exception.Code);
    }

    [Fact]
    public async Task Planning_Rejects_Stale_Assessment_On_Submission()
    {
        var f = new Fixture();
        var (value, option) = await f.PlanReuseAsync();
        f.Assessments.Value = f.Assessments.Value with { AssessmentVersion = 2 };

        var exception = await Assert.ThrowsAsync<RecoveryException>(async () =>
            await f.Proposals.SubmitAsync(value.Id,
                new(value.Version, option.Id, option.Version, null, null, f.Clock.Now.AddHours(1), "Reason"), "stale-submit", default));

        Assert.Equal("stale_assessment", exception.Code);
    }

    [Fact]
    public async Task Planning_Returns_Waiting_State_For_Unknown_Value()
    {
        var empty = new Fixture();
        var created = await empty.Planning.CreateAsync(new(empty.Assessments.Value.ItemId, empty.Inputs), "create", default);
        var waiting = await empty.Planning.PlanAsync(created.Id, new(created.Version), "plan", default);

        Assert.Equal(RecoveryCaseStatus.AwaitingInputs, waiting.Case.Status);
        Assert.Null(waiting.Options.Single().Estimate);
        Assert.Single(waiting.UnavailableInputs);
    }

    [Fact]
    public async Task Planning_Returns_NotFound_For_NonExistent_Case()
    {
        var empty = new Fixture();
        var created = await empty.Planning.CreateAsync(new(empty.Assessments.Value.ItemId, empty.Inputs), "create", default);

        var exception = await Assert.ThrowsAsync<RecoveryException>(async () =>
            await empty.Planning.GetAsync(Guid.NewGuid(), default));

        Assert.Equal("case_not_found", exception.Code);
    }

    [Fact]
    public async Task Planning_Propagates_Cancellation_Token()
    {
        var empty = new Fixture();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await empty.Planning.ListAsync(new RecoveryCaseQuery(null, null), cancellation.Token));

        Assert.True(exception is OperationCanceledException);
    }
}