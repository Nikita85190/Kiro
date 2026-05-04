export type TransactionType = 'income' | 'expense';

export interface Transaction {
  id: string;
  type: TransactionType;
  amount: number;
  date: string; // ISO 8601 date string (YYYY-MM-DD)
  categoryId: string;
  categoryName: string;
  description?: string;
  createdAt: string;
}

export interface Category {
  id: string;
  name: string;
}

export interface BalanceResponse {
  totalIncome: number;
  totalExpenses: number;
  balance: number;
}

export interface ReportResponse {
  totalIncome: number;
  totalExpenses: number;
  balance: number;
  expensesByCategory: CategoryBreakdown[];
  incomeByCategory: CategoryBreakdown[];
}

export interface CategoryBreakdown {
  categoryId: string;
  categoryName: string;
  total: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ApiError {
  error: string;
  message: string;
  details?: unknown;
}

export interface TransactionFilter {
  dateFrom?: string;
  dateTo?: string;
  categoryId?: string;
  type?: TransactionType;
  page?: number;
  pageSize?: number;
}
