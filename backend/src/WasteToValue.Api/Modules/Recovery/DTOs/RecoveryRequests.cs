namespace WasteToValue.Api.Modules.Recovery.DTOs;

public sealed record RecoveryInputs(string Objective, IReadOnlyList<RecoveryRoute> PreferredRoutes,
    string Currency, decimal? MaximumPickupCost, DateTimeOffset? Deadline);
public sealed record CreateRecoveryCaseRequest(Guid ItemId, RecoveryInputs Inputs);
public sealed record UpdateRecoveryInputsRequest(int ExpectedVersion, RecoveryInputs Inputs);
public sealed record StartPlanningRequest(int ExpectedVersion);
public sealed record SubmitProposalRequest(int ExpectedVersion, Guid OptionId, int OptionVersion,
    MatchChoice? Match, PickupChoice? Pickup, DateTimeOffset ExpiresAt, string Explanation);
public sealed record MatchChoice(Guid Id, int Version, string FreshnessToken);
public sealed record PickupChoice(Guid Id, int Version, string FreshnessToken);
public sealed record ProposalDecisionRequest(int ExpectedVersion, int ProposalRevision,
    ProposalDecisionKind Decision, string? Comment);
public sealed record CancelRecoveryCaseRequest(int ExpectedVersion);
public sealed record CreateValueReferenceRequest(Guid CategoryId, ConditionGrade Condition,
    RecoveryRoute Route, decimal ValueLow, decimal ValueHigh, string Currency,
    string SourceName, string? SourceReference, DateTimeOffset ObservedAt);
public sealed record VerifyValueReferenceRequest(int ExpectedVersion);
public sealed record UpdateValueReferenceRequest(int ExpectedVersion, ConditionGrade Condition,
    RecoveryRoute Route, decimal ValueLow, decimal ValueHigh, string Currency,
    string SourceName, string? SourceReference, DateTimeOffset ObservedAt);
public sealed record RecoveryCaseQuery(string? Search, RecoveryCaseStatus? Status,
    string? SortBy = "createdAt", string? SortDirection = "desc", int Page = 1, int PageSize = 25);
public sealed record ValueReferenceQuery(string? Search, ConditionGrade? Condition,
    RecoveryRoute? Route, string? Currency, string? SortBy = "observedAt",
    string? SortDirection = "desc", int Page = 1, int PageSize = 25);
