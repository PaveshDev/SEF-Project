import React, { useState } from 'react';
import { useParams, Link } from 'react-router';
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
    submitItem, addPhoto, submitConditionAnswers 
  } = useItemDetails(id);

  const { answerClarification, confirmAssessment, requestReassessment } = useAssessment(id);

  const [submitError, setSubmitError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isLoading) return (
    <div className="items-module-container" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '200px' }}>
      <div style={{ fontSize: '1.2rem', color: '#6b7280' }}>Loading item details...</div>
    </div>
  );
  
  if (error) {
    if (error.status === 404) return <div className="items-module-container" style={{ textAlign: 'center', padding: '4rem 2rem' }}><h3>Item not found.</h3></div>;
    if (error.status === 403) return <div className="items-module-container" style={{ textAlign: 'center', padding: '4rem 2rem' }}><h3>You do not have permission to view this item.</h3></div>;
    return <div className="items-module-container"><div className="error-alert" style={{ backgroundColor: '#fee2e2', color: '#b91c1c', padding: '1rem', borderRadius: '0.375rem' }}>Error: {error.message}</div></div>;
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

  const canEdit = item.status === 'Draft';
  const canSubmit = item.status === 'Draft' && item.conditionAnswers && item.conditionAnswers.length > 0;

  return (
    <div className="items-module-container" style={{ maxWidth: '1200px', margin: '0 auto', padding: '2rem' }}>
      
      {/* Action Bar & Header */}
      <div className="items-page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '2rem', flexWrap: 'wrap', gap: '1rem', backgroundColor: '#f9fafb', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #e5e7eb' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '0.5rem' }}>
            <h2 style={{ fontSize: '2rem', fontWeight: '800', margin: 0, color: '#111827' }}>{item.title}</h2>
            <span className={`status-badge status-${item.status.toLowerCase()}`} style={{ padding: '0.25rem 0.75rem', borderRadius: '9999px', fontSize: '0.875rem', fontWeight: '600' }}>
              {item.status.replace(/([A-Z])/g, ' $1').trim()}
            </span>
          </div>
          <p style={{ margin: 0, color: '#4b5563' }}>Created on {new Date(item.createdAt).toLocaleDateString()} {item.updatedAt ? ` • Last updated ${new Date(item.updatedAt).toLocaleDateString()}` : ''}</p>
        </div>
        
        <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
          {canEdit && (
            <Link to={`/items/${item.id}/edit`} className="btn-warning" style={{ textDecoration: 'none', padding: '0.5rem 1rem', borderRadius: '0.375rem', fontWeight: '600', backgroundColor: '#f59e0b', color: 'white', display: 'flex', alignItems: 'center' }}>
              Edit Item
            </Link>
          )}
          {canSubmit && (
            <button 
              onClick={handleAssessmentSubmit} 
              disabled={isSubmitting}
              className="btn-primary"
              style={{ padding: '0.5rem 1rem', borderRadius: '0.375rem', fontWeight: '600', backgroundColor: '#10b981', color: 'white', border: 'none', cursor: isSubmitting ? 'not-allowed' : 'pointer' }}
            >
              {isSubmitting ? 'Submitting...' : 'Submit for Assessment'}
            </button>
          )}
        </div>
      </div>
      
      {submitError && <div className="error-alert" role="alert" style={{ backgroundColor: '#fee2e2', color: '#b91c1c', padding: '1rem', borderRadius: '0.375rem', marginBottom: '2rem' }}>{submitError}</div>}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '2rem', marginBottom: '2rem' }}>
        
        {/* Details Card */}
        <div style={{ backgroundColor: 'white', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #e5e7eb', boxShadow: '0 1px 3px 0 rgba(0,0,0,0.1)' }}>
          <h3 style={{ marginTop: 0, marginBottom: '1.5rem', fontSize: '1.25rem', color: '#111827', borderBottom: '1px solid #e5e7eb', paddingBottom: '0.5rem' }}>Item Information</h3>
          
          <div style={{ display: 'grid', gap: '1rem' }}>
            <div>
              <div style={{ fontSize: '0.875rem', fontWeight: '600', color: '#6b7280', textTransform: 'uppercase' }}>Description</div>
              <div style={{ color: '#111827', marginTop: '0.25rem', whiteSpace: 'pre-wrap' }}>{item.description || 'No description provided.'}</div>
            </div>
            <div style={{ display: 'flex', gap: '2rem' }}>
              <div>
                <div style={{ fontSize: '0.875rem', fontWeight: '600', color: '#6b7280', textTransform: 'uppercase' }}>Category</div>
                <div style={{ color: '#111827', marginTop: '0.25rem', fontWeight: '500' }}>{item.category}</div>
              </div>
              <div>
                <div style={{ fontSize: '0.875rem', fontWeight: '600', color: '#6b7280', textTransform: 'uppercase' }}>Location</div>
                <div style={{ color: '#111827', marginTop: '0.25rem', fontWeight: '500' }}>{item.locationArea}</div>
              </div>
            </div>
          </div>
        </div>

        {/* Condition Answers */}
        <div style={{ backgroundColor: 'white', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #e5e7eb', boxShadow: '0 1px 3px 0 rgba(0,0,0,0.1)' }}>
          <h3 style={{ marginTop: 0, marginBottom: '1.5rem', fontSize: '1.25rem', color: '#111827', borderBottom: '1px solid #e5e7eb', paddingBottom: '0.5rem' }}>Condition Assessment</h3>
          
          {item.status === 'Draft' ? (
            <ConditionAnswersForm 
              existingAnswers={item.conditionAnswers} 
              onSubmit={submitConditionAnswers} 
            />
          ) : (
            <div>
              {item.conditionAnswers && item.conditionAnswers.length > 0 ? (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                  {item.conditionAnswers.map((ans, index) => (
                    <div key={ans.id || index} style={{ backgroundColor: '#f9fafb', padding: '1rem', borderRadius: '0.375rem', border: '1px solid #e5e7eb' }}>
                      <p style={{ margin: '0 0 0.5rem 0', fontWeight: '600', color: '#374151', fontSize: '0.95rem' }}>{ans.questionText}</p>
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

      {/* Photos Section */}
      <div style={{ backgroundColor: 'white', padding: '1.5rem', borderRadius: '0.5rem', border: '1px solid #e5e7eb', boxShadow: '0 1px 3px 0 rgba(0,0,0,0.1)', marginBottom: '2rem' }}>
        <h3 style={{ marginTop: 0, marginBottom: '1.5rem', fontSize: '1.25rem', color: '#111827', borderBottom: '1px solid #e5e7eb', paddingBottom: '0.5rem' }}>Photographs</h3>
        <PhotoManager 
          photos={item.photos} 
          onAddPhoto={addPhoto} 
        />
      </div>

      {/* Assessment History & Clarifications */}
      {((item.assessments && item.assessments.length > 0) || (item.assessments?.[0]?.clarifications?.length > 0)) && (
        <div style={{ marginTop: '3rem' }}>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '800', color: '#111827', marginBottom: '1.5rem', borderBottom: '2px solid #e5e7eb', paddingBottom: '0.5rem' }}>Assessment Review & Clarifications</h2>
          
          <div style={{ display: 'grid', gap: '2rem' }}>
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
