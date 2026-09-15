import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';

import 'package:waste_to_value/features/items/screens/my_items_screen.dart';
import 'package:waste_to_value/features/items/screens/create_item_screen.dart';
import 'package:waste_to_value/features/items/screens/item_details_screen.dart';
import 'package:waste_to_value/features/items/screens/edit_item_screen.dart';
import 'package:waste_to_value/features/items/screens/condition_answers_screen.dart';
import 'package:waste_to_value/features/items/screens/assessment_screen.dart';
import 'package:waste_to_value/features/items/screens/clarification_screen.dart';
import 'package:waste_to_value/features/items/screens/confirm_assessment_screen.dart';
import 'package:waste_to_value/features/items/screens/reassessment_screen.dart';
import 'package:waste_to_value/features/items/screens/add_photo_screen.dart';
import 'package:waste_to_value/core/network/api_client.dart';

// Simple Mock HttpClientAdapter for Dio
class MockDioAdapter implements HttpClientAdapter {
  final Future<ResponseBody> Function(RequestOptions options) onFetch;

  MockDioAdapter(this.onFetch);

  @override
  Future<ResponseBody> fetch(RequestOptions options, Stream<Uint8List>? requestStream, Future<void>? cancelFuture) {
    return onFetch(options);
  }
  
  @override
  void close({bool force = false}) {}
}

