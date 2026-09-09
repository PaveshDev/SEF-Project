import 'package:flutter/material.dart';

import '../models/recovery_models.dart';
import '../providers/recovery_controller.dart';

class RecoveryScreen extends StatefulWidget {
  const RecoveryScreen({super.key});

  @override
  State<RecoveryScreen> createState() => _RecoveryScreenState();
}

class _RecoveryScreenState extends State<RecoveryScreen> {
  late final RecoveryController controller;
  final searchController = TextEditingController();
  String status = '';

  @override
  void initState() {
    super.initState();
    controller = RecoveryController()..loadCases();
  }

  @override
  void dispose() {
    searchController.dispose();
    controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AnimatedBuilder(
        animation: controller,
        builder: (context, child) => Scaffold(
          backgroundColor: const Color(0xfff7f8f5),
          appBar: AppBar(
            title: const Text('Recovery command center'),
            backgroundColor: const Color(0xfff7f8f5),
            actions: [
              IconButton(
                tooltip: 'Create recovery case',
                onPressed:
                    controller.isBusy ? null : () => _showCaseForm(context),
                icon: const Icon(Icons.add_circle_outline),
              ),
            ],
          ),
          body: controller.selectedCase == null
              ? _buildList(context)
              : _buildDetail(context),
        ),
      );

  Widget _buildList(BuildContext context) => RefreshIndicator(
        onRefresh: controller.loadCases,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(18, 12, 18, 40),
          children: [
            Text(
                'Turn confirmed assessments into transparent recovery decisions.',
                style: Theme.of(context).textTheme.bodyLarge),
            const SizedBox(height: 20),
            Row(children: [
              Expanded(
                  child: TextField(
                      controller: searchController,
                      onSubmitted: (_) => controller.loadCases(
                          search: searchController.text, status: status),
                      decoration: const InputDecoration(
                          labelText: 'Search cases',
                          prefixIcon: Icon(Icons.search)))),
              const SizedBox(width: 10),
              DropdownButton<String>(
                  value: status,
                  hint: const Text('Status'),
                  items: [
                    '',
                    ...RecoveryCaseStatus.values.map((item) => item.name)
                  ]
                      .map((item) => DropdownMenuItem(
                          value: item,
                          child: Text(item.isEmpty ? 'All' : item)))
                      .toList(),
                  onChanged: (value) {
                    setState(() => status = value ?? '');
                    controller.loadCases(
                        search: searchController.text, status: value ?? '');
                  }),
            ]),
            const SizedBox(height: 18),
            if (controller.state == RecoveryLoadState.loading)
              const LinearProgressIndicator(),
            if (controller.state == RecoveryLoadState.offline)
              _StateCard(
                  icon: Icons.cloud_off,
                  title: 'You are offline',
                  detail: 'Reconnect to load Recovery cases.',
                  action: controller.loadCases),
            if (controller.state == RecoveryLoadState.unauthorized)
              const _StateCard(
                  icon: Icons.lock_outline,
                  title: 'Sign-in required',
                  detail: 'Your account cannot access Recovery cases.'),
            if (controller.state == RecoveryLoadState.stale)
              _StateCard(
                  icon: Icons.sync_problem,
                  title: 'Case changed',
                  detail: 'Reload before continuing.',
                  action: controller.loadCases),
            if (controller.state == RecoveryLoadState.error)
              _StateCard(
                  icon: Icons.error_outline,
                  title: 'Recovery is unavailable',
                  detail: controller.errorMessage ?? 'Try again later.',
                  action: controller.loadCases),
            if (controller.state == RecoveryLoadState.empty)
              const _StateCard(
                  icon: Icons.inventory_2_outlined,
                  title: 'No recovery cases',
                  detail: 'Create a recovery request to begin planning.'),
            ...controller.cases.items.map((item) => _CaseTile(
                item: item, onTap: () => controller.selectCase(item))),
          ],
        ),
      );

