import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:go_router/go_router.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class EditItemScreen extends StatefulWidget {
  final String itemId;

  const EditItemScreen({super.key, required this.itemId});

  @override
  State<EditItemScreen> createState() => _EditItemScreenState();
}

class _EditItemScreenState extends State<EditItemScreen> {
  final _formKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _customCategoryController = TextEditingController();
  final _locationController = TextEditingController();
  
  final ItemsService _itemsService = ItemsService();
  Item? _item;
  bool _isLoading = true;
  bool _isSubmitting = false;
  String? _errorMessage;

  String? _selectedCategory;
  final List<String> _standardCategories = [
    'Electronics',
    'Furniture',
    'Appliances',
    'Clothing',
    'Books'
  ];

  @override
  void initState() {
    super.initState();
    _loadItem();
  }

  @override
  void dispose() {
    _titleController.dispose();
    _descriptionController.dispose();
    _customCategoryController.dispose();
    _locationController.dispose();
    super.dispose();
  }

  Future<void> _loadItem() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final item = await _itemsService.getItem(widget.itemId);
      _item = item;
      _titleController.text = item.title;
      _descriptionController.text = item.description;
      _locationController.text = item.locationArea;
      
      if (_standardCategories.contains(item.category)) {
        _selectedCategory = item.category;
        _customCategoryController.text = '';
      } else {
        _selectedCategory = 'Other';
        _customCategoryController.text = item.category;
      }
      
      setState(() {
        _isLoading = false;
      });
    } on DioException catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = 'Failed to load item: ${e.response?.statusCode}';
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = 'An unexpected error occurred.';
      });
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate() || _item == null) return;
    if (_selectedCategory == null) return;

    setState(() {
      _isSubmitting = true;
    });

    try {
      String finalCategory = _selectedCategory!;
      if (_selectedCategory == 'Other') {
        finalCategory = _customCategoryController.text.trim();
      }

      final request = UpdateItemRequest(
        id: widget.itemId,
        title: _titleController.text.trim(),
        description: _descriptionController.text.trim(),
        category: finalCategory,
        locationArea: _locationController.text.trim(),
        version: _item!.version,
      );

      await _itemsService.updateItem(widget.itemId, request);
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Item updated successfully')),
        );
        context.pop(true);
      }
    } on DioException catch (e) {
      if (mounted) {
        if (e.response?.statusCode == 409) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Conflict detected: This item was modified by another process. Reloading latest data...'),
              backgroundColor: Colors.orange,
              duration: Duration(seconds: 4),
            ),
          );
          await _loadItem(); // Reload latest
        } else {
          String errorMsg = 'Failed to update item.';
          if (e.response?.statusCode == 400) {
            errorMsg = 'Validation error: ${e.response?.data}';
          }
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(errorMsg), backgroundColor: Colors.red),
          );
        }
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
        title: const Text('Edit Item'),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _errorMessage != null
              ? Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(_errorMessage!, style: const TextStyle(color: Colors.red)),
                      const SizedBox(height: 16),
                      ElevatedButton(onPressed: _loadItem, child: const Text('Retry'))
                    ],
                  ),
                )
              : SingleChildScrollView(
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
                              : const Text('Save Changes', style: TextStyle(fontSize: 16)),
                        ),
                      ],
                    ),
                  ),
                ),
    );
  }
}
