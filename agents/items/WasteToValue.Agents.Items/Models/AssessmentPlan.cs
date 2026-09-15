using System;
using System.Collections.Generic;

namespace WasteToValue.Agents.Items.Models
{
    public class AssessmentPlan
    {
        public Guid WorkflowId { get; set; }
        public Guid ItemId { get; set; }
        public int AssessmentVersion { get; set; }
        public List<string> Steps { get; set; } = new();
        public string CurrentStep { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class AssessmentSummary
    {
        public Guid ItemId { get; set; }
        public Guid AssessmentId { get; set; }
        public int AssessmentVersion { get; set; }
        public string? Category { get; set; }
        public string? ConditionGrade { get; set; }
        public string? ConditionSummary { get; set; }
        public string? Functionality { get; set; }
        public double Confidence { get; set; }
        public bool Confirmed { get; set; }
    }
}