  Widget _buildDetail(BuildContext context) {
    final item = controller.selectedCase!;
    final options = controller.options;
    return ListView(
        padding: const EdgeInsets.fromLTRB(18, 12, 18, 40),
        children: [
          TextButton.icon(
              onPressed: controller.isBusy ? null : controller.clearSelection,
              icon: const Icon(Icons.arrow_back),
              label: const Text('All cases'),
              style: TextButton.styleFrom(alignment: Alignment.centerLeft)),
          Card(
              child: Padding(
                  padding: const EdgeInsets.all(18),
                  child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(children: [
                          Expanded(
                              child: Text(item.objective,
                                  style: Theme.of(context)
                                      .textTheme
                                      .headlineSmall)),
                          _StatusChip(item.status.name)
                        ]),
                        const SizedBox(height: 8),
                        Text(
                            'Revision ${item.revision} · Version ${item.version} · ${item.currency}',
                            style: Theme.of(context).textTheme.bodySmall),
                        const SizedBox(height: 16),
                        Wrap(spacing: 18, runSpacing: 10, children: [
                          Text(
                              'Route: ${item.routes.map(routeLabel).join(', ')}'),
                          Text(
                              'Budget: ${_money(item.maximumPickupCost, item.currency)}'),
                          Text(
                              'Deadline: ${item.deadline == null ? 'Open' : _date(item.deadline!)}')
                        ]),
                      ]))),
          const SizedBox(height: 14),
          Row(children: [
            Expanded(
                child: Text('Planning progress',
                    style: Theme.of(context).textTheme.titleLarge)),
            FilledButton.icon(
                onPressed: controller.isBusy || controller.workflowBlocked
                    ? null
                    : () => controller.plan(
                        replan: item.status != RecoveryCaseStatus.draft),
                icon: const Icon(Icons.auto_awesome),
                label: Text(item.status == RecoveryCaseStatus.draft
                    ? 'Plan'
                    : 'Replan'))
          ]),
          TextButton.icon(
              onPressed: controller.isBusy ? null : controller.refreshWorkflow,
              icon: const Icon(Icons.refresh),
              label: const Text('Reload workflow')),
          if (controller.successMessage != null)
            Semantics(
                liveRegion: true, child: Text(controller.successMessage!)),
          if (controller.unavailableInputs.isNotEmpty) ...[
            const Text('Unavailable planning inputs',
                style: TextStyle(fontWeight: FontWeight.bold)),
            ...controller.unavailableInputs.map((input) => Text(input)),
          ],
          const SizedBox(height: 8),
          if (controller.state == RecoveryLoadState.loading)
            const LinearProgressIndicator(),
          if (controller.errorMessage != null &&
              controller.state != RecoveryLoadState.loading)
            _InlineError(message: controller.errorMessage!),
          const SizedBox(height: 12),
          if (options.isEmpty)
            const _StateCard(
                icon: Icons.compare_arrows,
                title: 'No options yet',
                detail:
                    'Planning will compare verified value references and feasible dependencies.'),
          if (options.isNotEmpty) ...[
            Text('Option comparison',
                style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 8),
            ...options.map((option) => _OptionCard(
                  option: option,
                  isSelected: controller.selectedOption?.id == option.id,
                  reason: optionUnavailable(option, controller.selectedCase),
                  onSelect: controller.isBusy ||
                          controller.workflowBlocked ||
                          controller.proposalId != null ||
                          optionUnavailable(option, controller.selectedCase) !=
                              null
                      ? null
                      : () => controller.selectOption(option),
                )),
          ],
          const SizedBox(height: 14),
          _ProposalCreationCard(
              key: ValueKey('${item.id}:${item.revision}'),
              controller: controller),
          const SizedBox(height: 14),
          _ProposalCard(controller: controller),
        ]);
  }

  Future<void> _showCaseForm(BuildContext context) async {
    await showModalBottomSheet<void>(
        context: context,
        isScrollControlled: true,
        builder: (_) => _CaseForm(controller: controller));
  }
}

class _CaseForm extends StatefulWidget {
  const _CaseForm({required this.controller});
  final RecoveryController controller;
  @override
  State<_CaseForm> createState() => _CaseFormState();
}

class _CaseFormState extends State<_CaseForm> {
  final formKey = GlobalKey<FormState>();
  final item = TextEditingController();
  final objective = TextEditingController();
  final budget = TextEditingController();
  RecoveryRoute route = RecoveryRoute.reuse;
  String currency = 'LKR';
  DateTime? deadline;

