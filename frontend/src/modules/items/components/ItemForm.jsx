import React, { useState } from 'react';

const STANDARD_CATEGORIES = [
  'Electronics',
  'Furniture',
  'Appliances',
  'Clothing',
  'Books'
];

export function ItemForm({ initialData, onSubmit, isSubmitting, fieldErrors = {} }) {
  
  const getInitialCategoryState = () => {
    if (!initialData?.category) return { selectedCategory: '', customCategory: '' };
    if (STANDARD_CATEGORIES.includes(initialData.category)) {
      return { selectedCategory: initialData.category, customCategory: '' };
    }
    return { selectedCategory: 'Other', customCategory: initialData.category };
  };

  const initialCatState = getInitialCategoryState();

  const [formData, setFormData] = useState({
    title: initialData?.title || '',
    description: initialData?.description || '',
    category: initialCatState.selectedCategory,
    customCategory: initialCatState.customCategory,
    locationArea: initialData?.locationArea || ''
  });

  const [validationErrors, setValidationErrors] = useState({});

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    // Clear validation error when user types
    if (validationErrors[name]) {
      setValidationErrors((prev) => ({ ...prev, [name]: null }));
    }
    if (name === 'category' || name === 'customCategory') {
      setValidationErrors((prev) => ({ ...prev, category: null, customCategory: null }));
    }
  };

  const validate = () => {
    const errors = {};
    
    const titleTrimmed = formData.title.trim();
    if (!titleTrimmed) {
      errors.title = "Title is required";
    } else if (!/[a-zA-Z]/.test(titleTrimmed)) {
      errors.title = "Title must contain at least one letter.";
    } else if (titleTrimmed.length > 200) {
      errors.title = "Title cannot exceed 200 characters";
    }

    if (formData.description && formData.description.length > 2000) {
      errors.description = "Description cannot exceed 2000 characters";
    }

    if (!formData.category) {
      errors.category = "Category is required";
    } else if (formData.category === 'Other') {
      const customCatTrimmed = formData.customCategory.trim();
      if (!customCatTrimmed) {
        errors.customCategory = "Specify Category is required";
      } else if (customCatTrimmed.length > 100) {
        errors.customCategory = "Category cannot exceed 100 characters";
      }
    } else if (!STANDARD_CATEGORIES.includes(formData.category)) {
      errors.category = "Please select a valid category";
    }

    const locationTrimmed = formData.locationArea.trim();
    if (!locationTrimmed) {
      errors.locationArea = "Location Area is required";
    } else if (locationTrimmed.length > 100) {
      errors.locationArea = "Location Area cannot exceed 100 characters";
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (validate()) {
      const finalCategory = formData.category === 'Other' ? formData.customCategory.trim() : formData.category;
      onSubmit({
        ...formData,
        title: formData.title.trim(),
        category: finalCategory,
        locationArea: formData.locationArea.trim()
      });
    }
  };

  const getError = (fieldName) => {
    const error = validationErrors[fieldName] || fieldErrors[fieldName];
    if (!error) return null;
    return (
      <div className="field-error" style={{ color: '#dc2626', fontSize: '0.875rem', marginTop: '0.25rem' }}>
        {Array.isArray(error) ? error.join(', ') : error}
      </div>
    );
  };

  return (
    <form onSubmit={handleSubmit} className="item-form">
      <div className="form-section">
        <h3 className="form-section-title">Basic Information</h3>
        <div className="form-group">
          <label htmlFor="title">Title *</label>
          <input 
            type="text" 
            id="title" 
            name="title" 
            value={formData.title} 
            onChange={handleChange} 
            required 
            maxLength={200}
            aria-invalid={!!(validationErrors.title || fieldErrors.title)}
            placeholder="Enter item title"
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
            maxLength={2000}
            aria-invalid={!!(validationErrors.description || fieldErrors.description)}
            placeholder="Describe the item's condition, features, etc."
          />
          {getError('description')}
        </div>
      </div>

      <div className="form-section">
        <h3 className="form-section-title">Categorization & Location</h3>
        <div className="form-group">
          <label htmlFor="category">Category *</label>
          <select
            id="category"
            name="category"
            value={formData.category}
            onChange={handleChange}
            required
            aria-invalid={!!(validationErrors.category || fieldErrors.category)}
            className="items-filter-select"
          >
            <option value="" disabled>Select category</option>
            {STANDARD_CATEGORIES.map(cat => (
              <option key={cat} value={cat}>{cat}</option>
            ))}
            <option value="Other">Other</option>
          </select>
          {getError('category')}
        </div>

        {formData.category === 'Other' && (
          <div className="form-group">
            <label htmlFor="customCategory">Specify Category *</label>
            <input 
              type="text" 
              id="customCategory" 
              name="customCategory" 
              value={formData.customCategory} 
              onChange={handleChange} 
              required 
              maxLength={100}
              aria-invalid={!!(validationErrors.customCategory || fieldErrors.category)}
              placeholder="e.g. Bicycle"
            />
            {getError('customCategory')}
          </div>
        )}
        
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
            aria-invalid={!!(validationErrors.locationArea || fieldErrors.locationArea)}
            placeholder="e.g. Warehouse A, Shelf 3"
          />
          {getError('locationArea')}
        </div>
      </div>
      
      <div className="form-actions">
        <button type="submit" className="btn-primary" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : 'Save Item'}
        </button>
      </div>
    </form>
  );
}
