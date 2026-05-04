import { useState, useEffect, useCallback } from 'react';
import type { BalanceResponse, ReportResponse } from '../types';
import { getBalance, getReport } from '../api/analytics';

interface Period {
  dateFrom: string;
  dateTo: string;
}

interface UseAnalyticsReturn {
  balance: BalanceResponse | null;
  report: ReportResponse | null;
  period: Period | null;
  loading: boolean;
  error: string | null;
  fetchBalance: (dateFrom?: string, dateTo?: string) => Promise<void>;
  fetchReport: (dateFrom: string, dateTo: string) => Promise<void>;
  setPeriod: (period: Period | null) => void;
}

export function useAnalytics(): UseAnalyticsReturn {
  const [balance, setBalance] = useState<BalanceResponse | null>(null);
  const [report, setReport] = useState<ReportResponse | null>(null);
  const [period, setPeriodState] = useState<Period | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchBalance = useCallback(async (dateFrom?: string, dateTo?: string) => {
    setLoading(true);
    setError(null);
    try {
      const result = await getBalance(dateFrom, dateTo);
      setBalance(result);
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'message' in err
          ? String((err as { message: string }).message)
          : 'Failed to fetch balance';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchReport = useCallback(async (dateFrom: string, dateTo: string) => {
    setLoading(true);
    setError(null);
    try {
      const result = await getReport(dateFrom, dateTo);
      setReport(result);
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'message' in err
          ? String((err as { message: string }).message)
          : 'Failed to fetch report';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, []);

  const setPeriod = useCallback((newPeriod: Period | null) => {
    setPeriodState(newPeriod);
  }, []);

  // Auto-fetch balance on mount (no period filter)
  useEffect(() => {
    fetchBalance();
  }, [fetchBalance]);

  return {
    balance,
    report,
    period,
    loading,
    error,
    fetchBalance,
    fetchReport,
    setPeriod,
  };
}
