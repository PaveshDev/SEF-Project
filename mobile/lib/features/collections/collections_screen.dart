import 'package:flutter/material.dart';

import 'demo_data.dart';

const _forest = Color(0xFF2D674E);
const _ink = Color(0xFF203B32);
const _muted = Color(0xFF64745F);
const _canvas = Color(0xFFF5F6F2);

class CollectionsScreen extends StatefulWidget {
  const CollectionsScreen({super.key});

  @override
  State<CollectionsScreen> createState() => _CollectionsScreenState();
}

class _CollectionsScreenState extends State<CollectionsScreen> {
  int _tab = 0;
  String _role = 'Collector';
  String _filter = 'All';
  String _query = '';
  final _search = TextEditingController();
  final Map<String, String> _localNotes = <String, String>{};
  DateTime? _availabilityDate;
  TimeOfDay? _availabilityStart;
  TimeOfDay? _availabilityEnd;

  @override
  void dispose() {
    _search.dispose();
    super.dispose();
  }

  void _message(String text) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(text)));
  }

  Future<void> _availability() async {
    final result = await showDialog<DemoAvailability>(
      context: context,
      builder: (context) => _AvailabilityDialog(
        initialDate: _availabilityDate,
        initialStart: _availabilityStart,
        initialEnd: _availabilityEnd,
      ),
    );
    if (!mounted || result == null) return;
    setState(() {
      _availabilityDate = result.date;
      _availabilityStart = result.start;
      _availabilityEnd = result.end;
    });
    _message('Availability saved for this session. No request was submitted.');
  }

  void _openPickup(DemoPickup pickup) {
    Navigator.of(context).push<void>(MaterialPageRoute<void>(
      builder: (context) => _PickupDetail(
        pickup: pickup,
        role: _role,
        initialNote: _localNotes[pickup.id],
        onNote: (note) => setState(() => _localNotes[pickup.id] = note),
      ),
    ));
  }

  @override
  Widget build(BuildContext context) {
    final visible = demoPickups.where((pickup) {
      final matchesText = '${pickup.id} ${pickup.item}'
          .toLowerCase()
          .contains(_query.toLowerCase());
      return matchesText && (_filter == 'All' || pickup.status == _filter);
    }).toList();

    return Theme(
      data: _collectionTheme(context),
      child: Scaffold(
        backgroundColor: _canvas,
        appBar: AppBar(
          backgroundColor: _canvas,
          foregroundColor: _ink,
          title: const Row(children: <Widget>[
            Icon(Icons.eco_outlined, color: _forest),
            SizedBox(width: 9),
            Text('Waste to Value', style: TextStyle(fontSize: 19)),
          ]),
        ),
        body: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 720),
              child: ListView(
                padding: const EdgeInsets.all(20),
                children: <Widget>[
                  const _PreviewBanner(),
                  const SizedBox(height: 24),
                  Text(
                    <String>['A good day to give back.', 'Your pickup jobs', 'Your availability'][_tab],
                    style: const TextStyle(
                      fontFamily: 'serif', fontSize: 30, color: _ink,
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'A little coordination. A whole new beginning.',
                    style: TextStyle(color: _muted, fontSize: 13),
                  ),
                  const SizedBox(height: 20),
                  SegmentedButton<String>(
                    segments: const <ButtonSegment<String>>[
                      ButtonSegment<String>(
                        value: 'Collector', label: Text('Collector view'),
                        icon: Icon(Icons.local_shipping_outlined),
                      ),
                      ButtonSegment<String>(
                        value: 'Owner', label: Text('Owner view'),
                        icon: Icon(Icons.person_outline),
                      ),
                    ],
                    selected: <String>{_role},
                    onSelectionChanged: (values) => setState(() => _role = values.first),
                  ),
                  const Padding(
                    padding: EdgeInsets.only(top: 8, bottom: 20),
                    child: Text(
                      'Display modes only; no sign-in or access control.',
                      style: TextStyle(color: _muted, fontSize: 11),
                    ),
                  ),
                  if (_tab == 0) ...<Widget>[
                    Row(children: <Widget>[
                      Expanded(child: _Metric(
                        label: 'In progress',
                        value: demoPickups.where((pickup) => collectionMilestones.take(4).contains(pickup.status)).length.toString(),
                        icon: Icons.inventory_2_outlined,
                      )),
                      const SizedBox(width: 12),
                      Expanded(child: _Metric(
                        label: 'Needs attention',
                        value: demoPickups.where((pickup) => pickup.status == 'Failed').length.toString(),
                        icon: Icons.error_outline,
                      )),
                    ]),
                    const SizedBox(height: 20),
                    _Surface(
                      color: const Color(0xFFEAF0E3),
                      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                        const _Eyebrow('COLLECTION PLAN · STATIC SAMPLE'),
                        const SizedBox(height: 12),
                        const Text('The chair’s next chapter.', style: TextStyle(fontSize: 25, fontFamily: 'serif')),
                        const SizedBox(height: 8),
                        const Text('Friday, 14:00–16:00 · Small van'),
                        const SizedBox(height: 12),
                        const Text('Travel estimate unavailable — manual review required.', style: TextStyle(color: Color(0xFF84652E), fontSize: 12)),
                        const SizedBox(height: 16),
                        FilledButton.icon(
                          onPressed: () => _openPickup(demoPickups.first),
                          icon: const Icon(Icons.arrow_forward, size: 17),
                          label: const Text('Review collection proposal'),
                        ),
                      ]),
                    ),
                    const SizedBox(height: 24),
                    const _SectionTitle('Next on the route'),
                    ...demoPickups.where((pickup) => pickup.status == 'Scheduled' || pickup.status == 'En Route').map((pickup) => _PickupCard(pickup: pickup, onTap: () => _openPickup(pickup))),
                    const SizedBox(height: 8),
                    _Surface(
                      color: const Color(0xFFFCF6E9),
                      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                        const Text('A collection needs a new plan', style: TextStyle(fontWeight: FontWeight.w600)),
                        const SizedBox(height: 7),
                        const Text('Small refrigerator · assigned vehicle unavailable.', style: TextStyle(color: _muted, fontSize: 12)),
                        TextButton(onPressed: () => _openPickup(demoPickups[2]), child: const Text('View issue →')),
                      ]),
                    ),
                  ],
                  if (_tab == 1) ...<Widget>[
                    TextField(
                      controller: _search,
                      decoration: const InputDecoration(labelText: 'Search item or pickup ID', prefixIcon: Icon(Icons.search)),
                      onChanged: (value) => setState(() => _query = value),
                    ),
                    const SizedBox(height: 14),
                    DropdownButtonFormField<String>(
                      value: _filter,
                      isExpanded: true,
                      decoration: const InputDecoration(labelText: 'Status'),
                      items: <String>['All', 'Awaiting review', ...collectionMilestones, 'Failed']
                          .map((status) => DropdownMenuItem<String>(value: status, child: Text(status))).toList(),
                      onChanged: (value) => setState(() => _filter = value ?? 'All'),
                    ),
                    const SizedBox(height: 20),
                    if (visible.isEmpty) const _Surface(child: Text('No demo pickups match your search.')),
                    ...visible.map((pickup) => _PickupCard(pickup: pickup, onTap: () => _openPickup(pickup))),
                  ],
                  if (_tab == 2) _Surface(
                    child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                      const Icon(Icons.event_available_outlined, color: _forest, size: 32),
                      const SizedBox(height: 15),
                      const _SectionTitle('Make room for a pickup'),
                      const Text('Choose a preferred date and time window. The backend will need to check slots and destination hours.', style: TextStyle(color: _muted)),
                      const SizedBox(height: 18),
                      if (_availabilityDate != null) ...<Widget>[
                        Text('Local preference: ${_availabilityDate!.day}/${_availabilityDate!.month}/${_availabilityDate!.year}', style: const TextStyle(fontWeight: FontWeight.w600)),
                        Text('${_availabilityStart!.format(context)} – ${_availabilityEnd!.format(context)}'),
                        const SizedBox(height: 15),
                      ],
                      FilledButton.icon(onPressed: _availability, icon: const Icon(Icons.calendar_month_outlined), label: Text(_availabilityDate == null ? 'Select availability' : 'Edit availability')),
                      if (_availabilityDate != null) TextButton(onPressed: () => setState(() {
                        _availabilityDate = null;
                        _availabilityStart = null;
                        _availabilityEnd = null;
                      }), child: const Text('Clear local preference')),
                    ]),
                  ),
                  const SizedBox(height: 28),
                  const Text('Member 4 · Pickup & handover UI preview', textAlign: TextAlign.center, style: TextStyle(fontSize: 10, color: _muted)),
                ],
              ),
            ),
          ),
        ),
        bottomNavigationBar: NavigationBar(
          selectedIndex: _tab,
          onDestinationSelected: (value) => setState(() => _tab = value),
          destinations: const <NavigationDestination>[
            NavigationDestination(icon: Icon(Icons.grid_view_outlined), label: 'Overview'),
            NavigationDestination(icon: Icon(Icons.inventory_2_outlined), label: 'Pickups'),
            NavigationDestination(icon: Icon(Icons.event_available_outlined), label: 'Availability'),
          ],
        ),
      ),
    );
  }
}

