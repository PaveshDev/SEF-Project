using LoopWorth.Domain.Enums;

namespace LoopWorth.Domain.Entities;

public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty; // References authenticated user ID

    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public string? Brand { get; set; }
    public string? Model { get; set; }
    
    public string? ConditionDescription { get; set; }

    public ItemStatus Status { get; set; } = ItemStatus.Draft;
    
    public RecoveryRoute? SelectedRecoveryRoute { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
    
    public ICollection<ItemAssessment> Assessments { get; set; } = new List<ItemAssessment>();
    
    // An item can have multiple workflows (e.g. Assessment workflow, then Recovery workflow)
    public ICollection<AgentWorkflow> Workflows { get; set; } = new List<AgentWorkflow>();
}
