abstract final class AppConfig {
  // Public build-time configuration. Never put secrets in --dart-define.
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:5080',
  );
}
