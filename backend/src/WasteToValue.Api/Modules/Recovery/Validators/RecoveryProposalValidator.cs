using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Entities;

namespace WasteToValue.Api.Modules.Recovery.Validators;

public static class RecoveryProposalValidator
{
    public static void Dependencies(RecoveryCase recoveryCase, RecoveryOption option,
        MatchSummary? match, PickupPlanSummary? pickup, DateTimeOffset now)
    {
        if (option.RequiresPartner != (match is not null) || option.RequiresPickup != (pickup is not null))
            throw RecoveryException.Conflict("inputs_unavailable", "Required matching or pickup inputs are missing, or unnecessary inputs were supplied.");
        if (match is not null)
        {
            RecoveryRequestValidator.Id(match.MatchId, "MatchId");
            RecoveryRequestValidator.Id(match.PartnerId, "PartnerId");
            RecoveryRequestValidator.Version(match.Version);
            RecoveryRequestValidator.Text(match.FreshnessToken, "MatchFreshnessToken", 500);
            if (match.RecoveryOptionId != option.Id || match.Eligibility != MatchEligibility.Eligible ||
                match.Response != PartnerResponse.Accepted || match.CheckedAt == default || match.CheckedAt > now)
                throw RecoveryException.Conflict("invalid_match", "The match must be current, eligible, accepted, and belong to this option.");
        }
        if (pickup is not null)
        {
            RecoveryRequestValidator.Id(pickup.PickupPlanId, "PickupPlanId");
            RecoveryRequestValidator.Version(pickup.Version);
            RecoveryRequestValidator.Text(pickup.FreshnessToken, "PickupFreshnessToken", 500);
            RecoveryRequestValidator.Money(pickup.EstimatedCost);
            if (pickup.MatchId != match!.MatchId || pickup.Feasibility != PickupFeasibility.Feasible ||
                pickup.ProposedEnd <= pickup.ProposedStart || pickup.ProposedStart <= now ||
                pickup.CheckedAt == default || pickup.CheckedAt > now ||
                pickup.Currency != recoveryCase.Currency ||
                (recoveryCase.MaximumPickupCost is { } budget && pickup.EstimatedCost > budget) ||
                (recoveryCase.Deadline is { } deadline && pickup.ProposedEnd > deadline))
                throw RecoveryException.Conflict("invalid_pickup", "Pickup must be feasible, current, within the budget/deadline, and use the case currency.");
            if (option.Status != RecoveryOptionStatus.Draft && pickup.EstimatedCost != option.EstimatedPickupCost)
                throw RecoveryException.Conflict("stale_estimate", "The pickup cost changed. Recalculate the option.");
        }
    }

    public static void MatchIdentity(MatchSummary summary, Guid id, int version, string token)
    {
        if (summary.MatchId != id || summary.Version != version || summary.FreshnessToken != token)
            throw RecoveryException.Conflict("stale_match", "The matching input changed.");
    }

    public static void PickupIdentity(PickupPlanSummary summary, Guid id, int version, string token)
    {
        if (summary.PickupPlanId != id || summary.Version != version || summary.FreshnessToken != token)
            throw RecoveryException.Conflict("stale_pickup", "The pickup input changed.");
    }
}
