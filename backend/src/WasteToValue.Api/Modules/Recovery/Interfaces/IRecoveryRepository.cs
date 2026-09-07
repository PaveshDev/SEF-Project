using WasteToValue.Api.Modules.Recovery.Entities;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

// Only Recovery-owned entities appear here.
public interface IRecoveryRepository
{
    Task<RecoveryCase?> FindCaseAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<RecoveryCase>> ListCasesAsync(Guid ownerId, CancellationToken ct);
    Task<bool> HasActiveCaseAsync(Guid itemId, Guid? exceptId, CancellationToken ct);
    Task<IReadOnlyList<RecoveryOption>> ListOptionsAsync(Guid caseId, CancellationToken ct);
    Task<RecoveryOption?> FindOptionAsync(Guid id, CancellationToken ct);
    Task<RecoveryProposal?> FindProposalAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<RecoveryProposal>> ListProposalsAsync(Guid caseId, CancellationToken ct);
    Task<IReadOnlyList<ValueReference>> ListReferencesAsync(CancellationToken ct);
    Task<ValueReference?> FindReferenceAsync(Guid id, CancellationToken ct);
    void Add(RecoveryCase value);
    void Add(RecoveryOption value);
    void Add(RecoveryProposal value);
    void Add(ProposalDecision value);
    void Add(ValueReference value);
    Task SaveAsync(CancellationToken ct);
}
