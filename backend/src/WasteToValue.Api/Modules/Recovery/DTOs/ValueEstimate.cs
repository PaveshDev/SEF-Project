namespace WasteToValue.Api.Modules.Recovery.DTOs;

public sealed record ValueEstimate(decimal ValueLow, decimal ValueHigh, decimal RepairCost,
    decimal PickupCost, decimal NetValue, decimal Shortfall, string Currency)
{
    public decimal TotalCost => RepairCost + PickupCost;
}
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
    IReadOnlyList<string> SocialBenefits, IReadOnlyList<string> EnvironmentalBenefits)
{
    // Signed outcome is retained in the reasoning contract; persistence uses surplus + shortfall.
    public ValueEstimate ToEstimate() => new(EstimatedProceeds,
        Inputs.Route == RecoveryRoute.Donate ? 0 : decimal.Round(Inputs.EstimatedProceedsHigh, 2, MidpointRounding.AwayFromZero),
        EstimatedRepairCost, EstimatedPickupCost, Math.Max(EstimatedNetValue, 0),
        Math.Max(-EstimatedNetValue, 0), Currency);
}
