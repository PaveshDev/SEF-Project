import { apiClient } from '../../../shared/services/apiClient.js'

const idempotencyKey = () => `recovery-${crypto.randomUUID()}`

export async function listCases(params = {}) {
  const { data } = await apiClient.get('/api/recovery-cases', { params })
  return data
}

export async function getCase(id) {
  const { data } = await apiClient.get(`/api/recovery-cases/${id}`)
  return data
}

export async function getOptions(caseId) {
  const { data } = await apiClient.get(`/api/recovery-cases/${caseId}/options`)
  return data
}

export async function submitProposal(caseId, payload) {
  const { data } = await apiClient.post(`/api/recovery-cases/${caseId}/proposals`, payload, {
    headers: { 'Idempotency-Key': idempotencyKey() },
  })
  return data
}

export async function createCase(payload) {
  const { data } = await apiClient.post('/api/recovery-cases', payload, {
    headers: { 'Idempotency-Key': idempotencyKey() },
  })
  return data
}

export async function updateCase(id, payload) {
  const { data } = await apiClient.put(`/api/recovery-cases/${id}`, payload, {
    headers: { 'Idempotency-Key': idempotencyKey() },
  })
  return data
}

export async function planCase(id, payload, replan = false) {
  const endpoint = replan ? 'replan' : 'plan'
  const { data } = await apiClient.post(`/api/recovery-cases/${id}/${endpoint}`, payload, {
    headers: { 'Idempotency-Key': idempotencyKey() },
  })
  return data
}

export async function listReferences(params = {}) {
  const { data } = await apiClient.get('/api/value-references', { params })
  return data
}

export async function createReference(payload) {
  const { data } = await apiClient.post('/api/value-references', payload, {
    headers: { 'Idempotency-Key': idempotencyKey() },
  })
  return data
}

export async function getProposal(id) {
  const { data } = await apiClient.get(`/api/recovery-proposals/${id}`)
  return data
}

export async function decideProposal(id, payload) {
  const { data } = await apiClient.post(`/api/recovery-proposals/${id}/decisions`, payload, {
    headers: { 'Idempotency-Key': idempotencyKey() },
  })
  return data
}
