using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Recovery.Agent;

public enum RecoveryPlannerToolName
{
    GetConfirmedAssessment,
    GetValueReferences,
    CalculateRecoveryValue,
    RequestRecipientMatches,
    RequestPickupFeasibility,
    SaveRecoveryOptions,
    CreateProposalDraft,
    RequestHumanApproval
}

public enum RecoveryPlannerWorkflowState { Created, Planning, AwaitingApproval, Completed, Failed }

public sealed record RecoveryPlannerAgentInput(Guid RecoveryCaseId, string Objective,
    AssessmentSummary Assessment, IReadOnlyList<RecoveryRoute> PreferredRoutes,
    decimal? MaximumPickupCost, string Currency, DateTimeOffset? Deadline);

public sealed record RecoveryPlannerValueReference(Guid ReferenceId, int Version,
    decimal ValueLow, decimal ValueHigh, string Currency, string SourceName,
    DateTimeOffset ObservedAt);

public sealed record RecoveryPlannerAlternative(RecoveryRoute Route,
    IReadOnlyList<Guid> ReferenceIds, ValueEstimationResult Valuation,
    MatchSummary? Match, PickupPlanSummary? Pickup);

public sealed record RecoveryPlannerOptionDraft(Guid RecoveryCaseId, RecoveryRoute Route,
    decimal EstimatedNetValue, string Currency, IReadOnlyList<Guid> ReferenceIds);

public sealed record RecoveryPlannerPersistedOption(Guid Id, Guid RecoveryCaseId,
    RecoveryRoute Route, decimal EstimatedNetValue, string Currency, int Version);

public sealed record RecoveryPlannerProposalDraft(Guid RecoveryCaseId, int CaseRevision,
    IReadOnlyList<RecoveryPlannerAlternative> Alternatives, DateTimeOffset ExpiresAt)
{
    public RecoveryReasoningResponse? Recommendation { get; init; }
}

public sealed record RecoveryPlannerExecutionSummary(Guid RecoveryCaseId, Guid RunId,
    RecoveryPlannerWorkflowState State, IReadOnlyList<RecoveryPlannerToolName> ToolsUsed,
    IReadOnlyList<string> Warnings, DateTimeOffset StartedAt, DateTimeOffset CompletedAt)
{
    public IReadOnlyList<RecoveryPlannerError> IntegrationFailures { get; init; } = Array.Empty<RecoveryPlannerError>();
}

public sealed record RecoveryPlannerAgentResult(RecoveryPlannerWorkflowState State,
    IReadOnlyList<RecoveryPlannerAlternative> Alternatives, Guid? ProposalId,
    RecoveryPlannerExecutionSummary Summary, RecoveryPlannerError? Error)
{
    public RecoveryReasoningResponse? Recommendation { get; init; }
}

public sealed record RecoveryPlannerError(string Code, string Message, bool Retryable,
    RecoveryPlannerToolName? Tool = null);

public sealed record RecoveryPlannerRuntimeOptions(TimeSpan ToolTimeout, int MaxRetries,
    TimeSpan ProposalLifetime)
{
    public static RecoveryPlannerRuntimeOptions Default { get; } =
        new(TimeSpan.FromSeconds(5), 2, TimeSpan.FromHours(2));
}

public sealed record PlannerToolResult<T>(T? Value, RecoveryPlannerError? Error)
{
    public bool IsSuccess => Error is null && Value is not null;
    public static PlannerToolResult<T> Success(T value) => new(value, null);
    public static PlannerToolResult<T> Failure(RecoveryPlannerError error) => new(default, error);
}

