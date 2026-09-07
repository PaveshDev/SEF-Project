using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class ValueReferenceService(RecoveryAccess access, IRecoveryRepository repository,
    IRecoveryCommandExecutor commands, TimeProvider time)
{
    public async Task<IReadOnlyList<ValueReferenceResponse>> ListAsync(CancellationToken ct)
    {
        var actor = await access.ActorAsync(ct);
        return (await repository.ListReferencesAsync(ct))
            .Where(r => r.IsVerified || actor.CanManageValueReferences).Take(100).Select(ValueReferenceResponse.From).ToArray();
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
