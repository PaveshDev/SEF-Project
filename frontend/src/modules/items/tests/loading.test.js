import { act, renderHook, waitFor } from '@testing-library/react'
import { beforeEach, expect, test, vi } from 'vitest'
import { useItems } from '../hooks/useItems.js'
import { useItemDetails } from '../hooks/useItemDetails.js'
import { itemsApi } from '../services/itemsApi.js'

vi.mock('../services/itemsApi.js', () => ({
  itemsApi: { getUserItems: vi.fn(), getItem: vi.fn(), submitItemForAssessment: vi.fn() },
}))

const deferred = () => {
  let resolve, reject
  const promise = new Promise((yes, no) => { resolve = yes; reject = no })
  return { promise, resolve, reject }
}

beforeEach(() => vi.resetAllMocks())

test('items refresh cancels the previous request and ignores its late response', async () => {
  const first = deferred(), second = deferred()
  itemsApi.getUserItems.mockReturnValueOnce(first.promise).mockReturnValueOnce(second.promise)
  const { result, unmount } = renderHook(() => useItems())
  expect(result.current.isLoading).toBe(true)
  expect(itemsApi.getUserItems).toHaveBeenCalledTimes(1)
  const firstSignal = itemsApi.getUserItems.mock.calls[0][0]
  let refresh
  act(() => { refresh = result.current.fetchItems() })
  expect(firstSignal.aborted).toBe(true)
  expect(itemsApi.getUserItems).toHaveBeenCalledTimes(2)
  await act(async () => { second.resolve([{ id: 'latest' }]); await refresh })
  await act(async () => { first.resolve([{ id: 'stale' }]); await first.promise })
  expect(result.current.items).toEqual([{ id: 'latest' }])
  expect(result.current.isLoading).toBe(false)
  unmount()
  expect(itemsApi.getUserItems.mock.calls[1][0].aborted).toBe(true)
})

test('items load errors clear on retry and empty success is not an error', async () => {
  itemsApi.getUserItems.mockRejectedValueOnce(new Error('Offline')).mockResolvedValueOnce([])
  const { result } = renderHook(() => useItems())
  await waitFor(() => expect(result.current.error).toBe('Offline'))
  let refresh
  act(() => { refresh = result.current.fetchItems() })
  expect(result.current.error).toBeNull()
  expect(result.current.isLoading).toBe(true)
  await act(async () => { await refresh })
  expect(result.current.items).toEqual([])
  expect(result.current.isLoading).toBe(false)
})

test('item route changes cancel old loads and never expose a stale item', async () => {
  const first = deferred(), second = deferred()
  itemsApi.getItem.mockReturnValueOnce(first.promise).mockReturnValueOnce(second.promise)
  const { result, rerender, unmount } = renderHook(({ id }) => useItemDetails(id), { initialProps: { id: 'first' } })
  const firstSignal = itemsApi.getItem.mock.calls[0][1]
  rerender({ id: 'second' })
  expect(firstSignal.aborted).toBe(true)
  expect(result.current.item).toBeNull()
  expect(result.current.isLoading).toBe(true)
  await act(async () => { second.resolve({ id: 'second' }); await second.promise })
  await act(async () => { first.resolve({ id: 'first' }); await first.promise })
  expect(result.current.item).toEqual({ id: 'second' })
  expect(itemsApi.getItem).toHaveBeenCalledTimes(2)
  unmount()
  expect(itemsApi.getItem.mock.calls[1][1].aborted).toBe(true)
})

test('item errors retain their status and submission errors still reject', async () => {
  const error = { response: { status: 403, data: { detail: 'Forbidden' } } }
  itemsApi.getItem.mockRejectedValueOnce(error).mockResolvedValueOnce({ id: 'item' })
  itemsApi.submitItemForAssessment.mockRejectedValue(error)
  const { result } = renderHook(() => useItemDetails('item'))
  await waitFor(() => expect(result.current.error).toEqual({ message: 'Forbidden', status: 403 }))
  await act(async () => { await result.current.fetchItem() })
  expect(result.current.error).toBeNull()
  await expect(result.current.submitItem()).rejects.toBe(error)
})

test('an unmounted item load cannot publish a late error', async () => {
  const pending = deferred()
  itemsApi.getItem.mockReturnValue(pending.promise)
  const { result, unmount } = renderHook(() => useItemDetails('item'))
  const state = result.current
  unmount()
  expect(itemsApi.getItem.mock.calls[0][1].aborted).toBe(true)
  await act(async () => { pending.reject(new Error('Late error')); await pending.promise.catch(() => {}) })
  expect(result.current).toBe(state)
})
