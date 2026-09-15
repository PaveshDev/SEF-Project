import 'package:flutter/material.dart';

class ProposalReviewScreen extends StatelessWidget {
  const ProposalReviewScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Review Proposal')),
      body: const Center(child: Text('Accept or reject proposed handovers.')),
    );
  }
}
