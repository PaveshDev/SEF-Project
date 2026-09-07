using System.Text.Json.Serialization;

namespace WasteToValue.Api.Modules.Recovery.DTOs;

[JsonConverter(typeof(JsonStringEnumConverter<RecoveryRoute>))]
public enum RecoveryRoute { Reuse, Donate, RepairThenReuse, Resell, Recycle }
[JsonConverter(typeof(JsonStringEnumConverter<RecoveryCaseStatus>))]
public enum RecoveryCaseStatus { Draft, Planning, AwaitingInputs, AwaitingApproval, Approved, Rejected, RevisionRequested, Completed, Failed, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter<RecoveryOptionStatus>))]
public enum RecoveryOptionStatus { Draft, Validated, Selected, Stale, Rejected }
[JsonConverter(typeof(JsonStringEnumConverter<RecoveryProposalStatus>))]
public enum RecoveryProposalStatus { Draft, AwaitingApproval, Approved, Rejected, RevisionRequested, Expired, Executed, Stale }
[JsonConverter(typeof(JsonStringEnumConverter<ProposalDecisionKind>))]
public enum ProposalDecisionKind { Approved, Rejected, RevisionRequested }
[JsonConverter(typeof(JsonStringEnumConverter<RecommendationOrigin>))]
public enum RecommendationOrigin { Human, Agent }
[JsonConverter(typeof(JsonStringEnumConverter<ConditionGrade>))]
public enum ConditionGrade { Excellent, Good, Fair, Poor, Unsafe, Unknown }
[JsonConverter(typeof(JsonStringEnumConverter<FunctionalStatus>))]
public enum FunctionalStatus { Working, PartiallyWorking, NotWorking, Unknown }
[JsonConverter(typeof(JsonStringEnumConverter<AssessmentStatus>))]
public enum AssessmentStatus { Draft, ClarificationRequired, ReadyForConfirmation, Confirmed, Superseded, Failed }
[JsonConverter(typeof(JsonStringEnumConverter<MatchEligibility>))]
public enum MatchEligibility { Eligible, Ineligible, RequiresReview }
[JsonConverter(typeof(JsonStringEnumConverter<PartnerResponse>))]
public enum PartnerResponse { Pending, Accepted, Rejected, Expired }
[JsonConverter(typeof(JsonStringEnumConverter<PickupFeasibility>))]
public enum PickupFeasibility { Feasible, Infeasible, ManualReview, Stale }
