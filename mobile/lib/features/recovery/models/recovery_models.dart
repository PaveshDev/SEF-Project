enum RecoveryCaseStatus {
  draft,
  planning,
  awaitingInputs,
  awaitingApproval,
  approved,
  rejected,
  revisionRequested,
  completed,
  failed,
  cancelled
}

enum RecoveryOptionStatus { draft, validated, selected, stale, rejected }

enum RecoveryProposalStatus {
  draft,
  awaitingApproval,
  approved,
  rejected,
  revisionRequested,
  expired,
  executed,
  stale
}

enum RecoveryRoute { reuse, donate, repairThenReuse, resell, recycle }

RecoveryCaseStatus recoveryCaseStatusFromJson(Object? value) =>
    RecoveryCaseStatus.values.firstWhere(
      (item) => item.name.toLowerCase() == value.toString().toLowerCase(),
      orElse: () => RecoveryCaseStatus.draft,
    );

RecoveryOptionStatus recoveryOptionStatusFromJson(Object? value) =>
    RecoveryOptionStatus.values.firstWhere(
      (item) => item.name.toLowerCase() == value.toString().toLowerCase(),
      orElse: () => RecoveryOptionStatus.draft,
    );

RecoveryProposalStatus recoveryProposalStatusFromJson(Object? value) =>
    RecoveryProposalStatus.values.firstWhere(
      (item) => item.name.toLowerCase() == value.toString().toLowerCase(),
      orElse: () => RecoveryProposalStatus.draft,
    );

RecoveryRoute recoveryRouteFromJson(Object? value) =>
    RecoveryRoute.values.firstWhere(
      (item) => item.name.toLowerCase() == value.toString().toLowerCase(),
      orElse: () => RecoveryRoute.reuse,
    );

String routeLabel(RecoveryRoute route) => switch (route) {
      RecoveryRoute.reuse => 'Reuse',
      RecoveryRoute.donate => 'Donate',
      RecoveryRoute.repairThenReuse => 'Repair then reuse',
      RecoveryRoute.resell => 'Resell',
      RecoveryRoute.recycle => 'Recycle',
    };

String routeApiValue(RecoveryRoute route) => switch (route) {
      RecoveryRoute.reuse => 'Reuse',
      RecoveryRoute.donate => 'Donate',
      RecoveryRoute.repairThenReuse => 'RepairThenReuse',
      RecoveryRoute.resell => 'Resell',
      RecoveryRoute.recycle => 'Recycle',
    };

String caseStatusApiValue(RecoveryCaseStatus status) => switch (status) {
      RecoveryCaseStatus.draft => 'Draft',
      RecoveryCaseStatus.planning => 'Planning',
      RecoveryCaseStatus.awaitingInputs => 'AwaitingInputs',
      RecoveryCaseStatus.awaitingApproval => 'AwaitingApproval',
      RecoveryCaseStatus.approved => 'Approved',
      RecoveryCaseStatus.rejected => 'Rejected',
      RecoveryCaseStatus.revisionRequested => 'RevisionRequested',
      RecoveryCaseStatus.completed => 'Completed',
      RecoveryCaseStatus.failed => 'Failed',
      RecoveryCaseStatus.cancelled => 'Cancelled',
    };

class RecoveryCase {
  const RecoveryCase({
    required this.id,
    required this.itemId,
    required this.objective,
    required this.routes,
    required this.currency,
    required this.status,
    required this.revision,
    required this.version,
    this.proposalId,
    this.maximumPickupCost,
    this.deadline,
  });

  final String id;
  final String itemId;
  final String objective;
  final List<RecoveryRoute> routes;
  final String currency;
  final RecoveryCaseStatus status;
  final int revision;
  final int version;
  final String? proposalId;
  final double? maximumPickupCost;
  final DateTime? deadline;

  factory RecoveryCase.fromJson(Map<String, dynamic> json) => RecoveryCase(
        id: json['id'] as String? ?? '',
        itemId: json['itemId'] as String? ?? '',
        objective: json['objective'] as String? ?? '',
        routes: ((json['preferredRoutes'] as List<dynamic>?) ?? [])
            .map(recoveryRouteFromJson)
            .toList(),
        currency: json['currency'] as String? ?? 'LKR',
        status: recoveryCaseStatusFromJson(json['status']),
        revision: json['revision'] as int? ?? 1,
        version: json['version'] as int? ?? 1,
        proposalId: json['proposalId'] as String?,
        maximumPickupCost: (json['maximumPickupCost'] as num?)?.toDouble(),
        deadline: DateTime.tryParse(json['deadline'] as String? ?? ''),
      );
}

