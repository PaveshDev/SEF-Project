using System;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class ItemPhotoResponse
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int PhotoOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
