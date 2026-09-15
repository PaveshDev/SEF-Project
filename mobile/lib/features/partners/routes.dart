import 'package:go_router/go_router.dart';
import 'screens/recipient_profiles_screen.dart';
import 'screens/match_explanations_screen.dart';
import 'screens/manage_needs_screen.dart';
import 'screens/proposal_review_screen.dart';
import 'screens/handover_status_screen.dart';

final List<RouteBase> partnersRoutes = <RouteBase>[
  GoRoute(
    path: '/partners/profiles',
    builder: (context, state) => const RecipientProfilesScreen(),
  ),
  GoRoute(
    path: '/partners/matches',
    builder: (context, state) => const MatchExplanationsScreen(),
  ),
  GoRoute(
    path: '/partners/needs',
    builder: (context, state) => const ManageNeedsScreen(),
  ),
  GoRoute(
    path: '/partners/proposals',
    builder: (context, state) => const ProposalReviewScreen(),
  ),
  GoRoute(
    path: '/partners/handovers',
    builder: (context, state) => const HandoverStatusScreen(),
  ),
];
