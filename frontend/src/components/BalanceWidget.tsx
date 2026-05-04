import type { BalanceResponse } from '../types';

interface BalanceWidgetProps {
  balance: BalanceResponse | null;
  loading: boolean;
}

const formatCurrency = (value: number) =>
  value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export function BalanceWidget({ balance, loading }: BalanceWidgetProps) {
  if (loading) {
    return (
      <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
        {['Income', 'Expenses', 'Balance'].map((label) => (
          <div key={label} style={{ ...cardStyle, background: '#f8f9fa' }}>
            <div style={{ fontSize: '12px', color: '#888', marginBottom: '6px' }}>{label}</div>
            <div style={{ fontSize: '22px', color: '#ccc' }}>—</div>
          </div>
        ))}
      </div>
    );
  }

  if (!balance) {
    return null;
  }

  return (
    <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
      <div style={{ ...cardStyle, borderTop: '3px solid #16a34a' }}>
        <div style={{ fontSize: '12px', color: '#666', fontWeight: 500, marginBottom: '6px', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Income</div>
        <div style={{ fontSize: '22px', fontWeight: 700, color: '#16a34a' }}>
          +{formatCurrency(balance.totalIncome)}
        </div>
      </div>

      <div style={{ ...cardStyle, borderTop: '3px solid #dc2626' }}>
        <div style={{ fontSize: '12px', color: '#666', fontWeight: 500, marginBottom: '6px', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Expenses</div>
        <div style={{ fontSize: '22px', fontWeight: 700, color: '#dc2626' }}>
          -{formatCurrency(balance.totalExpenses)}
        </div>
      </div>

      <div style={{ ...cardStyle, borderTop: `3px solid ${balance.balance >= 0 ? '#0d6efd' : '#f59e0b'}` }}>
        <div style={{ fontSize: '12px', color: '#666', fontWeight: 500, marginBottom: '6px', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Balance</div>
        <div style={{ fontSize: '22px', fontWeight: 700, color: balance.balance >= 0 ? '#0d6efd' : '#f59e0b' }}>
          {balance.balance >= 0 ? '+' : ''}{formatCurrency(balance.balance)}
        </div>
      </div>
    </div>
  );
}

const cardStyle: React.CSSProperties = {
  background: '#fff',
  border: '1px solid #e5e7eb',
  borderRadius: '8px',
  padding: '16px 20px',
  minWidth: '160px',
  flex: '1',
};
