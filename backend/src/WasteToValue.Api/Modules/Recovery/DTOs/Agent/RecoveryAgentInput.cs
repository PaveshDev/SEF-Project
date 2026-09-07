namespace WasteToValue.Api.Modules.Recovery.DTOs.Agent;

// Constructed by trusted application code. Contains no owner identity, address, credentials, or approval capability.
public sealed record RecoveryAgentInput(int ContractVersion, Guid RunId, Guid CaseId, int CaseRevision,
    string Objective, IReadOnlyList<RecoveryRoute> AllowedRoutes, decimal? MaximumPickupCost,
    string Currency, DateTimeOffset? Deadline, AgentAssessmentFacts Assessment,
    IReadOnlyList<ValueEvidence> VerifiedValueReferences, IReadOnlyList<string> MissingInputs);

public sealed record AgentAssessmentFacts(Guid AssessmentId, int ItemRevision, int AssessmentVersion,
    ConditionGrade Condition, FunctionalStatus Function, IReadOnlyList<string> EvidenceReferences);
