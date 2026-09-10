import { useCallback, useEffect, useState } from 'react'
import {
  fetchPickups, fetchSlots, fetchPickupEvents,
  createPickup, updatePickup, deletePickup, reschedulePickup,
  createSlot, updateSlot, deleteSlot,
  verifyHandoverCode, submitHandoverProof, fetchHandoverProofs,
  prepareCollectionPlan, rescheduleAgent, fetchProposal, approveProposal,
} from '../services/collectionsApi.js'

/**
 * Member 4 Collections Hook.
 * Communicates directly with the ASP.NET Core Collections API.
 * Distinguishes API connectivity by HTTP status, not record count.
 * Propagates real errors and avoids silent demo fallbacks.
 */
export function useCollections() {
  const [jobs, setJobs] = useState([])
  const [slots, setSlots] = useState([])
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
      // HTTP 200 from backend: API is connected even if arrays are empty []
      setApiConnected(true)
      setJobs(pickupData || [])
      setSlots(slotData || [])
    } catch (err) {
      setApiConnected(false)
      const msg = err.response?.data?.error || err.response?.data?.title || err.message || 'Failed to connect to backend API'
      setError(msg)
      setJobs([])
      setSlots([])
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { loadData() }, [loadData])

  // ── Pickup CRUD ──────────────────────────────────────────

  const addJob = useCallback(async (jobData) => {
    setError(null)
    const result = await createPickup(jobData)
    if (result) {
      await loadData()
      return result
    }
    return null
  }, [loadData])

  const updateJob = useCallback(async (id, update) => {
    setError(null)
    const result = await updatePickup(id, update)
    if (result) {
      await loadData()
      return result
    }
    return null
  }, [loadData])

  const removeJob = useCallback(async (id) => {
    setError(null)
    const ok = await deletePickup(id)
    if (ok) {
      await loadData()
      return true
    }
    return false
  }, [loadData])

  const rescheduleJob = useCallback(async (id, request) => {
    setError(null)
    const result = await reschedulePickup(id, request)
    if (result) {
      await loadData()
      return result
    }
    return null
  }, [loadData])

  const loadEvents = useCallback(async (pickupId) => {
    return await fetchPickupEvents(pickupId)
  }, [])

  // ── Collection Slot CRUD ─────────────────────────────────

  const addSlot = useCallback(async (slotData) => {
    setError(null)
    const result = await createSlot(slotData)
    if (result) {
      await loadData()
      return result
    }
    return null
  }, [loadData])

  const editSlot = useCallback(async (id, update) => {
    setError(null)
    const result = await updateSlot(id, update)
    if (result) {
      await loadData()
      return result
    }
    return null
  }, [loadData])

  const removeSlot = useCallback(async (id) => {
    setError(null)
    const ok = await deleteSlot(id)
    if (ok) {
      await loadData()
      return true
    }
    return false
  }, [loadData])

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
    verifyCode, submitProof, loadProofs,

    // Agent
    requestPlan, requestReschedule, submitApproval,
  }
}
