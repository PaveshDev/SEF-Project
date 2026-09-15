import 'package:flutter/material.dart';

class MatchExplanationsScreen extends StatelessWidget {
  const MatchExplanationsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Match Explanations')),
      body: const Center(child: Text('View explanations for matched items.')),
    );
  }
}
