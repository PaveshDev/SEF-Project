import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

import '../models/recovery_models.dart';
import '../services/recovery_service.dart';

enum RecoveryLoadState {
  idle,
  loading,
  ready,
  empty,
  offline,
  unauthorized,
  stale,
  error
}

class RecoveryController extends ChangeNotifier {
  RecoveryController({RecoveryService? service})
      : service = service ?? RecoveryService();

  final RecoveryService service;
  RecoveryLoadState state = RecoveryLoadState.idle;
  RecoveryPage<RecoveryCase> cases =
      const RecoveryPage(items: [], page: 1, totalPages: 1);
  RecoveryCase? selectedCase;
  Map<String, dynamic>? planning;
  RecoveryProposal? proposal;
  String? errorMessage;
  bool isSaving = false;

  Future<void> loadCases(
      {String search = '', String status = '', int page = 1}) async {
    state = RecoveryLoadState.loading;
    errorMessage = null;
    notifyListeners();
    try {
      cases =
          await service.listCases(search: search, status: status, page: page);
      state = cases.items.isEmpty
          ? RecoveryLoadState.empty
          : RecoveryLoadState.ready;
    } on DioException catch (error) {
      errorMessage = _message(error);
      state =
          error.response?.statusCode == 401 || error.response?.statusCode == 403
              ? RecoveryLoadState.unauthorized
              : error.type == DioExceptionType.connectionError
                  ? RecoveryLoadState.offline
                  : error.response?.statusCode == 409
                      ? RecoveryLoadState.stale
                      : RecoveryLoadState.error;
    } catch (error) {
      errorMessage = error.toString();
      state = RecoveryLoadState.error;
    }
    notifyListeners();
  }

  Future<void> createCase({
    required String itemId,
    required String objective,
    required RecoveryRoute route,
    required String currency,
    double? budget,
    DateTime? deadline,
  }) async {
    isSaving = true;
    errorMessage = null;
    notifyListeners();
    try {
      selectedCase = await service.createCase(
        itemId: itemId,
        objective: objective,
        route: route,
        currency: currency,
        budget: budget,
        deadline: deadline,
      );
      await loadCases();
    } on DioException catch (error) {
      errorMessage = _message(error);
      state = error.response?.statusCode == 401
          ? RecoveryLoadState.unauthorized
          : RecoveryLoadState.error;
    } finally {
      isSaving = false;
      notifyListeners();
    }
  }

  Future<void> plan({bool replan = false}) async {
    final item = selectedCase;
    if (item == null) return;
    state = RecoveryLoadState.loading;
    errorMessage = null;
    notifyListeners();
    try {
      planning = await service.planCase(item, replan: replan);
      final proposalJson = planning?['proposal'];
      if (proposalJson is Map<String, dynamic>) {
        proposal = RecoveryProposal.fromJson(proposalJson);
      } else if (planning?['proposalId'] is String) {
        await loadProposal(planning!['proposalId'] as String);
      }
      state = RecoveryLoadState.ready;
    } on DioException catch (error) {
      errorMessage = _message(error);
      state = error.response?.statusCode == 409
          ? RecoveryLoadState.stale
          : RecoveryLoadState.error;
    }
    notifyListeners();
  }

  Future<void> loadProposal(String proposalId) async {
    try {
      proposal = await service.getProposal(proposalId);
      notifyListeners();
    } on DioException catch (error) {
      errorMessage = _message(error);
      state = error.response?.statusCode == 404
          ? RecoveryLoadState.empty
          : error.response?.statusCode == 409
              ? RecoveryLoadState.stale
              : error.response?.statusCode == 401 ||
                      error.response?.statusCode == 403
                  ? RecoveryLoadState.unauthorized
                  : RecoveryLoadState.error;
      notifyListeners();
    }
  }

  Future<void> decideProposal(
      {required String id,
      required int version,
      required int revision,
      required String decision}) async {
    isSaving = true;
    errorMessage = null;
    notifyListeners();
    try {
      await service.decideProposal(
          id: id, version: version, revision: revision, decision: decision);
      await loadProposal(id);
    } on DioException catch (error) {
      errorMessage = _message(error);
      state = error.response?.statusCode == 409
          ? RecoveryLoadState.stale
          : RecoveryLoadState.error;
    } finally {
      isSaving = false;
      notifyListeners();
    }
  }

  String _message(DioException error) =>
      error.response?.data is Map<String, dynamic>
          ? (error.response!.data['detail'] as String? ??
              error.response!.data['title'] as String? ??
              (error.response!.data['extensions']
                  as Map<String, dynamic>?)?['code'] as String? ??
              error.message ??
              'Recovery request failed.')
          : error.message ?? 'Recovery request failed.';
}
