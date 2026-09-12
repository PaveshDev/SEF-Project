import 'package:flutter/material.dart';

class HandoverStatusScreen extends StatelessWidget {
  const HandoverStatusScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Handover Status')),
      body: const Center(child: Text('View handover acceptance status.')),
    );
  }
}
