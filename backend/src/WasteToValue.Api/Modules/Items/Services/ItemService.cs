using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WasteToValue.Api.Modules.Items.DTOs;
using WasteToValue.Api.Modules.Items.Entities;
using WasteToValue.Api.Modules.Items.Interfaces;

namespace WasteToValue.Api.Modules.Items.Services;

public class ItemService(IItemRepository repository) : IItemService
{
    private async Task<Item> GetValidItemAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdWithDetailsAsync(itemId, cancellationToken);
        if (item == null)
            throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        
        if (item.OwnerId != ownerId)
            throw new UnauthorizedAccessException("You do not have permission to access this item.");
            
        return item;
    }

    public async Task<ItemResponse> CreateItemAsync(CreateItemRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            LocationArea = request.LocationArea,
            Status = ItemStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return item.ToResponse();
    }

    public async Task<ItemResponse> GetItemAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(id, ownerId, cancellationToken);
        return item.ToResponse();
    }

    public async Task<IEnumerable<ItemSummaryResponse>> GetUserItemsAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetByOwnerIdAsync(ownerId, cancellationToken);
        return items.Select(x => x.ToSummaryResponse()).ToList();
    }

    public async Task<ItemResponse> UpdateItemAsync(UpdateItemRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(request.Id, ownerId, cancellationToken);

        if (item.Status != ItemStatus.Draft)
            throw new InvalidOperationException("Only items in Draft status can be updated.");

        if (item.Version != request.Version)
            throw new ItemConcurrencyException("The item has been modified by another process. Please reload and try again.");

        item.Title = request.Title;
        item.Description = request.Description;
        item.Category = request.Category;
        item.LocationArea = request.LocationArea;
        item.UpdatedAt = DateTime.UtcNow;
        item.Version++; // Increment version

        repository.Update(item);
        
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            throw new ItemConcurrencyException("A concurrency conflict occurred while saving the item.", ex);
        }

        return item.ToResponse();
    }

    public async Task DeleteDraftItemAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(id, ownerId, cancellationToken);

        if (item.Status != ItemStatus.Draft && item.Status != ItemStatus.Withdrawn)
            throw new InvalidOperationException("Only Draft or Withdrawn items can be deleted.");

        repository.Delete(item);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ItemPhotoResponse> AddPhotoAsync(AddPhotoRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(request.ItemId, ownerId, cancellationToken);

        var photo = new ItemPhoto
        {
            Id = Guid.NewGuid(),
            ItemId = request.ItemId,
            ImageUrl = request.ImageUrl,
            PhotoOrder = request.PhotoOrder,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddPhotoAsync(photo, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return photo.ToResponse();
    }

    public async Task<IEnumerable<ItemPhotoResponse>> GetItemPhotosAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(itemId, ownerId, cancellationToken);
        return item.Photos.Select(x => x.ToResponse()).ToList();
    }

    public async Task DeletePhotoAsync(Guid photoId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var photo = await repository.GetPhotoByIdAsync(photoId, cancellationToken);
        if (photo == null)
            throw new KeyNotFoundException("Photo not found.");

        if (photo.Item == null || photo.Item.OwnerId != ownerId)
            throw new UnauthorizedAccessException("You do not have permission to delete this photo.");

        repository.DeletePhoto(photo);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public Task<IEnumerable<ConditionQuestionResponse>> GetConditionQuestionsAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        // For now, return a static list or dummy questions as no DB table was required for these.
        var questions = new List<ConditionQuestionResponse>
        {
            new ConditionQuestionResponse { QuestionCode = "POWER", QuestionText = "Does the item power on?" },
            new ConditionQuestionResponse { QuestionCode = "DAMAGE", QuestionText = "Is there any visible physical damage?" }
        };
        return Task.FromResult<IEnumerable<ConditionQuestionResponse>>(questions);
    }

    public async Task<IEnumerable<ItemConditionAnswerResponse>> GetConditionAnswersAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(itemId, ownerId, cancellationToken);
        return item.ConditionAnswers.Select(x => x.ToResponse()).ToList();
    }

    public async Task<IEnumerable<ItemConditionAnswerResponse>> SubmitConditionAnswersAsync(SubmitConditionAnswersRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(request.ItemId, ownerId, cancellationToken);

        if (item.Status != ItemStatus.Draft && item.Status != ItemStatus.AwaitingOwnerConfirmation)
            throw new InvalidOperationException("Condition answers can only be updated when in Draft or Awaiting Confirmation.");

        var newAnswersDict = request.Answers.ToDictionary(a => a.QuestionCode);
        var existingAnswersDict = item.ConditionAnswers.ToDictionary(a => a.QuestionCode);

        // Update or add answers
        foreach (var answerDto in request.Answers)
        {
            if (existingAnswersDict.TryGetValue(answerDto.QuestionCode, out var existingAnswer))
            {
                existingAnswer.QuestionText = answerDto.QuestionText;
                existingAnswer.Answer = answerDto.Answer;
                existingAnswer.AnsweredAt = DateTime.UtcNow;
                existingAnswer.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                item.ConditionAnswers.Add(new ItemConditionAnswer
                {
                    ItemId = item.Id,
                    QuestionCode = answerDto.QuestionCode,
                    QuestionText = answerDto.QuestionText,
                    Answer = answerDto.Answer,
                    AnsweredAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        // Remove answers that are not in the request
        var codesToRemove = existingAnswersDict.Keys.Except(newAnswersDict.Keys).ToList();
        foreach (var code in codesToRemove)
        {
            var answerToRemove = existingAnswersDict[code];
            item.ConditionAnswers.Remove(answerToRemove);
        }

        item.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);

        return item.ConditionAnswers.Select(x => x.ToResponse()).ToList();
    }

    public async Task<IEnumerable<ItemAssessmentResponse>> GetAssessmentHistoryAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(itemId, ownerId, cancellationToken);
        var assessments = await repository.GetAssessmentsByItemIdAsync(itemId, cancellationToken);
        return assessments.Select(x => x.ToResponse()).ToList();
    }

    public async Task<ItemAssessmentResponse> GetAssessmentByIdAsync(Guid assessmentId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var assessment = await repository.GetAssessmentByIdAsync(assessmentId, cancellationToken);
        if (assessment == null)
            throw new KeyNotFoundException("Assessment not found.");

        if (assessment.Item == null || assessment.Item.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Unauthorized.");

        return assessment.ToResponse();
    }

    public async Task<ItemAssessmentResponse> CreateAssessmentAsync(CreateItemAssessmentRequest request, CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdWithDetailsAsync(request.ItemId, cancellationToken);
        if (item == null)
            throw new KeyNotFoundException("Item not found.");

        var existingAssessments = await repository.GetAssessmentsByItemIdAsync(request.ItemId, cancellationToken);
        
        int nextVersion = existingAssessments.Any() ? existingAssessments.Max(a => a.Version) + 1 : 1;

        var assessment = new ItemAssessment
        {
            Id = Guid.NewGuid(),
            ItemId = request.ItemId,
            Version = nextVersion,
            SuggestedCategory = request.SuggestedCategory ?? string.Empty,
            ConditionGrade = request.ConditionGrade ?? string.Empty,
            ConditionSummary = request.ConditionSummary ?? string.Empty,
            VisibleObservations = request.VisibleObservations ?? string.Empty,
            OwnerReportedFunctionality = request.OwnerReportedFunctionality ?? string.Empty,
            MissingInformation = request.MissingInformation ?? string.Empty,
            Confidence = request.Confidence,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAssessmentAsync(assessment, cancellationToken);
        
        // If the AI agent assigns a confirmation-pending status, update the item
        if (request.Status == AssessmentStatus.PendingConfirmation)
        {
            item.Status = ItemStatus.AwaitingOwnerConfirmation;
            item.UpdatedAt = DateTime.UtcNow;
            repository.Update(item);
        }

        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message != null && ex.InnerException.Message.Contains("unique constraint"))
        {
            throw new ItemConcurrencyException("An assessment with this version was already created concurrently. Please retry.", ex);
        }

        return assessment.ToResponse();
    }

    public async Task<AssessmentEvidenceResponse> AddEvidenceAsync(AddAssessmentEvidenceRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var assessment = await repository.GetAssessmentByIdAsync(request.AssessmentId, cancellationToken);
        if (assessment == null)
            throw new KeyNotFoundException("Assessment not found.");

        if (assessment.Item == null || assessment.Item.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Unauthorized.");

        if (assessment.Status == AssessmentStatus.Superseded || assessment.Status == AssessmentStatus.Confirmed || assessment.Status == AssessmentStatus.Failed || assessment.Status == AssessmentStatus.Rejected)
            throw new InvalidOperationException("Evidence cannot be added to an assessment in its current state.");

        var photo = await repository.GetPhotoByIdAsync(request.PhotoId, cancellationToken);
        if (photo == null)
            throw new KeyNotFoundException("Photo not found.");

        if (assessment.ItemId != photo.ItemId)
            throw new InvalidOperationException("The photo must belong to the same item as the assessment.");

        var evidence = new AssessmentEvidence
        {
            Id = Guid.NewGuid(),
            AssessmentId = assessment.Id,
            PhotoId = photo.Id,
            Observation = request.Observation,
            EvidenceType = request.EvidenceType,
            CreatedAt = DateTime.UtcNow
        };

        assessment.Evidences.Add(evidence);
        
        await repository.SaveChangesAsync(cancellationToken);

        return evidence.ToResponse();
    }

    public async Task<AssessmentClarificationResponse> CreateClarificationAsync(CreateClarificationRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(request.ItemId, ownerId, cancellationToken);

        if (request.AssessmentId.HasValue)
        {
            var assessment = await repository.GetAssessmentByIdAsync(request.AssessmentId.Value, cancellationToken);
            if (assessment == null || assessment.ItemId != request.ItemId)
                throw new InvalidOperationException("Invalid AssessmentId for this Item.");
                
            if (assessment.Status == AssessmentStatus.Superseded)
                throw new InvalidOperationException("Cannot ask a clarification on a superseded assessment.");
        }

        var clarification = new AssessmentClarification
        {
            Id = Guid.NewGuid(),
            ItemId = request.ItemId,
            AssessmentId = request.AssessmentId,
            QuestionCode = request.QuestionCode,
            Question = request.Question,
            Reason = request.Reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddClarificationAsync(clarification, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return clarification.ToResponse();
    }

    public async Task<IEnumerable<AssessmentClarificationResponse>> GetClarificationsAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(itemId, ownerId, cancellationToken);
        return item.Clarifications.Select(x => x.ToResponse()).ToList();
    }

    public async Task<AssessmentClarificationResponse> AnswerClarificationAsync(AnswerClarificationRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var clarification = await repository.GetClarificationByIdAsync(request.ClarificationId, cancellationToken);
        if (clarification == null)
            throw new KeyNotFoundException("Clarification not found.");

        if (clarification.Item == null || clarification.Item.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Unauthorized.");

        if (clarification.Status != "Pending")
            throw new InvalidOperationException("This clarification has already been answered.");

        clarification.Answer = request.Answer;
        clarification.AnsweredAt = DateTime.UtcNow;
        clarification.Status = "Answered";

        await repository.SaveChangesAsync(cancellationToken);

        return clarification.ToResponse();
    }

    public async Task<ItemResponse> SubmitItemForAssessmentAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var item = await GetValidItemAsync(itemId, ownerId, cancellationToken);

        if (item.Status != ItemStatus.Draft)
            throw new InvalidOperationException($"Item cannot be submitted because it is currently in '{item.Status}' status.");

        if (!item.Photos.Any())
            throw new InvalidOperationException("At least one photo must be provided before submission.");

        if (!item.ConditionAnswers.Any())
            throw new InvalidOperationException("Condition answers must be provided before submission.");

        item.Status = ItemStatus.Submitted;
        item.UpdatedAt = DateTime.UtcNow;

        repository.Update(item);
        await repository.SaveChangesAsync(cancellationToken);

        return item.ToResponse();
    }

    public async Task<ItemAssessmentResponse> ConfirmAssessmentAsync(ConfirmAssessmentRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        if (!request.OwnerConfirmation)
            throw new InvalidOperationException("Explicit owner confirmation is required.");

        var assessment = await repository.GetAssessmentByIdAsync(request.AssessmentId, cancellationToken);
        if (assessment == null)
            throw new KeyNotFoundException("Assessment not found.");

        var item = await GetValidItemAsync(assessment.ItemId, ownerId, cancellationToken);

        if (assessment.Status != AssessmentStatus.PendingConfirmation && assessment.Status != AssessmentStatus.AwaitingInformation)
            throw new InvalidOperationException("Assessment is not in a state that can be confirmed.");

        // Check if this is the latest valid assessment
        var existingAssessments = await repository.GetAssessmentsByItemIdAsync(item.Id, cancellationToken);
        var latestAssessment = existingAssessments.OrderByDescending(a => a.Version).FirstOrDefault();
        
        if (latestAssessment?.Id != assessment.Id)
            throw new InvalidOperationException("Only the latest assessment can be confirmed.");

        assessment.Status = AssessmentStatus.Confirmed;
        assessment.UpdatedAt = DateTime.UtcNow;

        item.Status = ItemStatus.Confirmed;
        item.UpdatedAt = DateTime.UtcNow;

        repository.Update(item);
        await repository.SaveChangesAsync(cancellationToken);

        return assessment.ToResponse();
    }

    public async Task<ItemAssessmentResponse> RequestReassessmentAsync(RequestReassessmentRequest request, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var assessment = await repository.GetAssessmentByIdAsync(request.AssessmentId, cancellationToken);
        if (assessment == null)
            throw new KeyNotFoundException("Assessment not found.");

        var item = await GetValidItemAsync(assessment.ItemId, ownerId, cancellationToken);

        if (item.Status == ItemStatus.Confirmed || item.Status == ItemStatus.Completed)
            throw new InvalidOperationException("Cannot request reassessment for a confirmed or completed item.");

        assessment.Status = AssessmentStatus.ReassessmentRequested;
        assessment.UpdatedAt = DateTime.UtcNow;

        item.Status = ItemStatus.ReassessmentRequested;
        item.UpdatedAt = DateTime.UtcNow;

        repository.Update(item);
        await repository.SaveChangesAsync(cancellationToken);

        return assessment.ToResponse();
    }
}
