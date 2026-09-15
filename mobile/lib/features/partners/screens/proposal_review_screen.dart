import 'package:flutter/material.dart';

class ProposalReviewScreen extends StatelessWidget {
  const ProposalReviewScreen({super.key});

  @override
  Widget build(BuildContext context) {
    // Dummy data for proposals, as specific endpoint might be missing.
    final List<Map<String, dynamic>> dummyProposals = [
      {
        'id': 'prop_001',
        'donorName': 'Tech Corp Inc.',
        'itemName': '20 Office Desks',
        'status': 'Pending Review',
        'date': '2026-09-14',
      },
      {
        'id': 'prop_002',
        'donorName': 'Local Logistics',
        'itemName': 'Used Delivery Van',
        'status': 'Pending Review',
        'date': '2026-09-15',
      }
    ];

    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        title: const Text('Proposal Review', style: TextStyle(fontWeight: FontWeight.bold)),
        elevation: 0,
        backgroundColor: Colors.white,
        foregroundColor: Colors.black87,
      ),
      body: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: dummyProposals.length,
        itemBuilder: (context, index) {
          final proposal = dummyProposals[index];
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
                        proposal['itemName'],
                        style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                      ),
                      Text(
                        proposal['date'],
                        style: TextStyle(color: Colors.grey[600], fontSize: 12),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Text('Donor: ${proposal['donorName']}', style: const TextStyle(fontSize: 14)),
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      Expanded(
                        child: OutlinedButton(
                          onPressed: () {
                            // Reject Logic
                            ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Proposal Rejected')));
                          },
                          style: OutlinedButton.styleFrom(
                            foregroundColor: Colors.red,
                            side: const BorderSide(color: Colors.red),
                          ),
                          child: const Text('Reject'),
                        ),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: ElevatedButton(
                          onPressed: () {
                            // Accept Logic
                            ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Proposal Accepted')));
                          },
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.green,
                          ),
                          child: const Text('Accept', style: TextStyle(color: Colors.white)),
                        ),
                      ),
                    ],
                  )
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
