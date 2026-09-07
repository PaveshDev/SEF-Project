using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.DTOs;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

// Only Recovery-owned entities appear here.
public interface IRecoveryRepository
{
    Task<RecoveryCase?> FindCaseAsync(Guid id, CancellationToken ct);
    Task<PagedResult<RecoveryCase>> ListCasesAsync(Guid ownerId, RecoveryCaseQuery query, CancellationToken ct);
    Task<bool> HasActiveCaseAsync(Guid itemId, Guid? exceptId, CancellationToken ct);
    Task<IReadOnlyList<RecoveryOption>> ListOptionsAsync(Guid caseId, CancellationToken ct);
    Task<RecoveryOption?> FindOptionAsync(Guid id, CancellationToken ct);
    Task<RecoveryProposal?> FindProposalAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<RecoveryProposal>> ListProposalsAsync(Guid caseId, CancellationToken ct);
    Task<PagedResult<ValueReference>> ListReferencesAsync(ValueReferenceQuery query, bool includeUnverified, CancellationToken ct);
    Task<ValueReference?> FindReferenceAsync(Guid id, CancellationToken ct);
    void Add(RecoveryCase value);
    void Add(RecoveryOption value);
    void Add(RecoveryProposal value);
    void Add(ProposalDecision value);
    void Add(ValueReference value);
    void Remove(RecoveryCase value);
    void Remove(ValueReference value);
    Task<bool> HasProposalsAsync(Guid caseId, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
