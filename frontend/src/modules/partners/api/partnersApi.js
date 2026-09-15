import { apiClient } from './client';

export const getPartners = () => apiClient.get('').then(res => res.data);
export const getPartner = (id) => apiClient.get(`/${id}`).then(res => res.data);
export const createPartner = (data) => apiClient.post('', data).then(res => res.data);
export const updatePartner = (id, data) => apiClient.put(`/${id}`, data).then(res => res.data);
export const deletePartner = (id) => apiClient.delete(`/${id}`).then(res => res.data);
