namespace WasteToValue.Api.Modules.Collections.DTOs;

public sealed record RescheduleRequestDto(
    Guid PickupRequestId,
    string Reason,
    string? ChangedConstraint,
    Guid RequestedBy);
