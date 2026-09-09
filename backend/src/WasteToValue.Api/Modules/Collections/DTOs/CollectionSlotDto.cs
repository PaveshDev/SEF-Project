namespace WasteToValue.Api.Modules.Collections.DTOs;

public sealed record CollectionSlotReadDto(
    Guid Id,
    Guid? CollectorId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string ServiceArea,
    int Capacity,
    int ReservedCount,
    string? VehicleClass,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateCollectionSlotRequest(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string ServiceArea,
    int Capacity,
    string? VehicleClass,
    Guid? CollectorId);

public sealed record UpdateCollectionSlotRequest(
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    string? ServiceArea,
    int? Capacity,
    string? VehicleClass,
    Guid? CollectorId,
    string? Status);
