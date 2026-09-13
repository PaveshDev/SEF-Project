using System;
using System.ComponentModel.DataAnnotations;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class ConfirmAssessmentRequest
{
    [Required(ErrorMessage = "AssessmentId is required")]
    public Guid AssessmentId { get; set; }

    [Required(ErrorMessage = "Owner confirmation is required")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must explicitly confirm the assessment")]
    public bool OwnerConfirmation { get; set; }
}
