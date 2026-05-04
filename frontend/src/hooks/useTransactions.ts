import { useState, useEffect, useCallback } from 'react';
import type { Transaction, PagedResult, TransactionFilter } from '../types';
import type { CreateTransactionData } from '../api/transactions';
import {
  getTransactions,
  createTransaction,
  updateTransaction,
  deleteTransaction,
} from '../api/transactions';

interface UseTransactionsReturn {
  transactions: PagedResult<Transaction> | null;
  filter: TransactionFilter;
  loading: boolean;
  error: string | null;
  create: (data: CreateTransactionData) => Promise<void>;
  update: (id: string, data: CreateTransactionData) => Promise<void>;
  remove: (id: string) => Promise<void>;
  setFilter: (filter: TransactionFilter) => void;
}

export function useTransactions(initialFilter: TransactionFilter = {}): UseTransactionsReturn {
  const [transactions, setTransactions] = useState<PagedResult<Transaction> | null>(null);
  const [filter, setFilterState] = useState<TransactionFilter>(initialFilter);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchTransactions = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getTransactions(filter);
      setTransactions(result);
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'message' in err
          ? String((err as { message: string }).message)
          : 'Failed to fetch transactions';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, [filter]);

  useEffect(() => {
    fetchTransactions();
  }, [fetchTransactions]);

  const create = useCallback(
    async (data: CreateTransactionData) => {
      await createTransaction(data);
      await fetchTransactions();
    },
    [fetchTransactions]
  );

  const update = useCallback(
    async (id: string, data: CreateTransactionData) => {
      await updateTransaction(id, data);
      await fetchTransactions();
    },
    [fetchTransactions]
  );

  const remove = useCallback(
    async (id: string) => {
      await deleteTransaction(id);
      await fetchTransactions();
    },
    [fetchTransactions]
  );

  const setFilter = useCallback((newFilter: TransactionFilter) => {
    setFilterState(newFilter);
  }, []);

  return {
    transactions,
    filter,
    loading,
    error,
    create,
    update,
    remove,
    setFilter,
  };
}