public interface IRecoveryPlannerToolset
{
    Task<PlannerToolResult<AssessmentSummary>> GetConfirmedAssessmentAsync(Guid caseId, CancellationToken ct);
    Task<PlannerToolResult<IReadOnlyList<RecoveryPlannerValueReference>>> GetValueReferencesAsync(Guid caseId, CancellationToken ct);
    Task<PlannerToolResult<ValueEstimationResult>> CalculateRecoveryValueAsync(ValueEstimationInput input, CancellationToken ct);
    Task<PlannerToolResult<IReadOnlyList<MatchSummary>>> RequestRecipientMatchesAsync(MatchRequest request, CancellationToken ct);
    Task<PlannerToolResult<IReadOnlyList<PickupPlanSummary>>> RequestPickupFeasibilityAsync(PickupPlanningRequest request, CancellationToken ct);
    Task<PlannerToolResult<IReadOnlyList<RecoveryPlannerPersistedOption>>> SaveRecoveryOptionsAsync(Guid runId, IReadOnlyList<RecoveryPlannerOptionDraft> options, CancellationToken ct);
    Task<PlannerToolResult<Guid>> CreateProposalDraftAsync(RecoveryPlannerProposalDraft draft, CancellationToken ct);
    Task<PlannerToolResult<bool>> RequestHumanApprovalAsync(Guid proposalId, CancellationToken ct);
}

public sealed class RecoveryPlannerToolInvoker(IRecoveryPlannerToolset tools, RecoveryPlannerRuntimeOptions options)
{
    private static readonly IReadOnlySet<RecoveryPlannerToolName> Allowed =
        Enum.GetValues<RecoveryPlannerToolName>().ToHashSet();

    public async Task<PlannerToolResult<T>> InvokeAsync<T>(RecoveryPlannerToolName name,
        Func<IRecoveryPlannerToolset, CancellationToken, Task<PlannerToolResult<T>>> operation,
        CancellationToken cancellationToken)
    {
        if (!Allowed.Contains(name))
            return PlannerToolResult<T>.Failure(new("tool_not_allowed", "The requested tool is not allow-listed.", false, name));

        RecoveryPlannerError? lastError = null;
        for (var attempt = 0; attempt <= options.MaxRetries; attempt++)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(options.ToolTimeout);
                var result = await operation(tools, timeout.Token);
                if (result.IsSuccess || result.Error is null || !result.Error.Retryable || attempt == options.MaxRetries)
                    return result;
                lastError = result.Error;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                lastError = new("tool_timeout", "The tool timed out.", true, name);
                if (attempt == options.MaxRetries) return PlannerToolResult<T>.Failure(lastError);
            }
        }

        return PlannerToolResult<T>.Failure(lastError ?? new("retry_exhausted", "Tool retries were exhausted.", false, name));
    }
}

public static class RecoveryPlannerInputValidator
{
    private static readonly string[] InjectionMarkers =
    {
        "ignore previous instructions", "ignore all instructions", "system prompt", "developer message",
        "call arbitrary tool", "reveal hidden reasoning", "chain of thought"
    };

