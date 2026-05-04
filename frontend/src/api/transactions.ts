import type { Transaction, PagedResult, TransactionFilter } from '../types';
import { apiClient } from './client';

export interface CreateTransactionData {
  type: string;
  amount: number;
  date: string;
  categoryId: string;
  description?: string;
}

export async function getTransactions(filter: TransactionFilter = {}): Promise<PagedResult<Transaction>> {
  const { data } = await apiClient.get<PagedResult<Transaction>>('/transactions', { params: filter });
  return data;
}

export async function createTransaction(data: CreateTransactionData): Promise<Transaction> {
  const { data: response } = await apiClient.post<Transaction>('/transactions', data);
  return response;
}

export async function updateTransaction(id: string, data: CreateTransactionData): Promise<Transaction> {
  const { data: response } = await apiClient.put<Transaction>(`/transactions/${id}`, data);
  return response;
}

export async function deleteTransaction(id: string): Promise<void> {
  await apiClient.delete(`/transactions/${id}`);
}
