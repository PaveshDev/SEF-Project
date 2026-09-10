import 'dart:convert';
import 'dart:math';
import 'package:dio/dio.dart';

import '../../../core/network/api_client.dart';
import '../models/recovery_models.dart';

class RecoveryService {
  RecoveryService({Dio? client}) : client = client ?? apiClient;

  final Dio client;
  final Map<String, String> _pending = {};
  Future<Response<T>> _mutation<T>(String method, String path,
      {Object? data}) async {
    final fingerprint = jsonEncode([method, path, data]);
    final key = _pending.putIfAbsent(
        fingerprint,
        () =>
            'mobile-recovery-${List.generate(24, (_) => Random.secure().nextInt(256).toRadixString(16).padLeft(2, '0')).join()}');
    try {
      final response = await client.request<T>(path,
          data: data,
          options: Options(method: method, headers: {'Idempotency-Key': key}));
      _pending.remove(fingerprint);
      return response;
    } on DioException catch (error) {
      final status = error.response?.statusCode;
      if (status != null &&
          status >= 400 &&
          status < 500 &&
          status != 408 &&
          status != 429) _pending.remove(fingerprint);
      rethrow;
    }
  }

  Future<RecoveryPage<RecoveryCase>> listCases({
    String search = '',
    String status = '',
    int page = 1,
  }) async {
    final response = await client.get<Map<String, dynamic>>(
      '/api/recovery-cases',
      queryParameters: {
        if (search.isNotEmpty) 'search': search,
        if (status.isNotEmpty)
          'status': caseStatusApiValue(RecoveryCaseStatus.values
              .firstWhere((item) => item.name == status)),
        'sortBy': 'createdAt',
        'sortDirection': 'desc',
        'page': page,
        'pageSize': 12,
      },
    );
    return RecoveryPage.fromJson(response.data ?? {}, RecoveryCase.fromJson);
  }

  Future<RecoveryPage<dynamic>> listReferences(
      {int page = 1, String search = ''}) async {
    final response = await client.get<Map<String, dynamic>>(
      '/api/value-references',
      queryParameters: {
        'page': page,
        if (search.isNotEmpty) 'search': search,
        'pageSize': 20,
        'sortBy': 'observedAt',
        'sortDirection': 'desc'
      },
    );
    return RecoveryPage.fromJson(response.data ?? {}, (json) => json);
  }

  Future<RecoveryCase> createCase({
    required String itemId,
    required String objective,
    required List<RecoveryRoute> routes,
    required String currency,
    double? budget,
    DateTime? deadline,
  }) async {
    final response = await _mutation<Map<String, dynamic>>(
      'POST',
      '/api/recovery-cases',
      data: {
        'itemId': itemId,
        'inputs': {
          'objective': objective,
          'preferredRoutes': routes.map(routeApiValue).toList(),
          'currency': currency,
          'maximumPickupCost': budget,
          'deadline': deadline?.toUtc().toIso8601String(),
        },
      },
    );
    return RecoveryCase.fromJson(response.data ?? {});
  }

  Future<Map<String, dynamic>> planCase(RecoveryCase item,
      {bool replan = false}) async {
    final endpoint = replan ? 'replan' : 'plan';
    final response = await _mutation<Map<String, dynamic>>(
      'POST',
      '/api/recovery-cases/${item.id}/$endpoint',
      data: {'expectedVersion': item.version},
    );
    return response.data ?? {};
  }

  Future<RecoveryCase> getCase(String id) async {
    final response =
        await client.get<Map<String, dynamic>>('/api/recovery-cases/$id');
    return RecoveryCase.fromJson(response.data ?? {});
  }

  Future<List<RecoveryOption>> getOptions(String caseId) async {
    final response = await client.get<List<dynamic>>(
      '/api/recovery-cases/$caseId/options',
    );
    return (response.data ?? [])
        .whereType<Map<String, dynamic>>()
        .map(RecoveryOption.fromJson)
        .toList();
  }

  Future<RecoveryProposal> submitProposal({
    required String caseId,
    required int expectedVersion,
    required String optionId,
    required int optionVersion,
    MatchChoice? match,
    PickupChoice? pickup,
    required DateTime expiresAt,
    required String explanation,
  }) async {
    final response = await _mutation<Map<String, dynamic>>(
      'POST',
      '/api/recovery-cases/$caseId/proposals',
      data: {
        'expectedVersion': expectedVersion,
        'optionId': optionId,
        'optionVersion': optionVersion,
        'match': match?.toJson(),
        'pickup': pickup?.toJson(),
        'expiresAt': expiresAt.toUtc().toIso8601String(),
        'explanation': explanation,
      },
    );
    return RecoveryProposal.fromJson(response.data ?? {});
  }

  Future<RecoveryProposal> decideProposal({
    required String id,
    required int version,
    required int revision,
    required String decision,
    String? comment,
  }) async {
    final response = await _mutation<Map<String, dynamic>>(
      'POST',
      '/api/recovery-proposals/$id/decisions',
      data: {
        'expectedVersion': version,
        'proposalRevision': revision,
        'decision': decision,
        'comment': comment,
      },
    );
    return RecoveryProposal.fromJson(response.data ?? {});
  }

  Future<RecoveryProposal> getProposal(String id) async {
    final response = await client.get<Map<String, dynamic>>(
      '/api/recovery-proposals/$id',
    );
    return RecoveryProposal.fromJson(response.data ?? {});
  }

  Future<List<RecoveryProposal>> listProposals(String caseId) async {
    final result = await client
        .get<List<dynamic>>('/api/recovery-cases/$caseId/proposals');
    return (result.data ?? [])
        .map(
            (value) => RecoveryProposal.fromJson(value as Map<String, dynamic>))
        .toList();
  }

  Future<RecoveryCase> updateCase(
          RecoveryCase item, Map<String, dynamic> inputs) async =>
      RecoveryCase.fromJson((await _mutation<Map<String, dynamic>>(
              'PUT', '/api/recovery-cases/${item.id}',
              data: {'expectedVersion': item.version, 'inputs': inputs}))
          .data!);
  Future<void> cancelCase(RecoveryCase item) async =>
      _mutation('POST', '/api/recovery-cases/${item.id}/cancel',
          data: {'expectedVersion': item.version});
  Future<void> deleteCase(String id) async =>
      _mutation('DELETE', '/api/recovery-cases/$id');
  Future<RecoveryProposal> refreshProposal(String id) async =>
      RecoveryProposal.fromJson((await _mutation<Map<String, dynamic>>(
              'POST', '/api/recovery-proposals/$id/refresh'))
          .data!);
  Future<bool> canManageReferences() async =>
      (await client.get<Map<String, dynamic>>('/api/recovery/access'))
          .data?['canManageValueReferences'] ==
      true;
  Future<void> saveReference(Map<String, dynamic> value, {String? id}) async =>
      _mutation(id == null ? 'POST' : 'PUT',
          '/api/value-references${id == null ? '' : '/$id'}',
          data: value);
  Future<void> deleteReference(String id) async =>
      _mutation('DELETE', '/api/value-references/$id');
  Future<void> verifyReference(String id, int version) async =>
      _mutation('POST', '/api/value-references/$id/verify',
          data: {'expectedVersion': version});
}
