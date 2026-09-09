import axios from 'axios'
import { demoJobs, demoSlots } from '../demoData.js'

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5080'

const api = axios.create({
  baseURL: `${API_BASE}/api/collections`,
  timeout: 8000,
  headers: { 'Content-Type': 'application/json' },
})

// ── Collection Slots ───────────────────────────────────────

export async function fetchSlots() {
  try {
    const { data } = await api.get('/slots')
    return data
  } catch {
    return demoSlots.map(slot => ({
      id: slot.id,
      startsAt: `2026-09-11T${slot.start}:00+05:30`,
      endsAt: `2026-09-11T${slot.end}:00+05:30`,
      serviceArea: 'Colombo',
      capacity: Number(slot.capacity),
      reservedCount: 0,
      vehicleClass: slot.vehicle,
      status: 'AVAILABLE',
      collectorId: null,
    }))
  }
}

export async function createSlot(slot) {
  try {
    const { data } = await api.post('/slots', slot)
    return data
  } catch {
    return { id: `SL-${crypto.randomUUID().slice(0, 8)}`, ...slot, status: 'AVAILABLE' }
  }
}

export async function updateSlot(id, update) {
  try {
    const { data } = await api.put(`/slots/${id}`, update)
    return data
  } catch {
    return null
  }
}

export async function deleteSlot(id) {
  try {
    await api.delete(`/slots/${id}`)
    return true
  } catch {
    return false
  }
}

// ── Pickup Requests ────────────────────────────────────────

export async function fetchPickups(status) {
  try {
    const params = status ? { status } : {}
    const { data } = await api.get('/pickups', { params })
    return data
  } catch {
    return demoJobs.map(job => ({
      id: job.id,
      scheduledStart: `2026-09-11T${job.window.split('–')[0]}:00+05:30`,
      scheduledEnd: `2026-09-11T${job.window.split('–')[1]}:00+05:30`,
      status: mapStatusToApi(job.status),
      collectorId: null,
      ownerId: null,
      _demo: job,
    }))
  }
}

export async function fetchPickup(id) {
  try {
    const { data } = await api.get(`/pickups/${id}`)
    return data
  } catch {
    return null
  }
}

export async function createPickup(pickup) {
  try {
    const { data } = await api.post('/pickups', pickup)
    return data
  } catch {
    return null
  }
}

export async function updatePickup(id, update) {
  try {
    const { data } = await api.put(`/pickups/${id}`, update)
    return data
  } catch {
    return null
  }
}

export async function deletePickup(id) {
  try {
    await api.delete(`/pickups/${id}`)
    return true
  } catch {
    return false
  }
}

export async function fetchPickupEvents(pickupId) {
  try {
    const { data } = await api.get(`/pickups/${pickupId}/events`)
    return data
  } catch {
    return []
  }
}

export async function reschedulePickup(id, request) {
  try {
    const { data } = await api.post(`/pickups/${id}/reschedule`, request)
    return data
  } catch {
    return null
  }
}

// ── Handover ───────────────────────────────────────────────

export async function verifyHandoverCode(pickupId, request) {
  try {
    const { data } = await api.post(`/pickups/${pickupId}/handover/verify`, request)
    return data
  } catch {
    return null
  }
}

export async function submitHandoverProof(pickupId, request) {
  try {
    const { data } = await api.post(`/pickups/${pickupId}/handover/proof`, request)
    return data
  } catch {
    return null
  }
}

export async function fetchHandoverProofs(pickupId) {
  try {
    const { data } = await api.get(`/pickups/${pickupId}/handover`)
    return data
  } catch {
    return []
  }
}

// ── Collection Agent ───────────────────────────────────────

export async function prepareCollectionPlan(pickupRequestId) {
  try {
    const { data } = await api.post('/agent/plan', { pickupRequestId })
    return data
  } catch {
    return null
  }
}

export async function rescheduleAgent(request) {
  try {
    const { data } = await api.post('/agent/reschedule', request)
    return data
  } catch {
    return null
  }
}

export async function fetchProposal(proposalId) {
  try {
    const { data } = await api.get(`/agent/proposals/${proposalId}`)
    return data
  } catch {
    return null
  }
}

export async function approveProposal(proposalId, decision) {
  try {
    const { data } = await api.post(`/agent/proposals/${proposalId}/approve`, decision)
    return data
  } catch {
    return null
  }
}

// ── Helpers ────────────────────────────────────────────────

function mapStatusToApi(uiStatus) {
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
