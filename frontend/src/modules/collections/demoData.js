// Fictional UI fixtures. No routing service, agent, or backend has evaluated them.
export const demoJobs = [
  { id: 'PU-1042', item: 'Oak dining chair', category: 'Furniture', owner: 'Demo owner A', address: 'Example collection point A, Colombo', destination: 'Demo reuse workshop', date: '2026-09-11', window: '14:00–16:00', vehicle: 'Small van', collector: 'Demo collector A', status: 'Awaiting review', weight: '8 kg', handling: 'Keep upright; protect wooden finish.', history: ['Approved recovery proposal received', 'Illustrative collection proposal ready for review'] },
  { id: 'PU-1041', item: 'Office desk', category: 'Furniture', owner: 'Demo owner B', address: 'Example collection point B, Colombo', destination: 'Demo repair hub', date: '2026-09-11', window: '09:00–11:00', vehicle: 'Cargo van', collector: 'Demo collector B', status: 'Scheduled', weight: '24 kg', handling: 'Two-person lift; secure removable legs.', history: ['Scheduled — sample assignment'] },
  { id: 'PU-1040', item: 'Small refrigerator', category: 'Appliance', owner: 'Demo owner C', address: 'Example collection point C, Colombo', destination: 'Demo repair hub', date: '2026-09-11', window: '11:00–13:00', vehicle: 'Cargo van', collector: 'Unassigned', status: 'Failed', weight: '38 kg', handling: 'Keep upright; two-person lift.', failure: 'Assigned vehicle became unavailable.', history: ['Scheduled — sample assignment', 'Failed — assigned vehicle unavailable'] },
  { id: 'PU-1039', item: 'Bookshelf', category: 'Furniture', owner: 'Demo owner D', address: 'Example collection point D, Colombo', destination: 'Demo reuse workshop', date: '2026-09-11', window: '08:00–10:00', vehicle: 'Cargo van', collector: 'Demo collector A', status: 'En Route', weight: '19 kg', handling: 'Remove loose shelves before loading.', history: ['Scheduled — sample assignment', 'En Route — sample milestone'] },
  { id: 'PU-1038', item: 'Reading lamp', category: 'Household', owner: 'Demo owner E', address: 'Example collection point E, Colombo', destination: 'Demo reuse workshop', date: '2026-09-10', window: '13:00–15:00', vehicle: 'Small van', collector: 'Demo collector B', status: 'Handover Verified', weight: '2 kg', handling: 'Wrap the shade separately.', history: ['Scheduled — sample milestone', 'En Route — sample milestone', 'Collected — sample milestone', 'Delivered — sample milestone', 'Handover Verified — fictional completed record'] },
]

export const demoSlots = [
  { id: 'SL-201', date: '2026-09-11', start: '09:00', end: '11:00', vehicle: 'Cargo van', capacity: '100', collector: 'Demo collector B' },
  { id: 'SL-202', date: '2026-09-11', start: '14:00', end: '16:00', vehicle: 'Small van', capacity: '50', collector: 'Demo collector A' },
  { id: 'SL-203', date: '2026-09-11', start: '16:00', end: '18:00', vehicle: 'Small van', capacity: '50', collector: 'Demo collector A' },
]

export const milestones = ['Scheduled', 'En Route', 'Collected', 'Delivered', 'Handover Verified']

export function formatDate(value) {
  return new Date(`${value}T12:00:00`).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })
}
