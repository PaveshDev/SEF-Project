import React from 'react';

export function AssessmentView({ assessment, onConfirm, onRequestReassessment }) {
  if (!assessment) return null;

  return (
    <div className="assessment-view item-details-section">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '24px', borderBottom: '1px solid #e5e7eb', paddingBottom: '16px' }}>
        <div>
          <h3 style={{ margin: '0 0 8px 0' }}>Assessment Results</h3>
          <p style={{ margin: 0, color: '#6b7280', fontSize: '14px' }}>Version {assessment.version}</p>
        </div>
        <div>
          <span className={`status-badge status-${assessment.status?.toLowerCase() || 'unknown'}`}>
            {assessment.status ? assessment.status.replace(/([A-Z])/g, ' $1').trim() : 'Unknown'}
          </span>
        </div>
      </div>
      
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '24px', marginBottom: '32px' }}>
        
        {/* AI Observations */}
        <div className="assessment-ai-card">
          <div className="assessment-ai-badge">AI GENERATED</div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '16px', marginTop: '4px' }}>
            <span style={{ fontSize: '20px' }}>🤖</span>
            <h4 style={{ margin: 0, color: '#1e3a8a', fontSize: '16px', fontWeight: '700' }}>AI OBSERVATIONS</h4>
          </div>
          
          <div style={{ display: 'grid', gap: '16px' }}>
            <div>
              <span style={{ fontSize: '12px', fontWeight: '600', color: '#3b82f6', textTransform: 'uppercase' }}>Suggested Category</span>
              <div style={{ color: '#1e3a8a', fontWeight: '600', marginTop: '4px' }}>{assessment.suggestedCategory || 'Not assessed'}</div>
            </div>
            
            <div style={{ display: 'flex', gap: '32px' }}>
              <div>
                <span style={{ fontSize: '12px', fontWeight: '600', color: '#3b82f6', textTransform: 'uppercase' }}>Condition Grade</span>
                <div style={{ color: '#1e3a8a', fontWeight: '600', marginTop: '4px' }}>{assessment.conditionGrade || 'N/A'}</div>
              </div>
              <div>
                <span style={{ fontSize: '12px', fontWeight: '600', color: '#3b82f6', textTransform: 'uppercase' }}>Confidence</span>
                <div style={{ color: '#1e3a8a', fontWeight: '600', marginTop: '4px' }}>{assessment.confidence != null ? `${(assessment.confidence * 100).toFixed(0)}%` : 'N/A'}</div>
              </div>
            </div>
            
            <div>
              <span style={{ fontSize: '12px', fontWeight: '600', color: '#3b82f6', textTransform: 'uppercase' }}>Condition Summary</span>
              <div style={{ color: '#1e3a8a', marginTop: '4px', lineHeight: '1.5' }}>{assessment.conditionSummary || 'N/A'}</div>
            </div>
            
            <div>
              <span style={{ fontSize: '12px', fontWeight: '600', color: '#3b82f6', textTransform: 'uppercase' }}>Visible Observations</span>
              <div style={{ color: '#1e3a8a', marginTop: '4px', lineHeight: '1.5' }}>{assessment.visibleObservations || 'N/A'}</div>
            </div>
          </div>
        </div>

        {/* Owner Reported */}
        <div className="assessment-owner-card">
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '16px' }}>
            <span style={{ fontSize: '20px' }}>👤</span>
            <h4 style={{ margin: 0, color: '#065f46', fontSize: '16px', fontWeight: '700' }}>OWNER REPORTED</h4>
          </div>
          
          <div style={{ display: 'grid', gap: '16px' }}>
            <div>
              <span style={{ fontSize: '12px', fontWeight: '600', color: '#10b981', textTransform: 'uppercase' }}>Reported Functionality</span>
              <div style={{ color: '#064e3b', marginTop: '4px', lineHeight: '1.5' }}>{assessment.ownerReportedFunctionality || 'None provided'}</div>
            </div>
            
            <div>
              <span style={{ fontSize: '12px', fontWeight: '600', color: '#10b981', textTransform: 'uppercase' }}>Missing Information</span>
              <div style={{ color: '#064e3b', marginTop: '4px', lineHeight: '1.5' }}>{assessment.missingInformation || 'None identified'}</div>
            </div>
          </div>
        </div>
      </div>

      {assessment.evidences && assessment.evidences.length > 0 && (
        <div style={{ marginBottom: '32px' }}>
          <h4 style={{ margin: '0 0 16px 0', color: '#374151', fontSize: '16px', fontWeight: '600' }}>Assessment Evidence</h4>
          <div style={{ display: 'flex', gap: '16px', overflowX: 'auto', paddingBottom: '8px' }}>
             {assessment.evidences.map((ev, index) => (
                <div key={ev.id || index} style={{ border: '1px solid #e5e7eb', padding: '12px 16px', borderRadius: '8px', backgroundColor: '#f9fafb' }}>
                  <span style={{ fontSize: '14px', color: '#4b5563', fontWeight: '500' }}>Evidence #{index + 1}</span>
                </div>
             ))}
          </div>
        </div>
      )}

      {(assessment.status === 'PendingConfirmation' || assessment.status === 'Confirmed') && (
        <div style={{ display: 'flex', gap: '16px', borderTop: '1px solid #e5e7eb', paddingTop: '24px' }}>
          {assessment.status === 'PendingConfirmation' && onConfirm && (
            <button 
              onClick={() => {
                if (window.confirm('Are you sure you want to confirm this assessment? I have reviewed the AI generated observations and agree with them.')) {
                  onConfirm(assessment.id);
                }
              }}
              className="btn-primary"
              style={{ backgroundColor: '#10b981', flex: 1 }}
            >
              ✓ Confirm Assessment
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
              className="btn-warning"
              style={{ flex: 1 }}
            >
              ↺ Request Reassessment
            </button>
          )}
        </div>
      )}
    </div>
  );
}