class RecoveryOption {
  const RecoveryOption({
    required this.id,
    required this.route,
    required this.status,
    required this.requiresPickup,
    required this.version,
    this.valueLow,
    this.valueHigh,
    this.repairCost,
    this.pickupCost,
    this.netValue,
    this.currency,
    this.nonFinancialBenefits = const [],
  });

  final String id;
  final RecoveryRoute route;
  final RecoveryOptionStatus status;
  final bool requiresPickup;
  final int version;
  final double? valueLow;
  final double? valueHigh;
  final double? repairCost;
  final double? pickupCost;
  final double? netValue;
  final String? currency;
  final List<String> nonFinancialBenefits;

  factory RecoveryOption.fromJson(Map<String, dynamic> json) {
    final estimate = json['estimate'] as Map<String, dynamic>?;
    return RecoveryOption(
      id: json['id'] as String? ?? '',
      route: recoveryRouteFromJson(json['route']),
      status: recoveryOptionStatusFromJson(json['status']),
      requiresPickup: json['requiresPickup'] as bool? ?? false,
      version: json['version'] as int? ?? 1,
      valueLow: (estimate?['valueLow'] as num?)?.toDouble(),
      valueHigh: (estimate?['valueHigh'] as num?)?.toDouble(),
      repairCost: (estimate?['repairCost'] as num?)?.toDouble(),
      pickupCost: (estimate?['pickupCost'] as num?)?.toDouble(),
      netValue: (estimate?['netValue'] as num?)?.toDouble(),
      currency: estimate?['currency'] as String?,
      nonFinancialBenefits:
          ((json['nonFinancialBenefits'] as List<dynamic>?) ?? [])
              .whereType<String>()
              .toList(),
    );
  }
}

class RecoveryProposal {
  const RecoveryProposal({
    required this.id,
    required this.revision,
    required this.version,
    required this.status,
    required this.expiresAt,
    required this.explanation,
    required this.currency,
    this.netValue,
    this.valueLow,
    this.valueHigh,
    this.repairCost,
    this.pickupCost,
  });

  final String id;
  final int revision;
  final int version;
  final RecoveryProposalStatus status;
  final DateTime expiresAt;
  final String explanation;
  final String currency;
  final double? netValue;
  final double? valueLow;
  final double? valueHigh;
  final double? repairCost;
  final double? pickupCost;

  factory RecoveryProposal.fromJson(Map<String, dynamic> json) {
    final estimate = json['estimate'] as Map<String, dynamic>?;
    return RecoveryProposal(
      id: json['id'] as String? ?? '',
      revision: json['revision'] as int? ?? 1,
      version: json['version'] as int? ?? 1,
      status: recoveryProposalStatusFromJson(json['status']),
      expiresAt: DateTime.tryParse(json['expiresAt'] as String? ?? '') ??
          DateTime.now(),
      explanation: json['explanation'] as String? ?? '',
      currency: estimate?['currency'] as String? ?? 'LKR',
      netValue: (estimate?['netValue'] as num?)?.toDouble(),
      valueLow: (estimate?['valueLow'] as num?)?.toDouble(),
      valueHigh: (estimate?['valueHigh'] as num?)?.toDouble(),
      repairCost: (estimate?['repairCost'] as num?)?.toDouble(),
      pickupCost: (estimate?['pickupCost'] as num?)?.toDouble(),
    );
  }
}

class RecoveryPage<T> {
  const RecoveryPage(
      {required this.items, required this.page, required this.totalPages});
  final List<T> items;
  final int page;
  final int totalPages;

  factory RecoveryPage.fromJson(
          Map<String, dynamic> json, T Function(Map<String, dynamic>) parse) =>
      RecoveryPage(
        items: ((json['items'] as List<dynamic>?) ?? [])
            .whereType<Map<String, dynamic>>()
            .map(parse)
            .toList(),
        page: json['page'] as int? ?? 1,
        totalPages: json['totalPages'] as int? ?? 1,
      );
}
