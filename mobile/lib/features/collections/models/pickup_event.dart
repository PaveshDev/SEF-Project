class PickupEvent {
  const PickupEvent({
    required this.id,
    required this.pickupRequestId,
    required this.eventType,
    required this.actorId,
    required this.eventAt,
    this.notes,
  });

  final String id;
  final String pickupRequestId;
  final String eventType;
  final String actorId;
  final DateTime eventAt;
  final String? notes;

  factory PickupEvent.fromJson(Map<String, dynamic> json) {
    return PickupEvent(
      id: json['id'] as String,
      pickupRequestId: json['pickupRequestId'] as String,
      eventType: json['eventType'] as String,
      actorId: json['actorId'] as String,
      eventAt: DateTime.parse(json['eventAt'] as String),
      notes: json['notes'] as String?,
    );
  }
}
