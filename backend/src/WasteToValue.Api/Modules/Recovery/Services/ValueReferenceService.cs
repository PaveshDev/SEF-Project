using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class ValueReferenceService(RecoveryAccess access, IRecoveryRepository repository,
    IRecoveryCommandExecutor commands, TimeProvider time)
{
    public async Task<PagedResponse<ValueReferenceResponse>> ListAsync(ValueReferenceQuery query, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct);
        ValidateQuery(query.Page, query.PageSize, query.SortDirection);
        var result = await repository.ListReferencesAsync(query, actor.CanManageValueReferences, ct);
        var items = result.Items.Select(ValueReferenceResponse.From).ToArray();
        return new(items, result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<ValueReferenceResponse> GetAsync(Guid id, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct);
        var reference = await repository.FindReferenceAsync(id, ct)
            ?? throw new Validators.RecoveryException(404, "reference_not_found", "Value reference was not found.");
        if (!reference.IsVerified && !actor.CanManageValueReferences)
            throw new Validators.RecoveryException(404, "reference_not_found", "Value reference was not found.");
        return ValueReferenceResponse.From(reference);
    }

    public async Task<ValueReferenceResponse> UpdateAsync(Guid id, UpdateValueReferenceRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Curator(actor);
        return await commands.ExecuteAsync(RecoveryCommand.Create(actor, "update_value_reference", key, new { id, request }),
            token => { token.ThrowIfCancellationRequested(); RecoveryAccess.Curator(actor); return Task.CompletedTask; },
            async token =>
            {
                var reference = await repository.FindReferenceAsync(id, token)
                    ?? throw new Validators.RecoveryException(404, "reference_not_found", "Value reference was not found.");
                reference.Update(request.ExpectedVersion, request, time.GetUtcNow());
                await repository.SaveAsync(token); return ValueReferenceResponse.From(reference);
            }, ct);
    }

    public async Task DeleteAsync(Guid id, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Curator(actor);
        await commands.ExecuteAsync(RecoveryCommand.Create(actor, "delete_value_reference", key, new { id }),
            async token => { await access.ActorAsync(token); RecoveryAccess.Curator(actor); },
            async token =>
            {
                var reference = await repository.FindReferenceAsync(id, token)
                    ?? throw new Validators.RecoveryException(404, "reference_not_found", "Value reference was not found.");
                if (reference.IsVerified) throw RecoveryException.Conflict("verified_reference_immutable", "Verified value references cannot be deleted.");
                repository.Remove(reference); await repository.SaveAsync(token); return true;
            }, ct);
    }

    private static void ValidateQuery(int page, int pageSize, string? direction)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw RecoveryException.Invalid("Page must be positive and page size must be between 1 and 100.");
        if (direction is not null && direction is not ("asc" or "desc")) throw RecoveryException.Invalid("Sort direction must be asc or desc.");
    }

    public async Task<ValueReferenceResponse> CreateAsync(CreateValueReferenceRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Curator(actor);
        return await commands.ExecuteAsync(RecoveryCommand.Create(actor, "create_value_reference", key, request),
            token => { token.ThrowIfCancellationRequested(); RecoveryAccess.Curator(actor); return Task.CompletedTask; },
            async token =>
            {
                var reference = ValueReference.Create(request, actor.UserId, time.GetUtcNow());
                repository.Add(reference); await repository.SaveAsync(token);
                return ValueReferenceResponse.From(reference);
            }, ct);
    }

    public async Task<ValueReferenceResponse> VerifyAsync(Guid id, VerifyValueReferenceRequest request, string key, CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct); RecoveryAccess.Curator(actor);
        return await commands.ExecuteAsync(RecoveryCommand.Create(actor, "verify_value_reference", key, new { id, request }),
            token => { token.ThrowIfCancellationRequested(); RecoveryAccess.Curator(actor); return Task.CompletedTask; },
            async token =>
            {
                var reference = await repository.FindReferenceAsync(id, token)
                    ?? throw new Validators.RecoveryException(404, "reference_not_found", "Value reference was not found.");
                reference.Verify(request.ExpectedVersion, time.GetUtcNow()); await repository.SaveAsync(token);
                return ValueReferenceResponse.From(reference);
            }, ct);
    }
}
