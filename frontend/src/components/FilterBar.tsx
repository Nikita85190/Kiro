import type { Category, TransactionFilter, TransactionType } from '../types';

interface FilterBarProps {
  categories: Category[];
  filter: TransactionFilter;
  onChange: (filter: TransactionFilter) => void;
}

export function FilterBar({ categories, filter, onChange }: FilterBarProps) {
  const handleChange = (field: keyof TransactionFilter, value: string) => {
    const updated: TransactionFilter = { ...filter, page: 1 };
    if (value === '') {
      delete updated[field];
    } else if (field === 'type') {
      updated.type = value as TransactionType;
    } else {
      (updated as Record<string, string>)[field] = value;
    }
    onChange(updated);
  };

  return (
    <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'center', padding: '12px', background: '#f8f9fa', borderRadius: '6px', marginBottom: '16px' }}>
      <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
        <label htmlFor="filter-date-from" style={{ fontSize: '12px', color: '#666', fontWeight: 500 }}>From</label>
        <input
          id="filter-date-from"
          type="date"
          value={filter.dateFrom ?? ''}
          onChange={(e) => handleChange('dateFrom', e.target.value)}
          style={{ padding: '6px 8px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
        />
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
        <label htmlFor="filter-date-to" style={{ fontSize: '12px', color: '#666', fontWeight: 500 }}>To</label>
        <input
          id="filter-date-to"
          type="date"
          value={filter.dateTo ?? ''}
          onChange={(e) => handleChange('dateTo', e.target.value)}
          style={{ padding: '6px 8px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px' }}
        />
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
        <label htmlFor="filter-category" style={{ fontSize: '12px', color: '#666', fontWeight: 500 }}>Category</label>
        <select
          id="filter-category"
          value={filter.categoryId ?? ''}
          onChange={(e) => handleChange('categoryId', e.target.value)}
          style={{ padding: '6px 8px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px', minWidth: '140px' }}
        >
          <option value="">All categories</option>
          {categories.map((cat) => (
            <option key={cat.id} value={cat.id}>{cat.name}</option>
          ))}
        </select>
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
        <label htmlFor="filter-type" style={{ fontSize: '12px', color: '#666', fontWeight: 500 }}>Type</label>
        <select
          id="filter-type"
          value={filter.type ?? ''}
          onChange={(e) => handleChange('type', e.target.value)}
          style={{ padding: '6px 8px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '14px', minWidth: '120px' }}
        >
          <option value="">All</option>
          <option value="income">Income</option>
          <option value="expense">Expense</option>
        </select>
      </div>
    </div>
  );
}