  @override
  void dispose() {
    item.dispose();
    objective.dispose();
    budget.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AnimatedBuilder(
      animation: widget.controller,
      builder: (context, child) => Padding(
          padding: EdgeInsets.only(
              left: 18,
              right: 18,
              top: 18,
              bottom: MediaQuery.viewInsetsOf(context).bottom + 18),
          child: Form(
              key: formKey,
              child: SingleChildScrollView(
                  child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                    Text('Create recovery request',
                        style: Theme.of(context).textTheme.headlineSmall),
                    const SizedBox(height: 16),
                    TextFormField(
                        controller: item,
                        decoration: const InputDecoration(labelText: 'Item ID'),
                        validator: _required),
                    TextFormField(
                        controller: objective,
                        maxLength: 1000,
                        decoration:
                            const InputDecoration(labelText: 'Objective'),
                        validator: _required),
                    DropdownButtonFormField<RecoveryRoute>(
                        value: route,
                        decoration:
                            const InputDecoration(labelText: 'Preferred route'),
                        items: RecoveryRoute.values
                            .map((value) => DropdownMenuItem(
                                value: value, child: Text(routeLabel(value))))
                            .toList(),
                        onChanged: (value) => setState(
                            () => route = value ?? RecoveryRoute.reuse)),
                    Row(children: [
                      Expanded(
                          child: TextFormField(
                              initialValue: currency,
                              decoration:
                                  const InputDecoration(labelText: 'Currency'),
                              onChanged: (value) =>
                                  currency = value.toUpperCase(),
                              validator: (value) => value?.length == 3
                                  ? null
                                  : 'Use a 3-letter currency')),
                      const SizedBox(width: 12),
                      Expanded(
                          child: TextFormField(
                              controller: budget,
                              keyboardType:
                                  const TextInputType.numberWithOptions(
                                      decimal: true),
                              decoration: const InputDecoration(
                                  labelText: 'Pickup budget')))
                    ]),
                    const SizedBox(height: 12),
                    OutlinedButton.icon(
                        onPressed: () async {
                          final value = await showDatePicker(
                              context: context,
                              firstDate: DateTime.now(),
                              lastDate:
                                  DateTime.now().add(const Duration(days: 730)),
                              initialDate:
                                  DateTime.now().add(const Duration(days: 7)));
                          if (value != null) setState(() => deadline = value);
                        },
                        icon: const Icon(Icons.event),
                        label: Text(deadline == null
                            ? 'Choose deadline'
                            : _date(deadline!))),
                    const SizedBox(height: 12),
                    SizedBox(
                        width: double.infinity,
                        child: FilledButton(
                            onPressed:
                                widget.controller.isSaving ? null : _submit,
                            child: Text(widget.controller.isSaving
                                ? 'Saving…'
                                : 'Create request'))),
                    if (widget.controller.errorMessage != null)
                      _InlineError(message: widget.controller.errorMessage!),
                  ])))));
  Future<void> _submit() async {
    if (!formKey.currentState!.validate()) return;
    await widget.controller.createCase(
        itemId: item.text.trim(),
        objective: objective.text.trim(),
        route: route,
        currency: currency,
        budget: double.tryParse(budget.text),
        deadline: deadline);
    if (mounted && widget.controller.errorMessage == null) {
      Navigator.pop(context);
    }
  }
}

String? _required(String? value) =>
    value == null || value.trim().isEmpty ? 'Required' : null;

class _CaseTile extends StatelessWidget {
  const _CaseTile({required this.item, required this.onTap});
  final RecoveryCase item;
  final VoidCallback onTap;
  @override
  Widget build(BuildContext context) => Card(
      child: ListTile(
          onTap: onTap,
          title: Text(item.objective),
          subtitle: Text(
              '${item.routes.map(routeLabel).join(' · ')} · ${item.currency}'),
          trailing: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                _StatusChip(item.status.name),
                Text('Rev ${item.revision}',
                    style: Theme.of(context).textTheme.bodySmall)
              ])));
}

