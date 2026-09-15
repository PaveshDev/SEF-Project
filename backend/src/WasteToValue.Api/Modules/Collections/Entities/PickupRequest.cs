namespace WasteToValue.Api.Modules.Collections.Entities;

public sealed class PickupRequest
{
    public Guid Id { get; set; }
    public Guid RecoveryProposalId { get; set; }
    public Guid CollectionSlotId { get; set; }
    public Guid? CollectorId { get; set; }
    public Guid OwnerId { get; set; }
    public string PickupAddressEncrypted { get; set; } = string.Empty;
    public DateTimeOffset ScheduledStart { get; set; }
    public DateTimeOffset ScheduledEnd { get; set; }
    public string Status { get; set; } = "CONFIRMED";
    public string? VerificationCode { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int Version { get; set; } = 1;

    public CollectionSlot? CollectionSlot { get; set; }
    public ICollection<PickupEvent> Events { get; set; } = new List<PickupEvent>();
    public ICollection<HandoverProof> HandoverProofs { get; set; } = new List<HandoverProof>();
}
