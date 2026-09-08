using System.Text.Json.Serialization;

namespace WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;

// Every field is required on the wire. Financial values and executable actions are absent.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecoveryReasoningResponse(
    [property: JsonRequired] RecoveryRoute RecommendedRoute,
    [property: JsonRequired] IReadOnlyList<Guid> RankedOptionIds,
    [property: JsonRequired] string ReasonSummary,
    [property: JsonRequired] IReadOnlyList<string> Benefits,
    [property: JsonRequired] IReadOnlyList<string> Risks,
    [property: JsonRequired] IReadOnlyList<string> Assumptions,
    [property: JsonRequired] IReadOnlyList<string> EvidenceReferences,
    [property: JsonRequired] IReadOnlyList<string> MissingInformation,
    [property: JsonRequired] double Confidence,
    [property: JsonRequired] bool RequiresHumanReview);
