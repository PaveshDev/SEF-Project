using LoopWorth.Domain.Entities;
using LoopWorth.Domain.Enums;

namespace LoopWorth.Application.Common.Interfaces;

public class ItemAssessmentResult
{
    public ConditionLevel ConditionLevel { get; set; }
    public RecoveryRoute RecommendedRoute { get; set; }
    public RecoveryRoute? AlternativeRoute { get; set; }
    public ConfidenceLevel ConfidenceLevel { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public interface IItemAssessmentAgent
{
    Task<ItemAssessmentResult> AssessItemAsync(Item item, CancellationToken cancellationToken = default);
}
