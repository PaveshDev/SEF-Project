using System.Text.Json;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Entities;

public sealed class RecoveryOption
{
    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; } = 1;

    public void RequireVersion(int expected)
    {
        RecoveryRequestValidator.Version(expected);
        if (Version != expected) throw RecoveryException.Conflict("stale_version", "The resource changed. Reload before retrying.");
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version = checked(Version + 1);
    }

    public Guid RecoveryCaseId { get; private set; }
    public int CaseRevision { get; private set; }
    public Guid AssessmentId { get; private set; }
    public int AssessmentVersion { get; private set; }
    public RecoveryRoute Route { get; private set; }
    public bool RequiresPartner { get; private set; }
    public bool RequiresPickup { get; private set; }
    public decimal? EstimatedValueLow { get; private set; }
    public decimal? EstimatedValueHigh { get; private set; }
    public decimal? EstimatedRepairCost { get; private set; }
    public decimal? EstimatedPickupCost { get; private set; }
    public decimal? EstimatedNetValue { get; private set; }
    public decimal? EstimatedShortfall { get; private set; }
    public string Currency { get; private set; } = "";
    public string EvidenceJson { get; private set; } = "[]";
    public string IntegrationSnapshotJson { get; private set; } = "{}";
    public string NonFinancialBenefitsJson { get; private set; } = "[]";
    public RecoveryOptionStatus Status { get; private set; } = RecoveryOptionStatus.Draft;
    private RecoveryOption() { }

    public static RecoveryOption Draft(RecoveryCase recoveryCase, RecoveryRoute route, bool requiresPartner, bool requiresPickup, DateTimeOffset now)
    {
        RecoveryRequestValidator.Id(recoveryCase.Id, "RecoveryCaseId");
        RecoveryRequestValidator.Defined(route);
        if (recoveryCase.Status != RecoveryCaseStatus.Planning || !recoveryCase.PreferredRoutes.Contains(route))
            throw RecoveryException.Conflict("invalid_option", "The option must belong to a planning case and requested route.");
        if (requiresPickup && !requiresPartner) throw RecoveryException.Invalid("A managed pickup requires a partner.");
        return new RecoveryOption
        {
            RecoveryCaseId = recoveryCase.Id, CaseRevision = recoveryCase.Revision,
            AssessmentId = recoveryCase.AssessmentId, AssessmentVersion = recoveryCase.AssessmentVersion,
            Route = route, RequiresPartner = requiresPartner, RequiresPickup = requiresPickup,
            Currency = recoveryCase.Currency, CreatedAt = now, UpdatedAt = now
        };
    }

    public void ValidateEstimate(ValueEstimate estimate, IReadOnlyList<ValueEvidence> evidence, DateTimeOffset now)
    {
        if (Status != RecoveryOptionStatus.Draft) throw RecoveryException.Conflict("immutable_option", "Only draft options can be valued.");
        if (estimate.Currency != Currency || evidence.Count == 0 ||
            evidence.Any(e => e.Currency != Currency || e.ReferenceId == Guid.Empty || e.Version < 1))
            throw RecoveryException.Invalid("A matching currency and identified value evidence are required.");
        foreach (var amount in new[] { estimate.ValueLow, estimate.ValueHigh, estimate.RepairCost, estimate.PickupCost, estimate.NetValue, estimate.Shortfall })
            RecoveryRequestValidator.Money(amount);
        if (estimate.ValueLow > estimate.ValueHigh || (!RequiresPickup && estimate.PickupCost != 0) ||
            (Route != RecoveryRoute.RepairThenReuse && estimate.RepairCost != 0))
            throw RecoveryException.Invalid("The estimate conflicts with the route or value bounds.");
        var cost = estimate.RepairCost + estimate.PickupCost;
        RecoveryRequestValidator.Money(cost);
        if (estimate.NetValue != Math.Max(estimate.ValueLow - cost, 0) ||
            estimate.Shortfall != Math.Max(cost - estimate.ValueLow, 0))
            throw RecoveryException.Invalid("Net value and shortfall must be computed by the service.");
        EstimatedValueLow = estimate.ValueLow; EstimatedValueHigh = estimate.ValueHigh;
        EstimatedRepairCost = estimate.RepairCost; EstimatedPickupCost = estimate.PickupCost;
        EstimatedNetValue = estimate.NetValue; EstimatedShortfall = estimate.Shortfall;
        EvidenceJson = JsonSerializer.Serialize(evidence);
        Status = RecoveryOptionStatus.Validated;
        Touch(now);
    }

    public void RecordIntegration(MatchSummary? match, PickupPlanSummary? pickup)
    {
        if (Status != RecoveryOptionStatus.Draft) throw RecoveryException.Conflict("immutable_option", "Only a draft option can record planning inputs.");
        if (RequiresPartner != (match is not null) || RequiresPickup != (pickup is not null) ||
            (match is not null && match.RecoveryOptionId != Id) || (pickup is not null && pickup.MatchId != match?.MatchId))
            throw RecoveryException.Invalid("Planning inputs do not belong to this option.");
        IntegrationSnapshotJson = JsonSerializer.Serialize(new OptionIntegrationSnapshot(match, pickup));
    }

    public ValueEstimate Estimate() => Status is RecoveryOptionStatus.Validated or RecoveryOptionStatus.Selected
        ? new(EstimatedValueLow!.Value, EstimatedValueHigh!.Value, EstimatedRepairCost!.Value,
            EstimatedPickupCost!.Value, EstimatedNetValue!.Value, EstimatedShortfall!.Value, Currency)
        : throw RecoveryException.Conflict("estimate_unavailable", "This option has no validated estimate.");

    public void Select(DateTimeOffset now)
    {
        if (Status != RecoveryOptionStatus.Validated) throw RecoveryException.Conflict("invalid_option", "Only a validated option can be selected.");
        Status = RecoveryOptionStatus.Selected; Touch(now);
    }

    public void MarkStale(DateTimeOffset now)
    {
        if (Status == RecoveryOptionStatus.Stale) return;
        Status = RecoveryOptionStatus.Stale; Touch(now);
    }
}
