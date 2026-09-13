import { useState, useEffect, useCallback } from 'react';
import { itemsApi } from '../services/itemsApi';

export function useItemDetails(id) {
  const [item, setItem] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [concurrencyError, setConcurrencyError] = useState(null);

  const fetchItem = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    setConcurrencyError(null);
    try {
      const data = await itemsApi.getItem(id);
      setItem(data);
    } catch (err) {
      setError({
        message: err.response?.data?.detail || err.message || 'Failed to fetch item details',
        status: err.response?.status
      });
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    if (id) {
      fetchItem();
    }
  }, [id, fetchItem]);

  const updateItem = async (request) => {
    try {
      const updatedItem = await itemsApi.updateItem(id, request);
      setItem(updatedItem);
      setConcurrencyError(null);
      return updatedItem;
    } catch (err) {
      if (err.response && err.response.status === 409) {
        setConcurrencyError(err.response.data?.detail || 'The item was modified by another user.');
      }
      throw err;
    }
  };

  const submitItem = async () => {
    try {
      const updatedItem = await itemsApi.submitItemForAssessment(id);
      setItem(updatedItem);
      return updatedItem;
    } catch (err) {
      throw err;
    }
  };

  const addPhoto = async (request) => {
    const newPhoto = await itemsApi.addPhoto(id, request);
    // Refresh item to get updated photos list
    await fetchItem();
    return newPhoto;
  };

  const submitConditionAnswers = async (request) => {
    await itemsApi.submitConditionAnswers(id, request);
    await fetchItem();
  };

  return { 
    item, 
    isLoading, 
    error, 
    concurrencyError, 
    fetchItem, 
    updateItem, 
    submitItem, 
    addPhoto, 
    submitConditionAnswers 
  };
}
