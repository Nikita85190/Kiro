import type { BalanceResponse, ReportResponse } from '../types';
import { apiClient } from './client';

export async function getBalance(dateFrom?: string, dateTo?: string): Promise<BalanceResponse> {
  const { data } = await apiClient.get<BalanceResponse>('/balance', {
    params: { dateFrom, dateTo },
  });
  return data;
}

export async function getReport(dateFrom: string, dateTo: string): Promise<ReportResponse> {
  const { data } = await apiClient.get<ReportResponse>('/reports', {
    params: { dateFrom, dateTo },
  });
  return data;
}
