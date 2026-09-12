import { apiClient } from './client';

export const getRules = () => apiClient.get('/acceptance-rules').then(res => res.data);
export const getRule = (id) => apiClient.get(`/acceptance-rules/${id}`).then(res => res.data);
export const createRule = (data) => apiClient.post('/acceptance-rules', data).then(res => res.data);
export const updateRule = (id, data) => apiClient.put(`/acceptance-rules/${id}`, data).then(res => res.data);
export const deleteRule = (id) => apiClient.delete(`/acceptance-rules/${id}`).then(res => res.data);
