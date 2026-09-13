import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:go_router/go_router.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class ConfirmAssessmentScreen extends StatefulWidget {
  final String itemId;
  final String assessmentId;

  const ConfirmAssessmentScreen({
    super.key,
    required this.itemId,
    required this.assessmentId,
  });

  @override
  State<ConfirmAssessmentScreen> createState() => _ConfirmAssessmentScreenState();
}

class _ConfirmAssessmentScreenState extends State<ConfirmAssessmentScreen> {
  final ItemsService _itemsService = ItemsService();
  bool _isSubmitting = false;

  Future<void> _submit(bool accepted) async {
    setState(() {
      _isSubmitting = true;
    });

    try {
      final request = ConfirmAssessmentRequest(accepted: accepted);
      await _itemsService.confirmAssessment(widget.itemId, widget.assessmentId, request);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(accepted ? 'Assessment confirmed' : 'Assessment rejected')),
        );
        context.pop(true);
      }
    } on DioException catch (e) {
      if (mounted) {
        String errorMsg = 'Failed to confirm assessment.';
        if (e.response?.statusCode == 400) {
          errorMsg = 'Validation error: ${e.response?.data}';
        } else if (e.response?.statusCode == 409) {
          errorMsg = 'Conflict: Assessment state has changed.';
        }
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(errorMsg), backgroundColor: Colors.red),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Confirm Assessment'),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const Text(
              'Do you accept the findings of this AI assessment?',
              style: TextStyle(fontSize: 18),
            ),
            const SizedBox(height: 32),
            if (_isSubmitting)
              const Center(child: CircularProgressIndicator())
            else ...[
              ElevatedButton(
                onPressed: () => _submit(true),
                style: ElevatedButton.styleFrom(backgroundColor: Colors.green),
                child: const Text('Accept Assessment'),
              ),
              const SizedBox(height: 16),
              OutlinedButton(
                onPressed: () => _submit(false),
                child: const Text('Reject Assessment', style: TextStyle(color: Colors.red)),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