class _PickupDetail extends StatefulWidget {
  const _PickupDetail({required this.pickup, required this.role, required this.onNote, this.initialNote});
  final DemoPickup pickup;
  final String role;
  final String? initialNote;
  final ValueChanged<String> onNote;

  @override
  State<_PickupDetail> createState() => _PickupDetailState();
}

class _PickupDetailState extends State<_PickupDetail> {
  final _code = TextEditingController();
  final _proof = TextEditingController();
  final _review = TextEditingController();
  final _handoverForm = GlobalKey<FormState>();
  String? _decision;
  String? _reviewError;
  String? _note;
  String? _milestonePreview;

  @override
  void initState() {
    super.initState();
    _note = widget.initialNote;
  }

  @override
  void dispose() {
    _code.dispose();
    _proof.dispose();
    _review.dispose();
    super.dispose();
  }

  void _message(String text) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(text)));
  }

  Future<void> _reportFailure() async {
    final note = await showDialog<String>(
      context: context,
      builder: (context) => const _FailureDialog(),
    );
    if (!mounted || note == null) return;
    setState(() => _note = note);
    widget.onNote(note);
    _message('Failure note saved locally. No re-planning workflow was started.');
  }

  void _recordDecision(String action) {
    if (action != 'Approve' && _review.text.trim().isEmpty) {
      setState(() => _reviewError = 'Add a reason for rejection or revision.');
      return;
    }
    setState(() {
      _reviewError = null;
      _decision = action;
    });
    _message('$action preference recorded locally. Only authorized staff and the backend can confirm a booking.');
  }

  @override
  Widget build(BuildContext context) {
    final pickup = widget.pickup;
    final stepIndex = collectionMilestones.indexOf(pickup.status);
    return Theme(
      data: _collectionTheme(context),
      child: Scaffold(
        backgroundColor: _canvas,
        appBar: AppBar(backgroundColor: _canvas, title: Text(pickup.id)),
        body: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 720),
              child: ListView(padding: const EdgeInsets.all(20), children: <Widget>[
                const _PreviewBanner(),
                const SizedBox(height: 22),
                Text(pickup.item, style: const TextStyle(fontSize: 30, fontFamily: 'serif', color: _ink)),
                const SizedBox(height: 10),
                Align(alignment: Alignment.centerLeft, child: _Status(pickup.status)),
                const SizedBox(height: 20),
                _Surface(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                  const _SectionTitle('Collection details'),
                  _DetailLine(Icons.schedule_outlined, 'Collection window', pickup.window),
                  _DetailLine(Icons.place_outlined, 'Pickup location', pickup.address),
                  _DetailLine(Icons.storefront_outlined, 'Destination', pickup.destination),
                  _DetailLine(Icons.local_shipping_outlined, 'Vehicle', pickup.vehicle),
                  _DetailLine(Icons.inventory_2_outlined, 'Handling', pickup.handling),
                  const SizedBox(height: 8),
                  const Text('Map and travel estimate unavailable. No routing service is connected.', style: TextStyle(color: _muted, fontSize: 12)),
                ])),
                const SizedBox(height: 20),
                if (pickup.status == 'Awaiting review') ...<Widget>[
                  _Surface(
                    color: const Color(0xFFEAF0E3),
                    child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                      const _Eyebrow('COLLECTION PROPOSAL · STATIC EXAMPLE'),
                      const SizedBox(height: 12),
                      const _SectionTitle('Friday, 14:00–16:00'),
                      const Text('Small van · 50 kg sample capacity\nOwner available 14:00–17:00\nDestination open 09:00–17:00', style: TextStyle(height: 1.8)),
                      const SizedBox(height: 15),
                      const Text('Why this sample option?', style: TextStyle(fontWeight: FontWeight.w600)),
                      const SizedBox(height: 6),
                      const Text('The listed capacity exceeds 8 kg and the time windows overlap. Vehicle dimensions, handling support, and actual availability are unvalidated.', style: TextStyle(color: _muted, fontSize: 12)),
                      const SizedBox(height: 15),
                      const Text('Travel distance, duration, and cost: unavailable. Feasibility: manual review required.', style: TextStyle(color: Color(0xFF84652E), fontSize: 12)),
                      const Divider(height: 28),
                      const Text('Fallback: 16:00–18:00', style: TextStyle(fontWeight: FontWeight.w600)),
                      const SizedBox(height: 6),
                      const Text('Destination closes at 17:00. This option needs revision; arrival time is unverified.', style: TextStyle(color: _muted, fontSize: 12)),
                    ]),
                  ),
                  const SizedBox(height: 20),
                  _Surface(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                    const _SectionTitle('Review preference preview'),
                    const Text('These controls save a UI preference. The plan stays unconfirmed; no agent has run.', style: TextStyle(color: _muted, fontSize: 12)),
                    const SizedBox(height: 16),
                    TextField(controller: _review, maxLines: 3, maxLength: 500, decoration: InputDecoration(labelText: 'Review notes', errorText: _reviewError)),
                    const SizedBox(height: 10),
                    Wrap(spacing: 8, runSpacing: 8, children: <Widget>[
                      FilledButton(onPressed: () => _recordDecision('Approve'), child: const Text('Approve · preview')),
                      OutlinedButton(onPressed: () => _recordDecision('Reject'), child: const Text('Reject · preview')),
                      OutlinedButton(onPressed: () => _recordDecision('Request Revision'), child: const Text('Request Revision · preview')),
                    ]),
                    if (_decision != null) Padding(padding: const EdgeInsets.only(top: 15), child: Text('Local preference: $_decision\nBooking remains unconfirmed.', style: const TextStyle(color: _forest, fontSize: 12))),
                  ])),
                  const SizedBox(height: 20),
                ],
                _Surface(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                  const _SectionTitle('Collection milestones'),
                  ...collectionMilestones.asMap().entries.map((entry) => Padding(
                    padding: const EdgeInsets.symmetric(vertical: 9),
                    child: Row(children: <Widget>[
                      Icon(entry.key <= stepIndex ? Icons.check_circle : Icons.radio_button_unchecked, color: entry.key <= stepIndex ? _forest : const Color(0xFFB8C3AF), size: 24),
                      const SizedBox(width: 12),
                      Expanded(child: Text(entry.value, style: TextStyle(color: entry.key <= stepIndex ? _ink : _muted))),
                      if (entry.key == stepIndex) const Text('Sample status', style: TextStyle(fontSize: 10, color: _muted)),
                    ]),
                  )),
                  if (stepIndex < 0) Padding(padding: const EdgeInsets.only(top: 12), child: Text('Current status: ${pickup.status}. No completed milestones are inferred.', style: const TextStyle(color: _muted, fontSize: 12))),
                  if (widget.role == 'Collector' && stepIndex >= 0 && stepIndex < 4) ...<Widget>[
                    const Divider(height: 30),
                    DropdownButtonFormField<String>(
                      value: _milestonePreview,
                      isExpanded: true,
                      decoration: const InputDecoration(labelText: 'Preview a status update'),
                      items: collectionMilestones.take(4).map((status) => DropdownMenuItem<String>(value: status, child: Text(status))).toList(),
                      onChanged: (value) => setState(() => _milestonePreview = value),
                    ),
                    const SizedBox(height: 12),
                    OutlinedButton(
                      onPressed: _milestonePreview == null ? null : () => _message('Preview request: $_milestonePreview. Backend validation is unavailable; the recorded status has not changed.'),
                      child: const Text('Preview status submission'),
                    ),
                  ],
                ])),
                const SizedBox(height: 20),
                if (pickup.status == 'Failed' || _note != null) ...<Widget>[
                  _Surface(color: const Color(0xFFFCF6E9), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                    const _SectionTitle('Collection issue'),
                    Text(_note ?? 'Assigned vehicle became unavailable.'),
                    const SizedBox(height: 10),
                    const Text('Re-planning must recheck slots, capacity, handling, availability, destination hours, and travel before requesting human approval.', style: TextStyle(color: _muted, fontSize: 12)),
                    const SizedBox(height: 8),
                    const Text('No revised plan generated. Local notes are not a persistent audit trail.', style: TextStyle(color: _muted, fontSize: 12)),
                  ])),
                  const SizedBox(height: 20),
                ],
                if (pickup.status != 'Handover Verified') ...<Widget>[
                  OutlinedButton.icon(onPressed: _reportFailure, icon: const Icon(Icons.report_problem_outlined), label: const Text('Draft failed-collection report')),
                  const SizedBox(height: 20),
                ],
                _Surface(child: Form(
                  key: _handoverForm,
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                    const _SectionTitle('Handover entry preview'),
                    const Text('Code validation and proof submission require the ASP.NET backend.', style: TextStyle(color: _muted, fontSize: 12)),
                    const SizedBox(height: 18),
                    const Row(children: <Widget>[
                      Icon(Icons.qr_code_scanner, size: 32, color: _muted),
                      SizedBox(width: 12),
                      Expanded(child: Text('QR scanner unavailable\nCamera integration is not connected.', style: TextStyle(color: _muted, fontSize: 12))),
                    ]),
                    const SizedBox(height: 20),
                    TextFormField(
                      controller: _code,
                      keyboardType: TextInputType.number,
                      maxLength: 6,
                      decoration: const InputDecoration(labelText: 'One-time code (demo: 6 digits)'),
                      validator: (value) => RegExp(r'^\d{6}$').hasMatch(value ?? '') ? null : 'Enter exactly 6 digits for the preview.',
                    ),
                    TextFormField(controller: _proof, maxLines: 2, maxLength: 300, decoration: const InputDecoration(labelText: 'Handover proof note (local preview)')),
                    const Text('Photo capture and upload are not connected.', style: TextStyle(color: _muted, fontSize: 11)),
                    const SizedBox(height: 16),
                    FilledButton.icon(
                      onPressed: () {
                        if (_handoverForm.currentState!.validate()) {
                          _message('Code format accepted for preview only. No code was verified, proof uploaded, or handover status changed.');
                        }
                      },
                      icon: const Icon(Icons.fact_check_outlined),
                      label: const Text('Preview code submission'),
                    ),
                  ]),
                )),
                const SizedBox(height: 24),
                const Text('Demo changes stay on this device and do not sync to React.', style: TextStyle(color: _muted, fontSize: 11), textAlign: TextAlign.center),
              ]),
            ),
          ),
        ),
      ),
    );
  }
}

