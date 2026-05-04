import { useState } from 'react';
import { useTransactions } from '../hooks/useTransactions';
import { useCategories } from '../hooks/useCategories';
import { TransactionList } from '../components/TransactionList';
import { TransactionForm } from '../components/TransactionForm';
import { FilterBar } from '../components/FilterBar';
import type { Transaction, TransactionFilter } from '../types';

export function TransactionsPage() {
  const { transactions, filter, loading, create, update, remove, setFilter } =
    useTransactions({ page: 1, pageSize: 20 });
  const { categories } = useCategories();

  const [modalOpen, setModalOpen] = useState(false);
  const [editingTransaction, setEditingTransaction] = useState<Transaction | null>(null);

  const openAddModal = () => {
    setEditingTransaction(null);
    setModalOpen(true);
  };

  const openEditModal = (txn: Transaction) => {
    setEditingTransaction(txn);
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
    setEditingTransaction(null);
  };

  const handleSubmit = async (data: Parameters<typeof create>[0]) => {
    if (editingTransaction) {
      await update(editingTransaction.id, data);
    } else {
      await create(data);
    }
    closeModal();
  };

  const handleFilterChange = (newFilter: TransactionFilter) => {
    setFilter(newFilter);
  };

  const handlePageChange = (page: number) => {
    setFilter({ ...filter, page });
  };

  return (
    <div style={{ padding: '24px', maxWidth: '1100px', margin: '0 auto' }}>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: '20px',
        }}
      >
        <h1 style={{ fontSize: '22px', fontWeight: 700, margin: 0, color: '#111' }}>
          Transactions
        </h1>
        <button onClick={openAddModal} style={addBtnStyle}>
          + Add Transaction
        </button>
      </div>

      <FilterBar categories={categories} filter={filter} onChange={handleFilterChange} />

      <TransactionList
        result={transactions}
        loading={loading}
        onEdit={openEditModal}
        onDelete={remove}
        onPageChange={handlePageChange}
      />

      {modalOpen && (
        <div style={overlayStyle} onClick={closeModal}>
          <div
            style={modalStyle}
            onClick={(e) => e.stopPropagation()}
            role="dialog"
            aria-modal="true"
            aria-label={editingTransaction ? 'Edit Transaction' : 'Add Transaction'}
          >
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '16px',
              }}
            >
              <h2 style={{ margin: 0, fontSize: '16px', fontWeight: 600 }}>
                {editingTransaction ? 'Edit Transaction' : 'Add Transaction'}
              </h2>
              <button
                onClick={closeModal}
                style={{
                  background: 'none',
                  border: 'none',
                  fontSize: '20px',
                  cursor: 'pointer',
                  color: '#666',
                  lineHeight: 1,
                  padding: '0 4px',
                }}
                aria-label="Close"
              >
                ×
              </button>
            </div>
            <TransactionForm
              categories={categories}
              initialValues={editingTransaction ?? undefined}
              onSubmit={handleSubmit}
              onCancel={closeModal}
            />
          </div>
        </div>
      )}
    </div>
  );
}

const addBtnStyle: React.CSSProperties = {
  padding: '9px 18px',
  background: '#0d6efd',
  color: '#fff',
  border: 'none',
  borderRadius: '6px',
  fontSize: '14px',
  fontWeight: 500,
  cursor: 'pointer',
};

const overlayStyle: React.CSSProperties = {
  position: 'fixed',
  inset: 0,
  background: 'rgba(0, 0, 0, 0.45)',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  zIndex: 1000,
};

const modalStyle: React.CSSProperties = {
  background: '#fff',
  borderRadius: '8px',
  padding: '24px',
  width: '100%',
  maxWidth: '480px',
  boxShadow: '0 8px 32px rgba(0,0,0,0.18)',
  maxHeight: '90vh',
  overflowY: 'auto',
};
