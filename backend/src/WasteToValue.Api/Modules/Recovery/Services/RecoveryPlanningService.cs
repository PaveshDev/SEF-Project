using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;
using static WasteToValue.Api.Modules.Recovery.Validators.RecoveryRequestValidator;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class RecoveryPlanningService(RecoveryAccess access, IRecoveryRepository repository,
    IRecoveryCommandExecutor commands, IAssessmentGateway assessments, IMatchingGateway matching,
    IPickupPlanningGateway pickupPlanning, IValueEstimationService valuation, TimeProvider time) : IRecoveryPlanningService
{
    public async Task<RecoveryCaseResponse> CreateAsync(CreateRecoveryCaseRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct);
        RecoveryAccess.Human(actor);
        Id(request.ItemId, "ItemId"); Inputs(request.Inputs, time.GetUtcNow());
        var command = RecoveryCommand.Create(actor, "create_case", key, request);
        return await commands.ExecuteAsync(command, async token =>
        {
            var assessment = Require(await assessments.GetCurrentAsync(request.ItemId, token));
            Assessment(assessment, request.ItemId, actor.UserId);
        }, async token =>
        {
            var assessment = Require(await assessments.GetCurrentAsync(request.ItemId, token));
            if (await repository.HasActiveCaseAsync(request.ItemId, null, token))
                throw RecoveryException.Conflict("active_case_exists", "An active recovery case already exists for this item.");
            var value = RecoveryCase.Create(actor.UserId, request.ItemId, assessment, request.Inputs, time.GetUtcNow());
            repository.Add(value); await repository.SaveAsync(token);
            return RecoveryCaseResponse.From(value);
        }, ct);
    }

    public async Task<PagedResponse<RecoveryCaseResponse>> ListAsync(RecoveryCaseQuery query, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct);
        ValidateQuery(query.Page, query.PageSize, query.SortDirection);
        var result = await repository.ListCasesAsync(actor.UserId, query, ct);
        return new(result.Items.Select(RecoveryCaseResponse.From).ToArray(), result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<RecoveryCaseResponse> GetAsync(Guid id, CancellationToken ct)
        => RecoveryCaseResponse.From(await access.OwnCaseAsync(id, await access.ActorAsync(ct), ct));

    public async Task<RecoveryCaseResponse> UpdateInputsAsync(Guid id, UpdateRecoveryInputsRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Human(actor);
        var command = RecoveryCommand.Create(actor, "update_inputs", key, new { id, request });
        return await commands.ExecuteAsync(command, async token => { await access.OwnCaseAsync(id, actor, token); },
            async token =>
            {
                var value = await access.OwnCaseAsync(id, actor, token);
                value.UpdateInputs(request.ExpectedVersion, request.Inputs,
                    Require(await assessments.GetCurrentAsync(value.ItemId, token)), time.GetUtcNow());
                foreach (var option in await repository.ListOptionsAsync(id, token)) option.MarkStale(time.GetUtcNow());
                await repository.SaveAsync(token);
                return RecoveryCaseResponse.From(value);
            }, ct);
    }

    public async Task<PlanningResponse> PlanAsync(Guid id, StartPlanningRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Human(actor);
        var command = RecoveryCommand.Create(actor, "plan", key, new { id, request });
        return await commands.ExecuteAsync(command, async token => { await access.OwnCaseAsync(id, actor, token); },
            async token =>
            {
                var value = await access.OwnCaseAsync(id, actor, token);
                value.RequireVersion(request.ExpectedVersion);
                var assessment = Require(await assessments.GetCurrentAsync(value.ItemId, token));
                value.RequireAssessment(assessment);
                if (await repository.HasActiveCaseAsync(value.ItemId, id, token))
                    throw RecoveryException.Conflict("active_case_exists", "Another active case prevents retrying this case.");
                value.StartPlanning(request.ExpectedVersion, time.GetUtcNow());
                foreach (var old in await repository.ListOptionsAsync(id, token)) old.MarkStale(time.GetUtcNow());
                var references = (await repository.ListReferencesAsync(new ValueReferenceQuery(null, null, null, value.Currency), false, token)).Items;
                var options = new List<RecoveryOption>();
                var missing = new List<string>();
                foreach (var route in value.PreferredRoutes)
                {
                    // Initial workflow supports owner reuse and managed partner transfer.
                    // Repair quotations are not fabricated from gross value references.
                    var transfer = route != RecoveryRoute.Reuse;
                    var option = RecoveryOption.Draft(value, route, transfer, transfer, time.GetUtcNow());
                    repository.Add(option); await repository.SaveAsync(token); // Obtain PostgreSQL-generated option ID.
                    options.Add(option);
                    var reference = references.Where(r => r.IsVerified && r.CategoryId == assessment.CategoryId &&
                        r.Condition == assessment.Condition && r.Route == route && r.Currency == value.Currency && r.ObservedAt <= time.GetUtcNow())
                        .OrderByDescending(r => r.ObservedAt).ThenBy(r => r.Id).FirstOrDefault();
                    if (reference is null) { missing.Add($"{route}: verified value reference unavailable."); continue; }
                    if (route == RecoveryRoute.RepairThenReuse) { missing.Add($"{route}: verified repair quotation integration is unavailable."); continue; }
                    MatchSummary? match = null; PickupPlanSummary? pickup = null;
                    if (transfer)
                    {
                        if (assessment.CategoryId is not { } category) { missing.Add($"{route}: category unavailable."); continue; }
                        var result = await matching.FindMatchesAsync(new(command.ChildOperation($"{route}:match"), id,
                            value.Revision, option.Id, option.Version, value.ItemId, value.AssessmentId,
                            value.AssessmentVersion, category, assessment.Condition, assessment.Function,
                            route, assessment.ServiceArea, value.Deadline), token);
                        if (result.Outcome != GatewayOutcome.Success) { missing.Add($"{route}: {result.Code}"); continue; }
                        match = result.Value!.Where(m => m.RecoveryOptionId == option.Id &&
                            m.Eligibility == MatchEligibility.Eligible && m.Response == PartnerResponse.Accepted)
                            .OrderBy(m => m.MatchId).FirstOrDefault();
                        if (match is null) { missing.Add($"{route}: accepted eligible match unavailable."); continue; }
                        var pickupResult = await pickupPlanning.PlanAsync(new(command.ChildOperation($"{route}:pickup"),
                            id, value.Revision, match.MatchId, match.Version, match.FreshnessToken, assessment.ServiceArea,
                            route, Array.Empty<string>(), value.Deadline, value.MaximumPickupCost, value.Currency), token);
                        if (pickupResult.Outcome != GatewayOutcome.Success) { missing.Add($"{route}: {pickupResult.Code}"); continue; }
                        pickup = pickupResult.Value!;
                        try { RecoveryProposalValidator.Dependencies(value, option, match, pickup, time.GetUtcNow()); }
                        catch (RecoveryException ex) when (ex.Status is 400 or 409) { missing.Add($"{route}: {ex.Code}"); continue; }
                    }
                    option.RecordIntegration(match, pickup);
                    option.ValidateEstimate(valuation.Calculate(reference.ValueLow, reference.ValueHigh, 0,
                        pickup?.EstimatedCost ?? 0, value.Currency, pickup?.Currency ?? value.Currency),
                        new[] { reference.Snapshot() }, time.GetUtcNow());
                }
                if (!options.Any(o => o.Status == RecoveryOptionStatus.Validated)) value.AwaitInputs(time.GetUtcNow());
                await repository.SaveAsync(token);
                return new PlanningResponse(RecoveryCaseResponse.From(value),
                    options.Select(RecoveryOptionResponse.From).ToArray(), missing);
            }, ct);
    }

    public async Task<IReadOnlyList<RecoveryOptionResponse>> OptionsAsync(Guid id, CancellationToken ct)
    {
        var value = await access.OwnCaseAsync(id, await access.ActorAsync(ct), ct);
        return (await repository.ListOptionsAsync(id, ct)).Where(o => o.CaseRevision == value.Revision)
            .Select(RecoveryOptionResponse.From).ToArray();
    }

    public async Task<RecoveryCaseResponse> CancelAsync(Guid id, CancelRecoveryCaseRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Human(actor);
        return await commands.ExecuteAsync(RecoveryCommand.Create(actor, "cancel", key, new { id, request }),
            async token => { await access.OwnCaseAsync(id, actor, token); }, async token =>
            {
                var value = await access.OwnCaseAsync(id, actor, token);
                value.Cancel(request.ExpectedVersion, time.GetUtcNow());
                foreach (var proposal in await repository.ListProposalsAsync(id, token))
                    if (proposal.Status == RecoveryProposalStatus.AwaitingApproval) proposal.MarkStale(time.GetUtcNow());
                await repository.SaveAsync(token);
                return RecoveryCaseResponse.From(value);
            }, ct);
    }

    public Task<PlanningResponse> ReplanAsync(Guid id, StartPlanningRequest request, string key, CancellationToken ct)
        => PlanAsync(id, request, key, ct);

    public async Task DeleteAsync(Guid id, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Human(actor);
        await commands.ExecuteAsync(RecoveryCommand.Create(actor, "delete_case", key, new { id }),
            async token => { await access.OwnCaseAsync(id, actor, token); },
            async token =>
            {
                var value = await access.OwnCaseAsync(id, actor, token);
                if (value.Status is RecoveryCaseStatus.Planning or RecoveryCaseStatus.AwaitingInputs or
                    RecoveryCaseStatus.AwaitingApproval or RecoveryCaseStatus.Approved or RecoveryCaseStatus.Completed or
                    RecoveryCaseStatus.RevisionRequested)
                    throw RecoveryException.Conflict("case_not_deletable", "This recovery case has business history and cannot be deleted.");
                if (await repository.HasProposalsAsync(id, token))
                    throw RecoveryException.Conflict("case_history_exists", "Cases with proposal history cannot be deleted.");
                repository.Remove(value); await repository.SaveAsync(token);
                return true;
            }, ct);
    }

    private static void ValidateQuery(int page, int pageSize, string? direction)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw RecoveryException.Invalid("Page must be positive and page size must be between 1 and 100.");
        if (direction is not null && direction is not ("asc" or "desc")) throw RecoveryException.Invalid("Sort direction must be asc or desc.");
    }
}
