class CollectionSlot {
  const CollectionSlot({
    required this.id,
    this.collectorId,
    required this.startsAt,
    required this.endsAt,
    required this.serviceArea,
    required this.capacity,
    required this.reservedCount,
    this.vehicleClass,
    required this.status,
  });

  final String id;
  final String? collectorId;
  final DateTime startsAt;
  final DateTime endsAt;
  final String serviceArea;
  final int capacity;
  final int reservedCount;
  final String? vehicleClass;
  final String status;

  factory CollectionSlot.fromJson(Map<String, dynamic> json) {
    return CollectionSlot(
      id: json['id'] as String,
      collectorId: json['collectorId'] as String?,
      startsAt: DateTime.parse(json['startsAt'] as String),
      endsAt: DateTime.parse(json['endsAt'] as String),
      serviceArea: json['serviceArea'] as String,
      capacity: json['capacity'] as int,
      reservedCount: json['reservedCount'] as int,
      vehicleClass: json['vehicleClass'] as String?,
      status: json['status'] as String,
    );
  }

  Map<String, dynamic> toJson() => <String, dynamic>{
    'id': id,
    'collectorId': collectorId,
    'startsAt': startsAt.toIso8601String(),
    'endsAt': endsAt.toIso8601String(),
    'serviceArea': serviceArea,
    'capacity': capacity,
    'reservedCount': reservedCount,
    'vehicleClass': vehicleClass,
    'status': status,
  };
}
