import 'package:flutter/material.dart';

const _forest = Color(0xFF2D674E);
const _ink = Color(0xFF203B32);
const _muted = Color(0xFF64745F);
const _canvas = Color(0xFFF5F6F2);

/// Mobile view of the AI collection proposal with approve/reject/revision controls.
class ProposalReviewScreen extends StatefulWidget {
  const ProposalReviewScreen({super.key});

  @override
  State<ProposalReviewScreen> createState() => _ProposalReviewScreenState();
}

class _ProposalReviewScreenState extends State<ProposalReviewScreen> {
  final _review = TextEditingController();
  String? _decision;
  String? _reviewError;

  @override
  void dispose() {
    _review.dispose();
    super.dispose();
  }

  void _message(String text) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(text)));
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
    _message('$action preference recorded locally. Backend authorization required.');
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
        appBar: AppBar(backgroundColor: _canvas, title: const Text('Proposal Review')),
        body: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 720),
              child: ListView(padding: const EdgeInsets.all(20), children: <Widget>[
                _Banner(),
                const SizedBox(height: 22),
                const Text('Oak dining chair', style: TextStyle(fontSize: 28, fontFamily: 'serif', color: _ink)),
                const SizedBox(height: 6),
                const Text('PU-1042 · Illustrative proposal', style: TextStyle(color: _muted, fontSize: 12)),
                const SizedBox(height: 20),

                // Proposal
                _Card(
                  color: const Color(0xFFEAF0E3),
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                    Text('COLLECTION PROPOSAL · STATIC SAMPLE', style: TextStyle(fontSize: 9, letterSpacing: 1.4, color: _muted)),
                    const SizedBox(height: 12),
                    const Text('Friday, 14:00–16:00', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                    const SizedBox(height: 8),
                    const Text('Small van · 50 kg capacity\nOwner available 14:00–17:00\nDestination open 09:00–17:00', style: TextStyle(height: 1.8)),
                    const SizedBox(height: 15),
                    const Text('Why this option?', style: TextStyle(fontWeight: FontWeight.w600)),
                    const SizedBox(height: 6),
                    const _Check(true, 'Vehicle capacity exceeds item weight'),
                    const _Check(true, 'Owner availability overlaps window'),
                    const _Check(true, 'Destination open during proposed time'),
                    const _Check(false, 'Travel estimate unavailable'),
                    const SizedBox(height: 15),
                    const Divider(height: 28),
                    const Text('Fallback: 16:00–18:00', style: TextStyle(fontWeight: FontWeight.w600)),
                    const SizedBox(height: 6),
                    const Text('Destination closes at 17:00. Needs revision.', style: TextStyle(color: _muted, fontSize: 12)),
                  ]),
                ),
                const SizedBox(height: 20),

                // Decision
                _Card(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: <Widget>[
                  const Text('Review preference', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600, color: _ink)),
                  const SizedBox(height: 8),
                  const Text('No agent has run. UI preview only.', style: TextStyle(color: _muted, fontSize: 12)),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _review,
                    maxLines: 3,
                    maxLength: 500,
                    decoration: InputDecoration(labelText: 'Review notes', errorText: _reviewError),
                  ),
                  const SizedBox(height: 12),
                  Wrap(spacing: 8, runSpacing: 8, children: <Widget>[
                    FilledButton(onPressed: () => _recordDecision('Approve'), child: const Text('Approve')),
                    OutlinedButton(onPressed: () => _recordDecision('Reject'), child: const Text('Reject')),
                    OutlinedButton(onPressed: () => _recordDecision('Request Revision'), child: const Text('Revise')),
                  ]),
                  if (_decision != null) Padding(
                    padding: const EdgeInsets.only(top: 15),
                    child: Text('Local preference: $_decision\nBooking unconfirmed.', style: const TextStyle(color: _forest, fontSize: 12)),
                  ),
                ])),
                const SizedBox(height: 24),
                const Text('Member 4 · Proposal review preview', textAlign: TextAlign.center, style: TextStyle(fontSize: 10, color: _muted)),
              ]),
            ),
          ),
        ),
      ),
    );
  }
}

class _Banner extends StatelessWidget {
  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(12),
    decoration: BoxDecoration(color: const Color(0xFFE7ECDE), borderRadius: BorderRadius.circular(8)),
    child: const Text('UI PREVIEW · No agent has run\nStatic sample data only.', style: TextStyle(fontSize: 11, color: Color(0xFF586C48), height: 1.5)),
  );
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

class _Check extends StatelessWidget {
  const _Check(this.passed, this.label);
  final bool passed;
  final String label;
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 4),
    child: Row(children: <Widget>[
      Icon(passed ? Icons.check_circle_outline : Icons.warning_amber_outlined, size: 18, color: passed ? _forest : const Color(0xFF84652E)),
      const SizedBox(width: 8),
      Expanded(child: Text(label, style: TextStyle(fontSize: 12, color: passed ? _ink : const Color(0xFF84652E)))),
    ]),
  );
}
