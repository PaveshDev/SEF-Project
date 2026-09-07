using System.Text.Json;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;
using static WasteToValue.Api.Modules.Recovery.Validators.RecoveryRequestValidator;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class ProposalDecisionService(RecoveryAccess access, IRecoveryRepository repository,
    IRecoveryCommandExecutor commands, IAssessmentGateway assessments, IMatchingGateway matching,
    IPickupPlanningGateway pickups, TimeProvider time) : IProposalDecisionService
{
    public async Task<RecoveryProposalResponse> SubmitAsync(Guid caseId, SubmitProposalRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Human(actor);
        return await commands.ExecuteAsync(RecoveryCommand.Create(actor, "submit_proposal", key, new { caseId, request }),
            async token => { await access.OwnCaseAsync(caseId, actor, token); }, async token =>
            {
                var value = await access.OwnCaseAsync(caseId, actor, token);
                value.RequireVersion(request.ExpectedVersion);
                var option = await repository.FindOptionAsync(request.OptionId, token)
                    ?? throw new RecoveryException(404, "option_not_found", "Recovery option was not found.");
                option.RequireVersion(request.OptionVersion);
                var assessment = Require(await assessments.GetCurrentAsync(value.ItemId, token));
                var match = await MatchAsync(request.Match, token);
                var pickup = await PickupAsync(request.Pickup, token);
                await RequireEvidenceAsync(option, token);
                var revisions = await repository.ListProposalsAsync(caseId, token);
                var revision = checked(revisions.Select(p => p.Revision).DefaultIfEmpty(0).Max() + 1);
                var proposal = RecoveryProposal.Submit(value, option, revision, assessment, match, pickup,
                    request.Explanation, request.ExpiresAt, RecommendationOrigin.Human, null, time.GetUtcNow());
                value.Submit(time.GetUtcNow());
                repository.Add(proposal); await repository.SaveAsync(token);
                return RecoveryProposalResponse.From(proposal);
            }, ct);
    }

    public async Task<RecoveryProposalResponse> GetAsync(Guid proposalId, CancellationToken ct)
        => RecoveryProposalResponse.From(await access.OwnProposalAsync(proposalId, await access.ActorAsync(ct), ct));

    public async Task<RecoveryProposalResponse> DecideAsync(Guid proposalId, ProposalDecisionRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Human(actor);
        return await commands.ExecuteAsync(RecoveryCommand.Create(actor, "proposal_decision", key, new { proposalId, request }),
            async token => { await access.OwnProposalAsync(proposalId, actor, token); }, async token =>
            {
                var proposal = await access.OwnProposalAsync(proposalId, actor, token);
                proposal.RequireDecidable(request.ExpectedVersion, request.ProposalRevision, time.GetUtcNow());
                var value = await access.OwnCaseAsync(proposal.RecoveryCaseId, actor, token);
                if (value.Revision != proposal.CaseRevision)
                    throw RecoveryException.Conflict("stale_case", "The case inputs changed.");
                RecoveryOption? option = null;
                if (request.Decision == ProposalDecisionKind.Approved)
                    option = await RevalidateAsync(value, proposal, token);
                var decision = ProposalDecision.Create(proposal, value.OwnerId, actor,
                    request.Decision, request.Comment, key, time.GetUtcNow());
                // Recheck expiry immediately before mutation; long-running gateways may cross it.
                proposal.Decide(request.Decision, request.ExpectedVersion, request.ProposalRevision, time.GetUtcNow());
                value.Decide(request.Decision, time.GetUtcNow());
                if (option is not null) option.Select(time.GetUtcNow());
                repository.Add(decision); await repository.SaveAsync(token);
                return RecoveryProposalResponse.From(proposal);
            }, ct);
    }

    // Explicit command so reads never mutate state. Unavailable providers do not mean stale data.
    public async Task<RecoveryProposalResponse> RefreshAsync(Guid proposalId, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Human(actor);
        return await commands.ExecuteAsync(RecoveryCommand.Create(actor, "refresh_proposal", key, new { proposalId }),
            async token => { await access.OwnProposalAsync(proposalId, actor, token); }, async token =>
            {
                var proposal = await access.OwnProposalAsync(proposalId, actor, token);
                if (proposal.Status != RecoveryProposalStatus.AwaitingApproval)
                    throw RecoveryException.Conflict("invalid_transition", "Only pending proposals can be refreshed.");
                var value = await access.OwnCaseAsync(proposal.RecoveryCaseId, actor, token);
                var stale = time.GetUtcNow() >= proposal.ExpiresAt;
                if (!stale)
                {
                    try { await RevalidateAsync(value, proposal, token); }
                    catch (RecoveryException ex) when (ex.Status is 404 or 409) { stale = true; }
                }
                if (stale || time.GetUtcNow() >= proposal.ExpiresAt)
                {
                    proposal.MarkStale(time.GetUtcNow()); value.RequireRevision(time.GetUtcNow());
                    await repository.SaveAsync(token);
                }
                return RecoveryProposalResponse.From(proposal);
            }, ct);
    }

    private async Task<RecoveryOption> RevalidateAsync(RecoveryCase value, RecoveryProposal proposal, CancellationToken ct)
    {
        if (value.Revision != proposal.CaseRevision || value.Status != RecoveryCaseStatus.AwaitingApproval)
            throw RecoveryException.Conflict("stale_case", "The case is no longer awaiting this proposal.");
        var option = await repository.FindOptionAsync(proposal.RecoveryOptionId, ct)
            ?? throw new RecoveryException(404, "option_not_found", "Recovery option was not found.");
        option.RequireVersion(proposal.OptionVersion);
        if (option.RecoveryCaseId != value.Id || option.CaseRevision != value.Revision ||
            option.Status != RecoveryOptionStatus.Validated)
            throw RecoveryException.Conflict("stale_option", "The selected option changed.");
        value.RequireAssessment(Require(await assessments.GetCurrentAsync(value.ItemId, ct)));
        var match = await MatchAsync(proposal.MatchId is { } matchId ?
            new(matchId, proposal.MatchVersion!.Value, proposal.MatchFreshnessToken!) : null, ct);
        var pickup = await PickupAsync(proposal.PickupPlanId is { } pickupId ?
            new(pickupId, proposal.PickupPlanVersion!.Value, proposal.PickupFreshnessToken!) : null, ct);
        RecoveryProposalValidator.Dependencies(value, option, match, pickup, time.GetUtcNow());
        await RequireEvidenceAsync(option, ct);
        if (JsonSerializer.Deserialize<ValueEstimate>(proposal.EstimateSnapshot) != option.Estimate())
            throw RecoveryException.Conflict("stale_estimate", "The proposal estimate no longer matches the option.");
        return option;
    }

    private async Task RequireEvidenceAsync(RecoveryOption option, CancellationToken ct)
    {
        foreach (var evidence in JsonSerializer.Deserialize<ValueEvidence[]>(option.EvidenceJson)!)
        {
            var reference = await repository.FindReferenceAsync(evidence.ReferenceId, ct);
            if (reference is null || !reference.IsVerified || reference.Version != evidence.Version ||
                reference.Snapshot() != evidence)
                throw RecoveryException.Conflict("stale_evidence", "A value reference changed or is unavailable.");
        }
    }

    private async Task<MatchSummary?> MatchAsync(MatchChoice? choice, CancellationToken ct)
    {
        if (choice is null) return null;
        Id(choice.Id, "MatchId"); Version(choice.Version); Text(choice.FreshnessToken, "MatchFreshnessToken", 500);
        var value = Require(await matching.RevalidateAsync(choice.Id, choice.Version, choice.FreshnessToken, ct));
        RecoveryProposalValidator.MatchIdentity(value, choice.Id, choice.Version, choice.FreshnessToken);
        return value;
    }

    private async Task<PickupPlanSummary?> PickupAsync(PickupChoice? choice, CancellationToken ct)
    {
        if (choice is null) return null;
        Id(choice.Id, "PickupPlanId"); Version(choice.Version); Text(choice.FreshnessToken, "PickupFreshnessToken", 500);
        var value = Require(await pickups.RevalidateAsync(choice.Id, choice.Version, choice.FreshnessToken, ct));
        RecoveryProposalValidator.PickupIdentity(value, choice.Id, choice.Version, choice.FreshnessToken);
        return value;
    }
}
