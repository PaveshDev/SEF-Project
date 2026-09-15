import 'package:flutter/material.dart';
import '../models/recovery_models.dart';
import '../models/recovery_validation.dart';
import '../services/recovery_service.dart';
import 'recovery_theme.dart';

class RecoveryReferencesScreen extends StatefulWidget {
  const RecoveryReferencesScreen({super.key, required this.service});
  final RecoveryService service;
  @override
  State<RecoveryReferencesScreen> createState() =>
      _RecoveryReferencesScreenState();
}

class _RecoveryReferencesScreenState extends State<RecoveryReferencesScreen> {
  final search = TextEditingController();
  RecoveryPage<dynamic>? data;
  bool busy = true;
  bool curator = false;
  String? error;
  String? notice;
  int page = 1;
  @override
  void initState() {
    super.initState();
    load();
  }

  @override
  void dispose() {
    search.dispose();
    super.dispose();
  }

  Future<void> load() async {
    setState(() {
      busy = true;
      error = null;
    });
    try {
      final allowed = await widget.service.canManageReferences();
      final result =
          await widget.service.listReferences(page: page, search: search.text);
      if (mounted) {
        setState(() {
          curator = allowed;
          data = result;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() => error =
            'References or permissions could not be loaded. Retry when the API is available.');
      }
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  Future<void> mutate(Future<void> Function() action, String message) async {
    setState(() {
      busy = true;
      error = null;
      notice = null;
    });
    try {
      await action();
      if (!mounted) return;
      setState(() => notice = message);
      await load();
    } catch (_) {
      if (mounted) {
        setState(() => error =
            'The operation could not be confirmed. Reload or retry the same submission. Verified evidence cannot be changed.');
      }
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  Future<void> edit([Map<String, dynamic>? item]) async {
    final saved = await showModalBottomSheet<bool>(
        context: context,
        isScrollControlled: true,
        isDismissible: false,
        enableDrag: false,
        builder: (_) => _ReferenceForm(service: widget.service, item: item));
    if (saved == true && mounted) {
      setState(() => notice = 'Reference saved.');
      await load();
    }
  }

  Future<bool> confirm(String message) async =>
      await showDialog<bool>(
          context: context,
          builder: (context) => AlertDialog(
                  title: const Text('Confirm reference action'),
                  content: Text(message),
                  actions: [
                    TextButton(
                        onPressed: () => Navigator.pop(context, false),
                        child: const Text('Cancel')),
                    FilledButton(
                        onPressed: () => Navigator.pop(context, true),
                        child: const Text('Confirm'))
                  ])) ??
      false;
  @override
  Widget build(BuildContext context) => Theme(
      data: recoveryTheme(context),
      child: Builder(
          builder: (context) => Scaffold(
              appBar: AppBar(title: const Text('Value references')),
              body: ListView(padding: const EdgeInsets.all(18), children: [
                const Text(
                    'Verified evidence supports recovery estimates. Authorized curators can manage unverified references.'),
                TextField(
                    controller: search,
                    enabled: !busy,
                    decoration:
                        const InputDecoration(labelText: 'Search sources'),
                    onSubmitted: (_) {
                      page = 1;
                      load();
                    }),
                if (busy) const LinearProgressIndicator(),
                if (notice != null) Text(notice!),
                if (error != null)
                  Column(children: [
                    Text(error!),
                    TextButton(
                        onPressed: busy ? null : load,
                        child: const Text('Retry'))
                  ]),
                if (!busy && error == null && data?.items.isEmpty == true)
                  const Text('No references match this search.'),
                if (curator)
                  FilledButton.icon(
                      onPressed: busy ? null : () => edit(),
                      icon: const Icon(Icons.add),
                      label: const Text('Add reference')),
                if (error == null)
                  ...?data?.items.map((raw) {
                    final item = raw as Map<String, dynamic>;
                    final verified = item['isVerified'] == true;
                    return Card(
                        child: Padding(
                            padding: const EdgeInsets.all(14),
                            child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                      item['sourceName'] as String? ??
                                          'Source unavailable',
                                      style: Theme.of(context)
                                          .textTheme
                                          .titleMedium),
                                  Text(
                                      '${item['currency']} ${item['valueLow']} – ${item['valueHigh']}'),
                                  Text(
                                      '${item['route']} · ${item['condition']} · ${verified ? 'Verified' : 'Unverified'}'),
                                  Text('Observed: ${item['observedAt']}'),
                                  if (curator && !verified)
                                    Wrap(spacing: 8, children: [
                                      TextButton(
                                          onPressed:
                                              busy ? null : () => edit(item),
                                          child: const Text('Edit')),
                                      TextButton(
                                          onPressed: busy
                                              ? null
                                              : () async {
                                                  if (await confirm(
                                                      'Verify this evidence? It will become immutable.')) {
                                                    await mutate(
                                                        () => widget.service
                                                            .verifyReference(
                                                                requiredUuid(
                                                                    item['id']),
                                                                requiredVersion(
                                                                    item[
                                                                        'version'])),
                                                        'Reference verified.');
                                                  }
                                                },
                                          child: const Text('Verify')),
                                      TextButton(
                                          onPressed: busy
                                              ? null
                                              : () async {
                                                  if (await confirm(
                                                      'Delete this unverified reference?')) {
                                                    await mutate(
                                                        () => widget.service
                                                            .deleteReference(
                                                                requiredUuid(
                                                                    item[
                                                                        'id'])),
                                                        'Reference deleted.');
                                                  }
                                                },
                                          child: const Text('Delete')),
                                    ]),
                                ])));
                  }),
                Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      TextButton(
                          onPressed: busy || page <= 1
                              ? null
                              : () {
                                  page--;
                                  load();
                                },
                          child: const Text('Previous')),
                      Text('Page $page'),
                      TextButton(
                          onPressed: busy || page >= (data?.totalPages ?? 1)
                              ? null
                              : () {
                                  page++;
                                  load();
                                },
                          child: const Text('Next'))
                    ]),
              ]))));
}

