import 'package:flutter/material.dart';

import 'router.dart';

class WasteToValueApp extends StatelessWidget {
  const WasteToValueApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'Waste-to-Value',
      debugShowCheckedModeBanner: false,
      routerConfig: appRouter,
    );
  }
}
