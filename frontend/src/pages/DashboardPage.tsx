import { useAnalytics } from '../hooks/useAnalytics';
import { useTransactions } from '../hooks/useTransactions';
import { BalanceWidget } from '../components/BalanceWidget';
import type { Transaction } from '../types';

const formatDate = (dateStr: string) => {
  const [year, month, day] = dateStr.split('-');
  return `${day}.${month}.${year}`;
};

const formatAmount = (amount: number, type: string) => {
  const formatted = amount.toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
  return type === 'income' ? `+${formatted}` : `-${formatted}`;
};

export function DashboardPage() {
  const { balance, loading: balanceLoading } = useAnalytics();
  const { transactions, loading: txLoading } = useTransactions({ page: 1, pageSize: 5 });

  const recentItems: Transaction[] = transactions?.items ?? [];

  return (
    <div style={{ padding: '24px', maxWidth: '900px', margin: '0 auto' }}>
      <h1 style={{ fontSize: '22px', fontWeight: 700, marginBottom: '20px', color: '#111' }}>
        Dashboard
      </h1>

      <section style={{ marginBottom: '32px' }}>
        <h2 style={sectionHeadingStyle}>Current Balance</h2>
        <BalanceWidget balance={balance} loading={balanceLoading} />
      </section>

      <section>
        <h2 style={sectionHeadingStyle}>Recent Transactions</h2>

        {txLoading ? (
          <div style={{ color: '#888', fontSize: '14px', padding: '16px 0' }}>Loading…</div>
        ) : recentItems.length === 0 ? (
          <div style={{ color: '#888', fontSize: '14px', padding: '16px 0' }}>
            No transactions yet.
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '14px' }}>
              <thead>
                <tr style={{ background: '#f8f9fa', borderBottom: '2px solid #dee2e6' }}>
                  <th style={thStyle}>Date</th>
                  <th style={thStyle}>Type</th>
                  <th style={{ ...thStyle, textAlign: 'right' }}>Amount</th>
                  <th style={thStyle}>Category</th>
                  <th style={thStyle}>Description</th>
                </tr>
              </thead>
              <tbody>
                {recentItems.map((txn) => (
                  <tr key={txn.id} style={{ borderBottom: '1px solid #dee2e6' }}>
                    <td style={tdStyle}>{formatDate(txn.date)}</td>
                    <td style={tdStyle}>
                      <span
                        style={{
                          display: 'inline-block',
                          padding: '2px 8px',
                          borderRadius: '12px',
                          fontSize: '12px',
                          fontWeight: 500,
                          background: txn.type === 'income' ? '#d1fae5' : '#fee2e2',
                          color: txn.type === 'income' ? '#065f46' : '#991b1b',
                        }}
                      >
                        {txn.type === 'income' ? 'Income' : 'Expense'}
                      </span>
                    </td>
                    <td
                      style={{
                        ...tdStyle,
                        textAlign: 'right',
                        fontWeight: 500,
                        color: txn.type === 'income' ? '#16a34a' : '#dc2626',
                      }}
                    >
                      {formatAmount(txn.amount, txn.type)}
                    </td>
                    <td style={tdStyle}>{txn.categoryName}</td>
                    <td style={{ ...tdStyle, color: '#666' }}>{txn.description ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}

const sectionHeadingStyle: React.CSSProperties = {
  fontSize: '15px',
  fontWeight: 600,
  color: '#333',
  marginBottom: '12px',
  marginTop: 0,
};

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
