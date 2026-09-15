using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Recovery.Agent;

public enum RecoveryWorkflowApprovalStatus { NotRequested, WaitingForApproval, Approved, Rejected, RevisionRequested }

public sealed record RecoveryWorkflowStep(string Name, string Status, DateTimeOffset RecordedAt);

public sealed record RecoveryWorkflowToolResult(RecoveryPlannerToolName Tool, string Outcome,
    string? Code, bool Retryable, int RetryCount, DateTimeOffset RecordedAt);

public sealed record RecoveryWorkflowValidationResult(string Name, bool IsValid,
    string? Code, string? Message, DateTimeOffset RecordedAt);

public sealed record RecoveryWorkflowErrorSummary(string Code, string Message,
    RecoveryPlannerToolName? Tool, DateTimeOffset RecordedAt);

public sealed record RecoveryWorkflowState(
    Guid WorkflowId,
    Guid RecoveryCaseId,
    string Objective,
    IReadOnlyList<RecoveryRoute> StructuredPlan,
    string CurrentStep,
    IReadOnlyList<RecoveryWorkflowStep> CompletedSteps,
    IReadOnlyList<RecoveryWorkflowStep> PendingSteps,
    IReadOnlyList<RecoveryWorkflowToolResult> ToolResults,
    IReadOnlyList<RecoveryWorkflowValidationResult> ValidationResults,
    IReadOnlyDictionary<RecoveryPlannerToolName, int> RetryCounts,
    RecoveryWorkflowApprovalStatus ApprovalStatus,
    Guid? ProposalId,
    int? ProposalRevision,
    DateTimeOffset? ProposalExpiresAt,
    IReadOnlyList<RecoveryWorkflowErrorSummary> ErrorSummaries,
    string? FinalOutcome,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version);

public sealed record RecoveryApprovalDecision(Guid ProposalId, int ProposalRevision,
    ProposalDecisionKind Decision, string? Comment);

public sealed record RecoveryWorkflowResult<T>(T? Value, RecoveryPlannerError? Error)
{
    public bool IsSuccess => Error is null && Value is not null;
    public static RecoveryWorkflowResult<T> Success(T value) => new(value, null);
    public static RecoveryWorkflowResult<T> Failure(string code, string message, bool retryable = false) =>
        new(default, new(code, message, retryable));
}

public interface IRecoveryWorkflowStore
{
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> CreateAsync(RecoveryWorkflowState state, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> LoadAsync(Guid workflowId, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> SaveAsync(RecoveryWorkflowState state, int expectedVersion, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> AppendStepResultAsync(Guid workflowId, RecoveryWorkflowStep step, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordToolCallSummaryAsync(Guid workflowId, RecoveryWorkflowToolResult result, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordValidationResultAsync(Guid workflowId, RecoveryWorkflowValidationResult result, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordSafeFailureAsync(Guid workflowId, RecoveryWorkflowErrorSummary error, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> MarkWaitingForApprovalAsync(Guid workflowId, Guid proposalId, int proposalRevision, DateTimeOffset proposalExpiresAt, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordAuthorizedApprovalDecisionAsync(Guid workflowId, RecoveryApprovalDecision decision, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> ResumeAsync(Guid workflowId, CancellationToken ct);
    Task<RecoveryWorkflowResult<RecoveryWorkflowState>> CompleteAsync(Guid workflowId, string finalOutcome, CancellationToken ct);
}

public sealed class UnavailableRecoveryWorkflowStore : IRecoveryWorkflowStore
{
    private static RecoveryWorkflowResult<T> Missing<T>() => RecoveryWorkflowResult<T>.Failure(
        "workflow_store_unavailable", "Durable Recovery workflow state is not configured.", true);

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> CreateAsync(RecoveryWorkflowState state, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> LoadAsync(Guid workflowId, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> SaveAsync(RecoveryWorkflowState state, int expectedVersion, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> AppendStepResultAsync(Guid workflowId, RecoveryWorkflowStep step, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordToolCallSummaryAsync(Guid workflowId, RecoveryWorkflowToolResult result, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordValidationResultAsync(Guid workflowId, RecoveryWorkflowValidationResult result, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordSafeFailureAsync(Guid workflowId, RecoveryWorkflowErrorSummary error, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> MarkWaitingForApprovalAsync(Guid workflowId, Guid proposalId, int proposalRevision, DateTimeOffset proposalExpiresAt, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordAuthorizedApprovalDecisionAsync(Guid workflowId, RecoveryApprovalDecision decision, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> ResumeAsync(Guid workflowId, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);
    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> CompleteAsync(Guid workflowId, string finalOutcome, CancellationToken ct) => MissingTask<RecoveryWorkflowState>(ct);

    private static async Task<RecoveryWorkflowResult<T>> MissingTask<T>(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return await Task.FromResult(Missing<T>());
    }
}
