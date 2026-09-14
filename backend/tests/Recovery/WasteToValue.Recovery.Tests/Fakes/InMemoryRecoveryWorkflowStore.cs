using WasteToValue.Recovery.Agent;
using WasteToValue.Api.Modules.Recovery.DTOs;

namespace WasteToValue.Recovery.Tests.Fakes;

internal sealed class InMemoryRecoveryWorkflowStore : IRecoveryWorkflowStore
{
    private readonly Dictionary<Guid, RecoveryWorkflowState> states = new();

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> CreateAsync(RecoveryWorkflowState state, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (states.ContainsKey(state.WorkflowId)) return Conflict();
        states[state.WorkflowId] = state;
        return Success(state);
    }

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> LoadAsync(Guid workflowId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return states.TryGetValue(workflowId, out var state)
            ? Success(state)
            : NotFound();
    }

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> SaveAsync(RecoveryWorkflowState state, int expectedVersion, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!states.TryGetValue(state.WorkflowId, out var current)) return NotFound();
        if (current.Version != expectedVersion) return Stale();
        var saved = state with { Version = expectedVersion + 1, UpdatedAt = DateTimeOffset.UtcNow };
        states[state.WorkflowId] = saved;
        return Success(saved);
    }

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> AppendStepResultAsync(Guid workflowId, RecoveryWorkflowStep step, CancellationToken ct)
        => Update(workflowId, state => state with
        {
            CurrentStep = step.Name,
            CompletedSteps = state.CompletedSteps.Append(step).ToArray(),
            PendingSteps = state.PendingSteps.Where(item => item.Name != step.Name).ToArray()
        }, ct);

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordToolCallSummaryAsync(Guid workflowId, RecoveryWorkflowToolResult result, CancellationToken ct)
        => Update(workflowId, state => state with
        {
            ToolResults = state.ToolResults.Append(result).ToArray(),
            RetryCounts = new Dictionary<RecoveryPlannerToolName, int>(state.RetryCounts) { [result.Tool] = result.RetryCount }
        }, ct);

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordValidationResultAsync(Guid workflowId, RecoveryWorkflowValidationResult result, CancellationToken ct)
        => Update(workflowId, state => state with { ValidationResults = state.ValidationResults.Append(result).ToArray() }, ct);

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordSafeFailureAsync(Guid workflowId, RecoveryWorkflowErrorSummary error, CancellationToken ct)
        => Update(workflowId, state => state with
        {
            ErrorSummaries = state.ErrorSummaries.Append(error).ToArray(),
            CurrentStep = "Failed",
            FinalOutcome = error.Code
        }, ct);

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> MarkWaitingForApprovalAsync(Guid workflowId, Guid proposalId, int proposalRevision, DateTimeOffset proposalExpiresAt, CancellationToken ct)
        => Update(workflowId, state => state with
        {
            ApprovalStatus = RecoveryWorkflowApprovalStatus.WaitingForApproval,
            ProposalId = proposalId,
            ProposalRevision = proposalRevision,
            ProposalExpiresAt = proposalExpiresAt,
            CurrentStep = "WaitingForApproval"
        }, ct);

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> RecordAuthorizedApprovalDecisionAsync(Guid workflowId, RecoveryApprovalDecision decision, CancellationToken ct)
        => Update(workflowId, state =>
        {
            if (state.ProposalId != decision.ProposalId || state.ProposalRevision != decision.ProposalRevision)
                throw new InvalidOperationException("proposal_revision_mismatch");
            if (state.ApprovalStatus is RecoveryWorkflowApprovalStatus.Approved or RecoveryWorkflowApprovalStatus.Rejected or RecoveryWorkflowApprovalStatus.RevisionRequested)
                return state;
            return state with
            {
                ApprovalStatus = decision.Decision switch
                {
                    ProposalDecisionKind.Approved => RecoveryWorkflowApprovalStatus.Approved,
                    ProposalDecisionKind.Rejected => RecoveryWorkflowApprovalStatus.Rejected,
                    _ => RecoveryWorkflowApprovalStatus.RevisionRequested
                },
                CurrentStep = decision.Decision == ProposalDecisionKind.Rejected ? "Rejected" : state.CurrentStep,
                FinalOutcome = decision.Decision == ProposalDecisionKind.Rejected ? "approval_rejected" : state.FinalOutcome
            };
        }, ct);

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> ResumeAsync(Guid workflowId, CancellationToken ct)
        => Update(workflowId, state =>
        {
            if (state.ApprovalStatus != RecoveryWorkflowApprovalStatus.WaitingForApproval &&
                state.ApprovalStatus != RecoveryWorkflowApprovalStatus.Approved &&
                state.ApprovalStatus != RecoveryWorkflowApprovalStatus.RevisionRequested)
                throw new InvalidOperationException("workflow_not_waiting");
            return state with
            {
                CurrentStep = state.ApprovalStatus is RecoveryWorkflowApprovalStatus.RevisionRequested or RecoveryWorkflowApprovalStatus.Approved ? "Planning" : state.CurrentStep,
                FinalOutcome = state.ApprovalStatus == RecoveryWorkflowApprovalStatus.Approved ? "approval_authorized" : state.FinalOutcome
            };
        }, ct);

    public Task<RecoveryWorkflowResult<RecoveryWorkflowState>> CompleteAsync(Guid workflowId, string finalOutcome, CancellationToken ct)
        => Update(workflowId, state => state with { CurrentStep = "Completed", FinalOutcome = finalOutcome }, ct);

    private Task<RecoveryWorkflowResult<RecoveryWorkflowState>> Update(Guid workflowId, Func<RecoveryWorkflowState, RecoveryWorkflowState> change, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!states.TryGetValue(workflowId, out var current)) return NotFound();
        try
        {
            var next = change(current) with { Version = current.Version + 1, UpdatedAt = DateTimeOffset.UtcNow };
            states[workflowId] = next;
            return Success(next);
        }
        catch (InvalidOperationException exception) when (exception.Message == "proposal_revision_mismatch")
        {
            return Task.FromResult(RecoveryWorkflowResult<RecoveryWorkflowState>.Failure("proposal_revision_mismatch", "The approval targets a different proposal revision."));
        }
        catch (InvalidOperationException exception) when (exception.Message == "workflow_not_waiting")
        {
            return Task.FromResult(RecoveryWorkflowResult<RecoveryWorkflowState>.Failure("workflow_not_resumable", "The workflow is not waiting for approval."));
        }
    }

    private static Task<RecoveryWorkflowResult<RecoveryWorkflowState>> Success(RecoveryWorkflowState state) => Task.FromResult(RecoveryWorkflowResult<RecoveryWorkflowState>.Success(state));
    private static Task<RecoveryWorkflowResult<RecoveryWorkflowState>> NotFound() => Task.FromResult(RecoveryWorkflowResult<RecoveryWorkflowState>.Failure("workflow_not_found", "Workflow state was not found."));
    private static Task<RecoveryWorkflowResult<RecoveryWorkflowState>> Stale() => Task.FromResult(RecoveryWorkflowResult<RecoveryWorkflowState>.Failure("workflow_stale", "Workflow state is stale."));
    private static Task<RecoveryWorkflowResult<RecoveryWorkflowState>> Conflict() => Task.FromResult(RecoveryWorkflowResult<RecoveryWorkflowState>.Failure("workflow_exists", "Workflow state already exists."));
}
