using System;
using System.ComponentModel.DataAnnotations;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class AddAssessmentEvidenceRequest
{
    [Required(ErrorMessage = "AssessmentId is required")]
    public Guid AssessmentId { get; set; }

    [Required(ErrorMessage = "PhotoId is required")]
    public Guid PhotoId { get; set; }

    [Required(ErrorMessage = "Observation is required")]
    [StringLength(1000, MinimumLength = 1)]
    public string Observation { get; set; } = string.Empty;

    [Required(ErrorMessage = "EvidenceType is required")]
    [StringLength(100, MinimumLength = 1)]
    public string EvidenceType { get; set; } = string.Empty;
}
