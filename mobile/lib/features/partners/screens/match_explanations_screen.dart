import 'package:flutter/material.dart';

class MatchExplanationsScreen extends StatelessWidget {
  const MatchExplanationsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    // Dummy data for now, since we don't have a direct endpoint for Match results yet.
    final List<Map<String, dynamic>> dummyMatches = [
      {
        'itemName': 'Office Chairs',
        'partnerName': 'Local High School',
        'matchScore': 95,
        'explanation': 'Strong match. School requested seating for computer lab, matching the quantity available.',
      },
      {
        'itemName': 'Used Laptops',
        'partnerName': 'Tech Charity Org',
        'matchScore': 80,
        'explanation': 'Good match. The charity requested electronics, though the exact specs were not specified.',
      },
    ];

    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        title: const Text('Match Explanations', style: TextStyle(fontWeight: FontWeight.bold)),
        elevation: 0,
        backgroundColor: Colors.white,
        foregroundColor: Colors.black87,
      ),
      body: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: dummyMatches.length,
        itemBuilder: (context, index) {
          final match = dummyMatches[index];
          return Card(
            margin: const EdgeInsets.only(bottom: 16),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            child: ExpansionTile(
              leading: CircularProgressIndicator(
                value: match['matchScore'] / 100,
                backgroundColor: Colors.grey[200],
                color: _getScoreColor(match['matchScore']),
              ),
              title: Text(match['itemName'], style: const TextStyle(fontWeight: FontWeight.bold)),
              subtitle: Text('Matched with: ${match['partnerName']}'),
              children: [
                Container(
                  padding: const EdgeInsets.all(16),
                  color: Colors.grey[50],
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Icon(Icons.info_outline, color: Colors.blue),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          match['explanation'],
                          style: const TextStyle(fontSize: 14, height: 1.4),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Color _getScoreColor(int score) {
    if (score >= 90) return Colors.green;
    if (score >= 70) return Colors.orange;
    return Colors.red;
  }
}
