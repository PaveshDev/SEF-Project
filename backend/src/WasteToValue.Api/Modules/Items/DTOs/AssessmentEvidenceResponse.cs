using System;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class AssessmentEvidenceResponse
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public Guid? PhotoId { get; set; }
    public string Observation { get; set; } = string.Empty;
    public string EvidenceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