    public static void Validate(RecoveryPlannerAgentInput input)
    {
        RecoveryRequestValidator.Id(input.RecoveryCaseId, "RecoveryCaseId");
        RecoveryRequestValidator.Text(input.Objective, "Objective", 1000);
        if (InjectionMarkers.Any(marker => input.Objective.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            throw RecoveryException.Invalid("Objective contains unsupported instruction content.");
        RecoveryRequestValidator.Currency(input.Currency);
        if (input.PreferredRoutes is null || input.PreferredRoutes.Count == 0)
            throw RecoveryException.Invalid("At least one preferred route is required.");
        foreach (var route in input.PreferredRoutes) RecoveryRequestValidator.Defined(route);
        if (input.MaximumPickupCost is { } budget) RecoveryRequestValidator.Money(budget);
        if (input.Deadline is { } deadline && deadline <= DateTimeOffset.UtcNow)
            throw RecoveryException.Invalid("Deadline must be in the future.");
    }
}

public sealed class RecoveryPlannerAgent(RecoveryPlannerToolset tools, RecoveryPlannerRuntimeOptions? options = null,
    TimeProvider? clock = null, IRecoveryWorkflowStore? workflows = null,
    IRecoveryActorAccessor? actors = null, IRecoveryCommandExecutor? commands = null,
    IRecoveryReasoningProvider? reasoning = null)
{
    private readonly RecoveryPlannerRuntimeOptions runtime = options ?? RecoveryPlannerRuntimeOptions.Default;
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    private readonly IRecoveryWorkflowStore? workflows = workflows;
    private readonly IRecoveryActorAccessor? actors = actors;
    private readonly IRecoveryCommandExecutor? commands = commands;
    private readonly IRecoveryReasoningProvider reasoning = reasoning ?? new UnavailableRecoveryReasoningProvider();

    public async Task<RecoveryPlannerAgentResult> RunAsync(RecoveryPlannerAgentInput input,
        int caseRevision, Guid runId, CancellationToken cancellationToken)
    {
        var started = time.GetUtcNow();
        var used = new List<RecoveryPlannerToolName>();
        var warnings = new List<string>();
        var integrationFailures = new List<RecoveryPlannerError>();
        var workflowCreated = false;
        async Task<RecoveryPlannerAgentResult> Fail(RecoveryPlannerError error)
        {
            if (workflowCreated)
            {
                // Failure finalization must still run when the caller cancels.
                using var cleanup = new CancellationTokenSource(runtime.ToolTimeout);
                try
                {
                    var recorded = await workflows!.RecordSafeFailureAsync(runId,
                        new(error.Code, error.Message, error.Tool, time.GetUtcNow()), cleanup.Token)
                        .WaitAsync(cleanup.Token);
                    if (!recorded.IsSuccess)
                        error = new("workflow_state_unavailable", "Workflow failure could not be persisted; reconciliation is required.", true);
                }
                catch (Exception)
                {
                    error = new("workflow_state_unavailable", "Workflow failure could not be persisted; reconciliation is required.", true);
                }
            }
            var failed = Failure(input, runId, started, used, warnings, error);
            return failed with { Summary = failed.Summary with { IntegrationFailures = integrationFailures.ToArray() } };
        }
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            RecoveryPlannerInputValidator.Validate(input);
            if (workflows is not null)
            {
                var created = await workflows.CreateAsync(new RecoveryWorkflowState(runId, input.RecoveryCaseId,
                    input.Objective.Trim(), input.PreferredRoutes, "Planning", Array.Empty<RecoveryWorkflowStep>(),
                    input.PreferredRoutes.Select(route => new RecoveryWorkflowStep(route.ToString(), "Pending", time.GetUtcNow())).ToArray(),
                    Array.Empty<RecoveryWorkflowToolResult>(), Array.Empty<RecoveryWorkflowValidationResult>(),
                    new Dictionary<RecoveryPlannerToolName, int>(), RecoveryWorkflowApprovalStatus.NotRequested,
                    null, null, null, Array.Empty<RecoveryWorkflowErrorSummary>(), null, started, started, 1), cancellationToken);
                if (!created.IsSuccess) return Failure(input, runId, started, used, warnings, created.Error!);
                workflowCreated = true;
            }
            var invoker = new RecoveryPlannerToolInvoker(tools, runtime);
            var assessment = await Call(invoker, RecoveryPlannerToolName.GetConfirmedAssessment,
                (t, ct) => t.GetConfirmedAssessmentAsync(input.RecoveryCaseId, ct), used, cancellationToken);
            if (!assessment.IsSuccess || !assessment.Value!.IsCurrent ||
                assessment.Value.Status != AssessmentStatus.Confirmed)
                return await Fail(assessment.Error ?? new("assessment_stale", "Confirmed assessment is stale or does not match.", false, RecoveryPlannerToolName.GetConfirmedAssessment));

            var references = await Call(invoker, RecoveryPlannerToolName.GetValueReferences,
                (t, ct) => t.GetValueReferencesAsync(input.RecoveryCaseId, ct), used, cancellationToken);
            if (!references.IsSuccess) return await Fail(references.Error!);

            var alternatives = new List<RecoveryPlannerAlternative>();
            foreach (var route in input.PreferredRoutes)
            {
                var reference = references.Value!.Where(x => x.Currency == input.Currency && x.ObservedAt <= time.GetUtcNow())
                    .OrderByDescending(x => x.ObservedAt).FirstOrDefault();
                if (reference is null) { warnings.Add($"{route}: verified value reference unavailable."); continue; }
                var valuationInput = new ValueEstimationInput(reference.ValueLow, reference.ValueHigh, 0, 0,
                    input.Currency, route, reference.SourceName, reference.ObservedAt, Array.Empty<string>(), Array.Empty<string>());
                var valuation = await Call(invoker, RecoveryPlannerToolName.CalculateRecoveryValue,
                    (t, ct) => t.CalculateRecoveryValueAsync(valuationInput, ct), used, cancellationToken);
                if (!valuation.IsSuccess) { warnings.Add($"{route}: {valuation.Error!.Code}"); continue; }
                alternatives.Add(new(route, new[] { reference.ReferenceId }, valuation.Value!, null, null));
            }

            if (alternatives.Count == 0)
                return await Fail(new("no_feasible_alternatives", "No feasible Recovery alternatives were produced.", false));
            var saved = await Call(invoker, RecoveryPlannerToolName.SaveRecoveryOptions,
                (t, ct) => t.SaveRecoveryOptionsAsync(runId, alternatives.Select(x => new RecoveryPlannerOptionDraft(input.RecoveryCaseId,
                    x.Route, x.Valuation.EstimatedNetValue, x.Valuation.Currency, x.ReferenceIds)).ToArray(), ct), used, cancellationToken);
            if (!saved.IsSuccess) return await Fail(saved.Error!);
            var persisted = ValidatePersistedOptions(input.RecoveryCaseId, alternatives, saved.Value!);
            var persistedByRoute = persisted.ToDictionary(item => item.Route);
            foreach (var route in input.PreferredRoutes.Where(route => route != RecoveryRoute.Reuse))
            {
                var alternative = alternatives.SingleOrDefault(item => item.Route == route);
                if (alternative is null) continue;
                var option = persistedByRoute[route];
                var matchResult = await Call(invoker, RecoveryPlannerToolName.RequestRecipientMatches,
                    (t, ct) => t.RequestRecipientMatchesAsync(new(Guid.NewGuid(), input.RecoveryCaseId, caseRevision,
                        option.Id, option.Version, input.Assessment.ItemId, input.Assessment.AssessmentId,
                        input.Assessment.AssessmentVersion, input.Assessment.CategoryId ?? Guid.Empty,
                        input.Assessment.Condition, input.Assessment.Function, route, input.Assessment.ServiceArea, input.Deadline), ct), used, cancellationToken);
                var match = matchResult.Value?.FirstOrDefault(x => x.RecoveryOptionId == option.Id &&
                    x.Eligibility == MatchEligibility.Eligible && x.Response == PartnerResponse.Accepted);
                if (match is null) { alternatives.Remove(alternative); warnings.Add($"{route}: eligible accepted match unavailable."); continue; }
                var pickupResult = await Call(invoker, RecoveryPlannerToolName.RequestPickupFeasibility,
                    (t, ct) => t.RequestPickupFeasibilityAsync(new(Guid.NewGuid(), input.RecoveryCaseId, caseRevision,
                        match.MatchId, match.Version, match.FreshnessToken, input.Assessment.ServiceArea, route,
                        Array.Empty<string>(), input.Deadline, input.MaximumPickupCost, input.Currency), ct), used, cancellationToken);
                var pickup = pickupResult.Value?.FirstOrDefault(x => x.Feasibility == PickupFeasibility.Feasible);
                if (pickup is null) { alternatives.Remove(alternative); warnings.Add($"{route}: feasible pickup unavailable."); continue; }
                alternatives[alternatives.IndexOf(alternative)] = alternative with { Match = match, Pickup = pickup };
            }
            if (alternatives.Count == 0)
                return await Fail(new("no_feasible_alternatives", "No feasible Recovery alternatives were produced.", false));
            var reasoningRequest = CreateReasoningRequest(input, assessment.Value!, alternatives, persistedByRoute);
            GatewayResult<RecoveryReasoningResponse> reasoned;
            try
            {
                reasoned = await reasoning.ReasonAsync(reasoningRequest, cancellationToken).WaitAsync(cancellationToken);
                if (reasoned.Outcome == GatewayOutcome.Success)
                    reasoned = new RecoveryReasoningValidator().Validate(reasoned.Value, reasoningRequest);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (OperationCanceledException)
            {
                reasoned = GatewayResult<RecoveryReasoningResponse>.Failure(GatewayOutcome.Unavailable,
                    "reasoning_timeout", "Recovery reasoning timed out.");
            }
            catch (Exception)
            {
                reasoned = GatewayResult<RecoveryReasoningResponse>.Failure(GatewayOutcome.Unavailable,
                    "reasoning_provider_failure", "Recovery reasoning is unavailable.");
            }

            var recommendation = reasoned.Outcome == GatewayOutcome.Success ? reasoned.Value : null;
            if (recommendation is not null)
            {
                var byId = alternatives.ToDictionary(x => persistedByRoute[x.Route].Id);
                alternatives = recommendation.RankedOptionIds.Select(id => byId[id]).ToList();
            }
            else
            {
                integrationFailures.Add(SafeReasoningFailure(reasoned));
                // Retain the pre-existing deterministic route order. Never synthesize an AI response.
                alternatives = alternatives.Where(x => !x.Valuation.IsReferenceStale).ToList();
                warnings.Add("Recovery reasoning unavailable or invalid; using existing deterministic planning with human approval.");
            }
            var reasoningRecorded = await RecordRecommendationOutcomeAsync(runId, reasoned,
                alternatives.Select(x => x.Route).ToArray(), cancellationToken);
            if (!reasoningRecorded.IsSuccess) return await Fail(reasoningRecorded.Error!);
            if (alternatives.Count == 0)
                return await Fail(new("IntegrationUnavailable", "Reasoning is unavailable and no valid deterministic fallback exists.", true));
            var proposal = await Call(invoker, RecoveryPlannerToolName.CreateProposalDraft,
                (t, ct) => t.CreateProposalDraftAsync(new(input.RecoveryCaseId, caseRevision, alternatives,
                    time.GetUtcNow().Add(runtime.ProposalLifetime)) { Recommendation = recommendation }, ct), used, cancellationToken);
            if (!proposal.IsSuccess) return await Fail(proposal.Error!);
            var approval = await Call(invoker, RecoveryPlannerToolName.RequestHumanApproval,
                (t, ct) => t.RequestHumanApprovalAsync(proposal.Value, ct), used, cancellationToken);
            if (!approval.IsSuccess || !approval.Value)
                return await Fail(approval.Error ?? new("approval_unavailable", "Human approval could not be requested.", false));
            if (workflows is not null)
            {
                var waiting = await workflows.MarkWaitingForApprovalAsync(runId, proposal.Value, caseRevision,
                    time.GetUtcNow().Add(runtime.ProposalLifetime), cancellationToken);
                if (!waiting.IsSuccess) return await Fail(waiting.Error!);
            }
            var success = Success(input, runId, started, used, warnings, alternatives, proposal.Value);
            return success with { Recommendation = recommendation,
                Summary = success.Summary with { IntegrationFailures = integrationFailures.ToArray() } };
        }
        catch (RecoveryException ex)
        {
            return await Fail(new(ex.Code, "Recovery planning failed validation.", false));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await Fail(new("workflow_cancelled", "Recovery planning was cancelled.", false));
            throw;
        }
        catch (Exception)
        {
            return await Fail(new("workflow_failed", "Recovery planning failed safely.", false));
        }
    }

    private static RecoveryReasoningRequest CreateReasoningRequest(RecoveryPlannerAgentInput input,
        AssessmentSummary assessment, IReadOnlyList<RecoveryPlannerAlternative> alternatives,
        IReadOnlyDictionary<RecoveryRoute, RecoveryPlannerPersistedOption> persisted) =>
        new(input.RecoveryCaseId, assessment.CategoryId?.ToString() ?? "Unknown",
            new(assessment.Function, assessment.EvidenceReferences.ToArray()), assessment.Condition,
            alternatives.Select(option => new RecoveryReasoningOption(persisted[option.Route].Id, option.Route,
                new(option.Valuation.Inputs.EstimatedProceedsLow, option.Valuation.Inputs.EstimatedProceedsHigh,
                    option.Valuation.EstimatedRepairCost, option.Valuation.EstimatedPickupCost,
                    option.Valuation.EstimatedNetValue, option.Valuation.Currency),
                option.ReferenceIds.Select(id => id.ToString()).ToArray(),
                option.Match is null ? null : new(option.Match.Eligibility.ToString(), option.Match.Response.ToString()),
                option.Pickup is null ? null : new(option.Pickup.Feasibility.ToString(), option.Pickup.EstimatedCost,
                    option.Pickup.Currency))).ToArray(),
            new(input.Objective, input.MaximumPickupCost, input.Currency, input.Deadline));

    private static RecoveryPlannerError SafeReasoningFailure(GatewayResult<RecoveryReasoningResponse> outcome)
    {
        // Provider errors are untrusted too. Only fixed codes and messages enter durable state.
        var code = outcome.Code switch
        {
            "reasoning_timeout" => "reasoning_timeout",
            "reasoning_authentication_failed" => "reasoning_authentication_failed",
            "reasoning_transient_failure" => "reasoning_transient_failure",
            "reasoning_invalid_json" => "reasoning_invalid_json",
            "reasoning_response_too_large" => "reasoning_response_too_large",
            _ => outcome.Outcome == GatewayOutcome.Invalid ? "reasoning_invalid_output" : "IntegrationUnavailable"
        };
        return new(code, "Recovery reasoning failed safely; no model recommendation was accepted.", outcome.Retryable);
    }

    private async Task<RecoveryWorkflowResult<bool>> RecordRecommendationOutcomeAsync(Guid runId,
        GatewayResult<RecoveryReasoningResponse> outcome, IReadOnlyList<RecoveryRoute> orderedRoutes,
        CancellationToken ct)
    {
        if (workflows is null) return RecoveryWorkflowResult<bool>.Success(true);
        var loaded = await workflows.LoadAsync(runId, ct);
        if (!loaded.IsSuccess) return RecoveryWorkflowResult<bool>.Failure("workflow_state_unavailable", "Workflow state could not be loaded.", true);
        var accepted = outcome.Outcome == GatewayOutcome.Success;
        var failure = SafeReasoningFailure(outcome);
        var state = loaded.Value!;
        var saved = await workflows.SaveAsync(state with
        {
            StructuredPlan = orderedRoutes,
            CurrentStep = accepted ? "RecommendationValidated" : "DeterministicFallback",
            ValidationResults = state.ValidationResults.Append(new("Recommendation", accepted,
                accepted ? null : failure.Code, accepted ? null : "Reasoning failed; existing deterministic fallback evaluated.", time.GetUtcNow())).ToArray(),
            ErrorSummaries = accepted ? state.ErrorSummaries : state.ErrorSummaries.Append(new(failure.Code,
                failure.Message, null, time.GetUtcNow())).ToArray(),
            CompletedSteps = state.CompletedSteps.Append(new(accepted ? "RecommendationValidated" : "DeterministicFallback",
                "Completed", time.GetUtcNow())).ToArray()
        }, state.Version, ct);
        return saved.IsSuccess ? RecoveryWorkflowResult<bool>.Success(true) :
            RecoveryWorkflowResult<bool>.Failure("workflow_state_unavailable", "Recommendation outcome could not be persisted.", true);
    }

    public async Task<RecoveryWorkflowResult<RecoveryWorkflowState>> ResumeAsync(Guid workflowId,
        RecoveryApprovalDecision decision, CancellationToken cancellationToken)
    {
        try
        {
            if (workflows is null || actors is null || commands is null)
                return RecoveryWorkflowResult<RecoveryWorkflowState>.Failure("workflow_resume_unavailable", "Workflow resume infrastructure is not configured.", true);
            var actor = await actors.GetAsync(cancellationToken);
            RecoveryAccess.Human(actor);
            var command = RecoveryCommand.Create(actor, "resume_recovery_workflow",
                $"{workflowId:D}:{decision.ProposalId:D}:{decision.ProposalRevision}:{decision.Decision}", decision);
            var resumedState = await commands.ExecuteAsync(command,
            async token =>
            {
                var loaded = await workflows.LoadAsync(workflowId, token);
                if (!loaded.IsSuccess) throw new RecoveryException(404, loaded.Error!.Code, loaded.Error.Message);
                if (loaded.Value!.ApprovalStatus != RecoveryWorkflowApprovalStatus.WaitingForApproval)
                    throw RecoveryException.Conflict("workflow_not_resumable", "Only workflows waiting for approval can be resumed.");
                if (loaded.Value.CurrentStep is "Completed" or "Failed" or "Cancelled" or "Rejected")
                    throw RecoveryException.Conflict("workflow_not_resumable", "Completed, failed, cancelled, or rejected workflows cannot be resumed.");
                if (loaded.Value.ProposalId != decision.ProposalId || loaded.Value.ProposalRevision != decision.ProposalRevision)
                    throw RecoveryException.Conflict("proposal_revision_mismatch", "Approval targets a different proposal revision.");
                if (loaded.Value.ProposalExpiresAt is { } expiry && time.GetUtcNow() >= expiry)
                    throw RecoveryException.Conflict("proposal_expired", "The proposal has expired.");
            }, async token =>
            {
                var recorded = await workflows.RecordAuthorizedApprovalDecisionAsync(workflowId, decision, token);
                if (!recorded.IsSuccess) throw new RecoveryException(409, recorded.Error!.Code, recorded.Error.Message);
                if (decision.Decision == ProposalDecisionKind.Rejected)
                    return recorded.Value!;
                var resumed = await workflows.ResumeAsync(workflowId, token);
                if (!resumed.IsSuccess) throw new RecoveryException(409, resumed.Error!.Code, resumed.Error.Message);
                return resumed.Value!;
            }, cancellationToken);
            return RecoveryWorkflowResult<RecoveryWorkflowState>.Success(resumedState);
        }
        catch (RecoveryException ex)
        {
            return RecoveryWorkflowResult<RecoveryWorkflowState>.Failure(ex.Code, ex.Message);
        }
    }

    private static IReadOnlyList<RecoveryPlannerPersistedOption> ValidatePersistedOptions(Guid caseId,
        IReadOnlyList<RecoveryPlannerAlternative> alternatives,
        IReadOnlyList<RecoveryPlannerPersistedOption> persisted)
    {
        if (persisted.Count != alternatives.Count || persisted.Any(option => option.Id == Guid.Empty || option.Version < 1))
            throw RecoveryException.Conflict("invalid_persisted_options", "Option persistence did not return valid authoritative options.");
        if (persisted.Select(option => option.Id).Distinct().Count() != persisted.Count ||
            persisted.Select(option => option.Route).Distinct().Count() != persisted.Count)
            throw RecoveryException.Conflict("invalid_persisted_options", "Option persistence returned duplicate options.");
        foreach (var alternative in alternatives)
        {
            var option = persisted.SingleOrDefault(item => item.Route == alternative.Route);
            if (option is null || option.RecoveryCaseId == Guid.Empty || option.RecoveryCaseId != caseId || option.Currency != alternative.Valuation.Currency ||
                option.EstimatedNetValue != alternative.Valuation.EstimatedNetValue)
                throw RecoveryException.Conflict("invalid_persisted_options", "Persisted options do not correspond to the planner drafts.");
        }
        return persisted;
    }

    private async Task<PlannerToolResult<T>> Call<T>(RecoveryPlannerToolInvoker invoker, RecoveryPlannerToolName name,
        Func<IRecoveryPlannerToolset, CancellationToken, Task<PlannerToolResult<T>>> operation,
        List<RecoveryPlannerToolName> used, CancellationToken ct)
    {
        used.Add(name);
        return await invoker.InvokeAsync(name, operation, ct);
    }

    private RecoveryPlannerAgentResult Success(RecoveryPlannerAgentInput input, Guid runId, DateTimeOffset started,
        IReadOnlyList<RecoveryPlannerToolName> used, IReadOnlyList<string> warnings,
        IReadOnlyList<RecoveryPlannerAlternative> alternatives, Guid proposalId) =>
        new(RecoveryPlannerWorkflowState.AwaitingApproval, alternatives, proposalId,
            new(input.RecoveryCaseId, runId, RecoveryPlannerWorkflowState.AwaitingApproval, used, warnings, started, time.GetUtcNow()), null);

    private RecoveryPlannerAgentResult Failure(RecoveryPlannerAgentInput input, Guid runId, DateTimeOffset started,
        IReadOnlyList<RecoveryPlannerToolName> used, IReadOnlyList<string> warnings, RecoveryPlannerError error) =>
        new(RecoveryPlannerWorkflowState.Failed, Array.Empty<RecoveryPlannerAlternative>(), null,
            new(input.RecoveryCaseId, runId, RecoveryPlannerWorkflowState.Failed, used, warnings, started, time.GetUtcNow()), error);
}

public sealed class RecoveryPlannerToolset(IRecoveryPlannerToolset inner, ValueEstimationService valuation) : IRecoveryPlannerToolset
{
    public Task<PlannerToolResult<AssessmentSummary>> GetConfirmedAssessmentAsync(Guid caseId, CancellationToken ct) => inner.GetConfirmedAssessmentAsync(caseId, ct);
    public Task<PlannerToolResult<IReadOnlyList<RecoveryPlannerValueReference>>> GetValueReferencesAsync(Guid caseId, CancellationToken ct) => inner.GetValueReferencesAsync(caseId, ct);
    public Task<PlannerToolResult<ValueEstimationResult>> CalculateRecoveryValueAsync(ValueEstimationInput input, CancellationToken ct)
        => Task.FromResult(PlannerToolResult<ValueEstimationResult>.Success(valuation.Evaluate(input)));
    public Task<PlannerToolResult<IReadOnlyList<MatchSummary>>> RequestRecipientMatchesAsync(MatchRequest request, CancellationToken ct) => inner.RequestRecipientMatchesAsync(request, ct);
    public Task<PlannerToolResult<IReadOnlyList<PickupPlanSummary>>> RequestPickupFeasibilityAsync(PickupPlanningRequest request, CancellationToken ct) => inner.RequestPickupFeasibilityAsync(request, ct);
    public Task<PlannerToolResult<IReadOnlyList<RecoveryPlannerPersistedOption>>> SaveRecoveryOptionsAsync(Guid runId, IReadOnlyList<RecoveryPlannerOptionDraft> options, CancellationToken ct) => inner.SaveRecoveryOptionsAsync(runId, options, ct);
    public Task<PlannerToolResult<Guid>> CreateProposalDraftAsync(RecoveryPlannerProposalDraft draft, CancellationToken ct) => inner.CreateProposalDraftAsync(draft, ct);
    public Task<PlannerToolResult<bool>> RequestHumanApprovalAsync(Guid proposalId, CancellationToken ct) => inner.RequestHumanApprovalAsync(proposalId, ct);
}
