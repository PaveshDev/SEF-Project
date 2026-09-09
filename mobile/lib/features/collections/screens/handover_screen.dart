import 'package:flutter/material.dart';

const _forest = Color(0xFF2D674E);
const _ink = Color(0xFF203B32);
const _muted = Color(0xFF64745F);
const _canvas = Color(0xFFF5F6F2);

/// Dedicated handover screen with QR scanner placeholder, one-time code entry,
/// and proof submission.
class HandoverScreen extends StatefulWidget {
  const HandoverScreen({super.key, required this.pickupId, required this.itemName});
  final String pickupId;
  final String itemName;

  @override
  State<HandoverScreen> createState() => _HandoverScreenState();
}

class _HandoverScreenState extends State<HandoverScreen> {
  final _code = TextEditingController();
  final _notes = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _submitted = false;

  @override
  void dispose() {
    _code.dispose();
    _notes.dispose();
    super.dispose();
  }

  void _message(String text) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(text)));
  }

  @override
  Widget build(BuildContext context) {
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
        appBar: AppBar(backgroundColor: _canvas, title: const Text('Handover Verification')),
        body: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 720),
              child: ListView(padding: const EdgeInsets.all(20), children: <Widget>[
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(color: const Color(0xFFE7ECDE), borderRadius: BorderRadius.circular(8)),
                  child: const Text('UI PREVIEW · No real verification\nCode validation requires the ASP.NET backend.', style: TextStyle(fontSize: 11, color: Color(0xFF586C48), height: 1.5)),
                ),
                const SizedBox(height: 22),
                Text(widget.itemName, style: const TextStyle(fontSize: 28, fontFamily: 'serif', color: _ink)),
                const SizedBox(height: 6),
                Text('Pickup ${widget.pickupId}', style: const TextStyle(color: _muted, fontSize: 12)),
                const SizedBox(height: 24),

                // QR Scanner placeholder
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(32),
                  decoration: BoxDecoration(
                    color: const Color(0xFFF6F8F1),
                    border: Border.all(color: const Color(0xFFCFDAC5), style: BorderStyle.none),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Column(children: <Widget>[
                    Icon(Icons.qr_code_scanner, size: 48, color: _muted),
                    SizedBox(height: 16),
                    Text('QR Scanner', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                    SizedBox(height: 8),
                    Text('Camera integration is not connected.\nScan the QR code on the item to verify handover.', textAlign: TextAlign.center, style: TextStyle(color: _muted, fontSize: 12)),
                  ]),
                ),
                const SizedBox(height: 24),

                // Code entry
                _Card(child: Form(
                  key: _formKey,
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                    const Text('One-time verification code', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _code,
                      keyboardType: TextInputType.number,
                      maxLength: 6,
                      decoration: const InputDecoration(
                        labelText: 'Enter 6-digit code',
                        prefixIcon: Icon(Icons.pin_outlined),
                      ),
                      validator: (value) => RegExp(r'^\d{6}$').hasMatch(value ?? '') ? null : 'Enter exactly 6 digits.',
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _notes,
                      maxLines: 3,
                      maxLength: 300,
                      decoration: const InputDecoration(
                        labelText: 'Handover notes (optional)',
                        prefixIcon: Icon(Icons.note_alt_outlined),
                      ),
                    ),
                    const SizedBox(height: 8),
                    const Text('Photo capture is not connected.', style: TextStyle(color: _muted, fontSize: 11)),
                    const SizedBox(height: 16),
                    SizedBox(
                      width: double.infinity,
                      child: FilledButton.icon(
                        onPressed: () {
                          if (_formKey.currentState!.validate()) {
                            setState(() => _submitted = true);
                            _message('Code format accepted. Verification unavailable without backend.');
                          }
                        },
                        icon: const Icon(Icons.fact_check_outlined),
                        label: const Text('Submit Verification'),
                      ),
                    ),
                    if (_submitted) ...<Widget>[
                      const SizedBox(height: 16),
                      Container(
                        padding: const EdgeInsets.all(14),
                        decoration: BoxDecoration(
                          color: const Color(0xFFEAF0E3),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: const Row(children: <Widget>[
                          Icon(Icons.check_circle_outline, color: _forest, size: 20),
                          SizedBox(width: 10),
                          Expanded(child: Text('Code format accepted for preview.\nBackend verification required to complete handover.', style: TextStyle(fontSize: 12, color: _forest))),
                        ]),
                      ),
                    ],
                  ]),
                )),
                const SizedBox(height: 24),
                const Text('Member 4 · Handover verification preview', textAlign: TextAlign.center, style: TextStyle(fontSize: 10, color: _muted)),
              ]),
            ),
          ),
        ),
      ),
    );
  }
}

class _Card extends StatelessWidget {
  const _Card({required this.child});
  final Widget child;
  @override
  Widget build(BuildContext context) => Container(
    width: double.infinity,
    padding: const EdgeInsets.all(20),
    decoration: BoxDecoration(color: Colors.white, border: Border.all(color: const Color(0xFFE0E6DA)), borderRadius: BorderRadius.circular(12)),
    child: child,
  );
}
