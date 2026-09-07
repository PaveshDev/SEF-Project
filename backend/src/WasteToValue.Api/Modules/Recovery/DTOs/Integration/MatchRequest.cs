namespace WasteToValue.Api.Modules.Recovery.DTOs.Integration;

public sealed record MatchRequest(Guid OperationId, Guid RecoveryCaseId, int CaseRevision,
    Guid RecoveryOptionId, int OptionVersion, Guid ItemId, Guid AssessmentId,
    int AssessmentVersion, Guid CategoryId, ConditionGrade Condition,
    FunctionalStatus Function, RecoveryRoute Route, string ServiceArea, DateTimeOffset? Deadline);
