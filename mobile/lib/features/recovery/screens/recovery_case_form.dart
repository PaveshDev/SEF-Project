import 'package:flutter/material.dart';
import '../models/recovery_models.dart';
import '../models/recovery_validation.dart';
import '../providers/recovery_controller.dart';

class RecoveryCaseForm extends StatefulWidget {
  const RecoveryCaseForm({super.key, required this.controller, this.selected});
  final RecoveryController controller;
  final RecoveryCase? selected;
  @override
  State<RecoveryCaseForm> createState() => _RecoveryCaseFormState();
}

class _RecoveryCaseFormState extends State<RecoveryCaseForm> {
  final form = GlobalKey<FormState>();
  late final TextEditingController item;
  late final TextEditingController objective;
  late final TextEditingController budget;
  late final TextEditingController currency;
  late List<RecoveryRoute> routes;
  DateTime? deadline;
  String? localError;
  bool savedConfirmed = false;
  @override
  void initState() {
    super.initState();
    final value = widget.selected;
    item = TextEditingController(text: value?.itemId ?? '');
    objective = TextEditingController(text: value?.objective ?? '');
    budget = TextEditingController(
        text: value?.maximumPickupCost?.toStringAsFixed(2) ?? '');
    currency = TextEditingController(text: value?.currency ?? 'LKR');
    routes = [
      ...(value?.routes ?? [RecoveryRoute.reuse])
    ];
    deadline = value?.deadline?.toLocal();
  }

  @override
  void dispose() {
    item.dispose();
    objective.dispose();
    budget.dispose();
    currency.dispose();
    super.dispose();
  }

  Future<void> pickDeadline() async {
    final now = DateTime.now();
    final date = await showDatePicker(
        context: context,
        initialDate: deadline?.isAfter(now) == true
            ? deadline!
            : now.add(const Duration(days: 1)),
        firstDate: DateTime(now.year, now.month, now.day),
        lastDate: now.add(const Duration(days: 730)));
    if (!mounted || date == null) return;
    final time = await showTimePicker(
        context: context, initialTime: TimeOfDay.fromDateTime(deadline ?? now));
    if (!mounted || time == null) return;
    setState(() => deadline =
        DateTime(date.year, date.month, date.day, time.hour, time.minute));
  }

  Future<void> save() async {
    if (!form.currentState!.validate()) return;
    if (routes.isEmpty ||
        deadline != null && !deadline!.isAfter(DateTime.now())) {
      setState(() => localError = routes.isEmpty
          ? 'Choose at least one route.'
          : 'Choose a future deadline.');
      return;
    }
    setState(() => localError = null);
    final inputs = <String, dynamic>{
      'objective': objective.text.trim(),
      'preferredRoutes': routes.map(routeApiValue).toList(),
      'currency': currency.text,
      'maximumPickupCost':
          budget.text.isEmpty ? null : double.parse(budget.text),
      'deadline': deadline?.toUtc().toIso8601String()
    };
    if (widget.selected != null) {
      await widget.controller.updateCase(inputs);
    } else {
      await widget.controller.createCase(
          itemId: item.text.trim(),
          objective: objective.text.trim(),
          routes: routes,
          currency: currency.text,
          budget: budget.text.isEmpty ? null : double.parse(budget.text),
          deadline: deadline);
    }
    if (mounted &&
        widget.controller.successMessage != null &&
        widget.controller.errorMessage != null) {
      setState(() => savedConfirmed = true);
    }
    if (mounted && widget.controller.errorMessage == null) {
      Navigator.pop(context);
    }
  }

