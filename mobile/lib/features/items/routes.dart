import 'package:go_router/go_router.dart';

import 'screens/my_items_screen.dart';
import 'screens/create_item_screen.dart';
import 'screens/item_details_screen.dart';
import 'screens/edit_item_screen.dart';
import 'screens/add_photo_screen.dart';
import 'screens/condition_answers_screen.dart';
import 'screens/assessment_screen.dart';
import 'screens/clarification_screen.dart';
import 'screens/confirm_assessment_screen.dart';
import 'screens/reassessment_screen.dart';

final List<RouteBase> itemsRoutes = <RouteBase>[
  GoRoute(
    path: '/items',
    builder: (context, state) => const MyItemsScreen(),
    routes: [
      GoRoute(
        path: 'create',
        builder: (context, state) => const CreateItemScreen(),
      ),
      GoRoute(
        path: ':id',
        builder: (context, state) {
          final id = state.pathParameters['id']!;
          return ItemDetailsScreen(itemId: id);
        },
        routes: [
          GoRoute(
            path: 'edit',
            builder: (context, state) {
              final id = state.pathParameters['id']!;
              return EditItemScreen(itemId: id);
            },
          ),
          GoRoute(
            path: 'photos/add',
            builder: (context, state) {
              final id = state.pathParameters['id']!;
              return AddPhotoScreen(itemId: id);
            },
          ),
          GoRoute(
            path: 'condition-answers',
            builder: (context, state) {
              final id = state.pathParameters['id']!;
              return ConditionAnswersScreen(itemId: id);
            },
          ),
          GoRoute(
            path: 'assessments/:assessmentId',
            builder: (context, state) {
              final id = state.pathParameters['id']!;
              final assessmentId = state.pathParameters['assessmentId']!;
              return AssessmentScreen(itemId: id, assessmentId: assessmentId);
            },
            routes: [
              GoRoute(
                path: 'confirm',
                builder: (context, state) {
                  final id = state.pathParameters['id']!;
                  final assessmentId = state.pathParameters['assessmentId']!;
                  return ConfirmAssessmentScreen(itemId: id, assessmentId: assessmentId);
                },
              ),
              GoRoute(
                path: 'reassessment',
                builder: (context, state) {
                  final id = state.pathParameters['id']!;
                  final assessmentId = state.pathParameters['assessmentId']!;
                  return ReassessmentScreen(itemId: id, assessmentId: assessmentId);
                },
              ),
            ],
          ),
          GoRoute(
            path: 'clarifications/:clarificationId',
            builder: (context, state) {
              final id = state.pathParameters['id']!;
              final clarificationId = state.pathParameters['clarificationId']!;
              return ClarificationScreen(itemId: id, clarificationId: clarificationId);
            },
          ),
        ],
      ),
    ],
  ),
];