class _ReferenceForm extends StatefulWidget {
  const _ReferenceForm({required this.service, this.item});
  final RecoveryService service;
  final Map<String, dynamic>? item;
  @override
  State<_ReferenceForm> createState() => _ReferenceFormState();
}

class _ReferenceFormState extends State<_ReferenceForm> {
  final form = GlobalKey<FormState>();
  final fields = <String, TextEditingController>{};
  late String condition;
  late RecoveryRoute route;
  late DateTime observed;
  bool busy = false;
  String? error;
  @override
  void initState() {
    super.initState();
    for (final key in [
      'categoryId',
      'sourceName',
      'sourceReference',
      'currency',
      'valueLow',
      'valueHigh'
    ]) {
      fields[key] = TextEditingController(
          text: widget.item?[key]?.toString() ??
              (key == 'currency' ? 'LKR' : ''));
    }
    condition = widget.item?['condition'] as String? ?? 'Good';
    route = widget.item == null
        ? RecoveryRoute.reuse
        : recoveryRouteFromJson(widget.item!['route']);
    observed = widget.item == null
        ? DateTime.now()
        : DateTime.parse(widget.item!['observedAt'] as String).toLocal();
  }

  @override
  void dispose() {
    for (final field in fields.values) {
      field.dispose();
    }
    super.dispose();
  }

