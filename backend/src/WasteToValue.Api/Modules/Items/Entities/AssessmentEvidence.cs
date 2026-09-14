using System;

namespace WasteToValue.Api.Modules.Items.Entities;

public class AssessmentEvidence
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public Guid? PhotoId { get; set; }
    
    public string Observation { get; set; } = string.Empty;
    public string EvidenceType { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }

    public ItemAssessment? Assessment { get; set; }
    public ItemPhoto? Photo { get; set; }
}
