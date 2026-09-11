import 'package:flutter/material.dart';

class ManageNeedsScreen extends StatelessWidget {
  const ManageNeedsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Manage Needs')),
      body: const Center(child: Text('Partners manage their needs here.')),
    );
  }
}
