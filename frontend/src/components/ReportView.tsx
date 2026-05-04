import { useState } from 'react';
import { useAnalytics } from '../hooks/useAnalytics';
import type { CategoryBreakdown } from '../types';

const formatCurrency = (value: number) =>
  value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export function ReportView() {
  const { report, loading, error, fetchReport } = useAnalytics();

  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [formError, setFormError] = useState<string | null>(null);

  const handleFetch = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!dateFrom || !dateTo) {
      setFormError('Both start and end dates are required.');
      return;
    }
    if (dateFrom > dateTo) {
      setFormError('Start date must be before or equal to end date.');
      return;
    }
    setFormError(null);
    await fetchReport(dateFrom, dateTo);
  };

  const hasData = report && (report.totalIncome > 0 || report.totalExpenses > 0);

  return (
    <div>
      <form onSubmit={handleFetch} style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'flex-end', marginBottom: '24px' }}>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
          <label htmlFor="report-date-from" style={labelStyle}>From</label>
          <input
            id="report-date-from"
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            style={{ ...inputStyle, borderColor: formError ? '#dc3545' : '#ccc' }}
          />
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
          <label htmlFor="report-date-to" style={labelStyle}>To</label>
          <input
            id="report-date-to"
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            style={{ ...inputStyle, borderColor: formError ? '#dc3545' : '#ccc' }}
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          style={{
            padding: '8px 20px',
            background: loading ? '#6c757d' : '#0d6efd',
            color: '#fff',
            border: 'none',
            borderRadius: '4px',
            fontSize: '14px',
            cursor: loading ? 'not-allowed' : 'pointer',
            fontWeight: 500,
            height: '36px',
          }}
        >
          {loading ? 'Loading…' : 'Generate Report'}
        </button>

        {formError && (
          <div style={{ width: '100%', color: '#dc3545', fontSize: '12px', marginTop: '-4px' }}>
            {formError}
          </div>
        )}
      </form>

      {error && (
        <div style={{ background: '#fff3f3', border: '1px solid #f5c6cb', borderRadius: '4px', padding: '10px 12px', marginBottom: '16px', color: '#721c24', fontSize: '14px' }}>
          {error}
        </div>
      )}

      {report && (
        <div>
          <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap', marginBottom: '24px' }}>
            <div style={{ ...summaryCardStyle, borderTop: '3px solid #16a34a' }}>
              <div style={summaryLabelStyle}>Total Income</div>
              <div style={{ ...summaryValueStyle, color: '#16a34a' }}>+{formatCurrency(report.totalIncome)}</div>
            </div>
            <div style={{ ...summaryCardStyle, borderTop: '3px solid #dc2626' }}>
              <div style={summaryLabelStyle}>Total Expenses</div>
              <div style={{ ...summaryValueStyle, color: '#dc2626' }}>-{formatCurrency(report.totalExpenses)}</div>
            </div>
            <div style={{ ...summaryCardStyle, borderTop: `3px solid ${report.balance >= 0 ? '#0d6efd' : '#f59e0b'}` }}>
              <div style={summaryLabelStyle}>Balance</div>
              <div style={{ ...summaryValueStyle, color: report.balance >= 0 ? '#0d6efd' : '#f59e0b' }}>
                {report.balance >= 0 ? '+' : ''}{formatCurrency(report.balance)}
              </div>
            </div>
          </div>

          {!hasData ? (
            <div style={{ textAlign: 'center', padding: '32px', color: '#888', fontSize: '14px', background: '#f8f9fa', borderRadius: '6px' }}>
              No data for the selected period.
            </div>
          ) : (
            <div style={{ display: 'flex', gap: '24px', flexWrap: 'wrap' }}>
              <div style={{ flex: 1, minWidth: '280px' }}>
                <h4 style={{ margin: '0 0 12px', fontSize: '14px', fontWeight: 600, color: '#333' }}>Expenses by Category</h4>
                <CategoryTable rows={report.expensesByCategory} color="#dc2626" />
              </div>
              <div style={{ flex: 1, minWidth: '280px' }}>
                <h4 style={{ margin: '0 0 12px', fontSize: '14px', fontWeight: 600, color: '#333' }}>Income by Category</h4>
                <CategoryTable rows={report.incomeByCategory} color="#16a34a" />
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

interface CategoryTableProps {
  rows: CategoryBreakdown[];
  color: string;
}

function CategoryTable({ rows, color }: CategoryTableProps) {
  if (rows.length === 0) {
    return <div style={{ color: '#888', fontSize: '13px', padding: '12px 0' }}>No data</div>;
  }

  return (
    <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '14px' }}>
      <thead>
        <tr style={{ background: '#f8f9fa', borderBottom: '2px solid #dee2e6' }}>
          <th style={{ ...thStyle, textAlign: 'left' }}>Category</th>
          <th style={{ ...thStyle, textAlign: 'right' }}>Amount</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row) => (
          <tr key={row.categoryId} style={{ borderBottom: '1px solid #dee2e6' }}>
            <td style={tdStyle}>{row.categoryName}</td>
            <td style={{ ...tdStyle, textAlign: 'right', fontWeight: 500, color }}>{formatCurrency(row.total)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

const labelStyle: React.CSSProperties = {
  fontSize: '12px',
  color: '#666',
  fontWeight: 500,
};

const inputStyle: React.CSSProperties = {
  padding: '6px 8px',
  border: '1px solid #ccc',
  borderRadius: '4px',
  fontSize: '14px',
};

const summaryCardStyle: React.CSSProperties = {
  background: '#fff',
  border: '1px solid #e5e7eb',
  borderRadius: '8px',
  padding: '16px 20px',
  minWidth: '160px',
  flex: '1',
};

const summaryLabelStyle: React.CSSProperties = {
  fontSize: '12px',
  color: '#666',
  fontWeight: 500,
  marginBottom: '6px',
  textTransform: 'uppercase',
  letterSpacing: '0.5px',
};

const summaryValueStyle: React.CSSProperties = {
  fontSize: '22px',
  fontWeight: 700,
};

const thStyle: React.CSSProperties = {
  padding: '8px 12px',
  fontWeight: 600,
  color: '#333',
};

const tdStyle: React.CSSProperties = {
  padding: '8px 12px',
};
