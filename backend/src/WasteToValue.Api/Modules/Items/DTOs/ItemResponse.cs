using System;
using System.Collections.Generic;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class ItemResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string LocationArea { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public uint Version { get; set; }

    public IEnumerable<ItemPhotoResponse> Photos { get; set; } = new List<ItemPhotoResponse>();
    public IEnumerable<ItemConditionAnswerResponse> ConditionAnswers { get; set; } = new List<ItemConditionAnswerResponse>();
    public IEnumerable<ItemAssessmentResponse> Assessments { get; set; } = new List<ItemAssessmentResponse>();
}
