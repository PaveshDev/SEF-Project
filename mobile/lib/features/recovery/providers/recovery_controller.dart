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
      const RecoveryPage<RecoveryCase>(items: [], page: 1, totalPages: 1);
  RecoveryCase? selectedCase;
  RecoveryProposal? proposal;
  String? proposalId;
  List<RecoveryOption> options = const [];
  List<String> unavailableInputs = const [];
  RecoveryOption? selectedOption;
  String? errorMessage;
  String? successMessage;
  bool isSaving = false;
  bool isCreatingProposal = false;
  bool workflowBlocked = false;
  bool _disposed = false;

  bool get isBusy =>
      isSaving || isCreatingProposal || state == RecoveryLoadState.loading;
  bool get canCreateProposal =>
      !isBusy &&
      !workflowBlocked &&
      proposalId == null &&
      selectedOption != null &&
      options.contains(selectedOption) &&
      optionUnavailable(selectedOption!, selectedCase) == null;
  bool get canDecide {
    final value = proposal;
    return !isBusy &&
        !workflowBlocked &&
        value != null &&
        value.id == proposalId &&
        value.caseId == selectedCase?.id &&
        value.caseRevision == selectedCase?.revision &&
        value.version > 0 &&
        value.revision > 0 &&
        value.status == RecoveryProposalStatus.awaitingApproval &&
        value.expiresAt != null &&
        value.expiresAt!.isAfter(DateTime.now());
  }

  void _notify() {
    if (!_disposed) notifyListeners();
  }

  @override
  void dispose() {
    _disposed = true;
    super.dispose();
  }

  Future<void> _run(Future<void> Function() action) async {
    if (isBusy || _disposed) return;
    isSaving = true;
    state = RecoveryLoadState.loading;
    errorMessage = null;
    successMessage = null;
    _notify();
    try {
      await action();
      state = selectedCase == null && cases.items.isEmpty
          ? RecoveryLoadState.empty
          : RecoveryLoadState.ready;
    } catch (error) {
      workflowBlocked = true;
      selectedOption = null;
      errorMessage = _message(error);
      state = error is DioException
          ? error.response?.statusCode == 401 ||
                  error.response?.statusCode == 403
              ? RecoveryLoadState.unauthorized
              : error.response?.statusCode == 409
                  ? RecoveryLoadState.stale
                  : error.type == DioExceptionType.connectionError
                      ? RecoveryLoadState.offline
                      : RecoveryLoadState.error
          : RecoveryLoadState.error;
    } finally {
      isSaving = false;
      isCreatingProposal = false;
      _notify();
    }
  }

  Future<void> loadCases(
          {String search = '', String status = '', int page = 1}) =>
      _run(() async {
        cases =
            await service.listCases(search: search, status: status, page: page);
      });

  void _clearWorkflow() {
    proposal = null;
    proposalId = null;
    selectedOption = null;
    options = const [];
    unavailableInputs = const [];
    workflowBlocked = false;
    errorMessage = null;
    successMessage = null;
  }

  Future<void> selectCase(RecoveryCase item) => _run(() async {
        _clearWorkflow();
        selectedCase = item;
        await _refresh();
      });

  void clearSelection() {
    if (isBusy) return;
    selectedCase = null;
    _clearWorkflow();
    _notify();
  }

  Future<void> createCase({
    required String itemId,
    required String objective,
    required RecoveryRoute route,
    required String currency,
    double? budget,
    DateTime? deadline,
  }) =>
      _run(() async {
        final created = await service.createCase(
            itemId: itemId,
            objective: objective,
            route: route,
            currency: currency,
            budget: budget,
            deadline: deadline);
        _clearWorkflow();
        selectedCase = created;
        cases = await service.listCases();
        successMessage = 'Case created.';
      });

  Future<void> plan({bool replan = false}) async {
    final item = selectedCase;
    if (item == null || workflowBlocked) return;
    await _run(() async {
      _clearWorkflow();
      final result = await service.planCase(item, replan: replan);
      final caseJson = result['case'];
      if (caseJson is! Map<String, dynamic>) throw const FormatException();
      final current = RecoveryCase.fromJson(caseJson);
      if (current.id != item.id) throw const FormatException();
      selectedCase = current;
      unavailableInputs = (result['unavailableInputs'] as List<dynamic>? ?? [])
          .whereType<String>()
          .toList();
      options = result['options'] is List
          ? (result['options'] as List)
              .whereType<Map<String, dynamic>>()
              .map(RecoveryOption.fromJson)
              .toList()
          : await service.getOptions(item.id);
      workflowBlocked = false;
      successMessage =
          'Planning completed. Select an eligible option to prepare a proposal.';
    });
  }

  Future<void> _refresh() async {
    final item = selectedCase;
    if (item == null) return;
    final current = await service.getCase(item.id);
    final values = await service.getOptions(item.id);
    final loaded =
        proposalId == null ? null : await service.getProposal(proposalId!);
    if (current.id != item.id ||
        (loaded != null &&
            (loaded.id != proposalId || loaded.caseId != item.id))) {
      throw const FormatException();
    }
    selectedCase = current;
    options = values;
    proposal = loaded;
    selectedOption = null;
    workflowBlocked = false;
    cases = await service.listCases();
  }

  Future<void> refreshWorkflow() => _run(_refresh);

  void selectOption(RecoveryOption option) {
    if (isBusy ||
        workflowBlocked ||
        proposalId != null ||
        !options.contains(option) ||
        optionUnavailable(option, selectedCase) != null) return;
    selectedOption = option;
    _notify();
  }

  Future<void> createProposal(
      {required DateTime expiresAt, required String explanation}) async {
    if (!canCreateProposal ||
        !expiresAt.isAfter(DateTime.now()) ||
        explanation.trim().isEmpty) return;
    final item = selectedCase!;
    final option = selectedOption!;
    await _run(() async {
      isCreatingProposal = true;
      _notify();
      final created = await service.submitProposal(
        caseId: item.id,
        expectedVersion: item.version,
        optionId: option.id,
        optionVersion: option.version,
        match: option.requiresPartner == true
            ? option.integration!.match!.choice
            : null,
        pickup: option.requiresPickup == true
            ? option.integration!.pickup!.choice
            : null,
        expiresAt: expiresAt,
        explanation: explanation.trim(),
      );
      if (created.id.isEmpty ||
          created.caseId != item.id ||
          created.optionId != option.id) throw const FormatException();
      proposalId = created.id;
      await _refresh();
      successMessage = 'Proposal created and loaded for review.';
    });
  }

  Future<void> decideProposal(
      {required String decision, String? comment}) async {
    if (!canDecide ||
        !['Approved', 'Rejected', 'RevisionRequested'].contains(decision) ||
        (decision == 'RevisionRequested' &&
            (comment == null || comment.trim().isEmpty))) return;
    final value = proposal!;
    await _run(() async {
      await service.decideProposal(
          id: value.id,
          version: value.version,
          revision: value.revision,
          decision: decision,
          comment: comment?.trim().isEmpty == true ? null : comment?.trim());
      await _refresh();
      successMessage =
          'Decision submitted. Proposal, case and options refreshed.';
    });
  }

  String _message(Object error) {
    const fallback =
        'The Recovery request could not be completed. Please try again.';
    if (error is! DioException || error.response?.data is! Map) return fallback;
    final data = error.response!.data as Map;
    for (final value in [data['detail'], data['title']]) {
      if (value is String &&
          value.isNotEmpty &&
          value.length <= 500 &&
          !RegExp(r'https?:|www\.|[\\/]|\bat\s|exception|stack|token|authorization|header|bearer|password|secret|connectionstring',
                  caseSensitive: false)
              .hasMatch(value)) return value;
    }
    return fallback;
  }
}
