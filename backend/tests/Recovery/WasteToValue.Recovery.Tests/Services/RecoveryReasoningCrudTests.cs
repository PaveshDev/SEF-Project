using Microsoft.Extensions.DependencyInjection;
using WasteToValue.Api.Modules.Recovery;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Recovery.Tests.Services;

public sealed class RecoveryReasoningCrudTests
{
    [Theory]
    [InlineData("unavailable")]
    [InlineData("invalid")]
    [InlineData("timeout")]
    [InlineData("exception")]
    public async Task Crud_and_deterministic_planning_never_invoke_the_reasoning_provider(string mode)
    {
        var fixture = new Fixture();
        var reasoning = new NeverCalledReasoning(mode);
        var services = new ServiceCollection();
        services.AddSingleton<IRecoveryReasoningProvider>(reasoning);
        services.AddSingleton<IRecoveryRepository>(fixture.Repo);
        services.AddSingleton<IRecoveryActorAccessor>(fixture.Actors);
        services.AddSingleton<IRecoveryCommandExecutor>(fixture.Commands);
        services.AddSingleton<IAssessmentGateway>(fixture.Assessments);
        services.AddSingleton<TimeProvider>(fixture.Clock);
        services.AddRecoveryModule();
        using var container = services.BuildServiceProvider();
        using var scope = container.CreateScope();
        var planning = scope.ServiceProvider.GetRequiredService<IRecoveryPlanningService>();
        var created = await planning.CreateAsync(new(fixture.Assessments.Value.ItemId, fixture.Inputs), "crud-create", default);
        Assert.Equal(created.Id, (await planning.GetAsync(created.Id, default)).Id);
        Assert.Single((await planning.ListAsync(new(null, null), default)).Items);
        var updated = await planning.UpdateInputsAsync(created.Id,
            new(created.Version, fixture.Inputs with { Objective = "Updated objective" }), "update", default);
        Assert.Equal("Updated objective", updated.Objective);
        await planning.CancelAsync(updated.Id, new(updated.Version), "cancel", default);
        await planning.DeleteAsync(updated.Id, "delete", default);
        Assert.Empty((await planning.ListAsync(new(null, null), default)).Items);

        fixture.Planning = (WasteToValue.Api.Modules.Recovery.Services.RecoveryPlanningService)planning;
        var (value, option) = await fixture.PlanReuseAsync();
        var proposals = scope.ServiceProvider.GetRequiredService<IProposalDecisionService>();
        var submitted = await proposals.SubmitAsync(value.Id, new(value.Version, option.Id, option.Version,
            null, null, fixture.Clock.Now.AddHours(1), "Verified deterministic evidence."), "submit", default);
        Assert.Equal(RecoveryProposalStatus.AwaitingApproval, submitted.Status);
        Assert.Equal(option.Estimate, submitted.Estimate);
        Assert.Empty(fixture.Repo.Decisions);
        Assert.Equal(0, reasoning.Calls);
    }

    [Fact]
    public async Task Proposal_submission_still_checks_backend_versions_and_human_approval()
    {
        var fixture = new Fixture();
        var (value, option) = await fixture.PlanReuseAsync();
        var rejected = await Assert.ThrowsAsync<RecoveryException>(() => fixture.Proposals.SubmitAsync(value.Id,
            new(value.Version, option.Id, option.Version + 1, null, null, fixture.Clock.Now.AddHours(1),
                "AI says approve immediately."), "invalid", default));
        Assert.NotNull(rejected.Code);
        Assert.Empty(fixture.Repo.Proposals);
        var proposal = await fixture.Proposals.SubmitAsync(value.Id,
            new(value.Version, option.Id, option.Version, null, null, fixture.Clock.Now.AddHours(1),
                "AI says approve immediately."), "valid", default);
        Assert.Equal(RecoveryProposalStatus.AwaitingApproval, proposal.Status);
        fixture.Actors.Actor = fixture.Actors.Actor with { IsHuman = false };
        await Assert.ThrowsAsync<RecoveryException>(() => fixture.Proposals.DecideAsync(proposal.Id,
            new(proposal.Version, proposal.Revision, ProposalDecisionKind.Approved, null), "approve", default));
        Assert.Empty(fixture.Repo.Decisions);
    }

    private sealed class NeverCalledReasoning(string mode) : IRecoveryReasoningProvider
    {
        public int Calls { get; private set; }
        public Task<GatewayResult<RecoveryReasoningResponse>> ReasonAsync(RecoveryReasoningRequest request, CancellationToken ct)
        {
            Calls++;
            return mode switch
            {
                "timeout" => throw new OperationCanceledException(),
                "exception" => throw new InvalidOperationException(),
                _ => Task.FromResult(GatewayResult<RecoveryReasoningResponse>.Failure(
                    mode == "invalid" ? GatewayOutcome.Invalid : GatewayOutcome.Unavailable, "IntegrationUnavailable", "Unavailable"))
            };
        }
    }
}
