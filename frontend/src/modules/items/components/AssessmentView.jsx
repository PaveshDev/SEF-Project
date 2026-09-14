import React from 'react';

export function AssessmentView({ assessment, onConfirm, onRequestReassessment }) {
  if (!assessment) return null;

  return (
    <div className="assessment-view" style={{ border: '1px solid #10b981', padding: '15px', borderRadius: '5px', marginTop: '20px' }}>
      <h3 style={{ color: '#059669', marginTop: 0 }}>AI Assessment Result</h3>
      <p><strong>Status:</strong> {assessment.status}</p>
      <p><strong>Version:</strong> {assessment.version}</p>
      
      <div style={{ display: 'flex', gap: '20px', marginTop: '15px' }}>
        <div style={{ flex: 1, backgroundColor: '#f0fdf4', padding: '10px', borderRadius: '4px' }}>
          <h4>AI OBSERVATIONS</h4>
          <p><strong>Suggested Category:</strong> {assessment.suggestedCategory || 'N/A'}</p>
          <p><strong>Condition Grade:</strong> {assessment.conditionGrade || 'N/A'}</p>
          <p><strong>Summary:</strong> {assessment.conditionSummary || 'N/A'}</p>
          <p><strong>Visible Observations:</strong> {assessment.visibleObservations || 'N/A'}</p>
          <p><strong>Confidence:</strong> {assessment.confidence ? `${(assessment.confidence * 100).toFixed(0)}%` : 'N/A'}</p>
        </div>

        <div style={{ flex: 1, backgroundColor: '#f8fafc', padding: '10px', borderRadius: '4px' }}>
          <h4>OWNER REPORTED</h4>
          <p><strong>Reported Functionality:</strong> {assessment.ownerReportedFunctionality || 'N/A'}</p>
          <p><strong>Missing Information:</strong> {assessment.missingInformation || 'N/A'}</p>
        </div>
      </div>

      <div style={{ marginTop: '20px', display: 'flex', gap: '10px' }}>
        {assessment.status === 'PendingConfirmation' && onConfirm && (
          <button 
            onClick={() => {
              if (window.confirm('Are you sure you want to confirm this assessment?')) {
                onConfirm(assessment.id);
              }
            }}
            style={{ backgroundColor: '#10b981', color: 'white', padding: '8px 16px', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
          >
            Confirm Assessment
          </button>
        )}
        
        {assessment.status === 'Confirmed' && onRequestReassessment && (
          <button 
            onClick={() => {
              const reason = window.prompt('Please provide a reason for reassessment:');
              if (reason) {
                onRequestReassessment(assessment.id, reason);
              }
            }}
            style={{ backgroundColor: '#f59e0b', color: 'white', padding: '8px 16px', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
          >
            Request Reassessment
          </button>
        )}
      </div>
    </div>
  );
}
