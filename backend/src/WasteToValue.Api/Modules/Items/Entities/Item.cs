using System;
using System.Collections.Generic;

namespace WasteToValue.Api.Modules.Items.Entities;

public class Item
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string LocationArea { get; set; } = string.Empty;
    public ItemStatus Status { get; set; } = ItemStatus.Draft;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public uint Version { get; set; } // xmin for concurrency if Postgres, or just an integer version

    public ICollection<ItemPhoto> Photos { get; set; } = new List<ItemPhoto>();
    public ICollection<ItemConditionAnswer> ConditionAnswers { get; set; } = new List<ItemConditionAnswer>();
    public ICollection<ItemAssessment> Assessments { get; set; } = new List<ItemAssessment>();
    public ICollection<AssessmentClarification> Clarifications { get; set; } = new List<AssessmentClarification>();
}
