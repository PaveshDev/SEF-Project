import { useState, useEffect, useCallback, useRef } from 'react';
import { itemsApi } from '../services/itemsApi';

export function useItemDetails(id) {
  const [state, setState] = useState({ id, item: null, isLoading: Boolean(id), error: null, concurrencyError: null });
  const activeRequest = useRef(null);

  const loadItem = useCallback((signal) => itemsApi.getItem(id, signal)
    .then(item => {
      if (!signal.aborted) setState({ id, item, isLoading: false, error: null, concurrencyError: null });
    }).catch(err => {
      if (!signal.aborted) setState({
        id, item: null, isLoading: false, concurrencyError: null,
        error: {
          message: err.response?.data?.detail || err.message || 'Failed to fetch item details',
          status: err.response?.status
        }
      });
    }), [id]);

  useEffect(() => {
    const controller = new AbortController();
    activeRequest.current = controller;
    if (id) loadItem(controller.signal);
    return () => activeRequest.current?.abort();
  }, [id, loadItem]);

  const fetchItem = useCallback(() => {
    if (!id || activeRequest.current?.signal.aborted) return;
    activeRequest.current?.abort();
    const controller = new AbortController();
    activeRequest.current = controller;
    setState(prev => ({ id, item: prev.id === id ? prev.item : null, isLoading: true, error: null, concurrencyError: null }));
    return loadItem(controller.signal);
  }, [id, loadItem]);

  const updateItem = async (request) => {
    const signal = activeRequest.current?.signal;
    try {
      const updatedItem = await itemsApi.updateItem(id, request);
      if (signal && !signal.aborted) setState(prev => ({ ...prev, item: updatedItem, concurrencyError: null }));
      return updatedItem;
    } catch (err) {
      if (signal && !signal.aborted && err.response?.status === 409) {
        setState(prev => ({ ...prev, concurrencyError: err.response.data?.detail || 'The item was modified by another user.' }));
      }
      throw err;
    }
  };

  const submitItem = async () => {
    const signal = activeRequest.current?.signal;
    const updatedItem = await itemsApi.submitItemForAssessment(id);
    if (signal && !signal.aborted) setState(prev => ({ ...prev, item: updatedItem }));
    return updatedItem;
  };

  const addPhoto = async (request) => {
    const signal = activeRequest.current?.signal;
    const newPhoto = await itemsApi.addPhoto(id, request);
    if (signal && !signal.aborted) await fetchItem();
    return newPhoto;
  };

  const submitConditionAnswers = async (request) => {
    const signal = activeRequest.current?.signal;
    await itemsApi.submitConditionAnswers(id, request);
    if (signal && !signal.aborted) await fetchItem();
  };

  // A new route must never display the previous item's data or errors while loading.
  const current = state.id === id ? state : { item: null, isLoading: Boolean(id), error: null, concurrencyError: null };
  return { ...current, fetchItem, updateItem, submitItem, addPhoto, submitConditionAnswers };
}
