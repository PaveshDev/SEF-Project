using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WasteToValue.Api.Modules.Items.DTOs;

public class SubmitConditionAnswersRequest
{
    [Required(ErrorMessage = "ItemId is required")]
    public Guid ItemId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one answer must be provided")]
    public ICollection<ConditionAnswerDto> Answers { get; set; } = new List<ConditionAnswerDto>();
}

public class ConditionAnswerDto
{
    [Required(ErrorMessage = "QuestionCode is required")]
    public string QuestionCode { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Answer is required")]
    public string Answer { get; set; } = string.Empty;
}
