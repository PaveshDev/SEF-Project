using System;
using System.ComponentModel.DataAnnotations;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class CreateClarificationRequest
{
    [Required(ErrorMessage = "ItemId is required")]
    public Guid ItemId { get; set; }

    public Guid? AssessmentId { get; set; }

    [Required(ErrorMessage = "QuestionCode is required")]
    public string QuestionCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Question is required")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Reason is required")]
    public string Reason { get; set; } = string.Empty;
}