class _OptionCard extends StatelessWidget {
  const _OptionCard(
      {required this.option,
      this.isSelected = false,
      this.onSelect,
      this.reason});
  final RecoveryOption option;
  final bool isSelected;
  final VoidCallback? onSelect;
  final String? reason;
  @override
  Widget build(BuildContext context) => Card(
        child: Padding(
            padding: const EdgeInsets.all(16),
            child:
                Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Wrap(
                  spacing: 12,
                  crossAxisAlignment: WrapCrossAlignment.center,
                  children: [
                    Text(routeLabel(option.route),
                        style: Theme.of(context).textTheme.titleMedium),
                    _StatusChip(option.status.name),
                  ]),
              Text(
                  'Value: ${_money(option.estimate?.valueLow, option.estimate?.currency)} - ${_money(option.estimate?.valueHigh, option.estimate?.currency)}'),
              Text(
                  'Repair: ${_money(option.estimate?.repairCost, option.estimate?.currency)}'),
              Text(
                  'Pickup: ${_money(option.estimate?.pickupCost, option.estimate?.currency)}'),
              Text(
                  'Net: ${_money(option.estimate?.netValue, option.estimate?.currency)}'),
              Text(
                  'Match: ${option.integration?.match?.response ?? 'Unavailable'}'),
              Text(
                  'Pickup feasibility: ${option.integration?.pickup?.feasibility ?? 'Unavailable'}'),
              ExpansionTile(
                  title: Text('Evidence (${option.evidence.length})'),
                  children: [
                    ...option.evidence.map((evidence) => ListTile(
                        title: Text(evidence.sourceName ?? 'Unnamed source'),
                        subtitle: Text(evidence.observedAt == null
                            ? 'Date unavailable'
                            : _date(evidence.observedAt!)))),
                  ]),
              ...option.nonFinancialBenefits.map(Text.new),
              if (reason != null) Text(reason!),
              OutlinedButton(
                  onPressed: onSelect,
                  child: Text(isSelected
                      ? 'Selected'
                      : 'Select ${routeLabel(option.route)}')),
            ])),
      );
}

class _ProposalCreationCard extends StatefulWidget {
  const _ProposalCreationCard({super.key, required this.controller});
  final RecoveryController controller;
  @override
  State<_ProposalCreationCard> createState() => _ProposalCreationCardState();
}

class _ProposalCreationCardState extends State<_ProposalCreationCard> {
  final explanation = TextEditingController();
  int expiryHours = 24;
  @override
  void dispose() {
    explanation.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final controller = widget.controller;
    return Card(
        semanticContainer: false,
        child: Padding(
            padding: const EdgeInsets.all(16),
            child:
                Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text('Proposal creation',
                  style: Theme.of(context).textTheme.titleLarge),
              Text(controller.proposalId != null
                  ? 'This proposal has been submitted for review.'
                  : controller.selectedOption == null
                      ? 'Select an eligible option above.'
                      : 'Selected route: ${routeLabel(controller.selectedOption!.route)}'),
              DropdownButtonFormField<int>(
                value: expiryHours,
                decoration:
                    const InputDecoration(labelText: 'Proposal expires after'),
                items: [24, 48, 168]
                    .map((hours) => DropdownMenuItem(
                        value: hours, child: Text('$hours hours')))
                    .toList(),
                onChanged: controller.isBusy || controller.proposalId != null
                    ? null
                    : (value) => setState(() => expiryHours = value ?? 24),
              ),
              TextField(
                  controller: explanation,
                  maxLength: 4000,
                  maxLines: 3,
                  enabled: !controller.isBusy && controller.proposalId == null,
                  decoration: const InputDecoration(
                      labelText: 'Explanation',
                      helperText:
                          'Explain why this option should be approved.'),
                  onChanged: (_) => setState(() {})),
              FilledButton(
                  onPressed: !controller.canCreateProposal ||
                          explanation.text.trim().isEmpty
                      ? null
                      : () => controller.createProposal(
                          expiresAt:
                              DateTime.now().add(Duration(hours: expiryHours)),
                          explanation: explanation.text),
                  child: Text(controller.isCreatingProposal
                      ? 'Creating proposal...'
                      : 'Create proposal')),
              if (controller.proposalId != null)
                Text(
                    'Created proposal ${controller.proposalId}. Replan to prepare another option.'),
            ])));
  }
}

class _ProposalCard extends StatefulWidget {
  const _ProposalCard({required this.controller});
  final RecoveryController controller;
  @override
  State<_ProposalCard> createState() => _ProposalCardState();
}

