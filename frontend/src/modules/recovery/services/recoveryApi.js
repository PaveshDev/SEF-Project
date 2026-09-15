import { apiClient } from '../../../shared/services/apiClient.js'

const pending = new Map()
async function mutation(method, url, payload) {
  const bytes = new TextEncoder().encode(JSON.stringify([method, url, payload ?? null]))
  const digest = Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256', bytes)), b => b.toString(16).padStart(2, '0')).join('')
  const slot = 'recovery-operation-' + digest
  let key = pending.get(slot)
  try { key ||= sessionStorage.getItem(slot) } catch { /* Optional storage. */ }
  key ||= 'recovery-' + crypto.randomUUID()
  pending.set(slot, key)
  try { sessionStorage.setItem(slot, key) } catch { /* Memory supports retries. */ }
  const clear = () => { pending.delete(slot); try { sessionStorage.removeItem(slot) } catch { /* Optional storage. */ } }
  try {
    const response = await apiClient.request({ method, url, data: payload, headers: { 'Idempotency-Key': key } })
    clear()
    return response
  } catch (error) {
    const status = error?.response?.status
    if (status >= 400 && status < 500 && ![408, 429].includes(status)) clear()
    throw error
  }
}

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
  const { data } = await mutation('post', `/api/recovery-cases/${caseId}/proposals`, payload)
  return data
}

export async function createCase(payload) {
  const { data } = await mutation('post', '/api/recovery-cases', payload)
  return data
}

export async function updateCase(id, payload) {
  const { data } = await mutation('put', `/api/recovery-cases/${id}`, payload)
  return data
}

export async function planCase(id, payload, replan = false) {
  const endpoint = replan ? 'replan' : 'plan'
  const { data } = await mutation('post', `/api/recovery-cases/${id}/${endpoint}`, payload)
  return data
}

export async function listReferences(params = {}) {
  const { data } = await apiClient.get('/api/value-references', { params })
  return data
}

export async function createReference(payload) {
  const { data } = await mutation('post', '/api/value-references', payload)
  return data
}

export async function getProposal(id) {
  const { data } = await apiClient.get(`/api/recovery-proposals/${id}`)
  return data
}

export async function decideProposal(id, payload) {
  const { data } = await mutation('post', `/api/recovery-proposals/${id}/decisions`, payload)
  return data
}

export const listProposals = async id => (await apiClient.get(`/api/recovery-cases/${id}/proposals`)).data
export const getAccess = async () => (await apiClient.get('/api/recovery/access')).data
export const cancelCase = async (id, version) => (await mutation('post', `/api/recovery-cases/${id}/cancel`, { expectedVersion: version })).data
export const deleteCase = async id => (await mutation('delete', `/api/recovery-cases/${id}`)).data
export const refreshProposal = async id => (await mutation('post', `/api/recovery-proposals/${id}/refresh`)).data
export const updateReference = async (id, payload) => (await mutation('put', `/api/value-references/${id}`, payload)).data
export const deleteReference = async id => (await mutation('delete', `/api/value-references/${id}`)).data
export const verifyReference = async (id, version) => (await mutation('post', `/api/value-references/${id}/verify`, { expectedVersion: version })).data
