import 'package:flutter/material.dart';

class HandoverStatusScreen extends StatelessWidget {
  const HandoverStatusScreen({super.key});

  @override
  Widget build(BuildContext context) {
    // Dummy data for handovers
    final List<Map<String, dynamic>> dummyHandovers = [
      {
        'id': 'ho_101',
        'itemName': 'Office Desks (20)',
        'donor': 'Tech Corp Inc.',
        'status': 'In Transit',
        'estimatedDelivery': '2026-09-16',
      },
      {
        'id': 'ho_102',
        'itemName': 'Used Delivery Van',
        'donor': 'Local Logistics',
        'status': 'Completed',
        'estimatedDelivery': '2026-09-10',
      }
    ];

    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        title: const Text('Handover Status', style: TextStyle(fontWeight: FontWeight.bold)),
        elevation: 0,
        backgroundColor: Colors.white,
        foregroundColor: Colors.black87,
      ),
      body: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: dummyHandovers.length,
        itemBuilder: (context, index) {
          final handover = dummyHandovers[index];
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
                        handover['itemName'],
                        style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                      ),
                      _buildStatusChip(handover['status']),
                    ],
                  ),
                  const SizedBox(height: 12),
                  _buildInfoRow(Icons.business, 'Donor', handover['donor']),
                  const SizedBox(height: 8),
                  _buildInfoRow(
                    isCompleted ? Icons.check_circle : Icons.local_shipping,
                    isCompleted ? 'Delivered On' : 'Est. Delivery',
                    handover['estimatedDelivery'],
                  ),
                  if (!isCompleted) ...[
                    const SizedBox(height: 16),
                    LinearProgressIndicator(
                      value: 0.6,
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