class _ProposalCardState extends State<_ProposalCard> {
  final comment = TextEditingController();
  @override
  void dispose() {
    comment.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final controller = widget.controller;
    final proposal = controller.proposal;
    if (proposal == null) {
      return _StateCard(
          icon: Icons.description_outlined,
          title: 'Proposal review',
          detail: controller.proposalId == null
              ? 'Create a proposal from a selected option to review it here.'
              : 'Proposal details could not be loaded. Use Reload workflow to retry.');
    }
    final disabled = !controller.canDecide;
    return Card(
        semanticContainer: false,
        child: Padding(
            padding: const EdgeInsets.all(16),
            child:
                Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text('Proposal review',
                  style: Theme.of(context).textTheme.titleLarge),
              _StatusChip(proposal.status.name),
              Text('Proposal ${proposal.id}'),
              Text('Case ${proposal.caseId}'),
              Text('Option ${proposal.optionId}'),
              Text(
                  'Revision ${proposal.revision} - Version ${proposal.version}'),
              Text(
                  'Expires: ${proposal.expiresAt?.toLocal().toString() ?? 'Date unavailable'}'),
              Text('Match: ${proposal.matchId ?? 'Not linked'}'),
              Text('Pickup: ${proposal.pickupPlanId ?? 'Not linked'}'),
              Text(proposal.explanation.isEmpty
                  ? 'No explanation supplied.'
                  : proposal.explanation),
              Text(
                  'Value: ${_money(proposal.estimate?.valueLow, proposal.estimate?.currency)} - ${_money(proposal.estimate?.valueHigh, proposal.estimate?.currency)}'),
              Text(
                  'Repair: ${_money(proposal.estimate?.repairCost, proposal.estimate?.currency)}'),
              Text(
                  'Pickup: ${_money(proposal.estimate?.pickupCost, proposal.estimate?.currency)}'),
              Text(
                  'Net: ${_money(proposal.estimate?.netValue, proposal.estimate?.currency)}'),
              if (disabled)
                const Text(
                    'Decisions require a current proposal awaiting approval with an available future expiry.'),
              TextField(
                  controller: comment,
                  maxLength: 2000,
                  enabled: !disabled,
                  decoration: const InputDecoration(
                      labelText: 'Decision comment',
                      helperText: 'Required when requesting a revision.'),
                  onChanged: (_) => setState(() {})),
              Wrap(spacing: 8, runSpacing: 8, children: [
                FilledButton(
                    onPressed: disabled ? null : () => _decide('Approved'),
                    child: const Text('Approve')),
                OutlinedButton(
                    onPressed: disabled || comment.text.trim().isEmpty
                        ? null
                        : () => _decide('RevisionRequested'),
                    child: const Text('Request revision')),
                TextButton(
                    onPressed: disabled ? null : () => _decide('Rejected'),
                    child: const Text('Reject')),
              ]),
            ])));
  }

  Future<void> _decide(String decision) => widget.controller
      .decideProposal(decision: decision, comment: comment.text);
}

class _StatusChip extends StatelessWidget {
  const _StatusChip(this.label);
  final String label;
  @override
  Widget build(BuildContext context) =>
      Chip(label: Text(label), visualDensity: VisualDensity.compact);
}

class _StateCard extends StatelessWidget {
  const _StateCard(
      {required this.icon,
      required this.title,
      required this.detail,
      this.action});
  final IconData icon;
  final String title;
  final String detail;
  final VoidCallback? action;
  @override
  Widget build(BuildContext context) => Card(
      child: Padding(
          padding: const EdgeInsets.all(20),
          child: Column(children: [
            Icon(icon, size: 32),
            const SizedBox(height: 8),
            Text(title, style: Theme.of(context).textTheme.titleMedium),
            Text(detail, textAlign: TextAlign.center),
            if (action != null)
              TextButton(onPressed: action, child: const Text('Try again'))
          ])));
}

class _InlineError extends StatelessWidget {
  const _InlineError({required this.message});
  final String message;
  @override
  Widget build(BuildContext context) => Container(
      width: double.infinity,
      margin: const EdgeInsets.only(top: 12),
      padding: const EdgeInsets.all(12),
      color: Colors.red.shade50,
      child: Text(message, style: TextStyle(color: Colors.red.shade900)));
}

String _money(double? value, String? currency) => value == null
    ? 'Pending'
    : '${currency ?? 'LKR'} ${value.toStringAsFixed(2)}';
String _date(DateTime value) => '${value.day}/${value.month}/${value.year}';
