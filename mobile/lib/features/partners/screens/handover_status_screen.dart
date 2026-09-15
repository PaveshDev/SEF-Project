import 'package:flutter/material.dart';
import '../services/partners_api_service.dart';

class HandoverStatusScreen extends StatefulWidget {
  const HandoverStatusScreen({super.key});

  @override
  State<HandoverStatusScreen> createState() => _HandoverStatusScreenState();
}

class _HandoverStatusScreenState extends State<HandoverStatusScreen> {
  final PartnersApiService _apiService = PartnersApiService();
  List<dynamic> _handovers = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadHandovers();
  }

  Future<void> _loadHandovers() async {
    setState(() => _isLoading = true);
    try {
      final handovers = await _apiService.getHandovers();
      setState(() {
        _handovers = handovers;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Failed to load handovers: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        title: const Text('Handover Status', style: TextStyle(fontWeight: FontWeight.bold)),
        elevation: 0,
        backgroundColor: Colors.white,
        foregroundColor: Colors.black87,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadHandovers,
          ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _handovers.isEmpty
              ? const Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(Icons.local_shipping_outlined, size: 64, color: Colors.grey),
                      SizedBox(height: 16),
                      Text('No handovers found.', style: TextStyle(fontSize: 18, color: Colors.grey)),
                    ],
                  ),
                )
              : RefreshIndicator(
                  onRefresh: _loadHandovers,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _handovers.length,
                    itemBuilder: (context, index) {
                      final handover = _handovers[index];
                      final bool isCompleted = handover['status'] == 'Completed';

                      return Card(
                        margin: const EdgeInsets.only(bottom: 16),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                        child: Padding(
                          padding: const EdgeInsets.all(16),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: [
                                  Text(
                                    handover['itemName'] ?? 'Unknown Item',
                                    style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                                  ),
                                  _buildStatusChip(handover['status'] ?? 'Pending'),
                                ],
                              ),
                              const SizedBox(height: 12),
                              _buildInfoRow(Icons.business, 'Donor', handover['donor'] ?? 'Unknown'),
                              const SizedBox(height: 8),
                              _buildInfoRow(
                                isCompleted ? Icons.check_circle : Icons.local_shipping,
                                isCompleted ? 'Delivered On' : 'Est. Delivery',
                                handover['estimatedDelivery'] ?? 'TBD',
                              ),
                              if (!isCompleted) ...[
                                const SizedBox(height: 16),
                                LinearProgressIndicator(
                                  value: 0.6, // Placeholder for progress logic
                                  backgroundColor: Colors.grey[200],
                                  valueColor: const AlwaysStoppedAnimation<Color>(Colors.blue),
                                ),
                                const SizedBox(height: 8),
                                const Text('Driver is on the way', style: TextStyle(fontSize: 12, color: Colors.blue)),
                              ],
                            ],
                          ),
                        ),
                      );
                    },
                  ),
                ),
    );
  }

  Widget _buildStatusChip(String status) {
    Color color = status == 'Completed' ? Colors.green : Colors.blue;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withOpacity(0.5)),
      ),
      child: Text(
        status.toUpperCase(),
        style: TextStyle(color: color, fontSize: 10, fontWeight: FontWeight.bold),
      ),
    );
  }

  Widget _buildInfoRow(IconData icon, String label, String value) {
    return Row(
      children: [
        Icon(icon, size: 16, color: Colors.grey[600]),
        const SizedBox(width: 8),
        Text('$label: ', style: TextStyle(fontSize: 14, color: Colors.grey[600])),
        Text(value, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500)),
      ],
    );
  }
}
