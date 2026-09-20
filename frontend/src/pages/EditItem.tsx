import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../api';
import { FormField, PrimaryButton, SecondaryButton } from '../components/SharedUI';

export const EditItem = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [category, setCategory] = useState('');
  const [desc, setDesc] = useState('');

  useEffect(() => {
    api.get(/items/\).then(res => {
      setName(res.data.name);
      setCategory(res.data.categoryId);
      setDesc(res.data.conditionDescription);
    });
  }, [id]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    await api.put(/items/\, { name, categoryId: category, conditionDescription: desc });
    navigate(/items/\);
  };

  return (
    <div className="max-w-2xl mx-auto card p-6">
      <h2 className="text-2xl font-bold mb-6">Edit Item</h2>
      <form onSubmit={handleSubmit}>
        <FormField label="Item Name" id="name" value={name} onChange={e => setName(e.target.value)} required />
        <FormField label="Category ID" id="category" value={category} onChange={e => setCategory(e.target.value)} required />
        <FormField label="Condition Description" id="desc" as="textarea" value={desc} onChange={e => setDesc(e.target.value)} required />
        <div className="flex gap-4 mt-6">
          <PrimaryButton type="submit">Update</PrimaryButton>
          <SecondaryButton onClick={() => navigate(/items/\)}>Cancel</SecondaryButton>
        </div>
      </form>
    </div>
  );
};
