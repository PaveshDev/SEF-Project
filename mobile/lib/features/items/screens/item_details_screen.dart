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
          if (_item != null && (_item!.status == 'Draft' || _item!.status == 'Withdrawn'))
            IconButton(
              icon: const Icon(Icons.delete, color: Colors.redAccent),
              onPressed: () async {
                final confirm = await showDialog<bool>(
                  context: context,
                  builder: (context) => AlertDialog(
                    title: const Text('Delete Item'),
                    content: const Text('Are you sure you want to delete this item? This action cannot be undone.'),
                    actions: [
                      TextButton(
                        onPressed: () => Navigator.pop(context, false),
                        child: const Text('Cancel'),
                      ),
                      TextButton(
                        onPressed: () => Navigator.pop(context, true),
                        style: TextButton.styleFrom(foregroundColor: Colors.red),
                        child: const Text('Delete'),
                      ),
                    ],
                  ),
                );

                if (confirm == true) {
                  try {
                    await _itemsService.deleteItem(widget.itemId);
                    if (mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Item deleted successfully')),
                      );
                      context.pop(true);
                    }
                  } catch (e) {
                    if (mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Failed to delete item'), backgroundColor: Colors.red),
                      );
                    }
                  }
                }
              },
            ),
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
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.02),
            blurRadius: 10,
            offset: const Offset(0, 4),
          )
        ]
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Text(
                  _item!.title,
                  style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: Color(0xFF111827)),
                ),
              ),
              const SizedBox(width: 12),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: _getStatusBgColor(_item!.status),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  _item!.status,
                  style: TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    color: _getStatusTextColor(_item!.status),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          _buildInfoRow(Icons.category, 'Category', _item!.category),
          const SizedBox(height: 8),
          _buildInfoRow(Icons.location_on, 'Location', _item!.locationArea),
          const SizedBox(height: 16),
          const Text('Description', style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: Colors.grey, letterSpacing: 0.5)),
          const SizedBox(height: 4),
          Text(_item!.description, style: const TextStyle(fontSize: 15, color: Color(0xFF374151), height: 1.5)),
        ],
      ),
    );
  }

  Widget _buildInfoRow(IconData icon, String label, String value) {
    return Row(
      children: [
        Icon(icon, size: 18, color: Colors.grey.shade600),
        const SizedBox(width: 8),
        Text('$label:', style: TextStyle(fontSize: 14, color: Colors.grey.shade600)),
        const SizedBox(width: 4),
        Text(value, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500, color: Color(0xFF111827))),
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
            const Text('Photos', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF111827))),
            if (_item!.status == 'Draft')
              TextButton.icon(
                icon: const Icon(Icons.add_a_photo, size: 18),
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
          const Text('No photos added.', style: TextStyle(color: Colors.grey, fontStyle: FontStyle.italic))
        else
          SizedBox(
            height: 120,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              itemCount: _item!.photos.length,
              itemBuilder: (context, index) {
                final photo = _item!.photos[index];
                return Padding(
                  padding: const EdgeInsets.only(right: 12.0),
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(12),
                    child: Image.network(
                      photo.imageUrl,
                      width: 120,
                      height: 120,
                      fit: BoxFit.cover,
                      errorBuilder: (_, __, ___) => Container(
                        width: 120,
                        height: 120,
                        color: Colors.grey[200],
                        child: const Icon(Icons.broken_image, color: Colors.grey),
                      ),
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
            const Text('Condition Answers', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF111827))),
            if (_item!.status == 'Draft')
              TextButton.icon(
                icon: const Icon(Icons.edit_note, size: 18),
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
        const SizedBox(height: 12),
        if (_item!.conditionAnswers.isEmpty)
          const Text('No condition answers provided.', style: TextStyle(color: Colors.grey, fontStyle: FontStyle.italic))
        else
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: const Color(0xFFF9FAFB),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: Colors.grey.shade200),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: _item!.conditionAnswers.map((a) => Padding(
                padding: const EdgeInsets.only(bottom: 12.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(a.questionCode, style: const TextStyle(fontWeight: FontWeight.w600, color: Color(0xFF374151), fontSize: 14)),
                    const SizedBox(height: 4),
                    Text(a.answer, style: const TextStyle(color: Color(0xFF111827))),
                  ],
                ),
              )).toList(),
            ),
          ),
      ],
    );
  }

  Widget _buildAssessmentsSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Assessments', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF111827))),
        const SizedBox(height: 12),
        if (_item!.assessments.isEmpty)
          const Text('No assessments yet.', style: TextStyle(color: Colors.grey, fontStyle: FontStyle.italic))
        else
          ..._item!.assessments.map((a) {
            final isLatest = _item!.assessments.last.id == a.id;
            return Card(
              elevation: 0,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
                side: BorderSide(color: isLatest ? Colors.blue.shade200 : Colors.grey.shade200),
              ),
              color: isLatest ? const Color(0xFFEFF6FF) : Colors.white,
              margin: const EdgeInsets.only(bottom: 12),
              child: ListTile(
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                title: Text('Assessment v${a.version}', style: const TextStyle(fontWeight: FontWeight.bold, color: Color(0xFF1E3A8A))),
                subtitle: Text('Status: ${a.status}\nGrade: ${a.conditionGrade ?? 'N/A'}', style: const TextStyle(color: Color(0xFF3B82F6))),
                trailing: isLatest ? Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(color: Colors.blue.shade100, borderRadius: BorderRadius.circular(12)),
                  child: const Text('Latest', style: TextStyle(fontSize: 12, color: Colors.blue, fontWeight: FontWeight.bold)),
                ) : null,
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

  Color _getStatusBgColor(String status) {
    switch (status.toLowerCase()) {
      case 'draft': return const Color(0xFFF3F4F6);
      case 'pending': return const Color(0xFFFEF3C7);
      case 'assessing': return const Color(0xFFDBEAFE);
      case 'assessed': return const Color(0xFFF3E8FF);
      case 'approved': return const Color(0xFFD1FAE5);
      case 'rejected': return const Color(0xFFFEE2E2);
      default: return const Color(0xFFF3F4F6);
    }
  }

  Color _getStatusTextColor(String status) {
    switch (status.toLowerCase()) {
      case 'draft': return const Color(0xFF4B5563);
      case 'pending': return const Color(0xFFB45309);
      case 'assessing': return const Color(0xFF1D4ED8);
      case 'assessed': return const Color(0xFF7E22CE);
      case 'approved': return const Color(0xFF047857);
      case 'rejected': return const Color(0xFFB91C1C);
      default: return const Color(0xFF4B5563);
    }
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