void main() {
  late MockDioAdapter mockAdapter;

  setUp(() {
    mockAdapter = MockDioAdapter((options) async {
      return ResponseBody.fromString('', 404, headers: {
        Headers.contentTypeHeader: ['application/json']
      });
    });
    apiClient.httpClientAdapter = mockAdapter;
  });

  Widget createTestApp(Widget home) {
    return MaterialApp(
      home: home,
    );
  }
  
  Widget createRouterApp(String initialLocation) {
    final router = GoRouter(
      initialLocation: '/',
      routes: [
        GoRoute(path: '/', builder: (context, state) => const Scaffold(body: Text('Home'))),
        GoRoute(path: '/items', builder: (context, state) => const MyItemsScreen()),
        GoRoute(path: '/items/create', builder: (context, state) => const CreateItemScreen()),
        GoRoute(path: '/items/:id', builder: (context, state) => ItemDetailsScreen(itemId: state.pathParameters['id']!)),
        GoRoute(path: '/items/:id/edit', builder: (context, state) => EditItemScreen(itemId: state.pathParameters['id']!)),
        GoRoute(path: '/items/:id/condition-answers', builder: (context, state) => ConditionAnswersScreen(itemId: state.pathParameters['id']!)),
        GoRoute(path: '/items/:id/assessments/:assessmentId', builder: (context, state) => AssessmentScreen(itemId: state.pathParameters['id']!, assessmentId: state.pathParameters['assessmentId']!)),
        GoRoute(path: '/items/:id/clarifications/:clarificationId', builder: (context, state) => ClarificationScreen(itemId: state.pathParameters['id']!, clarificationId: state.pathParameters['clarificationId']!)),
        GoRoute(path: '/items/:id/assessments/:assessmentId/confirm', builder: (context, state) => ConfirmAssessmentScreen(itemId: state.pathParameters['id']!, assessmentId: state.pathParameters['assessmentId']!)),
        GoRoute(path: '/items/:id/assessments/:assessmentId/reassessment', builder: (context, state) => ReassessmentScreen(itemId: state.pathParameters['id']!, assessmentId: state.pathParameters['assessmentId']!)),
      ],
    );
    WidgetsBinding.instance.addPostFrameCallback((_) {
      router.push(initialLocation);
    });
    return MaterialApp.router(
      routerConfig: router,
    );
  }

  testWidgets('1. Item list displays returned items', (WidgetTester tester) async {
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items') {
        final data = [
          {'id': '1', 'title': 'Laptop', 'category': 'Electronics', 'locationArea': 'Room A', 'status': 'Draft', 'createdAt': '2026-09-10T12:00:00Z'}
        ];
        return ResponseBody.fromString(jsonEncode(data), 200, headers: {Headers.contentTypeHeader: ['application/json']});
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createTestApp(const MyItemsScreen()));
    await tester.pumpAndSettle();

    expect(find.text('Laptop'), findsOneWidget);
    expect(find.text('Electronics • Room A'), findsOneWidget);
  });

  testWidgets('2. Empty item list displays empty state', (WidgetTester tester) async {
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items') {
        return ResponseBody.fromString('[]', 200, headers: {Headers.contentTypeHeader: ['application/json']});
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createTestApp(const MyItemsScreen()));
    await tester.pumpAndSettle();

    expect(find.text('No items found.'), findsOneWidget);
  });

  testWidgets('3. Create item form submits correct request', (WidgetTester tester) async {
    bool apiCalled = false;
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items' && options.method == 'POST') {
        apiCalled = true;
        final data = {'id': '2', 'title': 'Phone', 'category': 'Electronics', 'locationArea': 'A', 'status': 'Draft', 'createdAt': '2026-09-10T12:00:00Z'};
        return ResponseBody.fromString(jsonEncode(data), 200, headers: {Headers.contentTypeHeader: ['application/json']});
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createRouterApp('/items/create'));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextFormField).at(0), 'Phone');
    await tester.enterText(find.byType(TextFormField).at(1), 'A good phone');
    await tester.enterText(find.byType(TextFormField).at(2), 'Electronics');
    await tester.enterText(find.byType(TextFormField).at(3), 'Room A');
    
    await tester.tap(find.text('Create'));
    await tester.pumpAndSettle();

    expect(apiCalled, true);
    expect(find.text('Item created successfully'), findsOneWidget);
  });

  testWidgets('4. Item details display correctly', (WidgetTester tester) async {
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items/1') {
        final data = {
          'id': '1', 'ownerId': 'user1', 'title': 'Laptop', 'description': 'desc', 'category': 'Electronics', 
          'locationArea': 'Room A', 'status': 'Draft', 'createdAt': '2026-09-10T12:00:00Z', 'version': 1,
          'photos': [], 'conditionAnswers': [], 'assessments': []
        };
        return ResponseBody.fromString(jsonEncode(data), 200, headers: {Headers.contentTypeHeader: ['application/json']});
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createTestApp(const ItemDetailsScreen(itemId: '1')));
    await tester.pumpAndSettle();

    expect(find.text('Laptop'), findsOneWidget);
    expect(find.text('Status: Draft'), findsOneWidget);
  });

  testWidgets('5. 400 validation error is displayed', (WidgetTester tester) async {
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items' && options.method == 'POST') {
        return ResponseBody.fromString('Title too short', 400, headers: {Headers.contentTypeHeader: ['text/plain']});
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createRouterApp('/items/create'));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextFormField).at(0), 'T');
    await tester.enterText(find.byType(TextFormField).at(1), 'desc');
    await tester.enterText(find.byType(TextFormField).at(2), 'cat');
    await tester.enterText(find.byType(TextFormField).at(3), 'loc');
    
    await tester.tap(find.text('Create'));
    await tester.pumpAndSettle();

    expect(find.textContaining('Validation error:'), findsOneWidget);
  });

  testWidgets('6. 401 authentication error is handled', (WidgetTester tester) async {
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items') {
        return ResponseBody.fromString('Unauthorized', 401);
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createTestApp(const MyItemsScreen()));
    await tester.pumpAndSettle();

    expect(find.text('Unauthorized. Please login.'), findsOneWidget);
  });

  testWidgets('7. 409 concurrency conflict is displayed', (WidgetTester tester) async {
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.method == 'GET' && options.path == '/api/items/1') {
        final data = {
          'id': '1', 'ownerId': 'user1', 'title': 'Laptop', 'description': 'desc', 'category': 'cat', 
          'locationArea': 'loc', 'status': 'Draft', 'createdAt': '2026-09-10T12:00:00Z', 'version': 1,
          'photos': [], 'conditionAnswers': [], 'assessments': []
        };
        return ResponseBody.fromString(jsonEncode(data), 200, headers: {Headers.contentTypeHeader: ['application/json']});
      }
      if (options.method == 'PUT' && options.path == '/api/items/1') {
        return ResponseBody.fromString('Conflict', 409);
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createRouterApp('/items/1/edit'));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextFormField).at(0), 'Laptop 2');
    await tester.tap(find.text('Save Changes'));
    await tester.pumpAndSettle();

    expect(find.textContaining('Conflict detected:'), findsOneWidget);
  });

  testWidgets('8. Condition answers submit correctly', (WidgetTester tester) async {
    bool apiCalled = false;
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items/1/condition-answers' && options.method == 'POST') {
        apiCalled = true;
        return ResponseBody.fromString('', 200);
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createRouterApp('/items/1/condition-answers'));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextFormField).at(0), 'Yes');
    await tester.enterText(find.byType(TextFormField).at(1), 'No');
    
    await tester.tap(find.text('Submit Answers'));
    await tester.pumpAndSettle();

    expect(apiCalled, true);
    expect(find.text('Condition answers submitted successfully'), findsOneWidget);
  });

  testWidgets('9. Submit-for-assessment action calls the correct endpoint', (WidgetTester tester) async {
    bool apiCalled = false;
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items/1') {
        final data = {
          'id': '1', 'ownerId': 'user1', 'title': 'Laptop', 'description': 'desc', 'category': 'cat', 
          'locationArea': 'loc', 'status': 'Draft', 'createdAt': '2026-09-10T12:00:00Z', 'version': 1,
          'photos': [], 'conditionAnswers': [], 'assessments': []
        };
        return ResponseBody.fromString(jsonEncode(data), 200, headers: {Headers.contentTypeHeader: ['application/json']});
      }
      if (options.path == '/api/items/1/submit' && options.method == 'POST') {
        apiCalled = true;
        return ResponseBody.fromString('', 200);
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createTestApp(const ItemDetailsScreen(itemId: '1')));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Submit for Assessment'));
    await tester.pumpAndSettle();

    expect(apiCalled, true);
    expect(find.text('Item submitted for assessment'), findsOneWidget);
  });

  testWidgets('10. Assessment displays AI and owner information separately', (WidgetTester tester) async {
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items/1/assessments/a1') {
        final data = {
          'id': 'a1', 'itemId': '1', 'version': 1, 'suggestedCategory': 'Electronics', 
          'conditionGrade': 'A', 'conditionSummary': 'Good', 'visibleObservations': 'None',
          'ownerReportedFunctionality': 'Working fine', 'confidence': 0.95, 'status': 'PendingConfirmation', 
          'createdAt': '2026-09-10T12:00:00Z', 'evidences': [], 'clarifications': []
        };
        return ResponseBody.fromString(jsonEncode(data), 200, headers: {Headers.contentTypeHeader: ['application/json']});
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createTestApp(const AssessmentScreen(itemId: '1', assessmentId: 'a1')));
    await tester.pumpAndSettle();

    expect(find.text('AI ASSESSMENT'), findsOneWidget);
    expect(find.text('OWNER REPORTED'), findsOneWidget);
    expect(find.text('Working fine'), findsOneWidget);
  });

  testWidgets('11. Clarification answer submits correctly', (WidgetTester tester) async {
    bool apiCalled = false;
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items/1/clarifications/c1/answer' && options.method == 'POST') {
        apiCalled = true;
        return ResponseBody.fromString('', 200);
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createRouterApp('/items/1/clarifications/c1'));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextFormField).at(0), 'My answer');
    await tester.tap(find.text('Submit Answer'));
    await tester.pumpAndSettle();

    expect(apiCalled, true);
    expect(find.text('Clarification answered successfully'), findsOneWidget);
  });

  testWidgets('12. Assessment confirmation requires explicit interaction', (WidgetTester tester) async {
    bool apiCalled = false;
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items/1/assessments/a1/confirm' && options.method == 'POST') {
        apiCalled = true;
        return ResponseBody.fromString('', 200);
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createRouterApp('/items/1/assessments/a1/confirm'));
    await tester.pumpAndSettle();

    // Just verifying that the screen requires tapping a button to trigger API.
    expect(apiCalled, false);
    
    await tester.tap(find.text('Accept Assessment'));
    await tester.pumpAndSettle();

    expect(apiCalled, true);
    expect(find.text('Assessment confirmed'), findsOneWidget);
  });

  testWidgets('13. Reassessment requires a reason', (WidgetTester tester) async {
    bool apiCalled = false;
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items/1/assessments/a1/reassessment' && options.method == 'POST') {
        apiCalled = true;
        return ResponseBody.fromString('', 200);
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    await tester.pumpWidget(createRouterApp('/items/1/assessments/a1/reassessment'));
    await tester.pumpAndSettle();

    // Tap without reason
    await tester.tap(find.text('Submit Request'));
    await tester.pump(); // wait for validation

    expect(find.text('Reason is required'), findsOneWidget);
    expect(apiCalled, false);

    // Enter reason and tap
    await tester.enterText(find.byType(TextFormField).at(0), 'Not happy');
    await tester.tap(find.text('Submit Request'));
    await tester.pumpAndSettle();

    expect(apiCalled, true);
  });

  testWidgets('14. Add Photo screen handles UI controls', (WidgetTester tester) async {
    bool apiCalled = false;
    apiClient.httpClientAdapter = MockDioAdapter((options) async {
      if (options.path == '/api/items/1/photos' && options.method == 'POST') {
        apiCalled = true;
        return ResponseBody.fromString('', 201);
      }
      return ResponseBody.fromString('Not Found', 404);
    });

    final router = GoRouter(
      initialLocation: '/items/1/add-photo',
      routes: [
        GoRoute(
            path: '/items/:id/add-photo',
            builder: (context, state) =>
                AddPhotoScreen(itemId: state.pathParameters['id']!)),
      ],
    );

    await tester.pumpWidget(MaterialApp.router(routerConfig: router));
    await tester.pumpAndSettle();

    // Verify empty state buttons
    expect(find.text('Take a Photo'), findsOneWidget);
    expect(find.text('Choose from Gallery'), findsOneWidget);
    expect(find.text('No photo selected'), findsOneWidget);

    // Ensure apiCalled is referenced if we ever trigger the mock upload.
    expect(apiCalled, false);
  });
}
