import { useCallback, useEffect, useState } from 'react'
import {
  fetchPickups, fetchSlots, fetchPickupEvents,
  createPickup, updatePickup, deletePickup, reschedulePickup,
  createSlot, updateSlot, deleteSlot,
  verifyHandoverCode, submitHandoverProof, fetchHandoverProofs,
  prepareCollectionPlan, rescheduleAgent, fetchProposal, approveProposal,
} from '../services/collectionsApi.js'
import { demoJobs, demoSlots } from '../demoData.js'

/**
 * Central hook that tries the ASP.NET Collections API first
 * and falls back to local demo fixtures when the backend is unreachable.
 *
 * All CRUD helpers call the API service, then update local state
 * optimistically so the UI reflects the change immediately.
 */
export function useCollections() {
  const [jobs, setJobs] = useState(demoJobs)
  const [slots, setSlots] = useState(demoSlots)
  const [proposal, setProposal] = useState(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [apiConnected, setApiConnected] = useState(false)

  // ── Initial load ─────────────────────────────────────────

  const loadData = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [pickupData, slotData] = await Promise.all([fetchPickups(), fetchSlots()])
      // The API service returns objects without `_demo` when the backend responds
      if (pickupData && pickupData.length > 0 && !pickupData[0]._demo) {
        setApiConnected(true)
      }
      setJobs(pickupData[0]?._demo ? demoJobs : pickupData)
      setSlots(slotData)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { loadData() }, [loadData])

  // ── Pickup CRUD ──────────────────────────────────────────

  const addJob = useCallback(async (jobData) => {
    if (apiConnected) {
      const result = await createPickup(jobData)
      if (result) { setJobs(current => [...current, result]); return result }
    }
    // Fallback: add locally with a generated id
    const local = {
      ...jobData,
      id: jobData.id || `DRAFT-${crypto.randomUUID().slice(0, 8)}`,
      status: jobData.status || 'Draft',
      history: jobData.history || ['Draft created locally — not submitted'],
    }
    setJobs(current => [...current, local])
    return local
  }, [apiConnected])

  const updateJob = useCallback(async (id, update) => {
    if (apiConnected) {
      const result = await updatePickup(id, update)
      if (result) {
        setJobs(current => current.map(job => job.id === id ? result : job))
        return result
      }
    }
    // Fallback: update locally
    setJobs(current => current.map(job => job.id === id ? { ...job, ...update } : job))
    return null
  }, [apiConnected])

  const removeJob = useCallback(async (id) => {
    if (apiConnected) {
      const ok = await deletePickup(id)
      if (ok) { setJobs(current => current.filter(job => job.id !== id)); return true }
    }
    setJobs(current => current.filter(job => job.id !== id))
    return true
  }, [apiConnected])

  const rescheduleJob = useCallback(async (id, request) => {
    if (apiConnected) {
      const result = await reschedulePickup(id, request)
      if (result) {
        setJobs(current => current.map(job => job.id === id ? result : job))
        return result
      }
    }
    // Fallback: update status locally
    setJobs(current => current.map(job => job.id === id
      ? { ...job, status: 'Failed', rescheduleNote: request.reason, history: [...(job.history || []), `Local revision request: ${request.reason}`] }
      : job))
    return null
  }, [apiConnected])

  const loadEvents = useCallback(async (pickupId) => {
    return await fetchPickupEvents(pickupId)
  }, [])

  // ── Collection Slot CRUD ─────────────────────────────────

  const addSlot = useCallback(async (slotData) => {
    if (apiConnected) {
      const result = await createSlot(slotData)
      if (result) { setSlots(current => [...current, result]); return result }
    }
    const local = { ...slotData, id: slotData.id || `SL-${crypto.randomUUID().slice(0, 8)}` }
    setSlots(current => [...current, local])
    return local
  }, [apiConnected])

  const editSlot = useCallback(async (id, update) => {
    if (apiConnected) {
      const result = await updateSlot(id, update)
      if (result) {
        setSlots(current => current.map(s => s.id === id ? result : s))
        return result
      }
    }
    setSlots(current => current.map(s => s.id === id ? { ...s, ...update } : s))
    return null
  }, [apiConnected])

  const removeSlot = useCallback(async (id) => {
    if (apiConnected) {
      const ok = await deleteSlot(id)
      if (ok) { setSlots(current => current.filter(s => s.id !== id)); return true }
    }
    setSlots(current => current.filter(s => s.id !== id))
    return true
  }, [apiConnected])

  const updateSlots = useCallback((newSlots) => {
    setSlots(newSlots)
  }, [])

  // ── Handover ─────────────────────────────────────────────

  const verifyCode = useCallback(async (pickupId, code, actorId) => {
    return await verifyHandoverCode(pickupId, { code, actorId })
  }, [])

  const submitProof = useCallback(async (pickupId, proofData) => {
    return await submitHandoverProof(pickupId, proofData)
  }, [])

  const loadProofs = useCallback(async (pickupId) => {
    return await fetchHandoverProofs(pickupId)
  }, [])

  // ── Collection Agent ─────────────────────────────────────

  const requestPlan = useCallback(async (pickupRequestId) => {
    setLoading(true)
    try {
      const result = await prepareCollectionPlan(pickupRequestId)
      setProposal(result)
      return result
    } catch (err) {
      setError(err.message)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  const requestReschedule = useCallback(async (request) => {
    setLoading(true)
    try {
      const result = await rescheduleAgent(request)
      setProposal(result)
      return result
    } catch (err) {
      setError(err.message)
      return null
    } finally {
      setLoading(false)
    }
  }, [])

  const loadProposal = useCallback(async (proposalId) => {
    return await fetchProposal(proposalId)
  }, [])

  const submitApproval = useCallback(async (proposalId, decision) => {
    return await approveProposal(proposalId, decision)
  }, [])

  return {
    // State
    jobs, slots, proposal, loading, error, apiConnected,

    // Data loading
    loadData, loadEvents, loadProofs, loadProposal,

    // Pickup CRUD
    addJob, updateJob, removeJob, rescheduleJob,

    // Slot CRUD
    addSlot, editSlot, removeSlot, updateSlots, setSlots,

    // Handover
    verifyCode, submitProof,

    // Agent
    requestPlan, requestReschedule, submitApproval,
  }
}
