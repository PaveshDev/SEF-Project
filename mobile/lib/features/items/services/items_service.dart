import '../../../core/network/api_client.dart';
import '../models/item_models.dart';

class ItemsService {
  Future<List<ItemSummary>> getItems() async {
    final response = await apiClient.get('/api/items');
    final data = response.data as List<dynamic>;
    return data.map((e) => ItemSummary.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<Item> getItem(String id) async {
    final response = await apiClient.get('/api/items/$id');
    return Item.fromJson(response.data as Map<String, dynamic>);
  }

  Future<ItemSummary> createItem(CreateItemRequest request) async {
    final response = await apiClient.post(
      '/api/items',
      data: request.toJson(),
    );
    return ItemSummary.fromJson(response.data as Map<String, dynamic>);
  }

  Future<Item> updateItem(String id, UpdateItemRequest request) async {
    final response = await apiClient.put(
      '/api/items/$id',
      data: request.toJson(),
    );
    return Item.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> deleteItem(String id) async {
    await apiClient.delete('/api/items/$id');
  }

  Future<ItemPhoto> addPhoto(String id, AddPhotoRequest request) async {
    final response = await apiClient.post(
      '/api/items/$id/photos',
      data: request.toJson(),
    );
    return ItemPhoto.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> submitConditionAnswers(
      String id, SubmitConditionAnswersRequest request) async {
    await apiClient.post(
      '/api/items/$id/condition-answers',
      data: request.toJson(),
    );
  }

  Future<void> submitItemForAssessment(String id) async {
    await apiClient.post('/api/items/$id/submit');
  }

  Future<ItemAssessment> createAssessment(String id) async {
    // Depending on backend, this might take a CreateItemAssessmentRequest
    // Using empty body for now if it triggers assessment manually via backend,
    // though the DTOs had CreateItemAssessmentRequest.
    final response = await apiClient.post('/api/items/$id/assessments');
    return ItemAssessment.fromJson(response.data as Map<String, dynamic>);
  }

  Future<ItemAssessment> getAssessment(String id, String assessmentId) async {
    final response = await apiClient.get('/api/items/$id/assessments/$assessmentId');
    return ItemAssessment.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> addAssessmentEvidence(
      String id, String assessmentId, Map<String, dynamic> request) async {
    await apiClient.post(
      '/api/items/$id/assessments/$assessmentId/evidence',
      data: request,
    );
  }

  Future<void> createClarification(String id, Map<String, dynamic> request) async {
    await apiClient.post(
      '/api/items/$id/clarifications',
      data: request,
    );
  }

  Future<void> answerClarification(
      String id, String clarificationId, AnswerClarificationRequest request) async {
    await apiClient.post(
      '/api/items/$id/clarifications/$clarificationId/answer',
      data: request.toJson(),
    );
  }

  Future<void> confirmAssessment(
      String id, String assessmentId, ConfirmAssessmentRequest request) async {
    await apiClient.post(
      '/api/items/$id/assessments/$assessmentId/confirm',
      data: request.toJson(),
    );
  }

  Future<void> requestReassessment(
      String id, String assessmentId, RequestReassessmentRequest request) async {
    await apiClient.post(
      '/api/items/$id/assessments/$assessmentId/reassessment',
      data: request.toJson(),
    );
  }
}
