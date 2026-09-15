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
    public async Task<PagedResult<RecoveryCase>> ListCasesAsync(Guid ownerId, RecoveryCaseQuery query, CancellationToken ct)
    {
        var source = Context.Set<RecoveryCase>().AsNoTracking().Where(x => x.OwnerId == ownerId);
        if (query.Status is { } status) source = source.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(query.Search)) source = source.Where(x => x.Objective.Contains(query.Search));
        var total = await source.CountAsync(ct);
        var ordered = query.SortBy?.ToLowerInvariant() switch
        {
            "status" => query.SortDirection == "asc" ? source.OrderBy(x => x.Status) : source.OrderByDescending(x => x.Status),
            "updatedat" => query.SortDirection == "asc" ? source.OrderBy(x => x.UpdatedAt) : source.OrderByDescending(x => x.UpdatedAt),
            _ => query.SortDirection == "asc" ? source.OrderBy(x => x.CreatedAt) : source.OrderByDescending(x => x.CreatedAt)
        };
        var items = await ordered.ThenBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }
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
    public async Task<PagedResult<ValueReference>> ListReferencesAsync(ValueReferenceQuery query, bool includeUnverified, CancellationToken ct)
    {
        var source = Context.Set<ValueReference>().AsNoTracking();
        if (!includeUnverified) source = source.Where(x => x.IsVerified);
        if (query.CategoryId is { } category) source = source.Where(x => x.CategoryId == category);
        if (query.ObservedAfter is { } after) source = source.Where(x => x.ObservedAt >= after);
        if (query.ObservedBefore is { } before) source = source.Where(x => x.ObservedAt <= before);
        if (query.Condition is { } condition) source = source.Where(x => x.Condition == condition);
        if (query.Route is { } route) source = source.Where(x => x.Route == route);
        if (!string.IsNullOrWhiteSpace(query.Currency)) source = source.Where(x => x.Currency == query.Currency);
        if (!string.IsNullOrWhiteSpace(query.Search)) source = source.Where(x => x.SourceName.Contains(query.Search) || (x.SourceReference != null && x.SourceReference.Contains(query.Search)));
        var total = await source.CountAsync(ct);
        var ordered = query.SortBy?.ToLowerInvariant() switch
        {
            "sourcename" => query.SortDirection == "asc" ? source.OrderBy(x => x.SourceName) : source.OrderByDescending(x => x.SourceName),
            _ => query.SortDirection == "asc" ? source.OrderBy(x => x.ObservedAt) : source.OrderByDescending(x => x.ObservedAt)
        };
        var items = await ordered.ThenBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }
    public Task<ValueReference?> FindReferenceAsync(Guid id, CancellationToken ct)
        => Context.Set<ValueReference>().SingleOrDefaultAsync(x => x.Id == id, ct);
    public void Add(RecoveryCase value) => Context.Add(value);
    public void Add(RecoveryOption value) => Context.Add(value);
    public void Add(RecoveryProposal value) => Context.Add(value);
    public void Add(ProposalDecision value) => Context.Add(value);
    public void Add(ValueReference value) => Context.Add(value);
    public void Remove(RecoveryCase value) => Context.Remove(value);
    public void Remove(ValueReference value) => Context.Remove(value);
    public Task<bool> HasProposalsAsync(Guid caseId, CancellationToken ct)
        => Context.Set<RecoveryProposal>().AnyAsync(x => x.RecoveryCaseId == caseId, ct);
    public async Task SaveAsync(CancellationToken ct) => await Context.SaveChangesAsync(ct);
}
