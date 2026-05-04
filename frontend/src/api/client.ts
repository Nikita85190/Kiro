import axios, { AxiosError } from 'axios';
import type { ApiError } from '../types';

export const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const apiError: ApiError = {
      error: 'UNKNOWN_ERROR',
      message: 'An unexpected error occurred.',
    };

    if (error.response?.data) {
      const data = error.response.data as Partial<ApiError>;
      apiError.error = data.error ?? 'UNKNOWN_ERROR';
      apiError.message = data.message ?? apiError.message;
      apiError.details = data.details;
    }

    return Promise.reject(apiError);
  }
);
