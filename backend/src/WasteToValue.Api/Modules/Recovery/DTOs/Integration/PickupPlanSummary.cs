namespace WasteToValue.Api.Modules.Recovery.DTOs.Integration;

public sealed record PickupPlanSummary(Guid PickupPlanId, Guid MatchId, Guid? CollectionSlotId,
    int Version, DateTimeOffset ProposedStart, DateTimeOffset ProposedEnd,
    decimal EstimatedCost, string Currency, PickupFeasibility Feasibility,
    string FreshnessToken, DateTimeOffset CheckedAt, IReadOnlyList<string> Reasons);
