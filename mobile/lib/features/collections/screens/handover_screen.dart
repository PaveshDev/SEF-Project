import 'package:flutter/material.dart';
import '../services/collections_api_service.dart';

const _forest = Color(0xFF2D674E);
const _ink = Color(0xFF203B32);
const _muted = Color(0xFF64745F);
const _canvas = Color(0xFFF5F6F2);

/// Dedicated handover screen with QR scanner placeholder, real OTP verification,
/// loading indicators, error handling, and proof submission.
class HandoverScreen extends StatefulWidget {
  const HandoverScreen({super.key, required this.pickupId, required this.itemName, this.apiService});
  final String pickupId;
  final String itemName;
  final CollectionsApiService? apiService;

  @override
  State<HandoverScreen> createState() => _HandoverScreenState();
}

class _HandoverScreenState extends State<HandoverScreen> {
  late final CollectionsApiService _api;
  final _code = TextEditingController();
  final _notes = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  
  bool _submitting = false;
  bool _submitted = false;
  String? _errorMessage;
  Map<String, dynamic>? _verifiedProof;

  @override
  void initState() {
    super.initState();
    _api = widget.apiService ?? CollectionsApiService();
  }

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

  Future<void> _submitVerification() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _submitting = true;
      _errorMessage = null;
    });

    final actorId = 'b1000000-0000-0000-0000-000000000001'; // Default collector actor ID

    try {
      final result = await _api.verifyHandoverCode(widget.pickupId, _code.text.trim(), actorId);
      if (result != null) {
        if (_notes.text.trim().isNotEmpty) {
          await _api.submitHandoverProof(widget.pickupId, 'NOTE', actorId, storageKey: _notes.text.trim());
        }

        setState(() {
          _submitted = true;
          _verifiedProof = result;
        });
        _message('Handover code verified successfully! Status updated in backend.');
      } else {
        setState(() => _errorMessage = 'Invalid or expired verification code.');
      }
    } catch (e) {
      setState(() => _errorMessage = 'Verification failed: ${e.toString().replaceAll('Exception:', '').trim()}');
    } finally {
      setState(() => _submitting = false);
    }
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
                  child: const Text('API CONNECTED · Backend OTP Verification\nVerifies SHA-256 hashed 6-digit codes via ASP.NET Core API.', style: TextStyle(fontSize: 11, color: Color(0xFF586C48), height: 1.5)),
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
                    Icon(Icons.qr_code_scanner, size: 48, color: _forest),
                    SizedBox(height: 16),
                    Text('QR Scanner', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                    SizedBox(height: 8),
                    Text('Scan the QR code on the item or enter the 6-digit code below.', textAlign: TextAlign.center, style: TextStyle(color: _muted, fontSize: 12)),
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
                      enabled: !_submitting && !_submitted,
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
                      enabled: !_submitting && !_submitted,
                      decoration: const InputDecoration(
                        labelText: 'Handover notes (optional)',
                        prefixIcon: Icon(Icons.note_alt_outlined),
                      ),
                    ),
                    if (_errorMessage != null) ...<Widget>[
                      const SizedBox(height: 8),
                      Text(_errorMessage!, style: const TextStyle(color: Colors.red, fontSize: 13)),
                    ],
                    const SizedBox(height: 16),
                    SizedBox(
                      width: double.infinity,
                      child: FilledButton.icon(
                        onPressed: (_submitting || _submitted) ? null : _submitVerification,
                        icon: _submitting ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white)) : const Icon(Icons.fact_check_outlined),
                        label: Text(_submitting ? 'Verifying...' : _submitted ? 'Verified' : 'Submit Verification'),
                      ),
                    ),
                    if (_submitted && _verifiedProof != null) ...<Widget>[
                      const SizedBox(height: 16),
                      Container(
                        padding: const EdgeInsets.all(14),
                        decoration: BoxDecoration(
                          color: const Color(0xFFEAF0E3),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                          const Row(children: <Widget>[
                            Icon(Icons.check_circle_outline, color: _forest, size: 20),
                            SizedBox(width: 10),
                            Text('Handover Code Verified!', style: TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: _forest)),
                          ]),
                          const SizedBox(height: 6),
                          Text('Proof ID: ${_verifiedProof!['id'] ?? 'Recorded'}', style: const TextStyle(fontSize: 12, color: _forest)),
                          Text('Proof Type: ${_verifiedProof!['proofType'] ?? 'ONE_TIME_CODE'}', style: const TextStyle(fontSize: 12, color: _forest)),
                          const SizedBox(height: 8),
                          ElevatedButton(
                            onPressed: () => Navigator.pop(context, true),
                            child: const Text('Back to Pickup Details'),
                          ),
                        ]),
                      ),
                    ],
                  ]),
                )),
                const SizedBox(height: 24),
                const Text('Member 4 · Handover verification active', textAlign: TextAlign.center, style: TextStyle(fontSize: 10, color: _muted)),
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