class DemoAvailability {
  const DemoAvailability(this.date, this.start, this.end);
  final DateTime date;
  final TimeOfDay start;
  final TimeOfDay end;
}

class _AvailabilityDialog extends StatefulWidget {
  const _AvailabilityDialog({this.initialDate, this.initialStart, this.initialEnd});
  final DateTime? initialDate;
  final TimeOfDay? initialStart;
  final TimeOfDay? initialEnd;

  @override
  State<_AvailabilityDialog> createState() => _AvailabilityDialogState();
}

class _AvailabilityDialogState extends State<_AvailabilityDialog> {
  late DateTime _date;
  late TimeOfDay _start;
  late TimeOfDay _end;
  String? _error;

  @override
  void initState() {
    super.initState();
    _date = widget.initialDate ?? DateTime(2026, 9, 11);
    _start = widget.initialStart ?? const TimeOfDay(hour: 9, minute: 0);
    _end = widget.initialEnd ?? const TimeOfDay(hour: 11, minute: 0);
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: const Text('Pickup availability'),
    content: SingleChildScrollView(child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
      const Text('Save a preferred window locally. This does not book a slot.'),
      const SizedBox(height: 16),
      OutlinedButton.icon(
        onPressed: () async {
          final date = await showDatePicker(context: context, initialDate: _date, firstDate: DateTime(2026), lastDate: DateTime(2030, 12, 31));
          if (!mounted || date == null) return;
          setState(() => _date = date);
        },
        icon: const Icon(Icons.calendar_month_outlined),
        label: Text('${_date.day}/${_date.month}/${_date.year}'),
      ),
      OutlinedButton(onPressed: () async {
        final time = await showTimePicker(context: context, initialTime: _start);
        if (!mounted || time == null) return;
        setState(() => _start = time);
      }, child: Text('From ${_start.format(context)}')),
      OutlinedButton(onPressed: () async {
        final time = await showTimePicker(context: context, initialTime: _end);
        if (!mounted || time == null) return;
        setState(() => _end = time);
      }, child: Text('Until ${_end.format(context)}')),
      if (_error != null) Text(_error!, style: const TextStyle(color: Colors.red)),
    ])),
    actions: <Widget>[
      TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancel')),
      FilledButton(onPressed: () {
        if (_end.hour * 60 + _end.minute <= _start.hour * 60 + _start.minute) {
          setState(() => _error = 'End time must be after start time.');
          return;
        }
        Navigator.pop(context, DemoAvailability(_date, _start, _end));
      }, child: const Text('Save locally')),
    ],
  );
}

