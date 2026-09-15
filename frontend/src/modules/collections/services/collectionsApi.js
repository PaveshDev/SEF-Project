import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_BASE_URL || import.meta.env.VITE_API_URL || 'http://localhost:5080'

const api = axios.create({
  baseURL: `${API_BASE}/api/collections`,
  timeout: 8000,
  headers: { 'Content-Type': 'application/json' },
})

// ── Helpers ────────────────────────────────────────────────

export function mapSlotFromApi(slot) {
  if (!slot) return slot
  const dateStr = slot.startsAt ? slot.startsAt.slice(0, 10) : (slot.date || '2026-09-11')
  const startStr = slot.startsAt ? slot.startsAt.slice(11, 16) : (slot.start || '09:00')
  const endStr = slot.endsAt ? slot.endsAt.slice(11, 16) : (slot.end || '11:00')
  return {
    ...slot,
    id: String(slot.id),
    date: slot.date || dateStr,
    start: slot.start || startStr,
    end: slot.end || endStr,
    vehicle: slot.vehicle || slot.vehicleClass || 'Small van',
    vehicleClass: slot.vehicleClass || slot.vehicle || 'Small van',
    capacity: slot.capacity ?? 50,
    collector: slot.collector || (slot.collectorId ? `Collector ${String(slot.collectorId).slice(0, 8)}` : 'Unassigned'),
    startsAt: slot.startsAt || `${dateStr}T${startStr}:00+05:30`,
    endsAt: slot.endsAt || `${dateStr}T${endStr}:00+05:30`,
  }
}

export function mapPickupFromApi(job) {
  if (!job) return job
  const dateStr = job.scheduledStart ? job.scheduledStart.slice(0, 10) : (job.date || '2026-09-11')
  const startTime = job.scheduledStart ? job.scheduledStart.slice(11, 16) : '09:00'
  const endTime = job.scheduledEnd ? job.scheduledEnd.slice(11, 16) : '11:00'
  const windowStr = job.window || `${startTime}–${endTime}`
  const uiStatus = mapStatusFromApi(job.status)
  return {
    ...job,
    id: String(job.id),
    item: job.item || (job.recoveryProposalId ? `Pickup Item (${String(job.recoveryProposalId).slice(0, 8)})` : 'Waste Material'),
    owner: job.owner || (job.ownerId ? `Donor (${String(job.ownerId).slice(0, 8)})` : 'Community Donor'),
    address: job.pickupAddress || job.address || 'Pickup Location, Colombo',
    destination: job.destination || 'Processing Center',
    date: job.date || dateStr,
    window: windowStr,
    vehicle: job.vehicle || job.collectionSlot?.vehicleClass || 'Small van',
    collector: job.collector || (job.collectorId ? `Collector ${String(job.collectorId).slice(0, 8)}` : 'Unassigned'),
    status: uiStatus,
    handling: job.handling || 'Standard handling',
    history: job.history || [`Status: ${job.status}`, `Created: ${job.createdAt ? new Date(job.createdAt).toLocaleString() : 'N/A'}`],
  }
}

export function mapStatusToApi(uiStatus) {
  const map = {
    'Draft': 'DRAFT',
    'Awaiting review': 'PROPOSED',
    'Scheduled': 'CONFIRMED',
    'En Route': 'ASSIGNED',
    'Collected': 'COLLECTED',
    'Delivered': 'DELIVERED',
    'Handover Verified': 'DELIVERED',
    'Failed': 'FAILED',
    'Cancelled': 'CANCELLED',
  }
  return map[uiStatus] || uiStatus
}

export function mapStatusFromApi(apiStatus) {
  const map = {
    'DRAFT': 'Draft',
    'PROPOSED': 'Awaiting review',
    'CONFIRMED': 'Scheduled',
    'ASSIGNED': 'En Route',
    'COLLECTED': 'Collected',
    'DELIVERED': 'Handover Verified',
    'FAILED': 'Failed',
    'CANCELLED': 'Cancelled',
  }
  return map[apiStatus] || apiStatus
}

// ── Collection Slots ───────────────────────────────────────

export async function fetchSlots() {
  const { data } = await api.get('/slots')
  return (data || []).map(mapSlotFromApi)
}

export async function fetchSlot(id) {
  const { data } = await api.get(`/slots/${id}`)
  return mapSlotFromApi(data)
}

