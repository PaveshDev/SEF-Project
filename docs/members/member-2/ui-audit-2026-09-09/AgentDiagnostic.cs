using System.Reflection;
using System.Text.Json;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Recovery.Agent;
using WasteToValue.Recovery.Tests.Contracts;
using WasteToValue.Recovery.Tests.Services;

// Diagnostic only: reuse the existing test fixtures and a prompt, valid fake
// reasoning response. No database, network, credentials, or application changes.
var type = typeof(RecoveryPlannerAgentTests);
var flags = BindingFlags.Static | BindingFlags.NonPublic;
foreach (var scenario in new[] { "pickup_cost", "over_budget_pickup", "wrong_item_assessment" })
{
    var input = (RecoveryPlannerAgentInput)type.GetMethod("Input", flags)!.Invoke(null, new object[] { RecoveryRoute.Resell, "Audit the existing agent" })!;
    var toolset = type.GetMethod("MatchedTools", flags)!.Invoke(null, null)!;
    var returnedAssessment = scenario == "wrong_item_assessment"
        ? input.Assessment with { ItemId = Guid.NewGuid(), AssessmentId = Guid.NewGuid() }
        : input.Assessment;
    toolset.GetType().GetProperty("AssessmentResult")!.SetValue(toolset, PlannerToolResult<AssessmentSummary>.Success(returnedAssessment));
    if (scenario == "over_budget_pickup")
    {
        Func<PickupPlanningRequest, PlannerToolResult<IReadOnlyList<PickupPlanSummary>>> factory = request =>
        {
            var pickup = (PickupPlanSummary)type.GetMethod("Pickup", flags)!.Invoke(null, null)!;
            return PlannerToolResult<IReadOnlyList<PickupPlanSummary>>.Success(new[] { pickup with { MatchId = request.MatchId, EstimatedCost = 75m } });
        };
        toolset.GetType().GetProperty("PickupFactory")!.SetValue(toolset, factory);
    }
    var provider = new ImmediateReasoning();
    var agent = (RecoveryPlannerAgent)type.GetMethod("CreateAgent", flags)!.Invoke(null, new object?[] { toolset, null, null, null, null, provider })!;
    var result = await agent.RunAsync(input, 1, Guid.NewGuid(), default);
    var alternative = result.Alternatives.FirstOrDefault();
    Console.WriteLine(JsonSerializer.Serialize(new {
        scenario, state = result.State.ToString(), reasoned = result.Recommendation is not null,
        provider.Calls, error = result.Error?.Code, input.MaximumPickupCost,
        sameItem = returnedAssessment.ItemId == input.Assessment.ItemId,
        quotedPickupCost = alternative?.Pickup?.EstimatedCost,
        valuedPickupCost = alternative?.Valuation.EstimatedPickupCost,
        netValue = alternative?.Valuation.EstimatedNetValue
    }));
}

sealed class ImmediateReasoning : IRecoveryReasoningProvider
{
    public int Calls { get; private set; }
    public Task<GatewayResult<RecoveryReasoningResponse>> ReasonAsync(RecoveryReasoningRequest request, CancellationToken ct)
    {
        Calls++;
        var response = (RecoveryReasoningResponse)typeof(GeminiRecoveryReasoningProviderTests)
            .GetMethod("Recommendation", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, new object[] { request })!;
        return Task.FromResult(GatewayResult<RecoveryReasoningResponse>.Success(response));
    }
}