class _FailureDialog extends StatefulWidget {
  const _FailureDialog();
  @override
  State<_FailureDialog> createState() => _FailureDialogState();
}

class _FailureDialogState extends State<_FailureDialog> {
  final _reason = TextEditingController();
  final _form = GlobalKey<FormState>();

  @override
  void dispose() {
    _reason.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: const Text('Draft a failure report'),
    content: SingleChildScrollView(child: Form(
      key: _form,
      child: Column(mainAxisSize: MainAxisSize.min, children: <Widget>[
        const Text('Describe the changed constraint. This note stays in the UI session and does not start an agent.'),
        const SizedBox(height: 16),
        TextFormField(controller: _reason, maxLines: 4, maxLength: 500, decoration: const InputDecoration(labelText: 'Reason / changed constraint'), validator: (value) => value == null || value.trim().isEmpty ? 'Enter a reason.' : null),
      ]),
    )),
    actions: <Widget>[
      TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancel')),
      FilledButton(onPressed: () {
        if (_form.currentState!.validate()) Navigator.pop(context, _reason.text.trim());
      }, child: const Text('Save local note')),
    ],
  );
}

ThemeData _collectionTheme(BuildContext context) => Theme.of(context).copyWith(
  colorScheme: ColorScheme.fromSeed(seedColor: _forest),
  inputDecorationTheme: InputDecorationTheme(
    border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
    filled: true,
    fillColor: Colors.white,
  ),
);