  Future<void> save() async {
    if (!form.currentState!.validate()) return;
    if (observed.isAfter(DateTime.now())) {
      setState(() => error = 'Observation cannot be in the future.');
      return;
    }
    setState(() {
      busy = true;
      error = null;
    });
    try {
      await widget.service.saveReference({
        'categoryId': fields['categoryId']!.text.trim(),
        'sourceName': fields['sourceName']!.text.trim(),
        'sourceReference': fields['sourceReference']!.text.trim(),
        'currency': fields['currency']!.text,
        'valueLow': double.parse(fields['valueLow']!.text),
        'valueHigh': double.parse(fields['valueHigh']!.text),
        'condition': condition,
        'route': routeApiValue(route),
        'observedAt': observed.toUtc().toIso8601String(),
        if (widget.item != null) 'expectedVersion': widget.item!['version']
      }, id: widget.item?['id'] as String?);
      if (mounted) Navigator.pop(context, true);
    } catch (_) {
      if (mounted) {
        setState(() => error =
            'Save could not be confirmed. Check fields and retry the same submission.');
      }
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  Widget field(String key, String label, String? Function(String?) validator,
          {int? maximum}) =>
      TextFormField(
          controller: fields[key],
          enabled: !busy && (key != 'categoryId' || widget.item == null),
          maxLength: maximum,
          decoration: InputDecoration(labelText: label),
          validator: validator);
  @override
  Widget build(BuildContext context) => PopScope(
      canPop: !busy,
      child: Padding(
          padding: EdgeInsets.fromLTRB(
              18, 18, 18, MediaQuery.viewInsetsOf(context).bottom + 18),
          child: SingleChildScrollView(
              child: Form(
                  key: form,
                  child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                            widget.item == null
                                ? 'Add reference'
                                : 'Edit reference',
                            style: Theme.of(context).textTheme.titleLarge),
                        const Text(
                            'Category lookup is awaiting the Items integration.'),
                        field('categoryId', 'Category identifier', uuidError),
                        field(
                            'sourceName',
                            'Source name',
                            (value) => value == null || value.trim().isEmpty
                                ? 'Required'
                                : null,
                            maximum: 200),
                        field('sourceReference', 'Source reference (optional)',
                            (_) => null,
                            maximum: 1000),
                        field('currency', 'Currency', currencyError,
                            maximum: 3),
                        field('valueLow', 'Low value', moneyError),
                        field(
                            'valueHigh',
                            'High value',
                            (value) =>
                                moneyError(value) ??
                                (double.tryParse(fields['valueLow']!.text) !=
                                            null &&
                                        double.parse(value!) <
                                            double.parse(
                                                fields['valueLow']!.text)
                                    ? 'High must be at least low.'
                                    : null)),
                        DropdownButtonFormField<String>(
                            value: condition,
                            decoration:
                                const InputDecoration(labelText: 'Condition'),
                            items: [
                              'Excellent',
                              'Good',
                              'Fair',
                              'Poor',
                              'Unsafe',
                              'Unknown'
                            ]
                                .map((x) =>
                                    DropdownMenuItem(value: x, child: Text(x)))
                                .toList(),
                            onChanged: busy
                                ? null
                                : (x) => setState(() => condition = x!)),
                        DropdownButtonFormField<RecoveryRoute>(
                            value: route,
                            decoration:
                                const InputDecoration(labelText: 'Route'),
                            items: RecoveryRoute.values
                                .map((x) => DropdownMenuItem(
                                    value: x, child: Text(routeLabel(x))))
                                .toList(),
                            onChanged: busy
                                ? null
                                : (x) => setState(() => route = x!)),
                        OutlinedButton(
                            onPressed: busy
                                ? null
                                : () async {
                                    final date = await showDatePicker(
                                        context: context,
                                        initialDate: observed,
                                        firstDate: DateTime(2000),
                                        lastDate: DateTime.now());
                                    if (!context.mounted || date == null) {
                                      return;
                                    }
                                    final time = await showTimePicker(
                                        context: context,
                                        initialTime:
                                            TimeOfDay.fromDateTime(observed));
                                    if (mounted && time != null) {
                                      setState(() => observed = DateTime(
                                          date.year,
                                          date.month,
                                          date.day,
                                          time.hour,
                                          time.minute));
                                    }
                                  },
                            child: Text('Observed (local): $observed')),
                        if (error != null)
                          Text(error!,
                              style: TextStyle(
                                  color: Theme.of(context).colorScheme.error)),
                        Row(children: [
                          TextButton(
                              onPressed:
                                  busy ? null : () => Navigator.pop(context),
                              child: const Text('Cancel')),
                          const Spacer(),
                          FilledButton(
                              onPressed: busy ? null : save,
                              child: Text(busy ? 'Saving…' : 'Save reference'))
                        ]),
                      ])))));
}
