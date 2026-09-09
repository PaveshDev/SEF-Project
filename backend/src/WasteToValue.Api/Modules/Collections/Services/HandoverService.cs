using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Interfaces;
using WasteToValue.Api.Modules.Collections.Validators;

namespace WasteToValue.Api.Modules.Collections.Services;

/// <summary>
/// In-memory demo implementation of <see cref="IHandoverService"/>.
/// Validates the one-time code format and records a demo handover proof.
/// </summary>
public sealed class HandoverService : IHandoverService
{
    private readonly List<HandoverProofReadDto> _proofs = new();

    public Task<HandoverProofReadDto?> VerifyCodeAsync(Guid pickupRequestId, SubmitHandoverCodeRequest request, CancellationToken ct = default)
    {
        var errors = HandoverCodeValidator.Validate(request.Code);
        if (errors.Count > 0)
            return Task.FromResult<HandoverProofReadDto?>(null);

        var now = DateTimeOffset.UtcNow;
        var proof = new HandoverProofReadDto(
            Guid.NewGuid(), pickupRequestId, Guid.NewGuid(),
            "ONE_TIME_CODE", request.ActorId, now, now);
        _proofs.Add(proof);
        return Task.FromResult<HandoverProofReadDto?>(proof);
    }

    public Task<HandoverProofReadDto?> SubmitProofAsync(Guid pickupRequestId, SubmitHandoverProofRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var proof = new HandoverProofReadDto(
            Guid.NewGuid(), pickupRequestId, Guid.NewGuid(),
            request.ProofType, request.ActorId, now, now);
        _proofs.Add(proof);
        return Task.FromResult<HandoverProofReadDto?>(proof);
    }

    public Task<IReadOnlyList<HandoverProofReadDto>> GetProofsAsync(Guid pickupRequestId, CancellationToken ct = default)
    {
        var result = _proofs.Where(p => p.PickupRequestId == pickupRequestId).ToList();
        return Task.FromResult<IReadOnlyList<HandoverProofReadDto>>(result.AsReadOnly());
    }
}
