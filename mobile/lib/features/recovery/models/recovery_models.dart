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
        revision: json['revision'] as int? ?? 0,
        version: json['version'] as int? ?? 0,
        maximumPickupCost: (json['maximumPickupCost'] as num?)?.toDouble(),
        deadline: DateTime.tryParse(json['deadline'] as String? ?? ''),
      );
}

class RecoveryOption {
  const RecoveryOption({
    required this.id,
    required this.caseRevision,
    required this.route,
    required this.requiresPartner,
    required this.requiresPickup,
    required this.status,
    required this.version,
    this.estimate,
    this.evidence = const [],
    this.nonFinancialBenefits = const [],
    this.integration,
  });

  final String id;
  final int caseRevision;
  final RecoveryRoute route;
  final bool? requiresPartner;
  final bool? requiresPickup;
  final RecoveryOptionStatus status;
  final int version;
  final ValueEstimate? estimate;
  final List<ValueEvidence> evidence;
  final List<String> nonFinancialBenefits;
  final OptionIntegrationSnapshot? integration;

  factory RecoveryOption.fromJson(Map<String, dynamic> json) {
    final estimateJson = json['estimate'] as Map<String, dynamic>?;
    final evidenceJson = json['evidence'] as List<dynamic>?;
    final nonFinancialBenefitsJson =
        json['nonFinancialBenefits'] as List<dynamic>?;
    final integrationJson = json['integration'] as Map<String, dynamic>?;

    return RecoveryOption(
      id: json['id'] as String? ?? '',
      caseRevision: json['caseRevision'] as int? ?? 0,
      route: recoveryRouteFromJson(json['route']),
      requiresPartner: json['requiresPartner'] as bool?,
      requiresPickup: json['requiresPickup'] as bool?,
      status: recoveryOptionStatusFromJson(json['status']),
      version: json['version'] as int? ?? 0,
      estimate:
          estimateJson != null ? ValueEstimate.fromJson(estimateJson) : null,
      evidence: evidenceJson != null
          ? evidenceJson
              .whereType<Map<String, dynamic>>()
              .map(ValueEvidence.fromJson)
              .toList()
          : const [],
      nonFinancialBenefits:
          (nonFinancialBenefitsJson ?? []).whereType<String>().toList(),
      integration: integrationJson != null
          ? OptionIntegrationSnapshot.fromJson(integrationJson)
          : null,
    );
  }
}

class RecoveryProposal {
  const RecoveryProposal({
    required this.id,
    required this.caseId,
    required this.caseRevision,
    required this.revision,
    required this.version,
    required this.optionId,
    this.matchId,
    this.pickupPlanId,
    required this.explanation,
    required this.expiresAt,
    required this.status,
    required this.estimate,
  });

  final String id;
  final String caseId;
  final int caseRevision;
  final int revision;
  final int version;
  final String optionId;
  final String? matchId;
  final String? pickupPlanId;
  final String explanation;
  final DateTime? expiresAt;
  final RecoveryProposalStatus status;
  final ValueEstimate? estimate;

  factory RecoveryProposal.fromJson(Map<String, dynamic> json) =>
      RecoveryProposal(
        id: json['id'] as String? ?? '',
        caseId: json['caseId'] as String? ?? '',
        caseRevision: json['caseRevision'] as int? ?? 0,
        revision: json['revision'] as int? ?? 0,
        version: json['version'] as int? ?? 0,
        optionId: json['optionId'] as String? ?? '',
        matchId: json['matchId'] as String?,
        pickupPlanId: json['pickupPlanId'] as String?,
        explanation: json['explanation'] as String? ?? '',
        expiresAt: DateTime.tryParse(json['expiresAt'] as String? ?? ''),
        status: recoveryProposalStatusFromJson(json['status']),
        estimate: json['estimate'] is Map<String, dynamic>
            ? ValueEstimate.fromJson(json['estimate'])
            : null,
      );
}

class MatchChoice {
  const MatchChoice({
    required this.id,
    required this.version,
    required this.freshnessToken,
  });

  final String id;
  final int version;
  final String freshnessToken;

  factory MatchChoice.fromJson(Map<String, dynamic> json) => MatchChoice(
        id: json['id'] as String? ?? '',
        version: json['version'] as int? ?? 0,
        freshnessToken: json['freshnessToken'] as String? ?? '',
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'version': version,
        'freshnessToken': freshnessToken,
      };
}

class PickupChoice {
  const PickupChoice({
    required this.id,
    required this.version,
    required this.freshnessToken,
  });

  final String id;
  final int version;
  final String freshnessToken;

  factory PickupChoice.fromJson(Map<String, dynamic> json) => PickupChoice(
        id: json['id'] as String? ?? '',
        version: json['version'] as int? ?? 0,
        freshnessToken: json['freshnessToken'] as String? ?? '',
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'version': version,
        'freshnessToken': freshnessToken,
      };
}

class ValueEstimate {
  const ValueEstimate({
    required this.valueLow,
    required this.valueHigh,
    required this.repairCost,
    required this.pickupCost,
    required this.netValue,
    required this.currency,
  });

  final double? valueLow;
  final double? valueHigh;
  final double? repairCost;
  final double? pickupCost;
  final double? netValue;
  final String currency;

