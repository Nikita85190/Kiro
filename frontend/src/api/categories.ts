import type { Category } from '../types';
import { apiClient } from './client';

export async function getCategories(): Promise<Category[]> {
  const { data } = await apiClient.get<Category[]>('/categories');
  return data;
}

export async function createCategory(data: { name: string }): Promise<Category> {
  const { data: response } = await apiClient.post<Category>('/categories', data);
  return response;
}

export async function renameCategory(id: string, data: { name: string }): Promise<Category> {
  const { data: response } = await apiClient.put<Category>(`/categories/${id}`, data);
  return response;
}

export async function deleteCategory(id: string): Promise<void> {
  await apiClient.delete(`/categories/${id}`);
}
