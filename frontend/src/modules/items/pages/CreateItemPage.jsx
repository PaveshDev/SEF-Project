import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useItems } from '../hooks/useItems';
import { ItemForm } from '../components/ItemForm';
import '../styles/items.css';

export function CreateItemPage() {
  const navigate = useNavigate();
  const { createItem } = useItems();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [fieldErrors, setFieldErrors] = useState({});

  const handleSubmit = async (formData) => {
    setIsSubmitting(true);
    setError(null);
    setFieldErrors({});
    try {
      const newItem = await createItem(formData);
      navigate(`/items/${newItem.id}`);
    } catch (err) {
      if (err.response && err.response.status === 400) {
        // Display validation errors properly
        const validationErrors = err.response.data?.errors;
        if (validationErrors) {
          // the backend might send capitalized or lowercase keys, but let's assume it maps to field names
          // usually they are Title, Description etc. We handle it in ItemForm or here by lowercasing keys
          const lowerCaseErrors = {};
          for (const key in validationErrors) {
            lowerCaseErrors[key.charAt(0).toLowerCase() + key.slice(1)] = validationErrors[key];
          }
          setFieldErrors(lowerCaseErrors);
        } else {
          setError(err.response.data?.detail || 'Validation failed.');
        }
      } else {
        setError('An unexpected error occurred while creating the item.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="items-module-container">
      <div className="items-card items-card-narrow">
        <div className="items-page-header">
          <h2>Create New Item</h2>
          <p>Provide the basic details about the item you want to submit for assessment.</p>
        </div>
        
        {error && (
          <div className="error-alert" role="alert">
            <svg style={{ width: '24px', height: '24px' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            {error}
          </div>
        )}
        
        <ItemForm 
          onSubmit={handleSubmit} 
          isSubmitting={isSubmitting} 
          fieldErrors={fieldErrors} 
        />
      </div>
    </div>
  );
}
