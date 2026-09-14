import React, { useState } from 'react';

export function ItemForm({ initialData, onSubmit, isSubmitting, fieldErrors = {} }) {
  const [formData, setFormData] = useState({
    title: initialData?.title || '',
    description: initialData?.description || '',
    category: initialData?.category || '',
    locationArea: initialData?.locationArea || ''
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const getError = (fieldName) => {
    if (!fieldErrors[fieldName]) return null;
    return <div className="field-error">{Array.isArray(fieldErrors[fieldName]) ? fieldErrors[fieldName].join(', ') : fieldErrors[fieldName]}</div>;
  };

  return (
    <form onSubmit={handleSubmit} className="item-form">
      <div className="form-group">
        <label htmlFor="title">Title *</label>
        <input 
          type="text" 
          id="title" 
          name="title" 
          value={formData.title} 
          onChange={handleChange} 
          required 
          maxLength={100}
          aria-invalid={!!fieldErrors.title}
        />
        {getError('title')}
      </div>
      <div className="form-group">
        <label htmlFor="description">Description</label>
        <textarea 
          id="description" 
          name="description" 
          value={formData.description} 
          onChange={handleChange} 
          maxLength={1000}
          aria-invalid={!!fieldErrors.description}
        />
        {getError('description')}
      </div>
      <div className="form-group">
        <label htmlFor="category">Category *</label>
        <input 
          type="text" 
          id="category" 
          name="category" 
          value={formData.category} 
          onChange={handleChange} 
          required 
          maxLength={50}
          aria-invalid={!!fieldErrors.category}
        />
        {getError('category')}
      </div>
      <div className="form-group">
        <label htmlFor="locationArea">Location Area *</label>
        <input 
          type="text" 
          id="locationArea" 
          name="locationArea" 
          value={formData.locationArea} 
          onChange={handleChange} 
          required 
          maxLength={100}
          aria-invalid={!!fieldErrors.locationArea}
        />
        {getError('locationArea')}
      </div>
      <div className="form-actions">
        <button type="submit" className="btn-primary" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : 'Save Item'}
        </button>
      </div>
    </form>
  );
}
