using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;
using WasteToValue.Api.Modules.Recovery.Services;

namespace WasteToValue.Recovery.Tests.Services;

public sealed class RecoveryReasoningValidatorTests
{
    [Theory]
    [InlineData(0.1)]
    [InlineData(0.9)]
    public void All_confidence_levels_require_human_review(double confidence)
    {
        var request = GeminiRecoveryReasoningProviderTests.Request();
        var response = GeminiRecoveryReasoningProviderTests.Recommendation(request) with { Confidence = confidence };
        var result = new RecoveryReasoningValidator().Validate(response, request);
        Assert.Equal(GatewayOutcome.Success, result.Outcome);
        Assert.True(result.Value!.RequiresHumanReview);
    }

    [Fact]
    public void Unknown_duplicate_missing_or_mismatched_rankings_are_rejected()
    {
        var request = GeminiRecoveryReasoningProviderTests.Request();
        var valid = GeminiRecoveryReasoningProviderTests.Recommendation(request);
        var invalid = new[]
        {
            valid with { RankedOptionIds = new[] { Guid.NewGuid(), request.EligibleOptions[0].OptionId } },
            valid with { RankedOptionIds = new[] { request.EligibleOptions[0].OptionId, request.EligibleOptions[0].OptionId } },
            valid with { RankedOptionIds = new[] { request.EligibleOptions[0].OptionId } },
            valid with { RecommendedRoute = RecoveryRoute.Recycle },
            valid with { RecommendedRoute = RecoveryRoute.Reuse },
            valid with { Confidence = double.NaN },
            valid with { Confidence = double.PositiveInfinity },
            valid with { ReasonSummary = "" },
            valid with { Risks = new string[] { null! } }
        };
        Assert.All(invalid, response => Assert.Equal(GatewayOutcome.Invalid,
            new RecoveryReasoningValidator().Validate(response, request).Outcome));
    }

    [Fact]
    public void Response_has_only_the_permitted_nonfinancial_properties()
    {
        var names = typeof(RecoveryReasoningResponse).GetProperties().Select(x => x.Name).Order().ToArray();
        Assert.Equal(new[] { "RecommendedRoute", "RankedOptionIds", "ReasonSummary", "Benefits", "Risks",
            "Assumptions", "EvidenceReferences", "MissingInformation", "Confidence", "RequiresHumanReview" }.Order(), names);
    }
}
