import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:go_router/go_router.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class CreateItemScreen extends StatefulWidget {
  const CreateItemScreen({super.key});

  @override
  State<CreateItemScreen> createState() => _CreateItemScreenState();
}

class _CreateItemScreenState extends State<CreateItemScreen> {
  final _formKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _customCategoryController = TextEditingController();
  final _locationController = TextEditingController();
  
  final ItemsService _itemsService = ItemsService();
  bool _isSubmitting = false;

  String? _selectedCategory;
  final List<String> _standardCategories = [
    'Electronics',
    'Furniture',
    'Appliances',
    'Clothing',
    'Books'
  ];

  @override
  void dispose() {
    _titleController.dispose();
    _descriptionController.dispose();
    _customCategoryController.dispose();
    _locationController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedCategory == null) return;

    setState(() {
      _isSubmitting = true;
    });

    try {
      String finalCategory = _selectedCategory!;
      if (_selectedCategory == 'Other') {
        finalCategory = _customCategoryController.text.trim();
      }

      final request = CreateItemRequest(
        title: _titleController.text.trim(),
        description: _descriptionController.text.trim(),
        category: finalCategory,
        locationArea: _locationController.text.trim(),
      );

      final createdItem = await _itemsService.createItem(request);
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Item created successfully')),
        );
        context.pop(true);
        context.push('/items/${createdItem.id}');
      }
    } on DioException catch (e) {
      if (mounted) {
        String errorMsg = 'Failed to create item.';
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
        title: const Text('Create Item'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              TextFormField(
                controller: _titleController,
                decoration: const InputDecoration(
                  labelText: 'Title *',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.trim().isEmpty) return 'Title is required';
                  if (!RegExp(r'[a-zA-Z]').hasMatch(value)) return 'Title must contain at least one letter.';
                  if (value.trim().length > 200) return 'Title cannot exceed 200 characters';
                  return null;
                },
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _descriptionController,
                decoration: const InputDecoration(
                  labelText: 'Description',
                  border: OutlineInputBorder(),
                ),
                maxLines: 3,
                validator: (value) {
                  if (value != null && value.trim().length > 2000) return 'Description cannot exceed 2000 characters';
                  return null;
                },
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<String>(
                decoration: const InputDecoration(
                  labelText: 'Category *',
                  border: OutlineInputBorder(),
                ),
                value: _selectedCategory,
                items: [
                  ..._standardCategories.map((c) => DropdownMenuItem(value: c, child: Text(c))),
                  const DropdownMenuItem(value: 'Other', child: Text('Other')),
                ],
                onChanged: (val) {
                  setState(() {
                    _selectedCategory = val;
                  });
                },
                validator: (value) => value == null ? 'Category is required' : null,
              ),
              if (_selectedCategory == 'Other') ...[
                const SizedBox(height: 16),
                TextFormField(
                  controller: _customCategoryController,
                  decoration: const InputDecoration(
                    labelText: 'Specify Category *',
                    border: OutlineInputBorder(),
                  ),
                  validator: (value) {
                    if (_selectedCategory == 'Other') {
                      if (value == null || value.trim().isEmpty) return 'Specify Category is required';
                      if (value.trim().length > 100) return 'Category cannot exceed 100 characters';
                    }
                    return null;
                  },
                ),
              ],
              const SizedBox(height: 16),
              TextFormField(
                controller: _locationController,
                decoration: const InputDecoration(
                  labelText: 'Location Area *',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.trim().isEmpty) return 'Location Area is required';
                  if (value.trim().length > 100) return 'Location Area cannot exceed 100 characters';
                  return null;
                },
              ),
              const SizedBox(height: 24),
              ElevatedButton(
                onPressed: _isSubmitting ? null : _submit,
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                ),
                child: _isSubmitting
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Create', style: TextStyle(fontSize: 16)),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
