import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router';
import { useItemDetails } from '../hooks/useItemDetails';
import { ItemForm } from '../components/ItemForm';
import { ConcurrencyAlert } from '../components/ConcurrencyAlert';
import '../styles/items.css';

export function EditItemPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { item, isLoading, error, concurrencyError, updateItem, fetchItem } = useItemDetails(id);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState(null);
  const [fieldErrors, setFieldErrors] = useState({});

  if (isLoading) return (
    <div className="items-module-container">
      <div className="items-loading-spinner"></div>
      <div style={{ textAlign: 'center', color: '#6b7280' }}>Loading item for edit...</div>
    </div>
  );
  if (error) return (
    <div className="items-module-container">
      <div className="error-alert">Error loading item: {error.message}</div>
    </div>
  );
  if (!item) return (
    <div className="items-module-container">
      <div className="items-empty-state">Item not found.</div>
    </div>
  );
  
  if (item.status !== 'Draft') {
    return (
      <div className="items-module-container">
        <div className="error-alert">This item is no longer in Draft status and cannot be edited.</div>
      </div>
    );
  }

  const handleSubmit = async (formData) => {
    setIsSubmitting(true);
    setSubmitError(null);
    setFieldErrors({});
    try {
      const request = {
        id: item.id,
        version: item.version,
        ...formData
      };
      await updateItem(request);
      navigate(`/items/${item.id}`);
    } catch (err) {
      if (err.response && err.response.status === 400) {
        const validationErrors = err.response.data?.errors;
        if (validationErrors) {
          const lowerCaseErrors = {};
          for (const key in validationErrors) {
            lowerCaseErrors[key.charAt(0).toLowerCase() + key.slice(1)] = validationErrors[key];
          }
          setFieldErrors(lowerCaseErrors);
        } else {
          setSubmitError(err.response.data?.detail || 'Validation failed.');
        }
      } else if (err.response && err.response.status === 409) {
        // Concurrency error is handled by the hook and displayed in ConcurrencyAlert
      } else {
        setSubmitError('An unexpected error occurred while updating.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="items-module-container">
      <div className="items-card items-card-narrow">
        <div className="items-page-header">
          <h2>Edit Item</h2>
          <p>Update the details of your draft item.</p>
        </div>
        
        <ConcurrencyAlert error={concurrencyError} onReload={fetchItem} />
        
        {submitError && (
          <div className="error-alert" role="alert">
            <svg style={{ width: '24px', height: '24px' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            {submitError}
          </div>
        )}
        
        {/* Do not allow submission if there's a concurrency error until they reload */}
        {!concurrencyError && (
          <ItemForm initialData={item} onSubmit={handleSubmit} isSubmitting={isSubmitting} fieldErrors={fieldErrors} />
        )}
      </div>
    </div>
  );
}
