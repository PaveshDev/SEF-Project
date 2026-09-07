namespace WasteToValue.Api.Modules.Recovery.DTOs.Integration;

public sealed record PickupPlanningRequest(Guid OperationId, Guid RecoveryCaseId, int CaseRevision,
    Guid MatchId, int MatchVersion, string MatchFreshnessToken, string ServiceArea,
    RecoveryRoute Route, IReadOnlyList<string> HandlingRequirements, DateTimeOffset? Deadline,
    decimal? MaximumPickupCost, string Currency);
