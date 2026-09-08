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

  Future<RecoveryProposal> decideProposal({
    required String id,
    required int version,
    required int revision,
    required String decision,
  }) async {
    final response = await client.post<Map<String, dynamic>>(
      '/api/recovery-proposals/$id/decisions',
      data: {
        'expectedVersion': version,
        'proposalRevision': revision,
        'decision': decision,
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
