using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;

namespace WasteToValue.Api.Modules.Recovery.Validators;

public static class RecoveryRequestValidator
{
    public static string Text(string? value, string field, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max)
            throw RecoveryException.Invalid($"{field} is required and must not exceed {max} characters.");
        return value.Trim();
    }

    public static void Id(Guid value, string field)
    {
        if (value == Guid.Empty) throw RecoveryException.Invalid($"{field} must be a nonempty identifier.");
    }

    public static void Version(int value)
    {
        if (value < 1) throw RecoveryException.Invalid("Versions and revisions must be positive.");
    }

    public static void Defined<T>(T value) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw RecoveryException.Invalid($"Unknown {typeof(T).Name}.");
    }

    public static void Money(decimal value)
    {
        if (value < 0 || value > 9999999999.99m || decimal.Round(value, 2) != value)
            throw RecoveryException.Invalid("Money must be nonnegative, fit numeric(12,2), and have at most two decimal places.");
    }

    public static string Currency(string? value)
    {
        if (value is null || value.Length != 3 || value.Any(c => c < 'A' || c > 'Z'))
            throw RecoveryException.Invalid("Currency must contain exactly three uppercase ASCII letters.");
        return value;
    }

    public static RecoveryInputs Inputs(RecoveryInputs? input, DateTimeOffset now)
    {
        if (input is null) throw RecoveryException.Invalid("Planning inputs are required.");
        Text(input.Objective, "Objective", 1000);
        Currency(input.Currency);
        if (input.PreferredRoutes is null || input.PreferredRoutes.Count is < 1 or > 5 ||
            input.PreferredRoutes.Distinct().Count() != input.PreferredRoutes.Count)
            throw RecoveryException.Invalid("Choose between one and five distinct routes.");
        foreach (var route in input.PreferredRoutes) Defined(route);
        if (input.MaximumPickupCost is { } cost) Money(cost);
        if (input.Deadline is { } deadline && deadline <= now)
            throw RecoveryException.Invalid("The deadline must be in the future.");
        return input;
    }

    public static void Assessment(AssessmentSummary assessment, Guid itemId, Guid ownerId)
    {
        Id(assessment.AssessmentId, "AssessmentId");
        if (assessment.ItemId != itemId) throw RecoveryException.Conflict("assessment_mismatch", "Assessment belongs to another item.");
        if (assessment.OwnerId != ownerId) throw new RecoveryException(403, "owner_required", "Only the item owner may operate this case.");
        if (!assessment.IsCurrent || assessment.Status != AssessmentStatus.Confirmed || assessment.ConfirmedAt is null)
            throw RecoveryException.Conflict("assessment_unconfirmed", "A current confirmed assessment is required.");
        Version(assessment.ItemRevision);
        Version(assessment.AssessmentVersion);
        Defined(assessment.Condition);
        Defined(assessment.Function);
    }

    public static T Require<T>(GatewayResult<T> result)
    {
        if (result.Outcome == GatewayOutcome.Success && result.Value is not null) return result.Value;
        throw new RecoveryException(result.Outcome switch
        {
            GatewayOutcome.NotFound => 404,
            GatewayOutcome.Invalid => 400,
            GatewayOutcome.Stale => 409,
            _ => 503
        }, result.Code, result.Message);
    }
}
