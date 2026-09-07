namespace WasteToValue.Api.Modules.Recovery.DTOs;

public sealed record ValueEstimate(decimal ValueLow, decimal ValueHigh, decimal RepairCost,
    decimal PickupCost, decimal NetValue, decimal Shortfall, string Currency);
public sealed record ValueEvidence(Guid ReferenceId, int Version, DateTimeOffset ObservedAt,
    decimal ValueLow, decimal ValueHigh, string Currency, string SourceName);

public sealed record ValueEstimationInput(decimal EstimatedProceedsLow, decimal EstimatedProceedsHigh,
    decimal EstimatedRepairCost, decimal EstimatedPickupCost, string Currency, RecoveryRoute Route,
    string SourceName, DateTimeOffset SourceObservedAt, IReadOnlyList<string> SocialBenefits,
    IReadOnlyList<string> EnvironmentalBenefits, string? CostCurrency = null);

public sealed record ValueEstimationOptions(TimeSpan MaximumReferenceAge, string FormulaVersion = "recovery-valuation-v1");

public sealed record ValueEstimationResult(ValueEstimationInput Inputs, string FormulaVersion,
    decimal EstimatedProceeds, decimal EstimatedRepairCost, decimal EstimatedPickupCost,
    decimal EstimatedNetValue, string Currency, string SourceName, DateTimeOffset SourceObservedAt,
    bool IsReferenceStale, IReadOnlyList<string> Warnings,
    IReadOnlyList<string> SocialBenefits, IReadOnlyList<string> EnvironmentalBenefits);
