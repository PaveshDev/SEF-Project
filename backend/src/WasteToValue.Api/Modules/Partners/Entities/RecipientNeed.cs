namespace WasteToValue.Api.Modules.Partners.Entities;

public class RecipientNeed
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int QuantityRequired { get; set; }
    public int QuantityFulfilled { get; set; }
    public DateTimeOffset? Deadline { get; set; }
    public NeedStatus Status { get; set; } = NeedStatus.Open;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int Version { get; set; }

    public Partner Partner { get; set; } = null!;
}
