using System;
using System.ComponentModel.DataAnnotations;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class AddPhotoRequest
{
    [Required(ErrorMessage = "ItemId is required")]
    public Guid ItemId { get; set; }

    [Required(ErrorMessage = "ImageUrl is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "PhotoOrder must be between 0 and 100")]
    public int PhotoOrder { get; set; }
}
