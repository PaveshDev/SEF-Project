using System;
using System.Collections.Generic;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class ItemAssessmentResponse
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public int Version { get; set; }
    public string? SuggestedCategory { get; set; }
    public string? ConditionGrade { get; set; }
    public string? ConditionSummary { get; set; }
    public string? VisibleObservations { get; set; }
    public string? OwnerReportedFunctionality { get; set; }
    public string? MissingInformation { get; set; }
    public double Confidence { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public IEnumerable<AssessmentEvidenceResponse> Evidences { get; set; } = new List<AssessmentEvidenceResponse>();
    public IEnumerable<AssessmentClarificationResponse> Clarifications { get; set; } = new List<AssessmentClarificationResponse>();
}
