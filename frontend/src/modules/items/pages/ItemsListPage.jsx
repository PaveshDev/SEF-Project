import React from 'react';
import { Link } from 'react-router';
import { useItems } from '../hooks/useItems';
import '../styles/items.css';

export function ItemsListPage() {
  const { items, isLoading, error } = useItems();

  if (isLoading) return <div className="items-module-container">Loading items...</div>;

  return (
    <div className="items-module-container">
      <div className="items-page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h2>My Items</h2>
          <p>Manage and track the items you have submitted or are drafting.</p>
        </div>
        <Link to="/items/new" className="btn-primary">
          Create New Item
        </Link>
      </div>
      
      {error && <div className="error-alert" role="alert">{error}</div>}
      
      {!error && items.length === 0 && (
        <p>You have not created any items yet.</p>
      )}

      {items.length > 0 && (
        <div className="items-table-container">
          <table className="items-table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Category</th>
                <th>Location</th>
                <th>Status</th>
                <th>Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map(item => (
                <tr key={item.id}>
                  <td>{item.title}</td>
                  <td>{item.category}</td>
                  <td>{item.locationArea}</td>
                  <td>
                    <span className={`status-badge status-${item.status.toLowerCase()}`}>
                      {item.status}
                    </span>
                  </td>
                  <td>{new Date(item.createdAt).toLocaleDateString()}</td>
                  <td>
                    <Link to={`/items/${item.id}`} className="table-action-link">View</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
