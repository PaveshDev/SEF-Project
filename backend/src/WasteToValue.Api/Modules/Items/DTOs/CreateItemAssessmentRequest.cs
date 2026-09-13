using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class CreateItemAssessmentRequest : IValidatableObject
{
    [Required(ErrorMessage = "ItemId is required")]
    public Guid ItemId { get; set; }

    public string? SuggestedCategory { get; set; }
    public string? ConditionGrade { get; set; }
    public string? ConditionSummary { get; set; }
    public string? VisibleObservations { get; set; }
    public string? OwnerReportedFunctionality { get; set; }
    public string? MissingInformation { get; set; }

    [Range(0.0, 1.0, ErrorMessage = "Confidence must be between 0 and 1")]
    public double Confidence { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public AssessmentStatus Status { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Required fields based on assessment status
        // Do not require fields that are legitimately nullable during Draft/AwaitingInformation
        if (Status != AssessmentStatus.Draft && Status != AssessmentStatus.AwaitingInformation)
        {
            if (string.IsNullOrWhiteSpace(SuggestedCategory))
                yield return new ValidationResult("SuggestedCategory is required when status is beyond Draft/AwaitingInformation.", new[] { nameof(SuggestedCategory) });

            if (string.IsNullOrWhiteSpace(ConditionGrade))
                yield return new ValidationResult("ConditionGrade is required when status is beyond Draft/AwaitingInformation.", new[] { nameof(ConditionGrade) });

            if (string.IsNullOrWhiteSpace(ConditionSummary))
                yield return new ValidationResult("ConditionSummary is required when status is beyond Draft/AwaitingInformation.", new[] { nameof(ConditionSummary) });
        }
    }
}
