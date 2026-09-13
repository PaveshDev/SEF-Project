import { apiClient } from '../../../shared/services/apiClient.js';

export const itemsApi = {
  createItem: async (request) => {
    const { data } = await apiClient.post('/api/items', request);
    return data;
  },
  
  getItem: async (id) => {
    const { data } = await apiClient.get(`/api/items/${id}`);
    return data;
  },

  getUserItems: async () => {
    const { data } = await apiClient.get('/api/items');
    return data;
  },
  
  updateItem: async (id, request) => {
    const { data } = await apiClient.put(`/api/items/${id}`, request);
    return data;
  },
  
  deleteItem: async (id) => {
    await apiClient.delete(`/api/items/${id}`);
  },
  
  addPhoto: async (id, request) => {
    const { data } = await apiClient.post(`/api/items/${id}/photos`, request);
    return data;
  },
  
  submitConditionAnswers: async (id, request) => {
    const { data } = await apiClient.post(`/api/items/${id}/condition-answers`, request);
    return data;
  },
  
  submitItemForAssessment: async (id) => {
    const { data } = await apiClient.post(`/api/items/${id}/submit`);
    return data;
  },
  
  answerClarification: async (id, clarificationId, request) => {
    const { data } = await apiClient.post(`/api/items/${id}/clarifications/${clarificationId}/answer`, request);
    return data;
  },
  
  confirmAssessment: async (id, assessmentId, request) => {
    const { data } = await apiClient.post(`/api/items/${id}/assessments/${assessmentId}/confirm`, request);
    return data;
  },
  
  requestReassessment: async (id, assessmentId, request) => {
    const { data } = await apiClient.post(`/api/items/${id}/assessments/${assessmentId}/reassessment`, request);
    return data;
  }
};
