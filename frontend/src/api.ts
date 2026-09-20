import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7198/api', // Adjust based on actual backend URL
});

// Mock Auth interceptor for now (as requested by rules: use real ID, but MVP we might just simulate a token or hardcode dev header)
api.interceptors.request.use(config => {
  // In a real app, inject JWT here
  return config;
});

export default api;
