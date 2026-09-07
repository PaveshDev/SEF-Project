using System.Text.Json.Serialization;

namespace WasteToValue.Api.Modules.Recovery.DTOs.Agent;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecoveryAgentOutput(int ContractVersion, Guid RunId, Guid CaseId, int CaseRevision,
    IReadOnlyList<RecoveryAgentCandidate> Candidates, RecoveryRoute? PreferredRoute,
    IReadOnlyList<string> Unknowns, IReadOnlyList<string> Warnings,
    IReadOnlyList<string> ClarificationRequests, bool AwaitingInputs);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecoveryAgentCandidate(RecoveryRoute Route, IReadOnlyList<Guid> ReferenceIds,
    string Rationale, IReadOnlyList<string> NonFinancialBenefits);
