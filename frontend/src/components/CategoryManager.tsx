import { useState } from 'react';
import { useCategories } from '../hooks/useCategories';
import type { ApiError } from '../types';

export function CategoryManager() {
  const { categories, loading, create, rename, remove } = useCategories();

  const [newName, setNewName] = useState('');
  const [createError, setCreateError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);

  const [renamingId, setRenamingId] = useState<string | null>(null);
  const [renameValue, setRenameValue] = useState('');
  const [renameError, setRenameError] = useState<string | null>(null);
  const [renameSaving, setRenameSaving] = useState(false);

  const [deleteErrors, setDeleteErrors] = useState<Record<string, string>>({});
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const getErrorMessage = (err: unknown): string => {
    if (err && typeof err === 'object' && 'message' in err) {
      return String((err as ApiError).message);
    }
    return 'An unexpected error occurred.';
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = newName.trim();
    if (!trimmed) {
      setCreateError('Category name is required.');
      return;
    }
    setCreateError(null);
    setCreating(true);
    try {
      await create(trimmed);
      setNewName('');
    } catch (err: unknown) {
      const apiErr = err as ApiError;
      if (apiErr && typeof apiErr === 'object' && 'error' in apiErr && apiErr.error === 'CONFLICT') {
        setCreateError('A category with this name already exists.');
      } else {
        setCreateError(getErrorMessage(err));
      }
    } finally {
      setCreating(false);
    }
  };

  const startRename = (id: string, currentName: string) => {
    setRenamingId(id);
    setRenameValue(currentName);
    setRenameError(null);
  };

  const cancelRename = () => {
    setRenamingId(null);
    setRenameValue('');
    setRenameError(null);
  };

  const handleRename = async (id: string) => {
    const trimmed = renameValue.trim();
    if (!trimmed) {
      setRenameError('Name cannot be empty.');
      return;
    }
    setRenameError(null);
    setRenameSaving(true);
    try {
      await rename(id, trimmed);
      setRenamingId(null);
      setRenameValue('');
    } catch (err: unknown) {
      const apiErr = err as ApiError;
      if (apiErr && typeof apiErr === 'object' && 'error' in apiErr && apiErr.error === 'CONFLICT') {
        setRenameError('A category with this name already exists.');
      } else {
        setRenameError(getErrorMessage(err));
      }
    } finally {
      setRenameSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this category?')) return;
    setDeleteErrors((prev) => { const next = { ...prev }; delete next[id]; return next; });
    setDeletingId(id);
    try {
      await remove(id);
    } catch (err: unknown) {
      const apiErr = err as ApiError;
      let msg: string;
      if (apiErr && typeof apiErr === 'object' && 'error' in apiErr) {
        if (apiErr.error === 'CONFLICT') {
          msg = 'A category with this name already exists.';
        } else if (apiErr.error === 'BUSINESS_RULE_VIOLATION' || apiErr.error === 'UNPROCESSABLE') {
          msg = 'Cannot delete: this category has transactions.';
        } else {
          msg = getErrorMessage(err);
        }
      } else {
        msg = getErrorMessage(err);
      }
      setDeleteErrors((prev) => ({ ...prev, [id]: msg }));
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div style={{ maxWidth: '480px' }}>
      <h3 style={{ margin: '0 0 16px', fontSize: '16px', fontWeight: 600 }}>Categories</h3>

      {loading && (
        <div style={{ color: '#888', fontSize: '14px', marginBottom: '12px' }}>Loading…</div>
      )}

      <ul style={{ listStyle: 'none', padding: 0, margin: '0 0 20px' }}>
        {categories.map((cat) => (
          <li key={cat.id} style={{ borderBottom: '1px solid #e5e7eb', padding: '10px 0' }}>
            {renamingId === cat.id ? (
              <div>
                <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                  <input
                    type="text"
                    value={renameValue}
                    onChange={(e) => setRenameValue(e.target.value)}
                    onKeyDown={(e) => { if (e.key === 'Enter') handleRename(cat.id); if (e.key === 'Escape') cancelRename(); }}
                    autoFocus
                    disabled={renameSaving}
                    style={{ padding: '6px 8px', border: '1px solid #0d6efd', borderRadius: '4px', fontSize: '14px', flex: 1 }}
                  />
                  <button
                    onClick={() => handleRename(cat.id)}
                    disabled={renameSaving}
                    style={{ ...smallBtnStyle, background: '#0d6efd', color: '#fff', border: 'none' }}
                  >
                    {renameSaving ? '…' : 'Save'}
                  </button>
                  <button
                    onClick={cancelRename}
                    disabled={renameSaving}
                    style={smallBtnStyle}
                  >
                    Cancel
                  </button>
                </div>
                {renameError && <div style={inlineErrorStyle}>{renameError}</div>}
              </div>
            ) : (
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: '14px' }}>{cat.name}</span>
                  <div style={{ display: 'flex', gap: '6px' }}>
                    <button
                      onClick={() => startRename(cat.id, cat.name)}
                      style={smallBtnStyle}
                    >
                      Rename
                    </button>
                    <button
                      onClick={() => handleDelete(cat.id)}
                      disabled={deletingId === cat.id}
                      style={{ ...smallBtnStyle, color: '#dc3545', borderColor: '#dc3545', opacity: deletingId === cat.id ? 0.6 : 1 }}
                    >
                      {deletingId === cat.id ? '…' : 'Delete'}
                    </button>
                  </div>
                </div>
                {deleteErrors[cat.id] && <div style={inlineErrorStyle}>{deleteErrors[cat.id]}</div>}
              </div>
            )}
          </li>
        ))}
        {!loading && categories.length === 0 && (
          <li style={{ color: '#888', fontSize: '14px', padding: '10px 0' }}>No categories yet.</li>
        )}
      </ul>

      <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
        <label htmlFor="new-category-name" style={{ fontSize: '13px', fontWeight: 500, color: '#333' }}>
          New category
        </label>
        <div style={{ display: 'flex', gap: '8px' }}>
          <input
            id="new-category-name"
            type="text"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            placeholder="Category name"
            disabled={creating}
            style={{
              padding: '8px 10px',
              border: `1px solid ${createError ? '#dc3545' : '#ccc'}`,
              borderRadius: '4px',
              fontSize: '14px',
              flex: 1,
            }}
          />
          <button
            type="submit"
            disabled={creating}
            style={{
              padding: '8px 16px',
              background: creating ? '#6c757d' : '#0d6efd',
              color: '#fff',
              border: 'none',
              borderRadius: '4px',
              fontSize: '14px',
              cursor: creating ? 'not-allowed' : 'pointer',
              fontWeight: 500,
              whiteSpace: 'nowrap',
            }}
          >
            {creating ? 'Adding…' : 'Add'}
          </button>
        </div>
        {createError && <div style={inlineErrorStyle}>{createError}</div>}
      </form>
    </div>
  );
}

const smallBtnStyle: React.CSSProperties = {
  padding: '4px 10px',
  background: '#fff',
  border: '1px solid #ccc',
  borderRadius: '4px',
  fontSize: '12px',
  cursor: 'pointer',
  fontWeight: 500,
};

const inlineErrorStyle: React.CSSProperties = {
  color: '#dc3545',
  fontSize: '12px',
  marginTop: '4px',
};
