import { useState, useEffect, useCallback } from 'react';
import { itemsApi } from '../services/itemsApi';

export function useItems() {
  const [items, setItems] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchItems = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await itemsApi.getUserItems();
      setItems(data || []);
    } catch (err) {
      setError(err.response?.data?.detail || err.message || 'Failed to fetch items');
      setItems([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchItems();
  }, [fetchItems]);

  const createItem = async (request) => {
    const newItem = await itemsApi.createItem(request);
    setItems((prev) => [newItem, ...prev]);
    return newItem;
  };

  return { items, isLoading, error, fetchItems, createItem };
}
