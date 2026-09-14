using WasteToValue.Api.Modules.Collections.DTOs;

namespace WasteToValue.Api.Modules.Collections.Agents;

public sealed record ToolCallLog(
    string ToolName,
    string InputSummary,
    string OutputSummary,
    bool IsSuccess,
    DateTimeOffset ExecutedAt,
    string? ErrorMessage);

public sealed record AgentWorkflowContext(
    Guid WorkflowId,
    Guid PickupRequestId,
    string Objective,
    string CurrentStep,
    string Status,
    List<ToolCallLog> ToolLogs,
    List<ConstraintCheckDto> ValidationChecks,
    List<string> Errors,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public CollectionProposalDto? FinalProposal { get; set; }
}
