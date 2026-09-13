using System;
using System.Collections.Generic;

namespace WasteToValue.Agents.Items.Models
{
    public class AssessmentWorkflowState
    {
        public Guid WorkflowId { get; set; } = Guid.NewGuid();
        public Guid ItemId { get; set; }
        public Guid AssessmentId { get; set; } = Guid.NewGuid();
        public int AssessmentVersion { get; set; }
        public List<string> Steps { get; set; } = new();
        public string CurrentStep { get; set; } = string.Empty;
        public string WorkflowStatus { get; set; } = "Running";
        public List<ToolInvocation> ToolCalls { get; set; } = new();
        public List<string> ValidationResults { get; set; } = new();
        public List<Guid> ClarificationReferences { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public AssessmentDraft? FinalAssessmentDraft { get; set; }
    }

    public class ToolInvocation
    {
        public string ToolName { get; set; } = string.Empty;
        public string InputReference { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime InvokedAt { get; set; } = DateTime.UtcNow;
    }

    public class AssessmentDraft
    {
        public Guid ItemId { get; set; }
        public Guid AssessmentId { get; set; }
        public int AssessmentVersion { get; set; }
        public string? SuggestedCategory { get; set; }
        public string? ConditionGrade { get; set; }
        public string? ConditionSummary { get; set; }
        public string? VisibleObservations { get; set; }
        public string? OwnerReportedFunctionality { get; set; }
        public string? MissingInformation { get; set; }
        public double Confidence { get; set; }
        public List<EvidenceReference> EvidenceReferences { get; set; } = new();
        public string Status { get; set; } = "Draft";
    }

    public class EvidenceReference
    {
        public Guid AssessmentId { get; set; }
        public Guid? PhotoId { get; set; }
        public string Observation { get; set; } = string.Empty;
        public string EvidenceType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