  factory ValueEstimate.fromJson(Map<String, dynamic> json) => ValueEstimate(
        valueLow: (json['valueLow'] as num?)?.toDouble(),
        valueHigh: (json['valueHigh'] as num?)?.toDouble(),
        repairCost: (json['repairCost'] as num?)?.toDouble(),
        pickupCost: (json['pickupCost'] as num?)?.toDouble(),
        netValue: (json['netValue'] as num?)?.toDouble(),
        currency: json['currency'] as String? ?? 'LKR',
      );
}

class ValueEvidence {
  const ValueEvidence({
    required this.referenceId,
    required this.version,
    required this.observedAt,
    this.sourceName,
    this.valueLow,
    this.valueHigh,
    this.currency,
  });

  final String referenceId;
  final int version;
  final DateTime? observedAt;
  final String? sourceName;
  final double? valueLow;
  final double? valueHigh;
  final String? currency;

  factory ValueEvidence.fromJson(Map<String, dynamic> json) => ValueEvidence(
        referenceId: json['referenceId'] as String? ?? '',
        sourceName: json['sourceName'] as String?,
        valueLow: (json['valueLow'] as num?)?.toDouble(),
        valueHigh: (json['valueHigh'] as num?)?.toDouble(),
        currency: json['currency'] as String?,
        version: json['version'] as int? ?? 0,
        observedAt: DateTime.tryParse(json['observedAt'] as String? ?? ''),
      );
}

class OptionIntegrationSnapshot {
  const OptionIntegrationSnapshot({
    this.match,
    this.pickup,
  });

  final MatchSummary? match;
  final PickupPlanSummary? pickup;

  factory OptionIntegrationSnapshot.fromJson(Map<String, dynamic> json) =>
      OptionIntegrationSnapshot(
        match:
            json['match'] != null ? MatchSummary.fromJson(json['match']) : null,
        pickup: json['pickup'] != null
            ? PickupPlanSummary.fromJson(json['pickup'])
            : null,
      );
}

class MatchSummary {
  const MatchSummary(
      {required this.matchId,
      required this.recoveryOptionId,
      required this.version,
      required this.freshnessToken,
      required this.eligibility,
      required this.response});
  final String matchId;
  final String recoveryOptionId;
  final int version;
  final String freshnessToken;
  final String eligibility;
  final String response;
  factory MatchSummary.fromJson(Map<String, dynamic> json) => MatchSummary(
        matchId: json['matchId'] as String? ?? '',
        recoveryOptionId: json['recoveryOptionId'] as String? ?? '',
        version: json['version'] as int? ?? 0,
        freshnessToken: json['freshnessToken'] as String? ?? '',
        eligibility: json['eligibility'] as String? ?? 'Unavailable',
        response: json['response'] as String? ?? 'Unavailable',
      );
  MatchChoice get choice => MatchChoice(
      id: matchId, version: version, freshnessToken: freshnessToken);
}

class PickupPlanSummary {
  const PickupPlanSummary(
      {required this.pickupPlanId,
      required this.matchId,
      required this.version,
      required this.freshnessToken,
      required this.feasibility});
  final String pickupPlanId;
  final String matchId;
  final int version;
  final String freshnessToken;
  final String feasibility;
  factory PickupPlanSummary.fromJson(Map<String, dynamic> json) =>
      PickupPlanSummary(
        pickupPlanId: json['pickupPlanId'] as String? ?? '',
        matchId: json['matchId'] as String? ?? '',
        version: json['version'] as int? ?? 0,
        freshnessToken: json['freshnessToken'] as String? ?? '',
        feasibility: json['feasibility'] as String? ?? 'Unavailable',
      );
  PickupChoice get choice => PickupChoice(
      id: pickupPlanId, version: version, freshnessToken: freshnessToken);
}

String? optionUnavailable(RecoveryOption option, RecoveryCase? item) {
  if (item == null || item.id.isEmpty || item.version < 1) {
    return 'Reload the case before selecting an option.';
  }
  if (option.id.isEmpty ||
      option.version < 1 ||
      option.caseRevision != item.revision) {
    return 'This option is stale or incomplete. Replan the case.';
  }
  if (![RecoveryOptionStatus.validated, RecoveryOptionStatus.selected]
          .contains(option.status) ||
      option.estimate == null) return 'This option has not been validated.';
  if (option.requiresPartner == null || option.requiresPickup == null) {
    return 'Integration requirements are unavailable.';
  }
  final match = option.integration?.match;
  final pickup = option.integration?.pickup;
  if (option.requiresPartner! &&
      (match == null ||
          match.matchId.isEmpty ||
          match.recoveryOptionId != option.id ||
          match.version < 1 ||
          match.freshnessToken.isEmpty ||
          match.eligibility != 'Eligible' ||
          match.response != 'Accepted')) {
    return 'An eligible, accepted partner match is required.';
  }
  if (option.requiresPickup! &&
      (pickup == null ||
          pickup.pickupPlanId.isEmpty ||
          match == null ||
          match.matchId.isEmpty ||
          pickup.matchId != match.matchId ||
          pickup.version < 1 ||
          pickup.freshnessToken.isEmpty ||
          pickup.feasibility != 'Feasible')) {
    return 'A feasible pickup plan is required.';
  }
  return null;
}

class RecoveryPage<T> {
  const RecoveryPage({
    required this.items,
    required this.page,
    required this.totalPages,
  });

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
        page: json['page'] as int? ?? 0,
        totalPages: json['totalPages'] as int? ?? 0,
      );
}
