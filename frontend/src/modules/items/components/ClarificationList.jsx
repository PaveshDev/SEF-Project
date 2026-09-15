import React, { useState } from 'react';

export function ClarificationList({ clarifications, onAnswerClarification }) {
  if (!clarifications || clarifications.length === 0) return null;

  const pending = clarifications.filter(c => c.status === 'Pending');
  const answered = clarifications.filter(c => c.status === 'Answered');

  return (
    <div className="clarification-list" style={{ marginTop: '20px' }}>
      <h3 style={{ borderBottom: '1px solid #e5e7eb', paddingBottom: '0.5rem', marginBottom: '1.5rem', color: '#111827' }}>Clarification History</h3>
      
      {pending.length > 0 && (
        <div style={{ marginBottom: '2rem' }}>
          <h4 style={{ color: '#b45309', marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <span style={{ fontSize: '1.25rem' }}>⏳</span> Pending Clarifications
          </h4>
          <div style={{ display: 'grid', gap: '1rem' }}>
            {pending.map(clarification => (
              <ClarificationItem 
                key={clarification.id} 
                clarification={clarification} 
                onAnswer={onAnswerClarification} 
              />
            ))}
          </div>
        </div>
      )}

      {answered.length > 0 && (
        <div>
          <h4 style={{ color: '#15803d', marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <span style={{ fontSize: '1.25rem' }}>✓</span> Answered Clarifications
          </h4>
          <div style={{ display: 'grid', gap: '1rem' }}>
            {answered.map(clarification => (
              <ClarificationItem 
                key={clarification.id} 
                clarification={clarification} 
                onAnswer={onAnswerClarification} 
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

function ClarificationItem({ clarification, onAnswer }) {
  const [answer, setAnswer] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!answer.trim()) return;
    
    setIsSubmitting(true);
    try {
      await onAnswer(clarification.id, answer.trim());
    } finally {
      setIsSubmitting(false);
    }
  };

  const isAnswered = clarification.status === 'Answered';
  
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString() + ' ' + new Date(dateString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <div className={`clarification-item ${isAnswered ? 'answered' : 'pending'}`}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '16px', borderBottom: '1px solid #e5e7eb', paddingBottom: '8px' }}>
        <span style={{ fontWeight: '500', color: '#6b7280', fontSize: '14px' }}>
          Requested on {formatDate(clarification.createdAt)}
        </span>
        <span style={{ fontWeight: '600', color: isAnswered ? '#10b981' : '#f59e0b', fontSize: '14px' }}>
          {clarification.status}
        </span>
      </div>
      
      <div>
        <p className="clarification-q">Q: {clarification.question}</p>
        <p className="clarification-reason">Reason: {clarification.reason}</p>
      </div>
      
      {isAnswered ? (
        <div className="clarification-answer-box">
          <p style={{ margin: '0 0 4px 0', fontWeight: '600', color: '#111827', fontSize: '14px' }}>Your Answer:</p>
          <p style={{ margin: '0 0 8px 0', color: '#374151', lineHeight: '1.5' }}>{clarification.answer}</p>
          <p style={{ margin: 0, fontSize: '12px', color: '#6b7280' }}>Answered on {formatDate(clarification.answeredAt)}</p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="clarification-answer-box pending" style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
          <label htmlFor={`answer-${clarification.id}`} style={{ fontWeight: '600', fontSize: '14px', color: '#111827' }}>Provide your answer:</label>
          <textarea
            id={`answer-${clarification.id}`}
            placeholder="Type your detailed answer here..."
            value={answer}
            onChange={(e) => setAnswer(e.target.value)}
            required
            style={{ width: '100%', padding: '12px', borderRadius: '8px', border: '1px solid #d1d5db', minHeight: '100px', fontFamily: 'inherit', boxSizing: 'border-box' }}
          />
          <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
            <button 
              type="submit" 
              disabled={isSubmitting || !answer.trim()}
              className="btn-warning"
            >
              {isSubmitting ? 'Sending...' : 'Submit Answer'}
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
