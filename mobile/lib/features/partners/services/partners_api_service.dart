import 'package:dio/dio.dart';
import '../../../core/network/api_client.dart';

class PartnersApiService {
  // Fetch recipient profiles
  Future<List<dynamic>> getPartners() async {
    try {
      final response = await apiClient.get('/partners');
      return response.data;
    } on DioException catch (e) {
      throw Exception('Failed to load partners: ${e.message}');
    }
  }

  // Fetch proposals for review
  Future<List<dynamic>> getProposals() async {
    try {
      final response = await apiClient.get('/partners/proposals');
      return response.data;
    } on DioException catch (e) {
      throw Exception('Failed to load proposals: ${e.message}');
    }
  }

  // Accept a proposal
  Future<void> acceptProposal(String id) async {
    try {
      await apiClient.post('/partners/proposals/$id/accept');
    } on DioException catch (e) {
      throw Exception('Failed to accept proposal: ${e.message}');
    }
  }

  // Reject a proposal
  Future<void> rejectProposal(String id) async {
    try {
      await apiClient.post('/partners/proposals/$id/reject');
    } on DioException catch (e) {
      throw Exception('Failed to reject proposal: ${e.message}');
    }
  }

  // Fetch handovers
  Future<List<dynamic>> getHandovers() async {
    try {
      final response = await apiClient.get('/partners/handovers');
      return response.data;
    } on DioException catch (e) {
      throw Exception('Failed to load handovers: ${e.message}');
    }
  }
}
