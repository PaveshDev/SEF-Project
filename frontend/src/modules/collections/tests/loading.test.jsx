import { act, fireEvent, render, renderHook, screen, waitFor } from '@testing-library/react'
import { beforeEach, expect, test, vi } from 'vitest'
import { useCollections } from '../hooks/useCollections.js'
import ProposalReviewSection from '../components/ProposalReviewSection.jsx'
import * as api from '../services/collectionsApi.js'

vi.mock('../services/collectionsApi.js', () => ({
  fetchPickups: vi.fn(), fetchSlots: vi.fn(), fetchPickupEvents: vi.fn(),
  createPickup: vi.fn(), updatePickup: vi.fn(), deletePickup: vi.fn(), reschedulePickup: vi.fn(),
  createSlot: vi.fn(), updateSlot: vi.fn(), deleteSlot: vi.fn(),
  verifyHandoverCode: vi.fn(), submitHandoverProof: vi.fn(), fetchHandoverProofs: vi.fn(),
  prepareCollectionPlan: vi.fn(), rescheduleAgent: vi.fn(), fetchProposal: vi.fn(), approveProposal: vi.fn(),
}))

const deferred = () => {
  let resolve
  const promise = new Promise(yes => { resolve = yes })
  return { promise, resolve }
}
const Badge = ({ status }) => <span>{status}</span>

beforeEach(() => vi.resetAllMocks())

test('collections refresh ignores superseded data and aborts on unmount', async () => {
  const first = deferred()
  api.fetchPickups.mockReturnValueOnce(first.promise).mockResolvedValueOnce([{ id: 'latest' }])
  api.fetchSlots.mockResolvedValue([])
  const { result, unmount } = renderHook(() => useCollections())
  expect(result.current.loading).toBe(true)
  const signal = api.fetchPickups.mock.calls[0][1]
  await act(async () => { await result.current.loadData() })
  expect(signal.aborted).toBe(true)
  await act(async () => { first.resolve([{ id: 'stale' }]); await first.promise })
  expect(result.current.jobs).toEqual([{ id: 'latest' }])
  expect(result.current.apiConnected).toBe(true)
  expect(result.current.loading).toBe(false)
  expect(api.fetchPickups).toHaveBeenCalledTimes(2)
  expect(api.fetchSlots).toHaveBeenCalledTimes(2)
  unmount()
  expect(api.fetchPickups.mock.calls[1][1].aborted).toBe(true)
})

test('collections retry clears connection errors and accepts empty data', async () => {
  api.fetchPickups.mockRejectedValueOnce(new Error('Offline')).mockResolvedValueOnce([])
  api.fetchSlots.mockResolvedValue([])
  const { result } = renderHook(() => useCollections())
  await waitFor(() => expect(result.current.error).toBe('Offline'))
  expect(result.current.apiConnected).toBe(false)
  await act(async () => { await result.current.loadData() })
  expect(result.current.error).toBeNull()
  expect(result.current.apiConnected).toBe(true)
  expect(result.current.jobs).toEqual([])
})

test('proposal cleanup aborts its request and does not start an event request afterward', async () => {
  const pending = deferred()
  api.prepareCollectionPlan.mockReturnValue(pending.promise)
  const { unmount } = render(<ProposalReviewSection Badge={Badge} setReview={vi.fn()} tell={vi.fn()} />)
  expect(screen.getByText('Loading Agentic AI proposal...')).toBeInTheDocument()
  const signal = api.prepareCollectionPlan.mock.calls[0][1]
  unmount()
  expect(signal.aborted).toBe(true)
  await act(async () => { pending.resolve({ proposalId: 'stale' }); await pending.promise })
  expect(api.fetchPickupEvents).not.toHaveBeenCalled()
})

test('proposal decisions refresh once and keep validation and error handling', async () => {
  api.prepareCollectionPlan.mockResolvedValue({ proposalId: 'proposal', feasibilityStatus: 'FEASIBLE' })
  api.fetchPickupEvents.mockResolvedValue([])
  api.approveProposal.mockResolvedValue({ proposalId: 'proposal', feasibilityStatus: 'APPROVED' })
  const setReview = vi.fn(), tell = vi.fn()
  render(<ProposalReviewSection Badge={Badge} setReview={setReview} tell={tell} />)
  await screen.findByText('FEASIBLE', { selector: 'span' })
  await waitFor(() => expect(screen.queryByText('Loading Agentic AI proposal...')).not.toBeInTheDocument())
  fireEvent.click(screen.getByRole('button', { name: 'Reject', exact: true }))
  expect(screen.getByRole('alert')).toHaveTextContent('Add a reason')
  expect(api.approveProposal).not.toHaveBeenCalled()
  fireEvent.click(screen.getByRole('button', { name: 'Approve', exact: true }))
  await waitFor(() => expect(api.prepareCollectionPlan).toHaveBeenCalledTimes(2))
  await waitFor(() => expect(screen.getByRole('button', { name: 'Approve', exact: true })).toBeEnabled())
  expect(api.approveProposal).toHaveBeenCalledTimes(1)
  expect(api.fetchPickupEvents).toHaveBeenCalledTimes(2)
  expect(setReview).toHaveBeenCalledWith({ action: 'Approve', notes: '' })
})
