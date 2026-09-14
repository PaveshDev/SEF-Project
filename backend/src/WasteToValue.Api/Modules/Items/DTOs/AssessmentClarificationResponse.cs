using System;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class AssessmentClarificationResponse
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid? AssessmentId { get; set; }
    public string QuestionCode { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
}
