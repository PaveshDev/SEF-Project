namespace WasteToValue.Api.Modules.Recovery.DTOs.Integration;

// Recovery's required projection, never an Items persistence entity.
public sealed record AssessmentSummary(Guid AssessmentId, Guid ItemId, Guid OwnerId,
    Guid? CategoryId, int ItemRevision, int AssessmentVersion, bool IsCurrent,
    AssessmentStatus Status, ConditionGrade Condition, FunctionalStatus Function,
    DateTimeOffset? ConfirmedAt, string ServiceArea, IReadOnlyList<string> EvidenceReferences);
