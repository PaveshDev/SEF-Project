import 'package:go_router/go_router.dart';

import 'collections_screen.dart';

final List<RouteBase> collectionsRoutes = <RouteBase>[
  GoRoute(
    path: '/collections',
    builder: (context, state) => const CollectionsScreen(),
  ),
];
