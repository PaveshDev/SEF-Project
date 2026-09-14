import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:dio/dio.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class MyItemsScreen extends StatefulWidget {
  const MyItemsScreen({super.key});

  @override
  State<MyItemsScreen> createState() => _MyItemsScreenState();
}

class _MyItemsScreenState extends State<MyItemsScreen> {
  final ItemsService _itemsService = ItemsService();
  List<ItemSummary>? _items;
  String? _errorMessage;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadItems();
  }

  Future<void> _loadItems() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final items = await _itemsService.getItems();
      setState(() {
        _items = items;
        _isLoading = false;
      });
    } on DioException catch (e) {
      setState(() {
        _isLoading = false;
        if (e.response?.statusCode == 401) {
          _errorMessage = 'Unauthorized. Please login.';
        } else if (e.response?.statusCode == 403) {
          _errorMessage = 'Permission denied.';
        } else {
          _errorMessage = 'Failed to load items. Please try again.';
        }
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
        title: const Text('My Items'),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () async {
              final result = await context.push('/items/create');
              if (result == true) {
                _loadItems();
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
            Text(
              _errorMessage!,
              style: const TextStyle(color: Colors.red),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: _loadItems,
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }

    if (_items == null || _items!.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Text('No items found.'),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () async {
                final result = await context.push('/items/create');
                if (result == true) {
                  _loadItems();
                }
              },
              child: const Text('Create Item'),
            ),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _loadItems,
      child: ListView.builder(
        itemCount: _items!.length,
        itemBuilder: (context, index) {
          final item = _items![index];
          return ListTile(
            title: Text(item.title),
            subtitle: Text('${item.category} • ${item.locationArea}'),
            trailing: Chip(label: Text(item.status)),
            onTap: () async {
              final result = await context.push('/items/${item.id}');
              if (result == true) {
                _loadItems();
              }
            },
          );
        },
      ),
    );
  }
}
