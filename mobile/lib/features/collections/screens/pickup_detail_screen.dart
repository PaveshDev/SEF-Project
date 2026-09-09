import 'package:flutter/material.dart';

import '../demo_data.dart';

const _forest = Color(0xFF2D674E);
const _ink = Color(0xFF203B32);
const _muted = Color(0xFF64745F);
const _canvas = Color(0xFFF5F6F2);

/// Dedicated pickup detail screen with milestones, proposal review,
/// handover entry, and failure reporting.
class PickupDetailScreen extends StatefulWidget {
  const PickupDetailScreen({super.key, required this.pickup, required this.role});
  final DemoPickup pickup;
  final String role;

  @override
  State<PickupDetailScreen> createState() => _PickupDetailScreenState();
}

class _PickupDetailScreenState extends State<PickupDetailScreen> {
  final _code = TextEditingController();
  final _proof = TextEditingController();
  final _handoverForm = GlobalKey<FormState>();
  String? _milestonePreview;

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

  @override
  Widget build(BuildContext context) {
    final pickup = widget.pickup;
    final stepIndex = collectionMilestones.indexOf(pickup.status);

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
        appBar: AppBar(backgroundColor: _canvas, title: Text(pickup.id)),
        body: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 720),
              child: ListView(padding: const EdgeInsets.all(20), children: <Widget>[
                Text(pickup.item, style: const TextStyle(fontSize: 28, fontFamily: 'serif', color: _ink)),
                const SizedBox(height: 10),
                _StatusChip(pickup.status),
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
                  const SizedBox(height: 8),
                  const Text('Map and travel estimate unavailable.', style: TextStyle(color: _muted, fontSize: 12)),
                ])),
                const SizedBox(height: 20),

                // Milestones
                _Card(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                  const Text('Collection milestones', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
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
                  if (widget.role == 'Collector' && stepIndex >= 0 && stepIndex < 4) ...<Widget>[
                    const Divider(height: 30),
                    DropdownButtonFormField<String>(
                      value: _milestonePreview,
                      isExpanded: true,
                      decoration: const InputDecoration(labelText: 'Preview status update'),
                      items: collectionMilestones.take(4).map((status) =>
                        DropdownMenuItem<String>(value: status, child: Text(status)),
                      ).toList(),
                      onChanged: (value) => setState(() => _milestonePreview = value),
                    ),
                    const SizedBox(height: 12),
                    OutlinedButton(
                      onPressed: _milestonePreview == null ? null : () =>
                        _message('Preview: $_milestonePreview. Backend unavailable; status unchanged.'),
                      child: const Text('Preview status submission'),
                    ),
                  ],
                ])),
                const SizedBox(height: 20),

                // Handover entry
                _Card(child: Form(
                  key: _handoverForm,
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                    const Text('Handover verification', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                    const SizedBox(height: 8),
                    const Text('Code validation requires the ASP.NET backend.', style: TextStyle(color: _muted, fontSize: 12)),
                    const SizedBox(height: 18),
                    const Row(children: <Widget>[
                      Icon(Icons.qr_code_scanner, size: 32, color: _muted),
                      SizedBox(width: 12),
                      Expanded(child: Text('QR scanner unavailable\nCamera integration not connected.', style: TextStyle(color: _muted, fontSize: 12))),
                    ]),
                    const SizedBox(height: 20),
                    TextFormField(
                      controller: _code,
                      keyboardType: TextInputType.number,
                      maxLength: 6,
                      decoration: const InputDecoration(labelText: 'One-time code (6 digits)'),
                      validator: (value) => RegExp(r'^\d{6}$').hasMatch(value ?? '') ? null : 'Enter exactly 6 digits.',
                    ),
                    TextFormField(
                      controller: _proof,
                      maxLines: 2,
                      maxLength: 300,
                      decoration: const InputDecoration(labelText: 'Handover proof note'),
                    ),
                    const SizedBox(height: 16),
                    FilledButton.icon(
                      onPressed: () {
                        if (_handoverForm.currentState!.validate()) {
                          _message('Code format accepted. Verification unavailable without backend.');
                        }
                      },
                      icon: const Icon(Icons.fact_check_outlined),
                      label: const Text('Preview code submission'),
                    ),
                  ]),
                )),
                const SizedBox(height: 24),
                const Text('Changes stay on this device.', style: TextStyle(color: _muted, fontSize: 11), textAlign: TextAlign.center),
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
