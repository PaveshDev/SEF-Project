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

  final Map<String, String?> _choices = {};
  final Map<String, TextEditingController> _detailsControllers = {};

  @override
  void initState() {
    super.initState();
    for (var q in _staticQuestions) {
      _choices[q['code']!] = null;
      _detailsControllers[q['code']!] = TextEditingController();
    }
  }

  @override
  void dispose() {
    for (var controller in _detailsControllers.values) {
      controller.dispose();
    }
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    // Validate choices manually since ChoiceChip doesn't hook into Form validation easily
    bool allSelected = true;
    for (var q in _staticQuestions) {
      if (_choices[q['code']] == null) {
        allSelected = false;
        break;
      }
    }
    if (!allSelected) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select Yes, No, or Unknown for all questions')),
      );
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    try {
      final answers = _staticQuestions.map((q) {
        final choice = _choices[q['code']]!;
        final details = _detailsControllers[q['code']]!.text.trim();
        final finalAnswer = details.isNotEmpty ? '$choice\nDetails: $details' : choice;
        
        return ConditionAnswerDto(
          questionCode: q['code']!,
          questionText: q['text']!,
          answer: finalAnswer,
        );
      }).toList();

      final request = SubmitConditionAnswersRequest(answers: answers);

      // Submit
      await _itemsService.submitConditionAnswers(widget.itemId, request);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Condition answers submitted successfully')),
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
                    padding: const EdgeInsets.only(bottom: 24.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Text(
                          q['text']!,
                          style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: Color(0xFF374151)),
                        ),
                        const SizedBox(height: 12),
                        Wrap(
                          spacing: 8.0,
                          children: ['Yes', 'No', 'Unknown'].map((option) {
                            final isSelected = _choices[q['code']!] == option;
                            return ChoiceChip(
                              label: Text(option),
                              selected: isSelected,
                              onSelected: (selected) {
                                setState(() {
                                  if (selected) _choices[q['code']!] = option;
                                });
                              },
                              selectedColor: const Color(0xFFEFF6FF),
                              backgroundColor: Colors.white,
                              labelStyle: TextStyle(
                                color: isSelected ? const Color(0xFF1D4ED8) : const Color(0xFF4B5563),
                                fontWeight: isSelected ? FontWeight.w600 : FontWeight.w400,
                              ),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8),
                                side: BorderSide(
                                  color: isSelected ? const Color(0xFF3B82F6) : const Color(0xFFE5E7EB),
                                ),
                              ),
                            );
                          }).toList(),
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          controller: _detailsControllers[q['code']],
                          decoration: const InputDecoration(
                            labelText: 'Additional details (optional)',
                            border: OutlineInputBorder(),
                            isDense: true,
                          ),
                          maxLines: 2,
                        ),
                      ],
                    ),
                  )),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: _isSubmitting ? null : _submit,
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF10B981),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                ),
                child: _isSubmitting
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                      )
                    : const Text('Save Answers', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
