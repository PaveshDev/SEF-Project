import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:waste_to_value/features/collections/collections_screen.dart';
import 'package:waste_to_value/features/collections/demo_data.dart';
import 'package:waste_to_value/features/collections/screens/handover_screen.dart';
import 'package:waste_to_value/features/collections/screens/pickup_detail_screen.dart';
import 'package:waste_to_value/features/collections/screens/proposal_review_screen.dart';
import 'package:waste_to_value/features/collections/services/collections_api_service.dart';
import 'package:waste_to_value/features/collections/services/collections_identity.dart';

class FakeCollectionsApi extends CollectionsApiService {
  final verifiedActors = <String>[];
  final proofActors = <String>[];

  @override
  Future<List<Map<String, dynamic>>> fetchPickupEvents(String pickupId) async =>
      [];

  @override
  Future<Map<String, dynamic>?> fetchPickup(String id) async =>
      {'status': 'Scheduled'};

  @override
  Future<Map<String, dynamic>?> verifyHandoverCode(
      String pickupId, String code, String actorId) async {
    verifiedActors.add(actorId);
    return {'id': 'test-proof', 'proofType': 'ONE_TIME_CODE'};
  }

  @override
  Future<Map<String, dynamic>?> submitHandoverProof(
      String pickupId, String proofType, String actorId,
      {String? storageKey}) async {
    proofActors.add(actorId);
    return {'id': 'test-note'};
  }
}

const testActorId = '44444444-4444-4444-8444-444444444444';

void main() {
  Future<void> show(WidgetTester tester, Widget screen) async {
    await tester.binding.setSurfaceSize(const Size(900, 1800));
    addTearDown(() => tester.binding.setSurfaceSize(null));
    await tester.pumpWidget(MaterialApp(home: screen));
    await tester.pumpAndSettle();
    expect(tester.takeException(), isNull);
  }

  test('actor access rejects an unavailable provider and invalid identifiers',
      () async {
    await expectLater(requireCollectionsActorId(null), throwsStateError);
    for (final id in [
      null,
      '',
      'not-a-guid',
      '00000000-0000-0000-0000-000000000000'
    ]) {
      await expectLater(
          requireCollectionsActorId(() async => id), throwsStateError);
    }
  });

  test('actor access resolves the current session on every submission',
      () async {
    String? current = testActorId;
    Future<String?> provider() async => current;
    expect(await requireCollectionsActorId(provider), testActorId);
    current = null;
    await expectLater(requireCollectionsActorId(provider), throwsStateError);
  });

  testWidgets(
      'handover without shared identity is disabled and sends no request',
      (tester) async {
    final api = FakeCollectionsApi();
    await show(tester,
        HandoverScreen(pickupId: 'pickup', itemName: 'Chair', apiService: api));
    expect(find.text(collectionsIdentityUnavailable), findsOneWidget);
    final button = tester.widget<FilledButton>(
        find.byWidgetPredicate((widget) => widget is FilledButton));
    expect(button.onPressed, isNull);
    expect(api.verifiedActors, isEmpty);
    expect(api.proofActors, isEmpty);
  });

  testWidgets('pickup details without shared identity cannot verify a handover',
      (tester) async {
    final api = FakeCollectionsApi();
    await show(
        tester,
        PickupDetailScreen(
            pickup: demoPickups.first, role: 'Collector', apiService: api));
    expect(find.text(collectionsIdentityUnavailable), findsOneWidget);
    final button = tester.widget<FilledButton>(
        find.byWidgetPredicate((widget) => widget is FilledButton));
    expect(button.onPressed, isNull);
    expect(find.text('11 Sep 2026 · 14:00–16:00'), findsOneWidget);
    expect(api.verifiedActors, isEmpty);
  });

  testWidgets('handover rejects an expired session before calling the API',
      (tester) async {
    final api = FakeCollectionsApi();
    await show(
        tester,
        HandoverScreen(
            pickupId: 'pickup',
            itemName: 'Chair',
            apiService: api,
            actorProvider: () async => null));
    await tester.enterText(find.byType(TextFormField).first, '123456');
    await tester.tap(find.text('Submit Verification'));
    await tester.pumpAndSettle();
    expect(
        find.text('Sign in with a valid account before submitting a handover.'),
        findsOneWidget);
    expect(api.verifiedActors, isEmpty);
    expect(api.proofActors, isEmpty);
    expect(tester.takeException(), isNull);
  });

  testWidgets(
      'a valid injected session actor is forwarded for verification and proof',
      (tester) async {
    final api = FakeCollectionsApi();
    await show(
        tester,
        HandoverScreen(
            pickupId: 'pickup',
            itemName: 'Chair',
            apiService: api,
            actorProvider: () async => testActorId));
    await tester.enterText(find.byType(TextFormField).first, '123456');
    await tester.enterText(
        find.byType(TextFormField).last, 'Received in good condition');
    await tester.tap(find.text('Submit Verification'));
    await tester.pumpAndSettle();
    expect(api.verifiedActors, [testActorId]);
    expect(api.proofActors, [testActorId]);
    expect(find.text('Handover Code Verified!'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets(
      'pickup handover forwards the injected actor and preserves it on navigation',
      (tester) async {
    final api = FakeCollectionsApi();
    await show(
        tester,
        PickupDetailScreen(
            pickup: demoPickups.first,
            role: 'Collector',
            apiService: api,
            actorProvider: () async => testActorId));
    await tester.enterText(find.byType(TextFormField), '123456');
    await tester.tap(find.text('Submit OTP Code to Backend'));
    await tester.pumpAndSettle();
    expect(api.verifiedActors, [testActorId]);
    await tester.tap(find.text('Open QR / Full Handover Screen'));
    await tester.pumpAndSettle();
    await tester.enterText(find.byType(TextFormField).first, '654321');
    await tester.tap(find.text('Submit Verification'));
    await tester.pumpAndSettle();
    expect(api.verifiedActors, [testActorId, testActorId]);
    expect(tester.takeException(), isNull);
  });

  testWidgets('proposal preview renders Unicode separators and time ranges',
      (tester) async {
    await show(tester, const ProposalReviewScreen());
    expect(find.text('COLLECTION PROPOSAL · STATIC SAMPLE'), findsOneWidget);
    expect(find.text('Friday, 14:00–16:00'), findsOneWidget);
    expect(find.text('Fallback: 16:00–18:00'), findsOneWidget);
    expect(
        find.text(
            'Small van · 50 kg capacity\nOwner available 14:00–17:00\nDestination open 09:00–17:00'),
        findsOneWidget);
    await tester.tap(find.text('Reject'));
    await tester.pumpAndSettle();
    expect(
        find.text('Add a reason for rejection or revision.'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('collections overview renders without exceptions',
      (tester) async {
    await show(tester, const CollectionsScreen());
    expect(find.byType(CollectionsScreen), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
}
