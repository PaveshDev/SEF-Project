namespace WasteToValue.Api.Modules.Recovery.DTOs;

public sealed record ValueEstimate(decimal ValueLow, decimal ValueHigh, decimal RepairCost,
    decimal PickupCost, decimal NetValue, decimal Shortfall, string Currency);
public sealed record ValueEvidence(Guid ReferenceId, int Version, DateTimeOffset ObservedAt,
    decimal ValueLow, decimal ValueHigh, string Currency, string SourceName);