class _PreviewBanner extends StatelessWidget {
  const _PreviewBanner();
  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(color: const Color(0xFFE7ECDE), borderRadius: BorderRadius.circular(8)),
    child: const Text('UI PREVIEW · Fictional data\nSession only. No bookings, AI execution, or verification.', style: TextStyle(fontSize: 11, color: Color(0xFF586C48), height: 1.5)),
  );
}

class _Surface extends StatelessWidget {
  const _Surface({required this.child, this.color = Colors.white});
  final Widget child;
  final Color color;
  @override
  Widget build(BuildContext context) => Container(
    width: double.infinity,
    padding: const EdgeInsets.all(20),
    decoration: BoxDecoration(color: color, border: Border.all(color: const Color(0xFFE0E6DA)), borderRadius: BorderRadius.circular(12)),
    child: child,
  );
}

class _Metric extends StatelessWidget {
  const _Metric({required this.label, required this.value, required this.icon});
  final String label;
  final String value;
  final IconData icon;
  @override
  Widget build(BuildContext context) => _Surface(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
    Icon(icon, size: 21, color: _forest),
    const SizedBox(height: 12),
    Text(value.padLeft(2, '0'), style: const TextStyle(fontSize: 30, color: _ink)),
    Text(label, style: const TextStyle(fontSize: 11, color: _muted)),
  ]));
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle(this.title);
  final String title;
  @override
  Widget build(BuildContext context) => Padding(padding: const EdgeInsets.only(bottom: 13), child: Text(title, style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)));
}

