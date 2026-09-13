using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WasteToValue.Api.Modules.Items.DTOs;

namespace WasteToValue.Api.Modules.Items.Interfaces;

public interface IItemService
{
    Task<ItemResponse> CreateItemAsync(CreateItemRequest request, Guid ownerId, CancellationToken cancellationToken = default);
    Task<ItemResponse> GetItemAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ItemSummaryResponse>> GetUserItemsAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<ItemResponse> UpdateItemAsync(UpdateItemRequest request, Guid ownerId, CancellationToken cancellationToken = default);
    Task DeleteDraftItemAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
    
    Task<ItemPhotoResponse> AddPhotoAsync(AddPhotoRequest request, Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ItemPhotoResponse>> GetItemPhotosAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default);
    Task DeletePhotoAsync(Guid photoId, Guid ownerId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ConditionQuestionResponse>> GetConditionQuestionsAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ItemConditionAnswerResponse>> GetConditionAnswersAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ItemConditionAnswerResponse>> SubmitConditionAnswersAsync(SubmitConditionAnswersRequest request, Guid ownerId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ItemAssessmentResponse>> GetAssessmentHistoryAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<ItemAssessmentResponse> GetAssessmentByIdAsync(Guid assessmentId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<ItemAssessmentResponse> CreateAssessmentAsync(CreateItemAssessmentRequest request, CancellationToken cancellationToken = default);
    Task<AssessmentEvidenceResponse> AddEvidenceAsync(AddAssessmentEvidenceRequest request, Guid ownerId, CancellationToken cancellationToken = default);
    
    Task<AssessmentClarificationResponse> CreateClarificationAsync(CreateClarificationRequest request, Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AssessmentClarificationResponse>> GetClarificationsAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<AssessmentClarificationResponse> AnswerClarificationAsync(AnswerClarificationRequest request, Guid ownerId, CancellationToken cancellationToken = default);
    
    Task<ItemResponse> SubmitItemForAssessmentAsync(Guid itemId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<ItemAssessmentResponse> ConfirmAssessmentAsync(ConfirmAssessmentRequest request, Guid ownerId, CancellationToken cancellationToken = default);
    Task<ItemAssessmentResponse> RequestReassessmentAsync(RequestReassessmentRequest request, Guid ownerId, CancellationToken cancellationToken = default);
}
