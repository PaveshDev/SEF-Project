import { apiClient } from './client';

export const getNeeds = () => apiClient.get('/recipient-needs').then(res => res.data);
export const getNeed = (id) => apiClient.get(`/recipient-needs/${id}`).then(res => res.data);
export const createNeed = (data) => apiClient.post('/recipient-needs', data).then(res => res.data);
export const updateNeed = (id, data) => apiClient.put(`/recipient-needs/${id}`, data).then(res => res.data);
export const deleteNeed = (id) => apiClient.delete(`/recipient-needs/${id}`).then(res => res.data);
