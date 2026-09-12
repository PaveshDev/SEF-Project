import React, { useState, useEffect } from 'react';
import { Card } from '../../../shared/components/Card';
import { Button } from '../../../shared/components/Button';
import { Badge } from '../../../shared/components/Badge';
import { Modal } from '../../../shared/components/Modal';
import { Input } from '../../../shared/components/Input';
import { getNeeds, createNeed } from '../api/needsApi';

export function RecipientNeeds() {
  const [needs, setNeeds] = useState([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState({ description: '', quantityRequired: 1, partnerId: '00000000-0000-0000-0000-000000000000', categoryId: '00000000-0000-0000-0000-000000000000' });
  
  useEffect(() => {
    getNeeds().then(setNeeds).catch(console.error);
  }, []);

  const handleSubmit = (e) => {
    e.preventDefault();
    createNeed(formData).then(newNeed => {
      setNeeds([...needs, newNeed]);
      setIsModalOpen(false);
    }).catch(console.error);
  };

  return (
    <main>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <h1>Recipient Needs</h1>
        <Button onClick={() => setIsModalOpen(true)}>Log New Need</Button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1.5rem' }}>
        {needs.map(n => (
          <Card key={n.id}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }}>
              <h3 style={{ margin: 0, fontSize: '1.1rem' }}>{n.description}</h3>
              <Badge variant={n.status === 'Open' ? 'success' : 'neutral'}>
                {n.status}
              </Badge>
            </div>
            <p style={{ margin: '0.25rem 0' }}><strong>Required:</strong> {n.quantityRequired}</p>
            <p style={{ margin: '0.25rem 0' }}><strong>Fulfilled:</strong> {n.quantityFulfilled}</p>
          </Card>
        ))}
        {needs.length === 0 && <p>No active needs found.</p>}
      </div>

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Log Recipient Need">
        <form onSubmit={handleSubmit}>
          <Input label="Description" value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})} required />
          <Input type="number" label="Quantity Required" value={formData.quantityRequired} onChange={e => setFormData({...formData, quantityRequired: parseInt(e.target.value)})} min="1" required />
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '1.5rem', gap: '0.75rem' }}>
            <Button type="button" variant="secondary" onClick={() => setIsModalOpen(false)}>Cancel</Button>
            <Button type="submit">Submit Need</Button>
          </div>
        </form>
      </Modal>
    </main>
  );
}
