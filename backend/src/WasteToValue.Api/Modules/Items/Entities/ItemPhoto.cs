using System;

namespace WasteToValue.Api.Modules.Items.Entities;

public class ItemPhoto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int PhotoOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Item? Item { get; set; }
}
