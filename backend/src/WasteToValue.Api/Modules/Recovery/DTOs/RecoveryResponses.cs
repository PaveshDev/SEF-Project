using System.Text.Json;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;

namespace WasteToValue.Api.Modules.Recovery.DTOs;

public sealed record RecoveryActor(Guid UserId, bool IsHuman, bool CanManageValueReferences = false);
public sealed record OptionIntegrationSnapshot(MatchSummary? Match, PickupPlanSummary? Pickup);
public sealed record PlanningResponse(RecoveryCaseResponse Case, IReadOnlyList<RecoveryOptionResponse> Options,
    IReadOnlyList<string> UnavailableInputs);

public sealed record RecoveryCaseResponse(Guid Id, Guid ItemId, int Revision, int Version, string Objective,
    IReadOnlyList<RecoveryRoute> PreferredRoutes, string Currency, decimal? MaximumPickupCost,
    DateTimeOffset? Deadline, RecoveryCaseStatus Status)
{
    public static RecoveryCaseResponse From(RecoveryCase value) => new(value.Id, value.ItemId, value.Revision,
        value.Version, value.Objective, value.PreferredRoutes, value.Currency, value.MaximumPickupCost, value.Deadline, value.Status);
}

public sealed record RecoveryOptionResponse(Guid Id, int CaseRevision, int Version, RecoveryRoute Route,
    bool RequiresPartner, bool RequiresPickup, RecoveryOptionStatus Status, ValueEstimate? Estimate,
    IReadOnlyList<ValueEvidence> Evidence, IReadOnlyList<string> NonFinancialBenefits,
    OptionIntegrationSnapshot Integration)
{
    public static RecoveryOptionResponse From(RecoveryOption value) => new(value.Id, value.CaseRevision,
        value.Version, value.Route, value.RequiresPartner, value.RequiresPickup, value.Status,
        value.Status is RecoveryOptionStatus.Validated or RecoveryOptionStatus.Selected ? value.Estimate() : null,
        JsonSerializer.Deserialize<ValueEvidence[]>(value.EvidenceJson)!,
        JsonSerializer.Deserialize<string[]>(value.NonFinancialBenefitsJson)!,
        JsonSerializer.Deserialize<OptionIntegrationSnapshot>(value.IntegrationSnapshotJson)!);
}

public sealed record RecoveryProposalResponse(Guid Id, Guid CaseId, int CaseRevision, int Revision,
    int Version, Guid OptionId, Guid? MatchId, Guid? PickupPlanId, string Explanation,
    DateTimeOffset ExpiresAt, RecoveryProposalStatus Status, ValueEstimate Estimate)
{
    public static RecoveryProposalResponse From(RecoveryProposal value) => new(value.Id, value.RecoveryCaseId,
        value.CaseRevision, value.Revision, value.Version, value.RecoveryOptionId, value.MatchId,
        value.PickupPlanId, value.Explanation, value.ExpiresAt, value.Status,
        JsonSerializer.Deserialize<ValueEstimate>(value.EstimateSnapshot)!);
}

public sealed record ValueReferenceResponse(Guid Id, Guid CategoryId, ConditionGrade Condition,
    RecoveryRoute Route, decimal ValueLow, decimal ValueHigh, string Currency, string SourceName,
    string? SourceReference, DateTimeOffset ObservedAt, bool IsVerified, int Version)
{
    public static ValueReferenceResponse From(ValueReference value) => new(value.Id, value.CategoryId, value.Condition,
        value.Route, value.ValueLow, value.ValueHigh, value.Currency, value.SourceName, value.SourceReference,
        value.ObservedAt, value.IsVerified, value.Version);
}

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
