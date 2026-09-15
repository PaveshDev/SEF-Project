namespace WasteToValue.Api.Modules.Collections.DTOs;

public sealed record HandoverProofReadDto(
    Guid Id,
    Guid PickupRequestId,
    Guid PickupEventId,
    string ProofType,
    Guid? VerifiedBy,
    DateTimeOffset? VerifiedAt,
    DateTimeOffset CreatedAt);

public sealed record SubmitHandoverCodeRequest(
    string Code,
    Guid ActorId);

public sealed record SubmitHandoverProofRequest(
    string ProofType,
    string? StorageKey,
    string? VerificationHash,
    Guid ActorId);
