class HandoverProof {
  const HandoverProof({
    required this.id,
    required this.pickupRequestId,
    required this.pickupEventId,
    required this.proofType,
    this.verifiedBy,
    this.verifiedAt,
    required this.createdAt,
  });

  final String id;
  final String pickupRequestId;
  final String pickupEventId;
  final String proofType;
  final String? verifiedBy;
  final DateTime? verifiedAt;
  final DateTime createdAt;

  factory HandoverProof.fromJson(Map<String, dynamic> json) {
    return HandoverProof(
      id: json['id'] as String,
      pickupRequestId: json['pickupRequestId'] as String,
      pickupEventId: json['pickupEventId'] as String,
      proofType: json['proofType'] as String,
      verifiedBy: json['verifiedBy'] as String?,
      verifiedAt: json['verifiedAt'] != null ? DateTime.parse(json['verifiedAt'] as String) : null,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }
}
