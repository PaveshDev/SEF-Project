import 'package:flutter/material.dart';
import '../demo_data.dart';
import '../services/collections_api_service.dart';
import 'handover_screen.dart';

const _forest = Color(0xFF2D674E);
const _ink = Color(0xFF203B32);
const _muted = Color(0xFF64745F);
const _canvas = Color(0xFFF5F6F2);

/// Dedicated pickup detail screen connected to backend API for live milestones,
/// handover entry, and failure reporting.
class PickupDetailScreen extends StatefulWidget {
  const PickupDetailScreen({super.key, required this.pickup, required this.role, this.apiService});
  final DemoPickup pickup;
  final String role;
  final CollectionsApiService? apiService;

  @override
  State<PickupDetailScreen> createState() => _PickupDetailScreenState();
}

class _PickupDetailScreenState extends State<PickupDetailScreen> {
  late final CollectionsApiService _api;
  final _code = TextEditingController();
  final _proof = TextEditingController();
  final _handoverForm = GlobalKey<FormState>();
  
  bool _loadingEvents = false;
  bool _verifyingCode = false;
  List<Map<String, dynamic>> _events = <Map<String, dynamic>>[];
  String _currentStatus = '';
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _api = widget.apiService ?? CollectionsApiService();
    _currentStatus = widget.pickup.status;
    _loadEvents();
  }

  @override
  void dispose() {
    _code.dispose();
    _proof.dispose();
    super.dispose();
  }

  void _message(String text) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(text)));
  }

  Future<void> _loadEvents() async {
    setState(() => _loadingEvents = true);
    try {
      final events = await _api.fetchPickupEvents(widget.pickup.id);
      final fetchedPickup = await _api.fetchPickup(widget.pickup.id);
      
      if (mounted) {
        setState(() {
          _events = events;
          if (fetchedPickup != null && fetchedPickup['status'] != null) {
            _currentStatus = fetchedPickup['status'].toString();
          }
        });
      }
    } catch (_) {
      // Keep local state fallback
    } finally {
      if (mounted) setState(() => _loadingEvents = false);
    }
  }

  Future<void> _submitHandoverVerification() async {
    if (!_handoverForm.currentState!.validate()) return;

    setState(() {
      _verifyingCode = true;
      _errorMessage = null;
    });

    final actorId = 'b1000000-0000-0000-0000-000000000001';

    try {
      final result = await _api.verifyHandoverCode(widget.pickup.id, _code.text.trim(), actorId);
      if (result != null) {
        _message('Code verified successfully! Pickup updated in backend.');
        _code.clear();
        _proof.clear();
        await _loadEvents();
      } else {
        setState(() => _errorMessage = 'Invalid or expired OTP code.');
      }
    } catch (e) {
      setState(() => _errorMessage = 'Verification error: ${e.toString().replaceAll('Exception:', '').trim()}');
    } finally {
      if (mounted) setState(() => _verifyingCode = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final pickup = widget.pickup;
    final stepIndex = collectionMilestones.indexOf(_currentStatus);

    return Theme(
      data: Theme.of(context).copyWith(
        colorScheme: ColorScheme.fromSeed(seedColor: _forest),
        inputDecorationTheme: InputDecorationTheme(
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
          filled: true,
          fillColor: Colors.white,
        ),
      ),
      child: Scaffold(
        backgroundColor: _canvas,
        appBar: AppBar(
          backgroundColor: _canvas,
          title: Text(pickup.id),
          actions: <Widget>[
            IconButton(
              icon: const Icon(Icons.refresh),
              onPressed: _loadEvents,
              tooltip: 'Refresh details & audit history',
            ),
          ],
        ),
        body: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 720),
              child: ListView(padding: const EdgeInsets.all(20), children: <Widget>[
                Text(pickup.item, style: const TextStyle(fontSize: 28, fontFamily: 'serif', color: _ink)),
                const SizedBox(height: 10),
                _StatusChip(_currentStatus),
                const SizedBox(height: 20),

                // Collection details
                _Card(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                  const Text('Collection details', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                  const SizedBox(height: 16),
                  _DetailRow(Icons.schedule_outlined, 'Window', pickup.window),
                  _DetailRow(Icons.place_outlined, 'Location', pickup.address),
                  _DetailRow(Icons.storefront_outlined, 'Destination', pickup.destination),
                  _DetailRow(Icons.local_shipping_outlined, 'Vehicle', pickup.vehicle),
                  _DetailRow(Icons.inventory_2_outlined, 'Handling', pickup.handling),
                ])),
                const SizedBox(height: 20),

                // Milestones
                _Card(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                  Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: <Widget>[
                    const Text('Collection milestones', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                    if (_loadingEvents) const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2)),
                  ]),
                  const SizedBox(height: 16),
                  ...collectionMilestones.asMap().entries.map((entry) => Padding(
                    padding: const EdgeInsets.symmetric(vertical: 9),
                    child: Row(children: <Widget>[
                      Icon(
                        entry.key <= stepIndex ? Icons.check_circle : Icons.radio_button_unchecked,
                        color: entry.key <= stepIndex ? _forest : const Color(0xFFB8C3AF),
                        size: 24,
                      ),
                      const SizedBox(width: 12),
                      Expanded(child: Text(entry.value, style: TextStyle(color: entry.key <= stepIndex ? _ink : _muted))),
                      if (entry.key == stepIndex) const Text('Current', style: TextStyle(fontSize: 10, color: _muted)),
                    ]),
                  )),
                ])),
                const SizedBox(height: 20),

                // Backend Audit Event History
                if (_events.isNotEmpty) ...<Widget>[
                  _Card(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                    const Text('Backend Audit Event Logs', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                    const SizedBox(height: 12),
                    ..._events.map((e) => Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                        const Icon(Icons.history, size: 18, color: _forest),
                        const SizedBox(width: 8),
                        Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                          Text('${e['eventType'] ?? 'EVENT'}', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13)),
                          if (e['notes'] != null) Text('${e['notes']}', style: const TextStyle(fontSize: 12, color: _muted)),
                        ])),
                      ]),
                    )),
                  ])),
                  const SizedBox(height: 20),
                ],

                // Handover entry Form
                _Card(child: Form(
                  key: _handoverForm,
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                    const Text('Handover Verification (OTP)', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                    const SizedBox(height: 8),
                    const Text('Verification submits to the backend API and updates status.', style: TextStyle(color: _muted, fontSize: 12)),
                    const SizedBox(height: 18),
                    Row(children: <Widget>[
                      const Icon(Icons.qr_code_scanner, size: 32, color: _forest),
                      const SizedBox(width: 12),
                      Expanded(
                        child: OutlinedButton(
                          onPressed: () async {
                            final refreshed = await Navigator.push<bool>(
                              context,
                              MaterialPageRoute<bool>(
                                builder: (context) => HandoverScreen(pickupId: pickup.id, itemName: pickup.item, apiService: _api),
                              ),
                            );
                            if (refreshed == true) {
                              await _loadEvents();
                            }
                          },
                          child: const Text('Open QR / Full Handover Screen'),
                        ),
                      ),
                    ]),
                    const SizedBox(height: 20),
                    TextFormField(
                      controller: _code,
                      keyboardType: TextInputType.number,
                      maxLength: 6,
                      enabled: !_verifyingCode,
                      decoration: const InputDecoration(labelText: 'One-time code (6 digits)'),
                      validator: (value) => RegExp(r'^\d{6}$').hasMatch(value ?? '') ? null : 'Enter exactly 6 digits.',
                    ),
                    if (_errorMessage != null) ...<Widget>[
                      Text(_errorMessage!, style: const TextStyle(color: Colors.red, fontSize: 13)),
                      const SizedBox(height: 12),
                    ],
                    FilledButton.icon(
                      onPressed: _verifyingCode ? null : _submitHandoverVerification,
                      icon: _verifyingCode ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white)) : const Icon(Icons.fact_check_outlined),
                      label: Text(_verifyingCode ? 'Verifying...' : 'Submit OTP Code to Backend'),
                    ),
                  ]),
                )),
                const SizedBox(height: 24),
                const Text('Member 4 · Synchronized with ASP.NET backend', style: TextStyle(color: _muted, fontSize: 11), textAlign: TextAlign.center),
              ]),
            ),
          ),
        ),
      ),
    );
  }
}

class _Card extends StatelessWidget {
  const _Card({required this.child, this.color = Colors.white});
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

class _StatusChip extends StatelessWidget {
  const _StatusChip(this.status);
  final String status;
  @override
  Widget build(BuildContext context) => Align(
    alignment: Alignment.centerLeft,
    child: Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
      decoration: BoxDecoration(
        color: status == 'Failed' ? const Color(0xFFFBEDE8) : const Color(0xFFEAF0E3),
        borderRadius: BorderRadius.circular(5),
      ),
      child: Text(status, style: TextStyle(fontSize: 11, color: status == 'Failed' ? const Color(0xFFA15D41) : _forest)),
    ),
  );
}

class _DetailRow extends StatelessWidget {
  const _DetailRow(this.icon, this.label, this.value);
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
