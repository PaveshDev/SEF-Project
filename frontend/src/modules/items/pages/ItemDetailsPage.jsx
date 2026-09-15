import React, { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router';
import { useItemDetails } from '../hooks/useItemDetails';
import { useAssessment } from '../hooks/useAssessment';
import { PhotoManager } from '../components/PhotoManager';
import { ConditionAnswersForm } from '../components/ConditionAnswersForm';
import { AssessmentView } from '../components/AssessmentView';
import { ClarificationList } from '../components/ClarificationList';
import '../styles/items.css';

export function ItemDetailsPage() {
  const { id } = useParams();
  const { 
    item, isLoading, error, fetchItem, 
    submitItem, addPhoto, submitConditionAnswers, deleteItem 
  } = useItemDetails(id);

  const navigate = useNavigate();

  const { answerClarification, confirmAssessment, requestReassessment } = useAssessment(id);

  const [submitError, setSubmitError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isLoading) return (
    <div className="items-module-container">
      <div className="items-loading-spinner"></div>
      <div style={{ textAlign: 'center', color: '#6b7280' }}>Loading item details...</div>
    </div>
  );
  
  if (error) {
    if (error.status === 404) return (
      <div className="items-module-container">
        <div className="items-empty-state">
          <h3>Item not found.</h3>
        </div>
      </div>
    );
    if (error.status === 403) return (
      <div className="items-module-container">
        <div className="items-empty-state">
          <h3>You do not have permission to view this item.</h3>
        </div>
      </div>
    );
    return (
      <div className="items-module-container">
        <div className="error-alert">Error: {error.message}</div>
      </div>
    );
  }
  
  if (!item) return <div className="items-module-container">Item not found.</div>;

  const handleAssessmentSubmit = async () => {
    setIsSubmitting(true);
    setSubmitError(null);
    try {
      await submitItem();
    } catch (err) {
      setSubmitError(err.response?.data?.detail || 'Failed to submit item for assessment.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAnswerClarification = async (clarificationId, answer) => {
    await answerClarification(clarificationId, answer, fetchItem);
  };

  const handleConfirmAssessment = async (assessmentId) => {
    await confirmAssessment(assessmentId, fetchItem);
  };

  const handleRequestReassessment = async (assessmentId, reason) => {
    await requestReassessment(assessmentId, reason, fetchItem);
  };

  const handleDelete = async () => {
    if (!window.confirm("Are you sure you want to delete this item? This action cannot be undone.")) return;
    setIsSubmitting(true);
    setSubmitError(null);
    try {
      await deleteItem();
      navigate('/items');
    } catch (err) {
      setSubmitError(err.response?.data?.detail || 'Failed to delete item.');
      setIsSubmitting(false);
    }
  };

  const canEdit = item.status === 'Draft';
  const canDelete = item.status === 'Draft' || item.status === 'Withdrawn';
  const canSubmit = item.status === 'Draft' && item.conditionAnswers && item.conditionAnswers.length > 0;

  return (
    <div className="items-module-container">
      
      {/* Action Bar & Header */}
      <div className="items-page-header" style={{ backgroundColor: '#ffffff', padding: '24px', borderRadius: '16px', border: '1px solid #f3f4f6', boxShadow: '0 4px 20px rgba(0,0,0,0.02)' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginBottom: '8px' }}>
            <h2 style={{ margin: 0 }}>{item.title}</h2>
            <span className={`status-badge status-${item.status.toLowerCase()}`}>
              {item.status.replace(/([A-Z])/g, ' $1').trim()}
            </span>
          </div>
          <p style={{ margin: 0 }}>Created on {new Date(item.createdAt).toLocaleDateString()} {item.updatedAt ? ` • Last updated ${new Date(item.updatedAt).toLocaleDateString()}` : ''}</p>
        </div>
        
        <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap' }}>
          {canEdit && (
            <Link to={`/items/${item.id}/edit`} className="btn-warning">
              Edit Item
            </Link>
          )}
          {canDelete && (
            <button 
              onClick={handleDelete} 
              disabled={isSubmitting}
              className="btn-secondary"
              style={{ color: '#dc2626', borderColor: '#fca5a5', backgroundColor: '#fef2f2' }}
            >
              Delete Item
            </button>
          )}
          {canSubmit && (
            <button 
              onClick={handleAssessmentSubmit} 
              disabled={isSubmitting}
              className="btn-primary"
            >
              {isSubmitting ? 'Submitting...' : 'Submit for Assessment'}
            </button>
          )}
        </div>
      </div>
      
      {submitError && (
        <div className="error-alert" role="alert">
          {submitError}
        </div>
      )}

      <div className="item-details-layout">
        
        {/* Left Column (Information & Assessment Form) */}
        <div>
          <div className="item-details-section">
            <h3>Item Information</h3>
            <div className="detail-row">
              <div className="detail-label">Description</div>
              <div className="detail-value" style={{ whiteSpace: 'pre-wrap' }}>{item.description || 'No description provided.'}</div>
            </div>
            <div className="detail-row">
              <div className="detail-label">Category</div>
              <div className="detail-value">{item.category}</div>
            </div>
            <div className="detail-row">
              <div className="detail-label">Location</div>
              <div className="detail-value">{item.locationArea}</div>
            </div>
          </div>

          <div className="item-details-section">
            <h3>Condition Assessment</h3>
            
            {item.status === 'Draft' ? (
              <ConditionAnswersForm 
                existingAnswers={item.conditionAnswers} 
                onSubmit={submitConditionAnswers} 
              />
            ) : (
              <div>
                {item.conditionAnswers && item.conditionAnswers.length > 0 ? (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                    {item.conditionAnswers.map((ans, index) => (
                      <div key={ans.id || index} style={{ backgroundColor: '#f9fafb', padding: '16px', borderRadius: '8px', border: '1px solid #e5e7eb' }}>
                        <p style={{ margin: '0 0 8px 0', fontWeight: '600', color: '#374151', fontSize: '15px' }}>{ans.questionText}</p>
                        <p style={{ margin: '0', color: '#111827' }}>{ans.answer}</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p style={{ color: '#6b7280', fontStyle: 'italic' }}>No condition information provided.</p>
                )}
              </div>
            )}
          </div>
        </div>

        {/* Right Column (Photos) */}
        <div>
          <div className="item-details-section">
            <h3>Photographs</h3>
            <PhotoManager 
              photos={item.photos} 
              onAddPhoto={addPhoto} 
            />
          </div>
        </div>
      </div>

      {/* Assessment History & Clarifications */}
      {((item.assessments && item.assessments.length > 0) || (item.assessments?.[0]?.clarifications?.length > 0)) && (
        <div style={{ marginTop: '48px' }}>
          <h2 style={{ fontSize: '24px', fontWeight: '700', color: '#111827', marginBottom: '24px', paddingBottom: '12px', borderBottom: '2px solid #e5e7eb' }}>
            Assessment Review & Clarifications
          </h2>
          
          <div style={{ display: 'flex', flexDirection: 'column', gap: '32px' }}>
            {item.assessments && item.assessments.length > 0 && (
              <AssessmentView 
                assessment={item.assessments[0]} 
                onConfirm={handleConfirmAssessment}
                onRequestReassessment={handleRequestReassessment}
              />
            )}

            {item.assessments?.[0]?.clarifications && item.assessments[0].clarifications.length > 0 && (
              <ClarificationList 
                clarifications={item.assessments[0].clarifications}
                onAnswerClarification={handleAnswerClarification}
              />
            )}
          </div>
        </div>
      )}
    </div>
  );
}
