/// Integration seam for the shared authenticated session, not an identity store.
/// No provider is wired until the app has a verified session integration.
typedef CollectionsActorProvider = Future<String?> Function();

const collectionsIdentityUnavailable =
    'Handover unavailable: an authenticated session is required. '
    'Sign-in integration is not connected.';

Future<String> requireCollectionsActorId(
    CollectionsActorProvider? provider) async {
  if (provider == null) {
    throw StateError(collectionsIdentityUnavailable);
  }
  final actorId = (await provider())?.trim();
  final guid = RegExp(
      r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$');
  if (actorId == null ||
      !guid.hasMatch(actorId) ||
      actorId == '00000000-0000-0000-0000-000000000000') {
    throw StateError(
        'Sign in with a valid account before submitting a handover.');
  }
  return actorId;
}
