import React, { useState, useMemo } from 'react';
import { Link } from 'react-router';
import { useItems } from '../hooks/useItems';
import '../styles/items.css';

export function ItemsListPage() {
  const { items, isLoading, error } = useItems();
  
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const handleClearFilters = () => {
    setSearchTerm('');
    setCategoryFilter('');
    setStatusFilter('');
  };

  const filteredItems = useMemo(() => {
    if (!items) return [];
    return items.filter(item => {
      const matchesSearch = searchTerm === '' || 
        item.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (item.description && item.description.toLowerCase().includes(searchTerm.toLowerCase())) ||
        item.category.toLowerCase().includes(searchTerm.toLowerCase());
        
      const matchesCategory = categoryFilter === '' || item.category === categoryFilter;
      const matchesStatus = statusFilter === '' || item.status === statusFilter;
      
      return matchesSearch && matchesCategory && matchesStatus;
    });
  }, [items, searchTerm, categoryFilter, statusFilter]);

  // Dashboard calculations
  const stats = useMemo(() => {
    if (!items) return null;
    const s = {
      total: items.length,
      Draft: 0,
      Submitted: 0,
      Assessing: 0,
      PendingConfirmation: 0, // Awaiting Confirmation
      Confirmed: 0,
      ReassessmentRequested: 0,
    };
    const categories = {};

    items.forEach(item => {
      if (s[item.status] !== undefined) s[item.status]++;
      categories[item.category] = (categories[item.category] || 0) + 1;
    });

    return { ...s, categories };
  }, [items]);

  // Unique categories for the filter dropdown
  const uniqueCategories = useMemo(() => {
    if (!items) return [];
    return Array.from(new Set(items.map(i => i.category))).sort();
  }, [items]);

  if (isLoading) return (
    <div className="items-module-container" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '200px' }}>
      <div style={{ fontSize: '1.2rem', color: '#6b7280' }}>Loading your items...</div>
    </div>
  );

  return (
    <div className="items-module-container" style={{ maxWidth: '1200px', margin: '0 auto', padding: '2rem' }}>
      
      {/* Header */}
      <div className="items-page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h1 style={{ fontSize: '2.25rem', fontWeight: '800', margin: '0 0 0.5rem 0', color: '#111827' }}>MY ITEMS</h1>
          <p style={{ margin: 0, color: '#4b5563', fontSize: '1.1rem' }}>Manage and track the items you have submitted or are drafting.</p>
        </div>
        <Link 
          to="/items/new" 
          className="btn-primary" 
          style={{ padding: '0.75rem 1.5rem', fontSize: '1.1rem', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '0.5rem', backgroundColor: '#4f46e5', color: 'white', textDecoration: 'none', borderRadius: '0.375rem', boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)' }}
        >
          <span style={{ fontSize: '1.5rem', lineHeight: 1 }}>+</span> Add New Item
        </Link>
      </div>
      
      {error && (
        <div className="error-alert" role="alert" style={{ backgroundColor: '#fee2e2', border: '1px solid #ef4444', color: '#b91c1c', padding: '1rem', borderRadius: '0.375rem', marginBottom: '2rem' }}>
          <strong>Error loading items:</strong> {error}
        </div>
      )}
      
      {!error && items.length === 0 && (
        <div style={{ textAlign: 'center', padding: '4rem 2rem', backgroundColor: '#f9fafb', borderRadius: '0.5rem', border: '2px dashed #d1d5db' }}>
          <h3 style={{ fontSize: '1.5rem', color: '#374151', marginBottom: '1rem' }}>You have not created any items yet</h3>
          <p style={{ color: '#6b7280', marginBottom: '2rem' }}>Start by adding your first item to be assessed.</p>
          <Link to="/items/new" className="btn-primary" style={{ padding: '0.75rem 1.5rem', fontSize: '1rem', fontWeight: 'bold', textDecoration: 'none' }}>+ Create Your First Item</Link>
        </div>
      )}

      {items.length > 0 && stats && (
        <>
          {/* Dashboard Section */}
          <div style={{ marginBottom: '2rem', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem' }}>
            <div style={{ backgroundColor: '#eff6ff', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #bfdbfe' }}>
              <div style={{ color: '#1e40af', fontSize: '0.875rem', fontWeight: '600', textTransform: 'uppercase' }}>Total Items</div>
              <div style={{ fontSize: '2.5rem', fontWeight: '800', color: '#1d4ed8' }}>{stats.total}</div>
            </div>
            <div style={{ backgroundColor: '#f3f4f6', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #e5e7eb' }}>
              <div style={{ color: '#374151', fontSize: '0.875rem', fontWeight: '600', textTransform: 'uppercase' }}>Drafts</div>
              <div style={{ fontSize: '2.5rem', fontWeight: '800', color: '#111827' }}>{stats.Draft}</div>
            </div>
            <div style={{ backgroundColor: '#fef3c7', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #fde68a' }}>
              <div style={{ color: '#92400e', fontSize: '0.875rem', fontWeight: '600', textTransform: 'uppercase' }}>In Progress</div>
              <div style={{ fontSize: '2.5rem', fontWeight: '800', color: '#b45309' }}>{stats.Submitted + stats.Assessing}</div>
            </div>
            <div style={{ backgroundColor: '#f0fdf4', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #bbf7d0' }}>
              <div style={{ color: '#166534', fontSize: '0.875rem', fontWeight: '600', textTransform: 'uppercase' }}>Confirmed</div>
              <div style={{ fontSize: '2.5rem', fontWeight: '800', color: '#15803d' }}>{stats.Confirmed}</div>
            </div>
            <div style={{ backgroundColor: '#fef2f2', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #fecaca' }}>
              <div style={{ color: '#991b1b', fontSize: '0.875rem', fontWeight: '600', textTransform: 'uppercase' }}>Action Required</div>
              <div style={{ fontSize: '2.5rem', fontWeight: '800', color: '#b91c1c' }}>{stats.PendingConfirmation + stats.ReassessmentRequested}</div>
            </div>
          </div>

          <div style={{ marginBottom: '2rem', display: 'flex', gap: '2rem', flexWrap: 'wrap' }}>
            {/* Top Categories */}
            <div style={{ flex: '1', minWidth: '300px', backgroundColor: 'white', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #e5e7eb', boxShadow: '0 1px 3px 0 rgba(0,0,0,0.1)' }}>
              <h3 style={{ marginTop: 0, marginBottom: '1rem', color: '#374151', fontSize: '1.1rem' }}>Category Distribution</h3>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
                {Object.entries(stats.categories).sort((a,b)=>b[1]-a[1]).map(([cat, count]) => (
                  <span key={cat} style={{ backgroundColor: '#f3f4f6', padding: '0.25rem 0.75rem', borderRadius: '9999px', fontSize: '0.875rem', color: '#4b5563', border: '1px solid #d1d5db' }}>
                    {cat}: <strong>{count}</strong>
                  </span>
                ))}
              </div>
            </div>
          </div>

          {/* Filters */}
          <div style={{ display: 'flex', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap', backgroundColor: '#f9fafb', padding: '1rem', borderRadius: '0.5rem', border: '1px solid #e5e7eb' }}>
            <div style={{ flex: '1', minWidth: '200px' }}>
              <label htmlFor="search" style={{ display: 'block', fontSize: '0.875rem', fontWeight: '600', color: '#374151', marginBottom: '0.25rem' }}>Search</label>
              <input 
                id="search"
                type="text" 
                placeholder="Search title, desc, category..." 
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                style={{ width: '100%', padding: '0.5rem', borderRadius: '0.25rem', border: '1px solid #d1d5db' }}
              />
            </div>
            <div style={{ width: '200px' }}>
              <label htmlFor="categoryFilter" style={{ display: 'block', fontSize: '0.875rem', fontWeight: '600', color: '#374151', marginBottom: '0.25rem' }}>Category</label>
              <select 
                id="categoryFilter"
                value={categoryFilter} 
                onChange={(e) => setCategoryFilter(e.target.value)}
                style={{ width: '100%', padding: '0.5rem', borderRadius: '0.25rem', border: '1px solid #d1d5db', backgroundColor: 'white' }}
              >
                <option value="">All Categories</option>
                {uniqueCategories.map(cat => <option key={cat} value={cat}>{cat}</option>)}
              </select>
            </div>
            <div style={{ width: '200px' }}>
              <label htmlFor="statusFilter" style={{ display: 'block', fontSize: '0.875rem', fontWeight: '600', color: '#374151', marginBottom: '0.25rem' }}>Status</label>
              <select 
                id="statusFilter"
                value={statusFilter} 
                onChange={(e) => setStatusFilter(e.target.value)}
                style={{ width: '100%', padding: '0.5rem', borderRadius: '0.25rem', border: '1px solid #d1d5db', backgroundColor: 'white' }}
              >
                <option value="">All Statuses</option>
                <option value="Draft">Draft</option>
                <option value="Submitted">Submitted</option>
                <option value="Assessing">Assessing</option>
                <option value="PendingConfirmation">Pending Confirmation</option>
                <option value="Confirmed">Confirmed</option>
                <option value="ReassessmentRequested">Reassessment Requested</option>
              </select>
            </div>
            <div style={{ display: 'flex', alignItems: 'flex-end' }}>
              <button 
                onClick={handleClearFilters}
                style={{ padding: '0.5rem 1rem', backgroundColor: '#e5e7eb', color: '#374151', border: 'none', borderRadius: '0.25rem', cursor: 'pointer', fontWeight: '500' }}
              >
                Clear Filters
              </button>
            </div>
          </div>

          {/* Table */}
          <div className="items-table-container" style={{ overflowX: 'auto', backgroundColor: 'white', borderRadius: '0.5rem', border: '1px solid #e5e7eb', boxShadow: '0 1px 3px 0 rgba(0,0,0,0.1)' }}>
            <table className="items-table" style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
              <thead style={{ backgroundColor: '#f9fafb', borderBottom: '1px solid #e5e7eb' }}>
                <tr>
                  <th style={{ padding: '1rem', color: '#374151', fontWeight: '600' }}>Title</th>
                  <th style={{ padding: '1rem', color: '#374151', fontWeight: '600' }}>Category</th>
                  <th style={{ padding: '1rem', color: '#374151', fontWeight: '600' }}>Location</th>
                  <th style={{ padding: '1rem', color: '#374151', fontWeight: '600' }}>Status</th>
                  <th style={{ padding: '1rem', color: '#374151', fontWeight: '600' }}>Date</th>
                  <th style={{ padding: '1rem', color: '#374151', fontWeight: '600', textAlign: 'right' }}>Actions</th>
                </tr>
              </thead>
              <tbody style={{ divideY: '1px solid #e5e7eb' }}>
                {filteredItems.length === 0 ? (
                  <tr>
                    <td colSpan="6" style={{ padding: '2rem', textAlign: 'center', color: '#6b7280' }}>
                      No items match your current filters.
                    </td>
                  </tr>
                ) : (
                  filteredItems.map(item => (
                    <tr key={item.id} style={{ borderBottom: '1px solid #e5e7eb', transition: 'background-color 0.2s' }}>
                      <td style={{ padding: '1rem', fontWeight: '500', color: '#111827' }}>{item.title}</td>
                      <td style={{ padding: '1rem', color: '#4b5563' }}>{item.category}</td>
                      <td style={{ padding: '1rem', color: '#4b5563' }}>{item.locationArea}</td>
                      <td style={{ padding: '1rem' }}>
                        <span className={`status-badge status-${item.status.toLowerCase()}`} style={{ display: 'inline-block', padding: '0.25rem 0.75rem', borderRadius: '9999px', fontSize: '0.875rem', fontWeight: '600' }}>
                          {item.status.replace(/([A-Z])/g, ' $1').trim()}
                        </span>
                      </td>
                      <td style={{ padding: '1rem', color: '#4b5563', fontSize: '0.875rem' }}>{new Date(item.createdAt).toLocaleDateString()}</td>
                      <td style={{ padding: '1rem', textAlign: 'right' }}>
                        <Link to={`/items/${item.id}`} className="table-action-link" style={{ color: '#4f46e5', fontWeight: '600', textDecoration: 'none', padding: '0.5rem 1rem', border: '1px solid #e0e7ff', borderRadius: '0.25rem', backgroundColor: '#e0e7ff' }}>View</Link>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
