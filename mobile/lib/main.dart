import 'package:flutter/material.dart';
import 'theme/eco_theme.dart';
import 'screens/my_items_screen.dart';

void main() {
  runApp(const LoopWorthApp());
}

class LoopWorthApp extends StatelessWidget {
  const LoopWorthApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'LoopWorth',
      theme: EcoTheme.lightTheme,
      home: const MyItemsScreen(),
    );
  }
}
