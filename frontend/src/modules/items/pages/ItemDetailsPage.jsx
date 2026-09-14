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

  if (isLoading) return <div className="items-module-container">Loading item details...</div>;
  if (error) {
    if (error.status === 404) return <div className="items-module-container">Item not found.</div>;
    if (error.status === 403) return <div className="items-module-container">You do not have permission to view this item.</div>;
    return <div className="items-module-container"><div className="error-alert">Error: {error.message}</div></div>;
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

  // Only allow editing if in Draft state (per backend rules)
  const canEdit = item.status === 'Draft';
  // Only allow submit if in Draft and has condition answers
  const canSubmit = item.status === 'Draft' && item.conditionAnswers && item.conditionAnswers.length > 0;

  return (
    <div className="items-module-container">
      <div className="items-page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h2>{item.title}</h2>
          <p>Detailed view of this item and its assessment progress.</p>
        </div>
        <div style={{ display: 'flex', gap: '10px' }}>
          {canEdit && (
            <Link to={`/items/${item.id}/edit`} className="btn-warning">
              Edit Item
            </Link>
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
      
      {submitError && <div className="error-alert" role="alert">{submitError}</div>}

      <div className="item-details-card">
        <div className="item-details-grid">
          <div>
            <h3 style={{ marginTop: 0, marginBottom: '20px' }}>Item Details</h3>
            
            <div className="detail-row">
              <div className="detail-label">Description</div>
              <div className="detail-value">{item.description}</div>
            </div>
            
            <div className="detail-row">
              <div className="detail-label">Category</div>
              <div className="detail-value">{item.category}</div>
            </div>
            
            <div className="detail-row">
              <div className="detail-label">Location Area</div>
              <div className="detail-value">{item.locationArea}</div>
            </div>
            
            <div className="detail-row">
              <div className="detail-label">Status</div>
              <div className="detail-value">
                <span className={`status-badge status-${item.status.toLowerCase()}`}>{item.status}</span>
              </div>
            </div>
            
            <div className="detail-row">
              <div className="detail-label">Version</div>
              <div className="detail-value">{item.version}</div>
            </div>
          </div>
          
          <div>
            <h3 style={{ marginTop: 0, marginBottom: '20px' }}>Photos</h3>
            <PhotoManager 
              photos={item.photos} 
              onAddPhoto={addPhoto} 
            />
          </div>
        </div>
      </div>

      <hr style={{ margin: '30px 0' }} />

      {item.status === 'Draft' ? (
        <ConditionAnswersForm 
          existingAnswers={item.conditionAnswers} 
          onSubmit={submitConditionAnswers} 
        />
      ) : (
        <div>
          <h3>Condition Answers</h3>
          {item.conditionAnswers?.map(ans => (
            <div key={ans.id ?? ans.questionCode ?? ans.questionText} style={{ marginBottom: '10px' }}>
              <p style={{ margin: '0', fontWeight: 'bold' }}>{ans.questionText}</p>
              <p style={{ margin: '0' }}>{ans.answer}</p>
            </div>
          ))}
        </div>
      )}

      {/* Render the latest assessment if available */}
      {item.assessments && item.assessments.length > 0 && (
        <AssessmentView 
          assessment={item.assessments[0]} // Assessments are ordered by version desc in backend
          onConfirm={handleConfirmAssessment}
          onRequestReassessment={handleRequestReassessment}
        />
      )}

      {/* Clarifications typically come with an assessment, maybe pending */}
      <ClarificationList 
        clarifications={item.assessments?.[0]?.clarifications?.filter(c => !c.answer)}
        onAnswerClarification={handleAnswerClarification}
      />
    </div>
  );
}
