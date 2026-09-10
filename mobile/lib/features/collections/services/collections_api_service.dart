import 'package:dio/dio.dart';

/// Dio-based API client for Member 4 collection endpoints.
/// Interfaces with the ASP.NET Core backend for real data persistence and operations.
class CollectionsApiService {
  CollectionsApiService({String? baseUrl, String? authToken})
      : _dio = Dio(BaseOptions(
          baseUrl: '${baseUrl ?? 'http://localhost:5080'}/api/collections',
          connectTimeout: const Duration(seconds: 5),
          receiveTimeout: const Duration(seconds: 8),
          headers: <String, String>{
            'Content-Type': 'application/json',
            if (authToken != null) 'Authorization': 'Bearer $authToken',
          },
        ));

  final Dio _dio;

  // ── Slots ──────────────────────────────────────────────

  Future<List<Map<String, dynamic>>> fetchSlots() async {
    try {
      final response = await _dio.get<List<dynamic>>('/slots');
      return (response.data ?? <dynamic>[]).cast<Map<String, dynamic>>();
    } catch (_) {
      return <Map<String, dynamic>>[];
    }
  }

  // ── Pickups ────────────────────────────────────────────

  Future<List<Map<String, dynamic>>> fetchPickups({String? status}) async {
    try {
      final response = await _dio.get<List<dynamic>>(
        '/pickups',
        queryParameters: status != null ? <String, String>{'status': status} : null,
      );
      return (response.data ?? <dynamic>[]).cast<Map<String, dynamic>>();
    } catch (_) {
      return <Map<String, dynamic>>[];
    }
  }

  Future<Map<String, dynamic>?> fetchPickup(String id) async {
    try {
      final response = await _dio.get<Map<String, dynamic>>('/pickups/$id');
      return response.data;
    } catch (_) {
      return null;
    }
  }

  Future<List<Map<String, dynamic>>> fetchPickupEvents(String pickupId) async {
    try {
      final response = await _dio.get<List<dynamic>>('/pickups/$pickupId/events');
      return (response.data ?? <dynamic>[]).cast<Map<String, dynamic>>();
    } catch (_) {
      return <Map<String, dynamic>>[];
    }
  }

  Future<Map<String, dynamic>?> reschedulePickup(String id, String reason, String requestedBy) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/pickups/$id/reschedule',
        data: <String, String>{
          'pickupRequestId': id,
          'requestedBy': requestedBy,
          'reason': reason,
        },
      );
      return response.data;
    } catch (_) {
      return null;
    }
  }

  // ── Handover ───────────────────────────────────────────

  Future<Map<String, dynamic>?> verifyHandoverCode(
    String pickupId,
    String code,
    String actorId,
  ) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/pickups/$pickupId/handover/verify',
      data: <String, String>{'code': code, 'actorId': actorId},
    );
    return response.data;
  }

  Future<Map<String, dynamic>?> submitHandoverProof(
    String pickupId,
    String proofType,
    String actorId, {
    String? storageKey,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/pickups/$pickupId/handover/proof',
      data: <String, String>{
        'proofType': proofType,
        'actorId': actorId,
        if (storageKey != null) 'storageKey': storageKey,
      },
    );
    return response.data;
  }

  Future<List<Map<String, dynamic>>> fetchHandoverProofs(String pickupId) async {
    try {
      final response = await _dio.get<List<dynamic>>('/pickups/$pickupId/handover');
      return (response.data ?? <dynamic>[]).cast<Map<String, dynamic>>();
    } catch (_) {
      return <Map<String, dynamic>>[];
    }
  }

  // ── Agent ──────────────────────────────────────────────

  Future<Map<String, dynamic>?> prepareCollectionPlan(String pickupRequestId) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/agent/plan',
        data: <String, String>{'pickupRequestId': pickupRequestId},
      );
      return response.data;
    } catch (_) {
      return null;
    }
  }

  Future<Map<String, dynamic>?> approveProposal(
    String proposalId,
    String decision,
    String decidedBy, {
    String? notes,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/agent/proposals/$proposalId/approve',
      data: <String, dynamic>{
        'proposalId': proposalId,
        'decision': decision,
        'decidedBy': decidedBy,
        'notes': notes,
      },
    );
    return response.data;
  }

  /// Check if the API is reachable.
  Future<bool> isApiAvailable() async {
    try {
      await _dio.get<dynamic>('/slots');
      return true;
    } catch (_) {
      return false;
    }
  }
}
