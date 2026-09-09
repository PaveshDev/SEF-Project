namespace WasteToValue.Api.Modules.Collections.DTOs;

/// <summary>
/// Structured output from the Collection Agent. Contains the recommended
/// collection arrangement with feasibility analysis and a fallback option.
/// </summary>
public sealed record CollectionProposalDto(
    Guid ProposalId,
    Guid PickupRequestId,
    RecommendedOptionDto Recommended,
    RecommendedOptionDto? Fallback,
    string FeasibilityStatus,
    string ReasonForRecommendation,
    IReadOnlyList<ConstraintCheckDto> ConstraintChecks,
    DateTimeOffset CreatedAt);

public sealed record RecommendedOptionDto(
    DateTimeOffset ProposedStart,
    DateTimeOffset ProposedEnd,
    string CollectionMethod,
    string RequiredVehicleType,
    string? EstimatedTravelDistance,
    string? EstimatedTravelTime,
    decimal? EstimatedCost,
    string? Currency,
    IReadOnlyList<string> HandlingRequirements);

public sealed record ConstraintCheckDto(
    string Constraint,
    bool Passed,
    string Detail);
