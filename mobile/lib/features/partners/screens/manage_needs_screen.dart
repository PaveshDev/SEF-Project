import 'package:flutter/material.dart';
import '../services/api_service.dart';

class ManageNeedsScreen extends StatefulWidget {
  const ManageNeedsScreen({super.key});

  @override
  State<ManageNeedsScreen> createState() => _ManageNeedsScreenState();
}

class _ManageNeedsScreenState extends State<ManageNeedsScreen> {
  final PartnersApiService _apiService = PartnersApiService();
  List<dynamic> _needs = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadNeeds();
  }

  Future<void> _loadNeeds() async {
    setState(() => _isLoading = true);
    try {
      final needs = await _apiService.getRecipientNeeds();
      setState(() {
        _needs = needs;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Failed to load needs: $e')),
        );
      }
    }
  }

  void _showAddNeedDialog() {
    showDialog(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Add Need'),
          content: const Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(decoration: InputDecoration(labelText: 'Description')),
              TextField(decoration: InputDecoration(labelText: 'Quantity'), keyboardType: TextInputType.number),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () {
                // TODO: Wire up actual POST request
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Need added (mock)')));
              },
              child: const Text('Add'),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        title: const Text('Manage Needs', style: TextStyle(fontWeight: FontWeight.bold)),
        elevation: 0,
        backgroundColor: Colors.white,
        foregroundColor: Colors.black87,
        actions: [
          IconButton(icon: const Icon(Icons.refresh), onPressed: _loadNeeds),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _needs.isEmpty
              ? const Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(Icons.inventory_2_outlined, size: 64, color: Colors.grey),
                      SizedBox(height: 16),
                      Text('No needs requested yet.', style: TextStyle(fontSize: 18, color: Colors.grey)),
                    ],
                  ),
                )
              : RefreshIndicator(
                  onRefresh: _loadNeeds,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _needs.length,
                    itemBuilder: (context, index) {
                      final need = _needs[index];
                      return Card(
                        margin: const EdgeInsets.only(bottom: 12),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                        child: ListTile(
                          contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                          title: Text(need['description'] ?? 'Unnamed Need', style: const TextStyle(fontWeight: FontWeight.bold)),
                          subtitle: Text('Required: ${need['quantityRequired']} | Fulfilled: ${need['quantityFulfilled'] ?? 0}'),
                          trailing: _buildStatusChip(need['status'] ?? 'Open'),
                          onTap: () {
                            // Edit need functionality placeholder
                          },
                        ),
                      );
                    },
                  ),
                ),
      floatingActionButton: FloatingActionButton(
        onPressed: _showAddNeedDialog,
        child: const Icon(Icons.add),
      ),
    );
  }

  Widget _buildStatusChip(String status) {
    Color color;
    switch (status.toLowerCase()) {
      case 'open':
        color = Colors.blue;
        break;
      case 'fulfilled':
        color = Colors.green;
        break;
      default:
        color = Colors.grey;
    }
    return Chip(
      label: Text(status.toUpperCase(), style: TextStyle(color: color, fontSize: 10, fontWeight: FontWeight.bold)),
      backgroundColor: color.withOpacity(0.1),
      side: BorderSide(color: color.withOpacity(0.5)),
    );
  }
}
