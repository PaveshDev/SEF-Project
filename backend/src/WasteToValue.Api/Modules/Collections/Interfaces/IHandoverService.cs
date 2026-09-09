using WasteToValue.Api.Modules.Collections.DTOs;

namespace WasteToValue.Api.Modules.Collections.Interfaces;

public interface IHandoverService
{
    Task<HandoverProofReadDto?> VerifyCodeAsync(Guid pickupRequestId, SubmitHandoverCodeRequest request, CancellationToken ct = default);
    Task<HandoverProofReadDto?> SubmitProofAsync(Guid pickupRequestId, SubmitHandoverProofRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<HandoverProofReadDto>> GetProofsAsync(Guid pickupRequestId, CancellationToken ct = default);
}
