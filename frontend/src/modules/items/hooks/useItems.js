import { useState, useEffect, useCallback, useRef } from 'react';
import { itemsApi } from '../services/itemsApi';

export function useItems() {
  const [items, setItems] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const activeRequest = useRef(null);

  const loadItems = useCallback((signal) => itemsApi.getUserItems(signal)
    .then(data => {
      if (signal.aborted) return;
      setItems(data || []);
    }).catch(err => {
      if (signal.aborted) return;
      setError(err.response?.data?.detail || err.message || 'Failed to fetch items');
      setItems([]);
    }).finally(() => {
      if (!signal.aborted) setIsLoading(false);
    }), []);

  useEffect(() => {
    const controller = new AbortController();
    activeRequest.current = controller;
    loadItems(controller.signal);
    return () => activeRequest.current?.abort();
  }, [loadItems]);

  const fetchItems = useCallback(() => {
    if (activeRequest.current?.signal.aborted) return;
    activeRequest.current?.abort();
    const controller = new AbortController();
    activeRequest.current = controller;
    setIsLoading(true);
    setError(null);
    return loadItems(controller.signal);
  }, [loadItems]);

  const createItem = async (request) => {
    const signal = activeRequest.current?.signal;
    const newItem = await itemsApi.createItem(request);
    if (signal && !signal.aborted) setItems((prev) => [newItem, ...prev]);
    return newItem;
  };

  return { items, isLoading, error, fetchItems, createItem };
}
