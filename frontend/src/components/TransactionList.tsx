import { useState } from 'react';
import type { Transaction, PagedResult } from '../types';

interface TransactionListProps {
  result: PagedResult<Transaction> | null;
  loading: boolean;
  onEdit: (transaction: Transaction) => void;
  onDelete: (id: string) => Promise<void>;
  onPageChange: (page: number) => void;
}

export function TransactionList({ result, loading, onEdit, onDelete, onPageChange }: TransactionListProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this transaction?')) return;
    setDeletingId(id);
    try {
      await onDelete(id);
    } finally {
      setDeletingId(null);
    }
  };

  const formatAmount = (amount: number, type: string) => {
    const formatted = amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    return type === 'income' ? `+${formatted}` : `-${formatted}`;
  };

  const formatDate = (dateStr: string) => {
    const [year, month, day] = dateStr.split('-');
    return `${day}.${month}.${year}`;
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', padding: '40px', color: '#666' }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '24px', marginBottom: '8px' }}>⏳</div>
          <div>Loading transactions…</div>
        </div>
      </div>
    );
  }

  if (!result || result.items.length === 0) {
    return (
      <div style={{ textAlign: 'center', padding: '40px', color: '#888', fontSize: '14px' }}>
        No transactions found.
      </div>
    );
  }

  const totalPages = Math.ceil(result.totalCount / result.pageSize);

  return (
    <div>
      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '14px' }}>
          <thead>
            <tr style={{ background: '#f8f9fa', borderBottom: '2px solid #dee2e6' }}>
              <th style={thStyle}>Date</th>
              <th style={thStyle}>Type</th>
              <th style={{ ...thStyle, textAlign: 'right' }}>Amount</th>
              <th style={thStyle}>Category</th>
              <th style={thStyle}>Description</th>
              <th style={{ ...thStyle, textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {result.items.map((txn) => (
              <tr key={txn.id} style={{ borderBottom: '1px solid #dee2e6' }}>
                <td style={tdStyle}>{formatDate(txn.date)}</td>
                <td style={tdStyle}>
                  <span style={{
                    display: 'inline-block',
                    padding: '2px 8px',
                    borderRadius: '12px',
                    fontSize: '12px',
                    fontWeight: 500,
                    background: txn.type === 'income' ? '#d1fae5' : '#fee2e2',
                    color: txn.type === 'income' ? '#065f46' : '#991b1b',
                  }}>
                    {txn.type === 'income' ? 'Income' : 'Expense'}
                  </span>
                </td>
                <td style={{ ...tdStyle, textAlign: 'right', fontWeight: 500, color: txn.type === 'income' ? '#16a34a' : '#dc2626' }}>
                  {formatAmount(txn.amount, txn.type)}
                </td>
                <td style={tdStyle}>{txn.categoryName}</td>
                <td style={{ ...tdStyle, color: '#666' }}>{txn.description ?? '—'}</td>
                <td style={{ ...tdStyle, textAlign: 'center' }}>
                  <div style={{ display: 'flex', gap: '6px', justifyContent: 'center' }}>
                    <button
                      onClick={() => onEdit(txn)}
                      style={editBtnStyle}
                      title="Edit"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => handleDelete(txn.id)}
                      disabled={deletingId === txn.id}
                      style={{ ...deleteBtnStyle, opacity: deletingId === txn.id ? 0.6 : 1, cursor: deletingId === txn.id ? 'not-allowed' : 'pointer' }}
                      title="Delete"
                    >
                      {deletingId === txn.id ? '…' : 'Delete'}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '16px', fontSize: '14px', color: '#555' }}>
        <span>Total: {result.totalCount} transaction{result.totalCount !== 1 ? 's' : ''}</span>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <button
            onClick={() => onPageChange(result.page - 1)}
            disabled={result.page <= 1}
            style={{ ...pageBtnStyle, opacity: result.page <= 1 ? 0.4 : 1, cursor: result.page <= 1 ? 'not-allowed' : 'pointer' }}
          >
            ← Previous
          </button>
          <span>Page {result.page} of {totalPages}</span>
          <button
            onClick={() => onPageChange(result.page + 1)}
            disabled={result.page >= totalPages}
            style={{ ...pageBtnStyle, opacity: result.page >= totalPages ? 0.4 : 1, cursor: result.page >= totalPages ? 'not-allowed' : 'pointer' }}
          >
            Next →
          </button>
        </div>
      </div>
    </div>
  );
}

const thStyle: React.CSSProperties = {
  padding: '10px 12px',
  textAlign: 'left',
  fontWeight: 600,
  color: '#333',
  whiteSpace: 'nowrap',
};

const tdStyle: React.CSSProperties = {
  padding: '10px 12px',
  verticalAlign: 'middle',
};

const editBtnStyle: React.CSSProperties = {
  padding: '4px 10px',
  background: '#fff',
  border: '1px solid #0d6efd',
  color: '#0d6efd',
  borderRadius: '4px',
  fontSize: '12px',
  cursor: 'pointer',
  fontWeight: 500,
};

const deleteBtnStyle: React.CSSProperties = {
  padding: '4px 10px',
  background: '#fff',
  border: '1px solid #dc3545',
  color: '#dc3545',
  borderRadius: '4px',
  fontSize: '12px',
  fontWeight: 500,
};

const pageBtnStyle: React.CSSProperties = {
  padding: '6px 14px',
  background: '#fff',
  border: '1px solid #ccc',
  borderRadius: '4px',
  fontSize: '13px',
  fontWeight: 500,
};
