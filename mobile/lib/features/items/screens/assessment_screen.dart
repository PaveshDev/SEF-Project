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
        Text('Version: ${_assessment!.version}', style: Theme.of(context).textTheme.titleMedium),
        const SizedBox(height: 4),
        Text('Status: ${_assessment!.status}', style: const TextStyle(fontWeight: FontWeight.bold)),
        const SizedBox(height: 4),
        Text('Confidence: ${(_assessment!.confidence * 100).toStringAsFixed(1)}%'),
        Text('Date: ${_assessment!.createdAt.toLocal().toString().split('.')[0]}'),
      ],
    );
  }

  Widget _buildAIAssessmentSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('AI ASSESSMENT', style: Theme.of(context).textTheme.titleLarge?.copyWith(color: Colors.blue)),
        const SizedBox(height: 8),
        _buildInfoRow('Suggested Category', _assessment!.suggestedCategory),
        _buildInfoRow('Condition Grade', _assessment!.conditionGrade),
        _buildInfoRow('Condition Summary', _assessment!.conditionSummary),
        _buildInfoRow('Visible Observations', _assessment!.visibleObservations),
        _buildInfoRow('Missing Information', _assessment!.missingInformation),
      ],
    );
  }

  Widget _buildOwnerReportedSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('OWNER REPORTED', style: Theme.of(context).textTheme.titleLarge?.copyWith(color: Colors.green)),
        const SizedBox(height: 8),
        _buildInfoRow('Functionality', _assessment!.ownerReportedFunctionality),
      ],
    );
  }

  Widget _buildInfoRow(String label, String? value) {
    if (value == null || value.isEmpty) return const SizedBox.shrink();
    return Padding(
      padding: const EdgeInsets.only(bottom: 8.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12, color: Colors.grey)),
          Text(value),
        ],
      ),
    );
  }

  Widget _buildClarificationsSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Clarifications', style: Theme.of(context).textTheme.titleLarge),
        const SizedBox(height: 8),
        if (_assessment!.clarifications.isEmpty)
          const Text('No clarifications requested.')
        else
          ..._assessment!.clarifications.map((c) => Card(
                margin: const EdgeInsets.only(bottom: 8),
                child: ListTile(
                  title: Text(c.question),
                  subtitle: Text(c.answer != null && c.answer!.isNotEmpty
                      ? 'Answer: ${c.answer}'
                      : 'Status: ${c.status}'),
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
                      : null,
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
            onPressed: () async {
              final result = await context.push(
                '/items/${widget.itemId}/assessments/${widget.assessmentId}/confirm',
              );
              if (result == true) {
                _loadAssessment();
              }
            },
            child: const Text('Review & Confirm Assessment'),
          ),
          const SizedBox(height: 8),
          OutlinedButton(
            onPressed: () async {
              final result = await context.push(
                '/items/${widget.itemId}/assessments/${widget.assessmentId}/reassessment',
              );
              if (result == true) {
                _loadAssessment();
              }
            },
            child: const Text('Request Reassessment'),
          ),
        ],
      );
    }
    return const SizedBox.shrink();
  }
}
