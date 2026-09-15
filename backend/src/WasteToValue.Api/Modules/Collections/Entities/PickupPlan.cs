namespace WasteToValue.Api.Modules.Collections.Entities;

public sealed class PickupPlan
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid? CollectionSlotId { get; set; }
    public DateTimeOffset ProposedStart { get; set; }
    public DateTimeOffset ProposedEnd { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Currency { get; set; } = "LKR";
    public string HandlingRequirements { get; set; } = "[]";
    public string? TravelEstimate { get; set; }
    public string FeasibilityStatus { get; set; } = "MANUAL_REVIEW";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int Version { get; set; } = 1;

    public CollectionSlot? CollectionSlot { get; set; }
}
