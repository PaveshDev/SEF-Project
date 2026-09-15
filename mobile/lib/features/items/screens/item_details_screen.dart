import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:go_router/go_router.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class ItemDetailsScreen extends StatefulWidget {
  final String itemId;

  const ItemDetailsScreen({super.key, required this.itemId});

  @override
  State<ItemDetailsScreen> createState() => _ItemDetailsScreenState();
}

class _ItemDetailsScreenState extends State<ItemDetailsScreen> {
  final ItemsService _itemsService = ItemsService();
  Item? _item;
  String? _errorMessage;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadItem();
  }

  Future<void> _loadItem() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final item = await _itemsService.getItem(widget.itemId);
      setState(() {
        _item = item;
        _isLoading = false;
      });
    } on DioException catch (e) {
      setState(() {
        _isLoading = false;
        if (e.response?.statusCode == 404) {
          _errorMessage = 'Item not found.';
        } else {
          _errorMessage = 'Failed to load item.';
        }
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = 'An unexpected error occurred.';
      });
    }
  }

  Future<void> _submitForAssessment() async {
    try {
      await _itemsService.submitItemForAssessment(widget.itemId);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Item submitted for assessment')),
        );
      }
      _loadItem();
    } on DioException catch (e) {
      final errorMsg = e.response?.data?.toString() ?? 'Failed to submit item';
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(errorMsg), backgroundColor: Colors.red),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Item Details'),
        actions: [
          if (_item != null && (_item!.status == 'Draft' || _item!.status == 'Pending'))
            IconButton(
              icon: const Icon(Icons.edit),
              onPressed: () async {
                final result = await context.push('/items/${_item!.id}/edit');
                if (result == true) {
                  _loadItem();
                }
              },
            ),
        ],
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
            ElevatedButton(
              onPressed: _loadItem,
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }

    if (_item == null) {
      return const Center(child: Text('Item not found.'));
    }

    return RefreshIndicator(
      onRefresh: _loadItem,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildHeaderInfo(),
            const Divider(height: 32),
            _buildPhotosSection(),
            const Divider(height: 32),
            _buildConditionAnswersSection(),
            const Divider(height: 32),
            _buildAssessmentsSection(),
            const SizedBox(height: 32),
            _buildActions(),
          ],
        ),
      ),
    );
  }

  Widget _buildHeaderInfo() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(_item!.title, style: Theme.of(context).textTheme.headlineSmall),
        const SizedBox(height: 8),
        Text('Status: ${_item!.status}', style: const TextStyle(fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        Text('Category: ${_item!.category}'),
        Text('Location: ${_item!.locationArea}'),
        Text('Version: ${_item!.version}'),
        const SizedBox(height: 16),
        Text(_item!.description),
      ],
    );
  }

  Widget _buildPhotosSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text('Photos', style: Theme.of(context).textTheme.titleLarge),
            if (_item!.status == 'Draft')
              TextButton.icon(
                icon: const Icon(Icons.add_a_photo),
                label: const Text('Add'),
                onPressed: () async {
                  final result = await context.push('/items/${_item!.id}/photos/add');
                  if (result == true) {
                    _loadItem();
                  }
                },
              ),
          ],
        ),
        const SizedBox(height: 8),
        if (_item!.photos.isEmpty)
          const Text('No photos added.')
        else
          SizedBox(
            height: 100,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              itemCount: _item!.photos.length,
              itemBuilder: (context, index) {
                final photo = _item!.photos[index];
                return Padding(
                  padding: const EdgeInsets.only(right: 8.0),
                  child: Image.network(
                    photo.imageUrl,
                    width: 100,
                    height: 100,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => Container(
                      width: 100,
                      height: 100,
                      color: Colors.grey[300],
                      child: const Icon(Icons.broken_image),
                    ),
                  ),
                );
              },
            ),
          ),
      ],
    );
  }

  Widget _buildConditionAnswersSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text('Condition Answers', style: Theme.of(context).textTheme.titleLarge),
            if (_item!.status == 'Draft')
              TextButton.icon(
                icon: const Icon(Icons.edit_note),
                label: const Text('Edit'),
                onPressed: () async {
                  final result = await context.push('/items/${_item!.id}/condition-answers');
                  if (result == true) {
                    _loadItem();
                  }
                },
              ),
          ],
        ),
        const SizedBox(height: 8),
        if (_item!.conditionAnswers.isEmpty)
          const Text('No condition answers provided.')
        else
          ..._item!.conditionAnswers.map((a) => ListTile(
                contentPadding: EdgeInsets.zero,
                title: Text(a.questionCode),
                subtitle: Text(a.answer),
              )),
      ],
    );
  }

  Widget _buildAssessmentsSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Assessments', style: Theme.of(context).textTheme.titleLarge),
        const SizedBox(height: 8),
        if (_item!.assessments.isEmpty)
          const Text('No assessments yet.')
        else
          ..._item!.assessments.map((a) {
            final isLatest = _item!.assessments.last.id == a.id;
            return Card(
              margin: const EdgeInsets.only(bottom: 8),
              child: ListTile(
                title: Text('Assessment v${a.version} - ${a.status}'),
                subtitle: Text('Grade: ${a.conditionGrade ?? 'N/A'}'),
                trailing: isLatest ? const Chip(label: Text('Latest')) : null,
                onTap: () async {
                  final result = await context.push('/items/${_item!.id}/assessments/${a.id}');
                  if (result == true) {
                    _loadItem();
                  }
                },
              ),
            );
          }),
      ],
    );
  }

  Widget _buildActions() {
    if (_item!.status == 'Draft') {
      return SizedBox(
        width: double.infinity,
        child: ElevatedButton(
          onPressed: _submitForAssessment,
          child: const Text('Submit for Assessment'),
        ),
      );
    }
    return const SizedBox.shrink();
  }
}
