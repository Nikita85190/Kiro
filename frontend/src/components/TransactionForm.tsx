import { useState } from 'react';
import type { Category, Transaction } from '../types';
import type { CreateTransactionData } from '../api/transactions';

interface TransactionFormProps {
  categories: Category[];
  initialValues?: Partial<Transaction>;
  onSubmit: (data: CreateTransactionData) => Promise<void>;
  onCancel?: () => void;
}

interface FormErrors {
  type?: string;
  amount?: string;
  date?: string;
  categoryId?: string;
}

export function TransactionForm({ categories, initialValues, onSubmit, onCancel }: TransactionFormProps) {
  const [type, setType] = useState<string>(initialValues?.type ?? 'expense');
  const [amount, setAmount] = useState<string>(initialValues?.amount != null ? String(initialValues.amount) : '');
  const [date, setDate] = useState<string>(initialValues?.date ?? '');
  const [categoryId, setCategoryId] = useState<string>(initialValues?.categoryId ?? '');
  const [description, setDescription] = useState<string>(initialValues?.description ?? '');
  const [errors, setErrors] = useState<FormErrors>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const validate = (): FormErrors => {
    const errs: FormErrors = {};
    if (!type) errs.type = 'Type is required.';
    const parsedAmount = parseFloat(amount);
    if (!amount || isNaN(parsedAmount) || parsedAmount <= 0) {
      errs.amount = 'Amount must be a positive number.';
    }
    if (!date) errs.date = 'Date is required.';
    if (!categoryId) errs.categoryId = 'Category is required.';
    return errs;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const errs = validate();
    if (Object.keys(errs).length > 0) {
      setErrors(errs);
      return;
    }
    setErrors({});
    setSubmitError(null);
    setLoading(true);
    try {
      await onSubmit({
        type,
        amount: parseFloat(amount),
        date,
        categoryId,
        description: description.trim() || undefined,
      });
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'message' in err
          ? String((err as { message: string }).message)
          : 'Failed to save transaction.';
      setSubmitError(message);
    } finally {
      setLoading(false);
    }
  };

  const inputStyle: React.CSSProperties = {
    padding: '8px 10px',
    border: '1px solid #ccc',
    borderRadius: '4px',
    fontSize: '14px',
    width: '100%',
    boxSizing: 'border-box',
  };

  const errorStyle: React.CSSProperties = {
    color: '#dc3545',
    fontSize: '12px',
    marginTop: '4px',
  };

  const labelStyle: React.CSSProperties = {
    fontSize: '13px',
    fontWeight: 500,
    color: '#333',
    marginBottom: '4px',
    display: 'block',
  };

  const fieldStyle: React.CSSProperties = {
    display: 'flex',
    flexDirection: 'column',
    marginBottom: '14px',
  };

  return (
    <form onSubmit={handleSubmit} noValidate style={{ maxWidth: '420px' }}>
      {submitError && (
        <div style={{ background: '#fff3f3', border: '1px solid #f5c6cb', borderRadius: '4px', padding: '10px 12px', marginBottom: '14px', color: '#721c24', fontSize: '14px' }}>
          {submitError}
        </div>
      )}

      <div style={fieldStyle}>
        <label htmlFor="txn-type" style={labelStyle}>Type</label>
        <select
          id="txn-type"
          value={type}
          onChange={(e) => setType(e.target.value)}
          style={{ ...inputStyle, borderColor: errors.type ? '#dc3545' : '#ccc' }}
          disabled={loading}
        >
          <option value="income">Income</option>
          <option value="expense">Expense</option>
        </select>
        {errors.type && <span style={errorStyle}>{errors.type}</span>}
      </div>

      <div style={fieldStyle}>
        <label htmlFor="txn-amount" style={labelStyle}>Amount</label>
        <input
          id="txn-amount"
          type="number"
          min="0.01"
          step="0.01"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          placeholder="0.00"
          style={{ ...inputStyle, borderColor: errors.amount ? '#dc3545' : '#ccc' }}
          disabled={loading}
        />
        {errors.amount && <span style={errorStyle}>{errors.amount}</span>}
      </div>

      <div style={fieldStyle}>
        <label htmlFor="txn-date" style={labelStyle}>Date</label>
        <input
          id="txn-date"
          type="date"
          value={date}
          onChange={(e) => setDate(e.target.value)}
          style={{ ...inputStyle, borderColor: errors.date ? '#dc3545' : '#ccc' }}
          disabled={loading}
        />
        {errors.date && <span style={errorStyle}>{errors.date}</span>}
      </div>

      <div style={fieldStyle}>
        <label htmlFor="txn-category" style={labelStyle}>Category</label>
        <select
          id="txn-category"
          value={categoryId}
          onChange={(e) => setCategoryId(e.target.value)}
          style={{ ...inputStyle, borderColor: errors.categoryId ? '#dc3545' : '#ccc' }}
          disabled={loading}
        >
          <option value="">Select a category</option>
          {categories.map((cat) => (
            <option key={cat.id} value={cat.id}>{cat.name}</option>
          ))}
        </select>
        {errors.categoryId && <span style={errorStyle}>{errors.categoryId}</span>}
      </div>

      <div style={fieldStyle}>
        <label htmlFor="txn-description" style={labelStyle}>Description <span style={{ fontWeight: 400, color: '#888' }}>(optional)</span></label>
        <input
          id="txn-description"
          type="text"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Add a note..."
          style={inputStyle}
          disabled={loading}
        />
      </div>

      <div style={{ display: 'flex', gap: '10px', marginTop: '4px' }}>
        <button
          type="submit"
          disabled={loading}
          style={{
            padding: '9px 20px',
            background: loading ? '#6c757d' : '#0d6efd',
            color: '#fff',
            border: 'none',
            borderRadius: '4px',
            fontSize: '14px',
            cursor: loading ? 'not-allowed' : 'pointer',
            fontWeight: 500,
          }}
        >
          {loading ? 'Saving…' : 'Save'}
        </button>
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            disabled={loading}
            style={{
              padding: '9px 20px',
              background: '#fff',
              color: '#333',
              border: '1px solid #ccc',
              borderRadius: '4px',
              fontSize: '14px',
              cursor: loading ? 'not-allowed' : 'pointer',
            }}
          >
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
