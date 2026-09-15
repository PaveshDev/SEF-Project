namespace WasteToValue.Api.Modules.Partners.Entities;

public class PartnerMembership
{
    public Guid PartnerId { get; set; }
    public Guid UserId { get; set; }
    public MembershipRole MembershipRole { get; set; } = MembershipRole.Representative;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public Partner Partner { get; set; } = null!;
}
