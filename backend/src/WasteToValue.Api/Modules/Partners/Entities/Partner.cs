namespace WasteToValue.Api.Modules.Partners.Entities;

public class Partner
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PartnerType PartnerType { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public bool IsActive { get; set; } = true;
    public string ServiceArea { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int Capacity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int Version { get; set; }

    public ICollection<PartnerMembership> Memberships { get; set; } = new List<PartnerMembership>();
    public ICollection<AcceptanceRule> AcceptanceRules { get; set; } = new List<AcceptanceRule>();
    public ICollection<RecipientNeed> RecipientNeeds { get; set; } = new List<RecipientNeed>();
}
