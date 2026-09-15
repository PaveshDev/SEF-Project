import React, { useState, useMemo } from 'react';
import { Link } from 'react-router';
import { useItems } from '../hooks/useItems';
import '../styles/items.css';

export function ItemsListPage() {
  const { items, isLoading, error, deleteItem } = useItems();
  
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
    <div className="items-module-container">
      <div className="items-loading-spinner"></div>
      <div style={{ textAlign: 'center', color: '#6b7280' }}>Loading your items...</div>
    </div>
  );

  return (
    <div className="items-module-container">
      
      {/* Header */}
      <div className="items-page-header">
        <div>
          <h2>MY ITEMS</h2>
          <p>Manage and track the items you have submitted or are drafting.</p>
        </div>
        <Link to="/items/new" className="btn-primary">
          <span style={{ fontSize: '1.25rem', marginRight: '8px' }}>+</span> Add New Item
        </Link>
      </div>
      
      {error && (
        <div className="error-alert" role="alert">
          <strong>Error loading items:</strong> {error}
        </div>
      )}
      
      {!error && items.length === 0 && (
        <div className="items-empty-state">
          <svg style={{ width: '48px', height: '48px', margin: '0 auto', color: '#9ca3af' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <h3>You have not created any items yet</h3>
          <p style={{ marginBottom: '24px' }}>Start by adding your first item to be assessed.</p>
          <Link to="/items/new" className="btn-primary">Create Your First Item</Link>
        </div>
      )}

      {items.length > 0 && stats && (
        <>
          {/* Dashboard Section */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px', marginBottom: '24px' }}>
            <div className="items-card" style={{ padding: '24px' }}>
              <div style={{ color: '#1e40af', fontSize: '12px', fontWeight: '600', textTransform: 'uppercase' }}>Total Items</div>
              <div style={{ fontSize: '36px', fontWeight: '800', color: '#1d4ed8' }}>{stats.total}</div>
            </div>
            <div className="items-card" style={{ padding: '24px' }}>
              <div style={{ color: '#374151', fontSize: '12px', fontWeight: '600', textTransform: 'uppercase' }}>Drafts</div>
              <div style={{ fontSize: '36px', fontWeight: '800', color: '#111827' }}>{stats.Draft}</div>
            </div>
            <div className="items-card" style={{ padding: '24px' }}>
              <div style={{ color: '#92400e', fontSize: '12px', fontWeight: '600', textTransform: 'uppercase' }}>In Progress</div>
              <div style={{ fontSize: '36px', fontWeight: '800', color: '#b45309' }}>{stats.Submitted + stats.Assessing}</div>
            </div>
            <div className="items-card" style={{ padding: '24px' }}>
              <div style={{ color: '#166534', fontSize: '12px', fontWeight: '600', textTransform: 'uppercase' }}>Confirmed</div>
              <div style={{ fontSize: '36px', fontWeight: '800', color: '#15803d' }}>{stats.Confirmed}</div>
            </div>
            <div className="items-card" style={{ padding: '24px', borderLeft: '4px solid #ef4444' }}>
              <div style={{ color: '#991b1b', fontSize: '12px', fontWeight: '600', textTransform: 'uppercase' }}>Action Required</div>
              <div style={{ fontSize: '36px', fontWeight: '800', color: '#b91c1c' }}>{stats.PendingConfirmation + stats.ReassessmentRequested}</div>
            </div>
          </div>

          {/* Filters */}
          <div className="items-filter-bar">
            <div style={{ flex: '1', minWidth: '200px' }}>
              <label htmlFor="search" style={{ display: 'block', fontSize: '12px', fontWeight: '600', color: '#6b7280', marginBottom: '8px', textTransform: 'uppercase' }}>Search</label>
              <input 
                id="search"
                type="text" 
                className="items-filter-input"
                placeholder="Search title, desc, category..." 
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <div style={{ width: '200px' }}>
              <label htmlFor="categoryFilter" style={{ display: 'block', fontSize: '12px', fontWeight: '600', color: '#6b7280', marginBottom: '8px', textTransform: 'uppercase' }}>Category</label>
              <select 
                id="categoryFilter"
                className="items-filter-select"
                value={categoryFilter} 
                onChange={(e) => setCategoryFilter(e.target.value)}
                style={{ width: '100%' }}
              >
                <option value="">All Categories</option>
                {uniqueCategories.map(cat => <option key={cat} value={cat}>{cat}</option>)}
              </select>
            </div>
            <div style={{ width: '200px' }}>
              <label htmlFor="statusFilter" style={{ display: 'block', fontSize: '12px', fontWeight: '600', color: '#6b7280', marginBottom: '8px', textTransform: 'uppercase' }}>Status</label>
              <select 
                id="statusFilter"
                className="items-filter-select"
                value={statusFilter} 
                onChange={(e) => setStatusFilter(e.target.value)}
                style={{ width: '100%' }}
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
                className="btn-secondary"
              >
                Clear Filters
              </button>
            </div>
          </div>

          {/* Table */}
          <div className="items-table-container">
            <table className="items-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Category</th>
                  <th>Location</th>
                  <th>Status</th>
                  <th>Date</th>
                  <th style={{ textAlign: 'right' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredItems.length === 0 ? (
                  <tr>
                    <td colSpan="6" style={{ padding: '32px', textAlign: 'center', color: '#6b7280' }}>
                      No items match your current filters.
                    </td>
                  </tr>
                ) : (
                  filteredItems.map(item => (
                    <tr key={item.id}>
                      <td style={{ fontWeight: '600', color: '#111827' }}>{item.title}</td>
                      <td>{item.category}</td>
                      <td>{item.locationArea}</td>
                      <td>
                        <span className={`status-badge status-${item.status.toLowerCase()}`}>
                          {item.status.replace(/([A-Z])/g, ' $1').trim()}
                        </span>
                      </td>
                      <td style={{ color: '#6b7280', fontSize: '13px' }}>{new Date(item.createdAt).toLocaleDateString()}</td>
                      <td style={{ textAlign: 'right' }}>
                        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
                          <Link to={`/items/${item.id}`} className="table-action-link">View Details</Link>
                          {(item.status === 'Draft' || item.status === 'Withdrawn') && (
                            <button 
                              onClick={async (e) => {
                                e.preventDefault();
                                if (window.confirm("Are you sure you want to delete this item? This action cannot be undone.")) {
                                  try {
                                    await deleteItem(item.id);
                                  } catch (err) {
                                    alert('Failed to delete item.');
                                  }
                                }
                              }}
                              className="table-action-link"
                              style={{ color: '#dc2626', background: 'none', border: 'none', cursor: 'pointer', padding: 0 }}
                            >
                              Delete
                            </button>
                          )}
                        </div>
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
