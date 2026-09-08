namespace WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;

// Excludes owner identity, raw assessment documents and partner contact details.
public sealed record RecoveryReasoningRequest(Guid RecoveryCaseId, string ItemCategory,
    RecoveryReasoningAssessment AssessmentSummary, ConditionGrade Condition,
    IReadOnlyList<RecoveryReasoningOption> EligibleOptions, RecoveryReasoningConstraints Constraints);

public sealed record RecoveryReasoningAssessment(FunctionalStatus Function,
    IReadOnlyList<string> EvidenceReferences);

public sealed record RecoveryReasoningOption(Guid OptionId, RecoveryRoute Route,
    RecoveryReasoningValuation DeterministicValuation, IReadOnlyList<string> EvidenceReferences,
    RecoveryReasoningMatch? Match, RecoveryReasoningPickup? Pickup);

public sealed record RecoveryReasoningValuation(decimal EstimatedValueLow, decimal EstimatedValueHigh,
    decimal RepairCost, decimal PickupCost, decimal NetValue, string Currency);

public sealed record RecoveryReasoningMatch(string Eligibility, string Response);
public sealed record RecoveryReasoningPickup(string Feasibility, decimal EstimatedCost, string Currency);
public sealed record RecoveryReasoningConstraints(string Objective, decimal? MaximumPickupCost,
    string Currency, DateTimeOffset? Deadline);
