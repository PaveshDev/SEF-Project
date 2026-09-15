using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class RecoveryReasoningValidator
{
    public GatewayResult<RecoveryReasoningResponse> Validate(
        RecoveryReasoningResponse? response, RecoveryReasoningRequest request)
    {
        if (response is null || !Enum.IsDefined(response.RecommendedRoute) ||
            !request.EligibleOptions.Any(x => x.Route == response.RecommendedRoute))
            return Invalid();
        var ids = response.RankedOptionIds;
        if (ids is null || ids.Count == 0 || ids.Count != request.EligibleOptions.Count ||
            ids.Distinct().Count() != ids.Count ||
            ids.Any(id => id == Guid.Empty || !request.EligibleOptions.Any(x => x.OptionId == id)) ||
            request.EligibleOptions.Single(x => x.OptionId == ids[0]).Route != response.RecommendedRoute)
            return Invalid();
        if (!double.IsFinite(response.Confidence) || response.Confidence is < 0 or > 1 ||
            !Text(response.ReasonSummary) || !Texts(response.Benefits) || !Texts(response.Risks) ||
            !Texts(response.Assumptions) || !Texts(response.EvidenceReferences) || !Texts(response.MissingInformation))
            return Invalid();
        var evidence = request.AssessmentSummary.EvidenceReferences
            .Concat(request.EligibleOptions.SelectMany(x => x.EvidenceReferences)).ToHashSet(StringComparer.Ordinal);
        if (response.EvidenceReferences.Any(x => !evidence.Contains(x))) return Invalid();

        // Model confidence never relaxes the mandatory approval boundary.
        return GatewayResult<RecoveryReasoningResponse>.Success(response with { RequiresHumanReview = true });
    }

    private static bool Text(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 2000;
    private static bool Texts(IReadOnlyList<string>? values) => values is not null && values.Count <= 32 && values.All(Text);
    private static GatewayResult<RecoveryReasoningResponse> Invalid() =>
        GatewayResult<RecoveryReasoningResponse>.Failure(GatewayOutcome.Invalid,
            "reasoning_invalid_output", "The reasoning recommendation did not pass validation.");
}
