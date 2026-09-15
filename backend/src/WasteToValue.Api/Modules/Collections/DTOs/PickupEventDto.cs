namespace WasteToValue.Api.Modules.Collections.DTOs;

public sealed record PickupEventReadDto(
    Guid Id,
    Guid PickupRequestId,
    string EventType,
    Guid ActorId,
    DateTimeOffset EventAt,
    string? Notes);