export async function createSlot(slot) {
  const dateStr = slot.date || '2026-09-11'
  const startStr = slot.start || '09:00'
  const endStr = slot.end || '11:00'
  const payload = {
    startsAt: slot.startsAt || `${dateStr}T${startStr}:00+05:30`,
    endsAt: slot.endsAt || `${dateStr}T${endStr}:00+05:30`,
    serviceArea: slot.serviceArea || 'Colombo',
    capacity: Number(slot.capacity) || 50,
    vehicleClass: slot.vehicle || slot.vehicleClass || 'Small van',
    collectorId: slot.collectorId || null,
  }
  const { data } = await api.post('/slots', payload)
  return mapSlotFromApi(data)
}

export async function updateSlot(id, update) {
  const payload = {
    startsAt: update.date && update.start ? `${update.date}T${update.start}:00+05:30` : update.startsAt,
    endsAt: update.date && update.end ? `${update.date}T${update.end}:00+05:30` : update.endsAt,
    serviceArea: update.serviceArea,
    capacity: update.capacity !== undefined ? Number(update.capacity) : undefined,
    vehicleClass: update.vehicle || update.vehicleClass,
    collectorId: update.collectorId,
    status: update.status,
  }
  const { data } = await api.put(`/slots/${id}`, payload)
  return mapSlotFromApi(data)
}

export async function deleteSlot(id) {
  await api.delete(`/slots/${id}`)
  return true
}

// ── Pickup Requests ────────────────────────────────────────

export async function fetchPickups(status) {
  const params = status ? { status: mapStatusToApi(status) } : {}
  const { data } = await api.get('/pickups', { params })
  return (data || []).map(mapPickupFromApi)
}

export async function fetchPickup(id) {
  const { data } = await api.get(`/pickups/${id}`)
  return mapPickupFromApi(data)
}

export async function createPickup(pickup) {
  const dateStr = pickup.date || '2026-09-11'
  const startStr = pickup.start || (pickup.window ? pickup.window.split('–')[0] : '09:00')
  const endStr = pickup.end || (pickup.window ? pickup.window.split('–')[1] : '11:00')
  const payload = {
    recoveryProposalId: pickup.recoveryProposalId || crypto.randomUUID(),
    collectionSlotId: pickup.collectionSlotId || crypto.randomUUID(),
    ownerId: pickup.ownerId || crypto.randomUUID(),
    pickupAddress: pickup.address || pickup.pickupAddress || 'Pickup address, Colombo',
    scheduledStart: pickup.scheduledStart || `${dateStr}T${startStr}:00+05:30`,
    scheduledEnd: pickup.scheduledEnd || `${dateStr}T${endStr}:00+05:30`,
  }
  const { data } = await api.post('/pickups', payload)
  return mapPickupFromApi(data)
}

export async function updatePickup(id, update) {
  const payload = {
    scheduledStart: update.scheduledStart || (update.date && update.start ? `${update.date}T${update.start}:00+05:30` : undefined),
    scheduledEnd: update.scheduledEnd || (update.date && update.end ? `${update.date}T${update.end}:00+05:30` : undefined),
    pickupAddress: update.address || update.pickupAddress,
    collectorId: update.collectorId,
    status: update.status ? mapStatusToApi(update.status) : undefined,
  }
  const { data } = await api.put(`/pickups/${id}`, payload)
  return mapPickupFromApi(data)
}

export async function deletePickup(id) {
  await api.delete(`/pickups/${id}`)
  return true
}

export async function fetchPickupEvents(pickupId) {
  const { data } = await api.get(`/pickups/${pickupId}/events`)
  return data
}

export async function reschedulePickup(id, request) {
  const { data } = await api.post(`/pickups/${id}/reschedule`, {
    reason: request.reason,
    requestedBy: request.requestedBy || '00000000-0000-0000-0000-000000000000',
  })
  return mapPickupFromApi(data)
}

// ── Handover ───────────────────────────────────────────────

export async function verifyHandoverCode(pickupId, request) {
  const { data } = await api.post(`/pickups/${pickupId}/handover/verify`, request)
  return data
}

export async function submitHandoverProof(pickupId, request) {
  const { data } = await api.post(`/pickups/${pickupId}/handover/proof`, request)
  return data
}

export async function fetchHandoverProofs(pickupId) {
  const { data } = await api.get(`/pickups/${pickupId}/handover`)
  return data
}

// ── Collection Agent ───────────────────────────────────────

export async function prepareCollectionPlan(pickupRequestId) {
  const { data } = await api.post('/agent/plan', { pickupRequestId })
  return data
}

export async function rescheduleAgent(request) {
  const { data } = await api.post('/agent/reschedule', request)
  return data
}

export async function fetchProposal(proposalId) {
  const { data } = await api.get(`/agent/proposals/${proposalId}`)
  return data
}

export async function approveProposal(proposalId, decision) {
  const { data } = await api.post(`/agent/proposals/${proposalId}/approve`, decision)
  return data
}
