using System;
using System.ComponentModel.DataAnnotations;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class RequestReassessmentRequest
{
    [Required(ErrorMessage = "AssessmentId is required")]
    public Guid AssessmentId { get; set; }

    [Required(ErrorMessage = "Reason is required")]
    public string Reason { get; set; } = string.Empty;
}
