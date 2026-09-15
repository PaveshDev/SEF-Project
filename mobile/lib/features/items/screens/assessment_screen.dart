import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:go_router/go_router.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class AssessmentScreen extends StatefulWidget {
  final String itemId;
  final String assessmentId;

  const AssessmentScreen({
    super.key,
    required this.itemId,
    required this.assessmentId,
  });

  @override
  State<AssessmentScreen> createState() => _AssessmentScreenState();
}

class _AssessmentScreenState extends State<AssessmentScreen> {
  final ItemsService _itemsService = ItemsService();
  ItemAssessment? _assessment;
  String? _errorMessage;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadAssessment();
  }

  Future<void> _loadAssessment() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final assessment = await _itemsService.getAssessment(widget.itemId, widget.assessmentId);
      setState(() {
        _assessment = assessment;
        _isLoading = false;
      });
    } on DioException catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = 'Failed to load assessment. ${e.response?.statusCode ?? ''}';
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = 'An unexpected error occurred.';
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Assessment Details'),
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_errorMessage != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(_errorMessage!, style: const TextStyle(color: Colors.red)),
            const SizedBox(height: 16),
            ElevatedButton(onPressed: _loadAssessment, child: const Text('Retry')),
          ],
        ),
      );
    }

    if (_assessment == null) {
      return const Center(child: Text('Assessment not found.'));
    }

    return RefreshIndicator(
      onRefresh: _loadAssessment,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildGeneralInfo(),
            const Divider(height: 32),
            _buildAIAssessmentSection(),
            const Divider(height: 32),
            _buildOwnerReportedSection(),
            const Divider(height: 32),
            _buildClarificationsSection(),
            const SizedBox(height: 32),
            _buildActions(),
          ],
        ),
      ),
    );
  }

  Widget _buildGeneralInfo() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text('Version: ${_assessment!.version}', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF111827))),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(color: Colors.blue.shade100, borderRadius: BorderRadius.circular(12)),
              child: Text(_assessment!.status, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.blue)),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Text('Confidence: ${(_assessment!.confidence * 100).toStringAsFixed(1)}%', style: const TextStyle(color: Color(0xFF4B5563))),
        Text('Date: ${_assessment!.createdAt.toLocal().toString().split('.')[0]}', style: const TextStyle(color: Color(0xFF4B5563))),
      ],
    );
  }

  Widget _buildAIAssessmentSection() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFFEFF6FF), // blue.shade50
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.blue.shade200),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Text('🤖', style: TextStyle(fontSize: 24)),
              const SizedBox(width: 8),
              const Text('AI OBSERVATIONS', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Color(0xFF1E3A8A))),
            ],
          ),
          const SizedBox(height: 16),
          _buildInfoRow('Suggested Category', _assessment!.suggestedCategory, const Color(0xFF1E3A8A)),
          _buildInfoRow('Condition Grade', _assessment!.conditionGrade, const Color(0xFF1E3A8A)),
          _buildInfoRow('Condition Summary', _assessment!.conditionSummary, const Color(0xFF1E3A8A)),
          _buildInfoRow('Visible Observations', _assessment!.visibleObservations, const Color(0xFF1E3A8A)),
          _buildInfoRow('Missing Information', _assessment!.missingInformation, const Color(0xFF1E3A8A)),
        ],
      ),
    );
  }

  Widget _buildOwnerReportedSection() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFFF0FDF4), // green.shade50
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.green.shade200),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Text('👤', style: TextStyle(fontSize: 24)),
              const SizedBox(width: 8),
              const Text('OWNER REPORTED', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Color(0xFF065F46))),
            ],
          ),
          const SizedBox(height: 16),
          _buildInfoRow('Functionality', _assessment!.ownerReportedFunctionality, const Color(0xFF065F46)),
        ],
      ),
    );
  }

  Widget _buildInfoRow(String label, String? value, Color textColor) {
    if (value == null || value.isEmpty) return const SizedBox.shrink();
    return Padding(
      padding: const EdgeInsets.only(bottom: 12.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label.toUpperCase(), style: TextStyle(fontWeight: FontWeight.w600, fontSize: 12, color: textColor.withOpacity(0.7))),
          const SizedBox(height: 4),
          Text(value, style: TextStyle(color: textColor, fontWeight: FontWeight.w500)),
        ],
      ),
    );
  }

  Widget _buildClarificationsSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Clarifications', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF111827))),
        const SizedBox(height: 12),
        if (_assessment!.clarifications.isEmpty)
          const Text('No clarifications requested.', style: TextStyle(color: Colors.grey, fontStyle: FontStyle.italic))
        else
          ..._assessment!.clarifications.map((c) => Card(
                elevation: 0,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                  side: BorderSide(color: Colors.grey.shade300),
                ),
                margin: const EdgeInsets.only(bottom: 8),
                child: ListTile(
                  title: Text(c.question, style: const TextStyle(fontWeight: FontWeight.w500)),
                  subtitle: Padding(
                    padding: const EdgeInsets.only(top: 4.0),
                    child: Text(c.answer != null && c.answer!.isNotEmpty
                        ? 'Answer: ${c.answer}'
                        : 'Status: ${c.status}'),
                  ),
                  trailing: (c.status == 'Pending')
                      ? ElevatedButton(
                          onPressed: () async {
                            final result = await context.push(
                              '/items/${widget.itemId}/clarifications/${c.id}',
                            );
                            if (result == true) {
                              _loadAssessment();
                            }
                          },
                          child: const Text('Answer'),
                        )
                      : const Icon(Icons.check_circle, color: Colors.green),
                ),
              )),
      ],
    );
  }

  Widget _buildActions() {
    if (_assessment!.status == 'PendingConfirmation') {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF10B981), // emerald-500
              foregroundColor: Colors.white,
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            onPressed: () async {
              final result = await context.push(
                '/items/${widget.itemId}/assessments/${widget.assessmentId}/confirm',
              );
              if (result == true) {
                _loadAssessment();
              }
            },
            child: const Text('✓ Confirm Assessment', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          ),
          const SizedBox(height: 12),
          OutlinedButton(
            style: OutlinedButton.styleFrom(
              foregroundColor: const Color(0xFFF59E0B), // amber-500
              side: const BorderSide(color: Color(0xFFF59E0B)),
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            onPressed: () async {
              final result = await context.push(
                '/items/${widget.itemId}/assessments/${widget.assessmentId}/reassessment',
              );
              if (result == true) {
                _loadAssessment();
              }
            },
            child: const Text('↺ Request Reassessment', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          ),
        ],
      );
    }
    return const SizedBox.shrink();
  }
}
