import axios from 'axios';

// The base URL routes specifically to the Partners module backend controller
const API_URL = 'http://localhost:5000/api/partners';

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});
