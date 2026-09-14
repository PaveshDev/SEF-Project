// Fictional UI data; no backend validation or agent execution has occurred.
class DemoPickup {
  const DemoPickup({
    required this.id,
    required this.item,
    required this.address,
    required this.destination,
    required this.window,
    required this.vehicle,
    required this.status,
    required this.handling,
  });

  final String id;
  final String item;
  final String address;
  final String destination;
  final String window;
  final String vehicle;
  final String status;
  final String handling;
}

const collectionMilestones = <String>[
  'Scheduled',
  'En Route',
  'Collected',
  'Delivered',
  'Handover Verified',
];

const demoPickups = <DemoPickup>[
  DemoPickup(
    id: 'PU-1042',
    item: 'Oak dining chair',
    address: 'Example collection point A, Colombo',
    destination: 'Demo reuse workshop',
    window: '11 Sep 2026 · 14:00–16:00',
    vehicle: 'Small van',
    status: 'Awaiting review',
    handling: '8 kg · Keep upright; protect the wooden finish.',
  ),
  DemoPickup(
    id: 'PU-1041',
    item: 'Office desk',
    address: 'Example collection point B, Colombo',
    destination: 'Demo repair hub',
    window: '11 Sep 2026 · 09:00–11:00',
    vehicle: 'Cargo van',
    status: 'Scheduled',
    handling: '24 kg · Two-person lift; secure removable legs.',
  ),
  DemoPickup(
    id: 'PU-1040',
    item: 'Small refrigerator',
    address: 'Example collection point C, Colombo',
    destination: 'Demo repair hub',
    window: '11 Sep 2026 · 11:00–13:00',
    vehicle: 'Cargo van · unavailable',
    status: 'Failed',
    handling: '38 kg · Keep upright; two-person lift.',
  ),
  DemoPickup(
    id: 'PU-1039',
    item: 'Bookshelf',
    address: 'Example collection point D, Colombo',
    destination: 'Demo reuse workshop',
    window: '11 Sep 2026 · 08:00–10:00',
    vehicle: 'Cargo van',
    status: 'En Route',
    handling: '19 kg · Remove loose shelves before loading.',
  ),
  DemoPickup(
    id: 'PU-1038',
    item: 'Reading lamp',
    address: 'Example collection point E, Colombo',
    destination: 'Demo reuse workshop',
    window: '10 Sep 2026 · 13:00–15:00',
    vehicle: 'Small van',
    status: 'Handover Verified',
    handling: '2 kg · Wrap the shade separately.',
  ),
];
