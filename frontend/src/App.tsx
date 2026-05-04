import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import { DashboardPage } from './pages/DashboardPage';
import { TransactionsPage } from './pages/TransactionsPage';
import { CategoriesPage } from './pages/CategoriesPage';
import { ReportsPage } from './pages/ReportsPage';

const navLinks = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/transactions', label: 'Transactions', end: false },
  { to: '/categories', label: 'Categories', end: false },
  { to: '/reports', label: 'Reports', end: false },
];

function NavBar() {
  return (
    <nav style={navStyle} aria-label="Main navigation">
      <div style={navInnerStyle}>
        <span style={brandStyle}>💰 Expense Tracker</span>
        <ul style={navListStyle}>
          {navLinks.map(({ to, label, end }) => (
            <li key={to}>
              <NavLink
                to={to}
                end={end}
                style={({ isActive }) => ({
                  ...navLinkStyle,
                  ...(isActive ? navLinkActiveStyle : {}),
                })}
              >
                {label}
              </NavLink>
            </li>
          ))}
        </ul>
      </div>
    </nav>
  );
}

function App() {
  return (
    <BrowserRouter>
      <NavBar />
      <main>
        <Routes>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/transactions" element={<TransactionsPage />} />
          <Route path="/categories" element={<CategoriesPage />} />
          <Route path="/reports" element={<ReportsPage />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}

export default App;

// ── Styles ──────────────────────────────────────────────────────────────────

const navStyle: React.CSSProperties = {
  background: '#1e293b',
  borderBottom: '1px solid #334155',
  position: 'sticky',
  top: 0,
  zIndex: 100,
};

const navInnerStyle: React.CSSProperties = {
  maxWidth: '1100px',
  margin: '0 auto',
  padding: '0 24px',
  display: 'flex',
  alignItems: 'center',
  gap: '32px',
  height: '56px',
};

const brandStyle: React.CSSProperties = {
  fontSize: '16px',
  fontWeight: 700,
  color: '#f1f5f9',
  whiteSpace: 'nowrap',
};

const navListStyle: React.CSSProperties = {
  listStyle: 'none',
  margin: 0,
  padding: 0,
  display: 'flex',
  gap: '4px',
};

const navLinkStyle: React.CSSProperties = {
  display: 'inline-block',
  padding: '6px 14px',
  borderRadius: '6px',
  fontSize: '14px',
  fontWeight: 500,
  color: '#94a3b8',
  textDecoration: 'none',
  transition: 'color 0.15s, background 0.15s',
};

const navLinkActiveStyle: React.CSSProperties = {
  color: '#f1f5f9',
  background: '#334155',
};
