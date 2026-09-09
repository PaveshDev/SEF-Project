using WasteToValue.Api.Modules.Collections.DTOs;

namespace WasteToValue.Api.Modules.Collections.Validators;

public static class CreatePickupRequestValidator
{
    public static IReadOnlyList<string> Validate(CreatePickupRequestRequest request)
    {
        var errors = new List<string>();

        if (request.RecoveryProposalId == Guid.Empty)
            errors.Add("RecoveryProposalId is required.");

        if (request.CollectionSlotId == Guid.Empty)
            errors.Add("CollectionSlotId is required.");

        if (request.OwnerId == Guid.Empty)
            errors.Add("OwnerId is required.");

        if (string.IsNullOrWhiteSpace(request.PickupAddress))
            errors.Add("PickupAddress is required.");

        if (request.ScheduledEnd <= request.ScheduledStart)
            errors.Add("ScheduledEnd must be after ScheduledStart.");

        if (request.ScheduledStart < DateTimeOffset.UtcNow.AddMinutes(-5))
            errors.Add("ScheduledStart must not be in the past.");

        return errors;
    }
}
