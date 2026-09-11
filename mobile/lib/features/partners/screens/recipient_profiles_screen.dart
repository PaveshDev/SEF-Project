import 'package:flutter/material.dart';

class RecipientProfilesScreen extends StatelessWidget {
  const RecipientProfilesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Recipient Profiles')),
      body: const Center(child: Text('Browse eligible recipient profiles.')),
    );
  }
}
