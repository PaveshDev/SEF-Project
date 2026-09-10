using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Recovery.Agent;

namespace WasteToValue.Recovery.Tests.Contracts;

public sealed partial class RecoveryPlannerAgentTests
{
    [Theory]
    [InlineData(RecoveryRoute.Resell, 80, 0)]
    [InlineData(RecoveryRoute.Donate, 0, 20)]
    public async Task Actual_pickup_cost_is_in_final_estimate(RecoveryRoute route, decimal net, decimal shortfall)
    {
        var tools = MatchedTools();
        var result = await CreateAgent(tools).RunAsync(Input(route), 1, Guid.NewGuid(), default);
        Assert.Equal(RecoveryPlannerWorkflowState.AwaitingApproval, result.State);
        var estimate = Assert.Single(result.Alternatives).Valuation.ToEstimate();
        Assert.Equal(20m, estimate.PickupCost);
        Assert.Equal(net, estimate.NetValue);
        Assert.Equal(shortfall, estimate.Shortfall);
    }

    [Theory]
    [InlineData("budget")]
    [InlineData("currency")]
    [InlineData("link")]
    [InlineData("version")]
    [InlineData("token")]
    [InlineData("schedule")]
    [InlineData("deadline")]
    [InlineData("checked")]
    public async Task Invalid_pickup_cannot_become_a_proposal(string defect)
    {
        var tools = MatchedTools();
        tools.PickupFactory = request =>
        {
            var value = Pickup() with { MatchId = request.MatchId };
            value = defect switch
            {
                "budget" => value with { EstimatedCost = 75 },
                "currency" => value with { Currency = "USD" },
                "link" => value with { MatchId = Guid.NewGuid() },
                "version" => value with { Version = 0 },
                "token" => value with { FreshnessToken = "" },
                "schedule" => value with { ProposedEnd = value.ProposedStart },
                "deadline" => value with { ProposedEnd = DateTimeOffset.UtcNow.AddDays(2) },
                _ => value with { CheckedAt = DateTimeOffset.UtcNow.AddHours(1) }
            };
            return PlannerToolResult<IReadOnlyList<PickupPlanSummary>>.Success(new[] { value });
        };
        var result = await CreateAgent(tools).RunAsync(Input(RecoveryRoute.Resell), 1, Guid.NewGuid(), default);
        Assert.Equal(RecoveryPlannerWorkflowState.Failed, result.State);
        Assert.Null(tools.ProposalDraft);
        Assert.False(tools.ApprovalRequested);
    }

    [Theory]
    [InlineData("item")]
    [InlineData("owner")]
    [InlineData("assessment")]
    [InlineData("itemRevision")]
    [InlineData("assessmentVersion")]
    public async Task Assessment_identity_mismatch_stops_before_persistence(string defect)
    {
        var input = Input();
        var assessment = defect switch
        {
            "item" => input.Assessment with { ItemId = Guid.NewGuid() },
            "owner" => input.Assessment with { OwnerId = Guid.NewGuid() },
            "assessment" => input.Assessment with { AssessmentId = Guid.NewGuid() },
            "itemRevision" => input.Assessment with { ItemRevision = 2 },
            _ => input.Assessment with { AssessmentVersion = 2 }
        };
        var tools = new PlannerTools { AssessmentResult = PlannerToolResult<AssessmentSummary>.Success(assessment) };
        var result = await CreateAgent(tools).RunAsync(input, 1, Guid.NewGuid(), default);
        Assert.Equal(RecoveryPlannerWorkflowState.Failed, result.State);
        Assert.Equal(0, tools.SaveCalls);
    }

    [Fact]
    public async Task Repair_requires_a_real_quotation()
    {
        var tools = MatchedTools();
        var result = await CreateAgent(tools).RunAsync(Input(RecoveryRoute.RepairThenReuse), 1, Guid.NewGuid(), default);
        Assert.Equal(RecoveryPlannerWorkflowState.Failed, result.State);
        Assert.Contains(result.Summary.Warnings, text => text.Contains("quotation"));
        Assert.Equal(0, tools.SaveCalls);
    }

    [Fact]
    public async Task Retry_reuses_step_operation_identity()
    {
        var tools = MatchedTools();
        var ids = new List<Guid>();
        tools.MatchFactory = request =>
        {
            ids.Add(request.OperationId);
            return PlannerToolResult<IReadOnlyList<MatchSummary>>.Success(new[] { Match() with { RecoveryOptionId = request.RecoveryOptionId } });
        };
        var input = Input(RecoveryRoute.Resell);
        var run = Guid.NewGuid();
        await CreateAgent(tools).RunAsync(input, 1, run, default);
        await CreateAgent(tools).RunAsync(input, 1, run, default);
        Assert.Equal(2, ids.Count);
        Assert.Equal(ids[0], ids[1]);
    }
}
