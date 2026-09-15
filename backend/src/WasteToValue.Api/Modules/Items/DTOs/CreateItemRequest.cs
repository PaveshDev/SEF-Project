using System.ComponentModel.DataAnnotations;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class CreateItemRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@".*[a-zA-Z].*", ErrorMessage = "Title must contain at least one letter.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, MinimumLength = 1)]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "LocationArea is required")]
    [StringLength(100, MinimumLength = 1)]
    public string LocationArea { get; set; } = string.Empty;
}
