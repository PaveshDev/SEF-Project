using System;
using System.ComponentModel.DataAnnotations;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class AnswerClarificationRequest
{
    [Required(ErrorMessage = "ClarificationId is required")]
    public Guid ClarificationId { get; set; }

    [Required(ErrorMessage = "Answer is required")]
    public string Answer { get; set; } = string.Empty;
}
