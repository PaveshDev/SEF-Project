namespace WasteToValue.Api.Modules.Collections.Entities;

public sealed class PickupEvent
{
    public Guid Id { get; set; }
    public Guid PickupRequestId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid ActorId { get; set; }
    public DateTimeOffset EventAt { get; set; }
    public string? Notes { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;

    public PickupRequest? PickupRequest { get; set; }
}
