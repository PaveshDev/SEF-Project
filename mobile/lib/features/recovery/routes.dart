import 'package:go_router/go_router.dart';

import 'screens/recovery_screen.dart';

final List<RouteBase> recoveryRoutes = <RouteBase>[
  GoRoute(
    path: '/recovery',
    builder: (context, state) => const RecoveryScreen(),
  ),
];
