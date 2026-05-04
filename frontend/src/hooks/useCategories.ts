import { useState, useEffect, useCallback } from 'react';
import type { Category } from '../types';
import {
  getCategories,
  createCategory,
  renameCategory,
  deleteCategory,
} from '../api/categories';

interface UseCategoriesReturn {
  categories: Category[];
  loading: boolean;
  error: string | null;
  create: (name: string) => Promise<void>;
  rename: (id: string, name: string) => Promise<void>;
  remove: (id: string) => Promise<void>;
}

export function useCategories(): UseCategoriesReturn {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchCategories = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getCategories();
      setCategories(result);
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'message' in err
          ? String((err as { message: string }).message)
          : 'Failed to fetch categories';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  const create = useCallback(
    async (name: string) => {
      await createCategory({ name });
      await fetchCategories();
    },
    [fetchCategories]
  );

  const rename = useCallback(
    async (id: string, name: string) => {
      await renameCategory(id, { name });
      await fetchCategories();
    },
    [fetchCategories]
  );

  const remove = useCallback(
    async (id: string) => {
      await deleteCategory(id);
      await fetchCategories();
    },
    [fetchCategories]
  );

  return {
    categories,
    loading,
    error,
    create,
    rename,
    remove,
  };
}
