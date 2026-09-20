import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api';
import { PrimaryButton, StatusChip } from '../components/SharedUI';
import { Plus } from 'lucide-react';

export const MyItems = () => {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Mock fetch for now as backend auth is not fully hooked up with UI
    // api.get('/items').then(res => setItems(res.data)).finally(() => setLoading(false));
    setItems([
      { id: '1', name: 'Old iPhone', category: { name: 'PHONE' }, status: 'Draft' }
    ]);
    setLoading(false);
  }, []);

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold">My Items</h2>
        <Link to="/items/add"><PrimaryButton><span className="flex items-center gap-2"><Plus size={16}/> Add Item</span></PrimaryButton></Link>
      </div>
      <div className="card overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50 border-b border-border-main text-text-muted text-sm uppercase">
              <th className="p-4">Name</th>
              <th className="p-4">Category</th>
              <th className="p-4">Status</th>
              <th className="p-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            {items.map(item => (
              <tr key={item.id} className="border-b border-border-main last:border-0 hover:bg-gray-50">
                <td className="p-4 font-medium">{item.name}</td>
                <td className="p-4">{item.category?.name}</td>
                <td className="p-4"><StatusChip status={item.status} /></td>
                <td className="p-4">
                  <Link to={/items/\} className="text-primary hover:underline">View</Link>
                </td>
              </tr>
            ))}
            {items.length === 0 && (
              <tr><td colSpan={4} className="p-8 text-center text-text-muted">No items found.</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