  @override
  Widget build(BuildContext context) => AnimatedBuilder(
      animation: widget.controller,
      builder: (context, _) {
        final busy = widget.controller.isBusy;
        final errors = widget.controller.fieldErrors;
        return PopScope(
            canPop: !busy,
            child: Padding(
                padding: EdgeInsets.fromLTRB(
                    18, 18, 18, MediaQuery.viewInsetsOf(context).bottom + 18),
                child: Form(
                    key: form,
                    child: SingleChildScrollView(
                        child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                          Text(
                              widget.selected == null
                                  ? 'New recovery case'
                                  : 'Edit recovery case',
                              style: Theme.of(context).textTheme.headlineSmall),
                          const Text(
                              'Use an item with a confirmed assessment. Item selection is awaiting the team integration.'),
                          TextFormField(
                              controller: item,
                              enabled: !busy && widget.selected == null,
                              decoration: InputDecoration(
                                  labelText: 'Item identifier',
                                  errorText: errors['itemId']),
                              validator: uuidError),
                          TextFormField(
                              controller: objective,
                              enabled: !busy,
                              maxLength: 1000,
                              decoration: InputDecoration(
                                  labelText: 'Objective',
                                  errorText: errors['objective']),
                              validator: (value) => value == null ||
                                      value.trim().isEmpty ||
                                      value.trim().length > 1000
                                  ? 'Enter an objective of 1–1,000 characters.'
                                  : null),
                          const Text(
                              'Preferred routes — choose all that apply'),
                          Wrap(
                              spacing: 8,
                              children: RecoveryRoute.values
                                  .map((route) => FilterChip(
                                      label: Text(routeLabel(route)),
                                      selected: routes.contains(route),
                                      onSelected: busy
                                          ? null
                                          : (selected) => setState(() {
                                                if (selected) {
                                                  routes.add(route);
                                                } else {
                                                  routes.remove(route);
                                                }
                                              })))
                                  .toList()),
                          TextFormField(
                              controller: currency,
                              enabled: !busy,
                              maxLength: 3,
                              textCapitalization: TextCapitalization.characters,
                              decoration: InputDecoration(
                                  labelText: 'Currency',
                                  errorText: errors['currency']),
                              validator: currencyError),
                          TextFormField(
                              controller: budget,
                              enabled: !busy,
                              keyboardType:
                                  const TextInputType.numberWithOptions(
                                      decimal: true),
                              decoration: InputDecoration(
                                  labelText: 'Maximum pickup cost (optional)',
                                  errorText: errors['maximumPickupCost']),
                              validator: (value) =>
                                  moneyError(value, optional: true)),
                          Wrap(spacing: 8, children: [
                            OutlinedButton.icon(
                                onPressed: busy ? null : pickDeadline,
                                icon: const Icon(Icons.event),
                                label: Text(deadline == null
                                    ? 'Choose deadline (local time)'
                                    : deadline.toString())),
                            if (deadline != null)
                              TextButton(
                                  onPressed: busy
                                      ? null
                                      : () => setState(() => deadline = null),
                                  child: const Text('Clear deadline'))
                          ]),
                          if (localError != null)
                            Text(localError!,
                                style: TextStyle(
                                    color:
                                        Theme.of(context).colorScheme.error)),
                          if (savedConfirmed &&
                              widget.controller.errorMessage != null)
                            TextButton(
                                onPressed: busy
                                    ? null
                                    : () async {
                                        await widget.controller
                                            .refreshWorkflow();
                                        if (context.mounted &&
                                            widget.controller.errorMessage ==
                                                null) Navigator.pop(context);
                                      },
                                child: const Text('Recover saved case')),
                          if (widget.controller.errorMessage != null)
                            Text(widget.controller.errorMessage!,
                                style: TextStyle(
                                    color:
                                        Theme.of(context).colorScheme.error)),
                          Row(children: [
                            TextButton(
                                onPressed:
                                    busy ? null : () => Navigator.pop(context),
                                child: const Text('Cancel')),
                            const Spacer(),
                            FilledButton(
                                onPressed: busy || savedConfirmed ? null : save,
                                child: Text(busy ? 'Saving…' : 'Save case'))
                          ]),
                        ])))));
      });
}
