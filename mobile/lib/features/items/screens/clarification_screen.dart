import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:go_router/go_router.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class ClarificationScreen extends StatefulWidget {
  final String itemId;
  final String clarificationId;

  const ClarificationScreen({
    super.key,
    required this.itemId,
    required this.clarificationId,
  });

  @override
  State<ClarificationScreen> createState() => _ClarificationScreenState();
}

class _ClarificationScreenState extends State<ClarificationScreen> {
  final _formKey = GlobalKey<FormState>();
  final _answerController = TextEditingController();
  final ItemsService _itemsService = ItemsService();
  bool _isSubmitting = false;

  @override
  void dispose() {
    _answerController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isSubmitting = true;
    });

    try {
      final request = AnswerClarificationRequest(
        answer: _answerController.text.trim(),
      );

      await _itemsService.answerClarification(
        widget.itemId,
        widget.clarificationId,
        request,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Clarification answered successfully')),
        );
        context.pop(true);
      }
    } on DioException catch (e) {
      if (mounted) {
        String errorMsg = 'Failed to answer clarification.';
        if (e.response?.statusCode == 400) {
          errorMsg = 'Validation error: ${e.response?.data}';
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
        title: const Text('Answer Clarification'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'Please provide an answer to the requested clarification.',
                style: TextStyle(fontSize: 16),
              ),
              const SizedBox(height: 24),
              TextFormField(
                controller: _answerController,
                decoration: const InputDecoration(
                  labelText: 'Your Answer',
                  border: OutlineInputBorder(),
                ),
                maxLines: 4,
                validator: (value) => value == null || value.isEmpty
                    ? 'Answer is required'
                    : null,
              ),
              const SizedBox(height: 24),
              ElevatedButton(
                onPressed: _isSubmitting ? null : _submit,
                child: _isSubmitting
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Submit Answer'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
