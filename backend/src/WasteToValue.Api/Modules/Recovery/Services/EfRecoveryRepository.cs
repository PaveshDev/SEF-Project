using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Services;

// Context resolution is deferred so missing database settings do not break API startup or identity errors.
public sealed class EfRecoveryRepository(IServiceProvider services, IConfiguration configuration) : IRecoveryRepository
{
    private AppDbContext Context => !string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection"))
        ? services.GetRequiredService<AppDbContext>()
        : throw RecoveryException.Unavailable("database_unavailable", "Recovery database configuration is missing.");

    public Task<RecoveryCase?> FindCaseAsync(Guid id, CancellationToken ct)
        => Context.Set<RecoveryCase>().SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<RecoveryCase>> ListCasesAsync(Guid ownerId, CancellationToken ct)
        => await Context.Set<RecoveryCase>().AsNoTracking().Where(x => x.OwnerId == ownerId)
            .OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
    public Task<bool> HasActiveCaseAsync(Guid itemId, Guid? exceptId, CancellationToken ct)
        => Context.Set<RecoveryCase>().AnyAsync(x => x.ItemId == itemId && x.Id != exceptId &&
            x.Status != RecoveryCaseStatus.Rejected && x.Status != RecoveryCaseStatus.Completed &&
            x.Status != RecoveryCaseStatus.Failed && x.Status != RecoveryCaseStatus.Cancelled, ct);
    public async Task<IReadOnlyList<RecoveryOption>> ListOptionsAsync(Guid caseId, CancellationToken ct)
        => await Context.Set<RecoveryOption>().Where(x => x.RecoveryCaseId == caseId).OrderBy(x => x.CreatedAt).ToListAsync(ct);
    public Task<RecoveryOption?> FindOptionAsync(Guid id, CancellationToken ct)
        => Context.Set<RecoveryOption>().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<RecoveryProposal?> FindProposalAsync(Guid id, CancellationToken ct)
        => Context.Set<RecoveryProposal>().SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<RecoveryProposal>> ListProposalsAsync(Guid caseId, CancellationToken ct)
        => await Context.Set<RecoveryProposal>().Where(x => x.RecoveryCaseId == caseId).ToListAsync(ct);
    public async Task<IReadOnlyList<ValueReference>> ListReferencesAsync(CancellationToken ct)
        => await Context.Set<ValueReference>().OrderByDescending(x => x.ObservedAt).ToListAsync(ct);
    public Task<ValueReference?> FindReferenceAsync(Guid id, CancellationToken ct)
        => Context.Set<ValueReference>().SingleOrDefaultAsync(x => x.Id == id, ct);
    public void Add(RecoveryCase value) => Context.Add(value);
    public void Add(RecoveryOption value) => Context.Add(value);
    public void Add(RecoveryProposal value) => Context.Add(value);
    public void Add(ProposalDecision value) => Context.Add(value);
    public void Add(ValueReference value) => Context.Add(value);
    public async Task SaveAsync(CancellationToken ct) => await Context.SaveChangesAsync(ct);
}
