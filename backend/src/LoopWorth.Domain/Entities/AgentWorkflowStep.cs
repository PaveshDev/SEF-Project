namespace LoopWorth.Domain.Entities;

public class AgentWorkflowStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid WorkflowId { get; set; }
    public AgentWorkflow? Workflow { get; set; }
    
    public string AgentName { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    
    public string ExecutionStatus { get; set; } = "NotStarted"; // e.g., Running, Completed, Failed
    public string? InputSummary { get; set; }
    public string? OutputSummary { get; set; }
    public string? ValidationStatus { get; set; } // e.g., Valid, Invalid
    public string? ErrorMessage { get; set; }
    
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
