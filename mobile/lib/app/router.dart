import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../features/items/routes.dart';
import '../features/recovery/routes.dart';
import '../features/partners/routes.dart';
import '../features/collections/routes.dart';

final GoRouter appRouter = GoRouter(
  routes: <RouteBase>[
    GoRoute(
      path: '/',
      builder: (context, state) => const Scaffold(
        body: SafeArea(
          child: Padding(
            padding: EdgeInsets.all(16),
            child: Text('Waste-to-Value — project skeleton'),
          ),
        ),
      ),
    ),
    ...itemsRoutes,
    ...recoveryRoutes,
    ...partnersRoutes,
    ...collectionsRoutes,
  ],
);
