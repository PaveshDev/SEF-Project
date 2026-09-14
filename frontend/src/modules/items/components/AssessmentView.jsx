import React from 'react';

export function AssessmentView({ assessment, onConfirm, onRequestReassessment }) {
  if (!assessment) return null;

  return (
    <div className="assessment-view" style={{ border: '1px solid #e5e7eb', backgroundColor: 'white', padding: '1.5rem', borderRadius: '0.5rem', boxShadow: '0 1px 3px 0 rgba(0,0,0,0.1)' }}>
      
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1.5rem', borderBottom: '1px solid #e5e7eb', paddingBottom: '1rem' }}>
        <div>
          <h3 style={{ color: '#111827', margin: '0 0 0.5rem 0', fontSize: '1.25rem' }}>Assessment Results</h3>
          <p style={{ margin: 0, color: '#6b7280', fontSize: '0.875rem' }}>Version {assessment.version}</p>
        </div>
        <div>
          <span className={`status-badge status-${assessment.status?.toLowerCase() || 'unknown'}`} style={{ padding: '0.25rem 0.75rem', borderRadius: '9999px', fontSize: '0.875rem', fontWeight: '600' }}>
            {assessment.status ? assessment.status.replace(/([A-Z])/g, ' $1').trim() : 'Unknown'}
          </span>
        </div>
      </div>
      
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1.5rem', marginBottom: '2rem' }}>
        
        {/* AI Observations */}
        <div style={{ backgroundColor: '#f0fdf4', border: '1px solid #bbf7d0', padding: '1.25rem', borderRadius: '0.5rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1rem' }}>
            <span style={{ fontSize: '1.2rem' }}>🤖</span>
            <h4 style={{ margin: 0, color: '#166534', fontSize: '1rem', fontWeight: '700' }}>AI OBSERVATIONS</h4>
          </div>
          
          <div style={{ display: 'grid', gap: '1rem' }}>
            <div>
              <span style={{ fontSize: '0.875rem', fontWeight: '600', color: '#166534', textTransform: 'uppercase' }}>Suggested Category</span>
              <div style={{ color: '#14532d', fontWeight: '500' }}>{assessment.suggestedCategory || 'Not assessed'}</div>
            </div>
            
            <div style={{ display: 'flex', gap: '2rem' }}>
              <div>
                <span style={{ fontSize: '0.875rem', fontWeight: '600', color: '#166534', textTransform: 'uppercase' }}>Condition Grade</span>
                <div style={{ color: '#14532d', fontWeight: '500' }}>{assessment.conditionGrade || 'N/A'}</div>
              </div>
              <div>
                <span style={{ fontSize: '0.875rem', fontWeight: '600', color: '#166534', textTransform: 'uppercase' }}>Confidence</span>
                <div style={{ color: '#14532d', fontWeight: '500' }}>{assessment.confidence != null ? `${(assessment.confidence * 100).toFixed(0)}%` : 'N/A'}</div>
              </div>
            </div>
            
            <div>
              <span style={{ fontSize: '0.875rem', fontWeight: '600', color: '#166534', textTransform: 'uppercase' }}>Condition Summary</span>
              <div style={{ color: '#14532d' }}>{assessment.conditionSummary || 'N/A'}</div>
            </div>
            
            <div>
              <span style={{ fontSize: '0.875rem', fontWeight: '600', color: '#166534', textTransform: 'uppercase' }}>Visible Observations</span>
              <div style={{ color: '#14532d' }}>{assessment.visibleObservations || 'N/A'}</div>
            </div>
          </div>
        </div>

        {/* Owner Reported */}
        <div style={{ backgroundColor: '#eff6ff', border: '1px solid #bfdbfe', padding: '1.25rem', borderRadius: '0.5rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1rem' }}>
            <span style={{ fontSize: '1.2rem' }}>👤</span>
            <h4 style={{ margin: 0, color: '#1e40af', fontSize: '1rem', fontWeight: '700' }}>OWNER REPORTED</h4>
          </div>
          
          <div style={{ display: 'grid', gap: '1rem' }}>
            <div>
              <span style={{ fontSize: '0.875rem', fontWeight: '600', color: '#1e40af', textTransform: 'uppercase' }}>Reported Functionality</span>
              <div style={{ color: '#1e3a8a' }}>{assessment.ownerReportedFunctionality || 'None provided'}</div>
            </div>
            
            <div>
              <span style={{ fontSize: '0.875rem', fontWeight: '600', color: '#1e40af', textTransform: 'uppercase' }}>Missing Information</span>
              <div style={{ color: '#1e3a8a' }}>{assessment.missingInformation || 'None identified'}</div>
            </div>
          </div>
        </div>
      </div>

      {assessment.evidences && assessment.evidences.length > 0 && (
        <div style={{ marginBottom: '2rem' }}>
          <h4 style={{ margin: '0 0 1rem 0', color: '#374151', fontSize: '1rem', fontWeight: '600' }}>Assessment Evidence</h4>
          <div style={{ display: 'flex', gap: '1rem', overflowX: 'auto', paddingBottom: '0.5rem' }}>
             {assessment.evidences.map((ev, index) => (
                <div key={ev.id || index} style={{ border: '1px solid #e5e7eb', padding: '0.5rem', borderRadius: '0.25rem', backgroundColor: '#f9fafb' }}>
                  <span style={{ fontSize: '0.875rem', color: '#4b5563' }}>Evidence #{index + 1}</span>
                </div>
             ))}
          </div>
        </div>
      )}

      {(assessment.status === 'PendingConfirmation' || assessment.status === 'Confirmed') && (
        <div style={{ display: 'flex', gap: '1rem', borderTop: '1px solid #e5e7eb', paddingTop: '1.5rem' }}>
          {assessment.status === 'PendingConfirmation' && onConfirm && (
            <button 
              onClick={() => {
                if (window.confirm('Are you sure you want to confirm this assessment? I have reviewed the AI generated observations and agree with them.')) {
                  onConfirm(assessment.id);
                }
              }}
              className="btn-primary"
              style={{ backgroundColor: '#10b981', color: 'white', padding: '0.75rem 1.5rem', border: 'none', borderRadius: '0.375rem', cursor: 'pointer', fontWeight: '600', flex: 1 }}
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
              style={{ backgroundColor: '#f59e0b', color: 'white', padding: '0.75rem 1.5rem', border: 'none', borderRadius: '0.375rem', cursor: 'pointer', fontWeight: '600', flex: 1 }}
            >
              ↺ Request Reassessment
            </button>
          )}
        </div>
      )}
    </div>
  );
}