class _Eyebrow extends StatelessWidget {
  const _Eyebrow(this.text);
  final String text;
  @override
  Widget build(BuildContext context) => Text(text, style: const TextStyle(fontSize: 9, letterSpacing: 1.4, color: _muted));
}

class _Status extends StatelessWidget {
  const _Status(this.status);
  final String status;
  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
    decoration: BoxDecoration(color: status == 'Failed' ? const Color(0xFFFBEDE8) : const Color(0xFFEAF0E3), borderRadius: BorderRadius.circular(5)),
    child: Text(status, style: TextStyle(fontSize: 11, color: status == 'Failed' ? const Color(0xFFA15D41) : _forest)),
  );
}

class _PickupCard extends StatelessWidget {
  const _PickupCard({required this.pickup, required this.onTap});
  final DemoPickup pickup;
  final VoidCallback onTap;
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 12),
    child: Material(
      color: Colors.white,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12), side: const BorderSide(color: Color(0xFFE0E6DA))),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        child: Padding(padding: const EdgeInsets.all(18), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
          Row(children: <Widget>[
            const Icon(Icons.inventory_2_outlined, color: _forest),
            const SizedBox(width: 12),
            Expanded(child: Text(pickup.item, style: const TextStyle(fontWeight: FontWeight.w600, color: _ink))),
            const Icon(Icons.chevron_right, color: _muted),
          ]),
          const SizedBox(height: 10),
          Text('${pickup.id} · ${pickup.window}', style: const TextStyle(color: _muted, fontSize: 11)),
          const SizedBox(height: 12),
          _Status(pickup.status),
        ])),
      ),
    ),
  );
}

class _DetailLine extends StatelessWidget {
  const _DetailLine(this.icon, this.label, this.value);
  final IconData icon;
  final String label;
  final String value;
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 17),
    child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
      Icon(icon, size: 21, color: _muted),
      const SizedBox(width: 12),
      Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
        Text(label, style: const TextStyle(fontSize: 10, color: _muted)),
        const SizedBox(height: 4),
        Text(value, style: const TextStyle(fontSize: 13, color: _ink)),
      ])),
    ]),
  );
}
