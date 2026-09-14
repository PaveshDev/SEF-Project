import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:go_router/go_router.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class ConditionAnswersScreen extends StatefulWidget {
  final String itemId;

  const ConditionAnswersScreen({super.key, required this.itemId});

  @override
  State<ConditionAnswersScreen> createState() => _ConditionAnswersScreenState();
}

class _ConditionAnswersScreenState extends State<ConditionAnswersScreen> {
  final _formKey = GlobalKey<FormState>();
  final ItemsService _itemsService = ItemsService();
  bool _isSubmitting = false;

  final List<Map<String, String>> _staticQuestions = [
    {'code': 'POWER', 'text': 'Does the item power on?'},
    {'code': 'DAMAGE', 'text': 'Is there any visible physical damage?'},
  ];

  final Map<String, TextEditingController> _controllers = {};

  @override
  void initState() {
    super.initState();
    for (var q in _staticQuestions) {
      _controllers[q['code']!] = TextEditingController();
    }
    // Ideally we would load existing answers here and pre-fill,
    // but the prompt focuses on standard functionality.
  }

  @override
  void dispose() {
    for (var controller in _controllers.values) {
      controller.dispose();
    }
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isSubmitting = true;
    });

    try {
      final answers = _staticQuestions
          .map((q) => ConditionAnswerDto(
                questionCode: q['code']!,
                questionText: q['text']!,
                answer: _controllers[q['code']]!.text.trim(),
              ))
          .toList();

      final request = SubmitConditionAnswersRequest(answers: answers);

      // Submit
      await _itemsService.submitConditionAnswers(widget.itemId, request);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
              content: Text('Condition answers submitted successfully')),
        );
        context.pop(true);
      }
    } on DioException catch (e) {
      if (mounted) {
        String errorMsg = 'Failed to submit condition answers.';
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
        title: const Text('Condition Answers'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'Please answer the following questions to help us assess your item.',
                style: TextStyle(fontSize: 16),
              ),
              const SizedBox(height: 24),
              ..._staticQuestions.map((q) => Padding(
                    padding: const EdgeInsets.only(bottom: 16.0),
                    child: TextFormField(
                      controller: _controllers[q['code']],
                      decoration: InputDecoration(
                        labelText: q['text'],
                        border: const OutlineInputBorder(),
                      ),
                      maxLines: 2,
                      validator: (value) => value == null || value.isEmpty
                          ? 'This field is required'
                          : null,
                    ),
                  )),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: _isSubmitting ? null : _submit,
                child: _isSubmitting
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Submit Answers'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
