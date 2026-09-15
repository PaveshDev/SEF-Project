class PickupRequest {
  const PickupRequest({
    required this.id,
    required this.recoveryProposalId,
    required this.collectionSlotId,
    this.collectorId,
    required this.ownerId,
    required this.scheduledStart,
    required this.scheduledEnd,
    required this.status,
    required this.createdAt,
    required this.updatedAt,
  });

  final String id;
  final String recoveryProposalId;
  final String collectionSlotId;
  final String? collectorId;
  final String ownerId;
  final DateTime scheduledStart;
  final DateTime scheduledEnd;
  final String status;
  final DateTime createdAt;
  final DateTime updatedAt;

  factory PickupRequest.fromJson(Map<String, dynamic> json) {
    return PickupRequest(
      id: json['id'] as String,
      recoveryProposalId: json['recoveryProposalId'] as String,
      collectionSlotId: json['collectionSlotId'] as String,
      collectorId: json['collectorId'] as String?,
      ownerId: json['ownerId'] as String,
      scheduledStart: DateTime.parse(json['scheduledStart'] as String),
      scheduledEnd: DateTime.parse(json['scheduledEnd'] as String),
      status: json['status'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }

  Map<String, dynamic> toJson() => <String, dynamic>{
    'id': id,
    'recoveryProposalId': recoveryProposalId,
    'collectionSlotId': collectionSlotId,
    'collectorId': collectorId,
    'ownerId': ownerId,
    'scheduledStart': scheduledStart.toIso8601String(),
    'scheduledEnd': scheduledEnd.toIso8601String(),
    'status': status,
    'createdAt': createdAt.toIso8601String(),
    'updatedAt': updatedAt.toIso8601String(),
  };
}
