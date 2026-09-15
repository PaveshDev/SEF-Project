bool validUuid(String? value) =>
    value != null &&
    RegExp(r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$')
        .hasMatch(value) &&
    value != '00000000-0000-0000-0000-000000000000';
String? uuidError(String? value) =>
    validUuid(value?.trim()) ? null : 'Enter a valid nonempty UUID.';
String? currencyError(String? value) =>
    RegExp(r'^[A-Z]{3}$').hasMatch(value ?? '')
        ? null
        : 'Use three uppercase letters.';
String? moneyError(String? value, {bool optional = false}) {
  if (optional && (value == null || value.isEmpty)) return null;
  if (!RegExp(r'^(0|[1-9]\d{0,9})(\.\d{1,2})?$').hasMatch(value ?? '')) {
    return 'Use a nonnegative amount with at most two decimals.';
  }
  return null;
}

String requiredUuid(Object? value) {
  if (value is! String || !validUuid(value)) {
    throw const FormatException('Invalid identifier.');
  }
  return value;
}

int requiredVersion(Object? value) {
  if (value is! int || value < 1) {
    throw const FormatException('Invalid version.');
  }
  return value;
}

String requiredCurrency(Object? value) {
  if (value is! String || currencyError(value) != null) {
    throw const FormatException('Invalid currency.');
  }
  return value;
}

double? responseMoney(Object? value) {
  if (value == null) return null;
  if (value is! num ||
      !value.isFinite ||
      value < 0 ||
      value > 9999999999.99 ||
      (value * 100 - (value * 100).round()).abs() > .0002) {
    throw const FormatException('Invalid amount.');
  }
  return value.toDouble();
}

DateTime? responseDate(Object? value) {
  if (value == null) return null;
  if (value is! String || !RegExp(r'(Z|[+-]\d{2}:\d{2})$').hasMatch(value)) {
    throw const FormatException('Invalid timestamp.');
  }
  return DateTime.parse(value);
}
