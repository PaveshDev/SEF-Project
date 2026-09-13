using System;
using System.Collections.Generic;

namespace WasteToValue.Api.Modules.Items.Entities;

public class ItemAssessment
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    
    public int Version { get; set; } // Assessment Version must be unique per Item.
    
    public string? SuggestedCategory { get; set; }
    public string? ConditionGrade { get; set; }
    public string? ConditionSummary { get; set; }
    public string? VisibleObservations { get; set; }
    public string? OwnerReportedFunctionality { get; set; }
    public string? MissingInformation { get; set; }
    
    public double Confidence { get; set; } // Confidence must be between 0 and 1.
    
    public AssessmentStatus Status { get; set; } = AssessmentStatus.Draft;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Item? Item { get; set; }
    public ICollection<AssessmentEvidence> Evidences { get; set; } = new List<AssessmentEvidence>();
    public ICollection<AssessmentClarification> Clarifications { get; set; } = new List<AssessmentClarification>();
}
