import 'package:dio/dio.dart';

import '../../../core/network/api_client.dart';
import '../models/recovery_models.dart';

class RecoveryService {
  RecoveryService({Dio? client}) : client = client ?? apiClient;

  final Dio client;

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

  Future<RecoveryPage<dynamic>> listReferences() async {
    final response = await client.get<Map<String, dynamic>>(
      '/api/value-references',
      queryParameters: {
        'page': 1,
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
    required RecoveryRoute route,
    required String currency,
    double? budget,
    DateTime? deadline,
  }) async {
    final response = await client.post<Map<String, dynamic>>(
      '/api/recovery-cases',
      data: {
        'itemId': itemId,
        'inputs': {
          'objective': objective,
          'preferredRoutes': [routeApiValue(route)],
          'currency': currency,
          'maximumPickupCost': budget,
          'deadline': deadline?.toUtc().toIso8601String(),
        },
      },
      options: Options(headers: {'Idempotency-Key': _key()}),
    );
    return RecoveryCase.fromJson(response.data ?? {});
  }

  Future<Map<String, dynamic>> planCase(RecoveryCase item,
      {bool replan = false}) async {
    final endpoint = replan ? 'replan' : 'plan';
    final response = await client.post<Map<String, dynamic>>(
      '/api/recovery-cases/${item.id}/$endpoint',
      data: {'expectedVersion': item.version},
      options: Options(headers: {'Idempotency-Key': _key()}),
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
    final response = await client.post<Map<String, dynamic>>(
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
      options: Options(headers: {'Idempotency-Key': _key()}),
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
    final response = await client.post<Map<String, dynamic>>(
      '/api/recovery-proposals/$id/decisions',
      data: {
        'expectedVersion': version,
        'proposalRevision': revision,
        'decision': decision,
        'comment': comment,
      },
      options: Options(headers: {'Idempotency-Key': _key()}),
    );
    return RecoveryProposal.fromJson(response.data ?? {});
  }

  Future<RecoveryProposal> getProposal(String id) async {
    final response = await client.get<Map<String, dynamic>>(
      '/api/recovery-proposals/$id',
    );
    return RecoveryProposal.fromJson(response.data ?? {});
  }

  String _key() => 'mobile-recovery-${DateTime.now().microsecondsSinceEpoch}';
}
