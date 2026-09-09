namespace WasteToValue.Api.Modules.Collections.Entities;

public sealed class HandoverProof
{
    public Guid Id { get; set; }
    public Guid PickupRequestId { get; set; }
    public Guid PickupEventId { get; set; }
    public string ProofType { get; set; } = string.Empty;
    public string? StorageKey { get; set; }
    public string? VerificationHash { get; set; }
    public Guid? VerifiedBy { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public PickupRequest? PickupRequest { get; set; }
    public PickupEvent? PickupEvent { get; set; }
}
