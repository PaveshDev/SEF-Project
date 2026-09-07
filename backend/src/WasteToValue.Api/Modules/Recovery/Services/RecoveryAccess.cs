using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class RecoveryAccess(IRecoveryActorAccessor actors, IRecoveryRepository repository)
{
    public async Task<RecoveryActor> ActorAsync(CancellationToken ct)
    {
        var actor = await actors.GetAsync(ct);
        if (actor.UserId == Guid.Empty) throw new RecoveryException(401, "authentication_required", "A verified actor is required.");
        return actor;
    }

    public async Task<RecoveryCase> OwnCaseAsync(Guid id, RecoveryActor actor, CancellationToken ct)
    {
        RecoveryRequestValidator.Id(id, "CaseId");
        var value = await repository.FindCaseAsync(id, ct);
        if (value is null || value.OwnerId != actor.UserId)
            throw new RecoveryException(404, "case_not_found", "Recovery case was not found.");
        return value;
    }

    public async Task<RecoveryProposal> OwnProposalAsync(Guid id, RecoveryActor actor, CancellationToken ct)
    {
        RecoveryRequestValidator.Id(id, "ProposalId");
        var proposal = await repository.FindProposalAsync(id, ct)
            ?? throw new RecoveryException(404, "proposal_not_found", "Recovery proposal was not found.");
        await OwnCaseAsync(proposal.RecoveryCaseId, actor, ct);
        return proposal;
    }

    public static void Human(RecoveryActor actor)
    {
        if (!actor.IsHuman) throw new RecoveryException(403, "human_required", "This operation requires a human actor.");
    }

    public static void Curator(RecoveryActor actor)
    {
        Human(actor);
        if (!actor.CanManageValueReferences) throw new RecoveryException(403, "curator_required", "Verified value-reference curator permission is required.");
    }
}
