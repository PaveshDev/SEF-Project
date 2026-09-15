namespace WasteToValue.Api.Modules.Collections.Entities;

public sealed class CollectionSlot
{
    public Guid Id { get; set; }
    public Guid? CollectorId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string ServiceArea { get; set; } = string.Empty;
    public int Capacity { get; set; } = 1;
    public int ReservedCount { get; set; }
    public string? VehicleClass { get; set; }
    public string Status { get; set; } = "AVAILABLE";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int Version { get; set; } = 1;
}
