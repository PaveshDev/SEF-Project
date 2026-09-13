using System;

namespace WasteToValue.Api.Modules.Items.Entities;

public class ItemConditionAnswer
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string QuestionCode { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    
    public DateTime AnsweredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Item? Item { get; set; }
}
