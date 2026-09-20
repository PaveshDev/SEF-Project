import 'package:flutter/material.dart';

class ItemDetailsScreen extends StatelessWidget {
  const ItemDetailsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Item Details')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Old iPhone', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
            const Chip(label: Text('Submitted')),
            const SizedBox(height: 16),
            const Text('Condition Description: Screen is cracked.'),
            const Spacer(),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () {
                  // Trigger Assessment
                },
                child: const Text('Run AI Assessment'),
              ),
            )
          ],
        ),
      ),
    );
  }
}
