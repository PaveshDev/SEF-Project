import React, { useState, useEffect } from 'react';
import { Card } from '../../../shared/components/Card';
import { Button } from '../../../shared/components/Button';
import { Badge } from '../../../shared/components/Badge';
import { Modal } from '../../../shared/components/Modal';
import { Input, Select } from '../../../shared/components/Input';
import { getPartners, createPartner } from '../api/partnersApi';

export function PartnerDirectory() {
  const [partners, setPartners] = useState([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState({ name: '', partnerType: 'DonationOrganization', serviceArea: '' });
  
  useEffect(() => {
    getPartners().then(setPartners).catch(console.error);
  }, []);

  const handleSubmit = (e) => {
    e.preventDefault();
    createPartner(formData).then(newPartner => {
      setPartners([...partners, newPartner]);
      setIsModalOpen(false);
    }).catch(console.error);
  };

  return (
    <main>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <h1>Partner Directory</h1>
        <Button onClick={() => setIsModalOpen(true)}>Register Partner</Button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1.5rem' }}>
        {partners.map(p => (
          <Card key={p.id}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }}>
              <h3 style={{ margin: 0 }}>{p.name}</h3>
              <Badge variant={p.verificationStatus === 'Verified' ? 'success' : 'warning'}>
                {p.verificationStatus}
              </Badge>
            </div>
            <p style={{ margin: '0.25rem 0' }}><strong>Type:</strong> {p.partnerType}</p>
            <p style={{ margin: '0.25rem 0' }}><strong>Area:</strong> {p.serviceArea}</p>
          </Card>
        ))}
        {partners.length === 0 && <p>No partners registered yet.</p>}
      </div>

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Register New Partner">
        <form onSubmit={handleSubmit}>
          <Input label="Name" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} required />
          <Select label="Partner Type" options={[
            {value: 'DonationOrganization', label: 'Donation Organization'},
            {value: 'School', label: 'School'},
            {value: 'RepairPartner', label: 'Repair Partner'},
            {value: 'Reseller', label: 'Reseller'},
            {value: 'Recycler', label: 'Recycler'}
          ]} value={formData.partnerType} onChange={e => setFormData({...formData, partnerType: e.target.value})} />
          <Input label="Service Area" value={formData.serviceArea} onChange={e => setFormData({...formData, serviceArea: e.target.value})} required />
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '1.5rem', gap: '0.75rem' }}>
            <Button type="button" variant="secondary" onClick={() => setIsModalOpen(false)}>Cancel</Button>
            <Button type="submit">Save</Button>
          </div>
        </form>
      </Modal>
    </main>
  );
}
