import { ReportView } from '../components/ReportView';

export function ReportsPage() {
  return (
    <div style={{ padding: '24px', maxWidth: '900px', margin: '0 auto' }}>
      <h1 style={{ fontSize: '22px', fontWeight: 700, marginBottom: '20px', color: '#111' }}>
        Reports
      </h1>
      <ReportView />
    </div>
  );
}
