namespace WasteToValue.Api.Modules.Partners.Entities;

public class AcceptanceRule
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public Guid CategoryId { get; set; }
    public RouteType RouteType { get; set; }
    public MinimumCondition MinimumCondition { get; set; }
    public string Restrictions { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Partner Partner { get; set; } = null!;
}
