class CollectionProposal {
  const CollectionProposal({
    required this.proposalId,
    required this.pickupRequestId,
    required this.recommended,
    this.fallback,
    required this.feasibilityStatus,
    required this.reasonForRecommendation,
    required this.constraintChecks,
    required this.createdAt,
  });

  final String proposalId;
  final String pickupRequestId;
  final ProposalOption recommended;
  final ProposalOption? fallback;
  final String feasibilityStatus;
  final String reasonForRecommendation;
  final List<ConstraintCheck> constraintChecks;
  final DateTime createdAt;

  factory CollectionProposal.fromJson(Map<String, dynamic> json) {
    return CollectionProposal(
      proposalId: json['proposalId'] as String,
      pickupRequestId: json['pickupRequestId'] as String,
      recommended: ProposalOption.fromJson(json['recommended'] as Map<String, dynamic>),
      fallback: json['fallback'] != null
          ? ProposalOption.fromJson(json['fallback'] as Map<String, dynamic>)
          : null,
      feasibilityStatus: json['feasibilityStatus'] as String,
      reasonForRecommendation: json['reasonForRecommendation'] as String,
      constraintChecks: (json['constraintChecks'] as List<dynamic>)
          .map((e) => ConstraintCheck.fromJson(e as Map<String, dynamic>))
          .toList(),
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }
}

class ProposalOption {
  const ProposalOption({
    required this.proposedStart,
    required this.proposedEnd,
    required this.collectionMethod,
    required this.requiredVehicleType,
    this.estimatedTravelDistance,
    this.estimatedTravelTime,
    this.estimatedCost,
    this.currency,
    required this.handlingRequirements,
  });

  final DateTime proposedStart;
  final DateTime proposedEnd;
  final String collectionMethod;
  final String requiredVehicleType;
  final String? estimatedTravelDistance;
  final String? estimatedTravelTime;
  final double? estimatedCost;
  final String? currency;
  final List<String> handlingRequirements;

  factory ProposalOption.fromJson(Map<String, dynamic> json) {
    return ProposalOption(
      proposedStart: DateTime.parse(json['proposedStart'] as String),
      proposedEnd: DateTime.parse(json['proposedEnd'] as String),
      collectionMethod: json['collectionMethod'] as String,
      requiredVehicleType: json['requiredVehicleType'] as String,
      estimatedTravelDistance: json['estimatedTravelDistance'] as String?,
      estimatedTravelTime: json['estimatedTravelTime'] as String?,
      estimatedCost: (json['estimatedCost'] as num?)?.toDouble(),
      currency: json['currency'] as String?,
      handlingRequirements: (json['handlingRequirements'] as List<dynamic>)
          .map((e) => e as String)
          .toList(),
    );
  }
}

class ConstraintCheck {
  const ConstraintCheck({
    required this.constraint,
    required this.passed,
    required this.detail,
  });

  final String constraint;
  final bool passed;
  final String detail;

  factory ConstraintCheck.fromJson(Map<String, dynamic> json) {
    return ConstraintCheck(
      constraint: json['constraint'] as String,
      passed: json['passed'] as bool,
      detail: json['detail'] as String,
    );
  }
}
