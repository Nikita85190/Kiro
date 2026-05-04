import { CategoryManager } from '../components/CategoryManager';

export function CategoriesPage() {
  return (
    <div style={{ padding: '24px', maxWidth: '600px', margin: '0 auto' }}>
      <h1 style={{ fontSize: '22px', fontWeight: 700, marginBottom: '20px', color: '#111' }}>
        Categories
      </h1>
      <CategoryManager />
    </div>
  );
}
