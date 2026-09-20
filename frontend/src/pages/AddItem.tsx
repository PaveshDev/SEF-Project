import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';
import { FormField, PrimaryButton, SecondaryButton } from '../components/SharedUI';

export const AddItem = () => {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [category, setCategory] = useState('');
  const [desc, setDesc] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      // Assuming a mock category ID for now
      await api.post('/items', { name, categoryId: category || '00000000-0000-0000-0000-000000000000', conditionDescription: desc });
      navigate('/items');
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="max-w-2xl mx-auto card p-6">
      <h2 className="text-2xl font-bold mb-6">Add New Item</h2>
      <form onSubmit={handleSubmit}>
        <FormField label="Item Name" id="name" value={name} onChange={e => setName(e.target.value)} required />
        <FormField label="Category ID" id="category" value={category} onChange={e => setCategory(e.target.value)} required />
        <FormField label="Condition Description" id="desc" as="textarea" value={desc} onChange={e => setDesc(e.target.value)} required />
        <div className="flex gap-4 mt-6">
          <PrimaryButton type="submit">Save Draft</PrimaryButton>
          <SecondaryButton onClick={() => navigate('/items')}>Cancel</SecondaryButton>
        </div>
      </form>
    </div>
  );
};
