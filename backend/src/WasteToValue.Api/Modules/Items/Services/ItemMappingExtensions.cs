using System.Linq;
using WasteToValue.Api.Modules.Items.DTOs;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Api.Modules.Items.Services;

public static class ItemMappingExtensions
{
    public static ItemResponse ToResponse(this Item item)
    {
        return new ItemResponse
        {
            Id = item.Id,
            OwnerId = item.OwnerId,
            Title = item.Title,
            Description = item.Description,
            Category = item.Category,
            LocationArea = item.LocationArea,
            Status = item.Status.ToString(),
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            Version = item.Version,
            Photos = item.Photos.Select(p => p.ToResponse()).ToList(),
            ConditionAnswers = item.ConditionAnswers.Select(c => c.ToResponse()).ToList(),
            Assessments = item.Assessments.Select(a => a.ToResponse()).ToList()
        };
    }

    public static ItemSummaryResponse ToSummaryResponse(this Item item)
    {
        return new ItemSummaryResponse
        {
            Id = item.Id,
            Title = item.Title,
            Category = item.Category,
            LocationArea = item.LocationArea,
            Status = item.Status.ToString(),
            CreatedAt = item.CreatedAt
        };
    }

    public static ItemPhotoResponse ToResponse(this ItemPhoto photo)
    {
        return new ItemPhotoResponse
        {
            Id = photo.Id,
            ItemId = photo.ItemId,
            ImageUrl = photo.ImageUrl,
            PhotoOrder = photo.PhotoOrder,
            CreatedAt = photo.CreatedAt
        };
    }

    public static ItemConditionAnswerResponse ToResponse(this ItemConditionAnswer answer)
    {
        return new ItemConditionAnswerResponse
        {
            Id = answer.Id,
            ItemId = answer.ItemId,
            QuestionCode = answer.QuestionCode,
            QuestionText = answer.QuestionText,
            Answer = answer.Answer,
            AnsweredAt = answer.AnsweredAt,
            CreatedAt = answer.CreatedAt,
            UpdatedAt = answer.UpdatedAt
        };
    }

    public static ItemAssessmentResponse ToResponse(this ItemAssessment assessment)
    {
        return new ItemAssessmentResponse
        {
            Id = assessment.Id,
            ItemId = assessment.ItemId,
            Version = assessment.Version,
            SuggestedCategory = assessment.SuggestedCategory,
            ConditionGrade = assessment.ConditionGrade,
            ConditionSummary = assessment.ConditionSummary,
            VisibleObservations = assessment.VisibleObservations,
            OwnerReportedFunctionality = assessment.OwnerReportedFunctionality,
            MissingInformation = assessment.MissingInformation,
            Confidence = assessment.Confidence,
            Status = assessment.Status.ToString(),
            CreatedAt = assessment.CreatedAt,
            UpdatedAt = assessment.UpdatedAt,
            Evidences = assessment.Evidences.Select(e => e.ToResponse()).ToList(),
            Clarifications = assessment.Clarifications.Select(c => c.ToResponse()).ToList()
        };
    }

    public static AssessmentEvidenceResponse ToResponse(this AssessmentEvidence evidence)
    {
        return new AssessmentEvidenceResponse
        {
            Id = evidence.Id,
            AssessmentId = evidence.AssessmentId,
            PhotoId = evidence.PhotoId,
            Observation = evidence.Observation,
            EvidenceType = evidence.EvidenceType,
            CreatedAt = evidence.CreatedAt
        };
    }

    public static AssessmentClarificationResponse ToResponse(this AssessmentClarification clarification)
    {
        return new AssessmentClarificationResponse
        {
            Id = clarification.Id,
            ItemId = clarification.ItemId,
            AssessmentId = clarification.AssessmentId,
            QuestionCode = clarification.QuestionCode,
            Question = clarification.Question,
            Reason = clarification.Reason,
            Status = clarification.Status,
            Answer = clarification.Answer,
            CreatedAt = clarification.CreatedAt,
            AnsweredAt = clarification.AnsweredAt
        };
    }
}
