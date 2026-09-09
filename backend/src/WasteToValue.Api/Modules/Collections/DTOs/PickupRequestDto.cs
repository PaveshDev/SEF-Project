namespace WasteToValue.Api.Modules.Collections.DTOs;

public sealed record PickupRequestReadDto(
    Guid Id,
    Guid RecoveryProposalId,
    Guid CollectionSlotId,
    Guid? CollectorId,
    Guid OwnerId,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    CollectionSlotReadDto? CollectionSlot);

public sealed record CreatePickupRequestRequest(
    Guid RecoveryProposalId,
    Guid CollectionSlotId,
    Guid OwnerId,
    string PickupAddress,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd);

public sealed record UpdatePickupRequestRequest(
    DateTimeOffset? ScheduledStart,
    DateTimeOffset? ScheduledEnd,
    string? PickupAddress,
    Guid? CollectorId,
    string? Status);
