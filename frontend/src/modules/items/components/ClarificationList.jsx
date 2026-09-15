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
    <div style={{ border: `1px solid ${isAnswered ? '#bbf7d0' : '#fde68a'}`, backgroundColor: isAnswered ? '#f0fdf4' : '#fffbeb', padding: '1.25rem', borderRadius: '0.5rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem', borderBottom: `1px solid ${isAnswered ? '#bbf7d0' : '#fde68a'}`, paddingBottom: '0.5rem' }}>
        <span style={{ fontWeight: '600', color: isAnswered ? '#166534' : '#92400e' }}>
          Requested on {formatDate(clarification.createdAt)}
        </span>
        <span style={{ fontWeight: '600', color: isAnswered ? '#166534' : '#92400e' }}>
          Status: {clarification.status}
        </span>
      </div>
      
      <div style={{ marginBottom: '1rem' }}>
        <p style={{ fontWeight: '700', color: '#111827', margin: '0 0 0.25rem 0' }}>Q: {clarification.question}</p>
        <p style={{ margin: 0, color: '#4b5563', fontSize: '0.9rem' }}><em>Reason: {clarification.reason}</em></p>
      </div>
      
      {isAnswered ? (
        <div style={{ backgroundColor: '#dcfce7', padding: '1rem', borderRadius: '0.375rem', border: '1px solid #bbf7d0' }}>
          <p style={{ margin: '0 0 0.25rem 0', fontWeight: '600', color: '#166534' }}>Your Answer:</p>
          <p style={{ margin: '0 0 0.5rem 0', color: '#14532d' }}>{clarification.answer}</p>
          <p style={{ margin: 0, fontSize: '0.8rem', color: '#166534' }}>Answered on {formatDate(clarification.answeredAt)}</p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', backgroundColor: 'white', padding: '1rem', borderRadius: '0.375rem', border: '1px solid #fde68a' }}>
          <label htmlFor={`answer-${clarification.id}`} style={{ fontWeight: '600', fontSize: '0.9rem', color: '#92400e' }}>Provide your answer:</label>
          <textarea
            id={`answer-${clarification.id}`}
            placeholder="Type your detailed answer here..."
            value={answer}
            onChange={(e) => setAnswer(e.target.value)}
            required
            style={{ width: '100%', padding: '0.75rem', borderRadius: '0.25rem', border: '1px solid #d1d5db', minHeight: '80px', fontFamily: 'inherit' }}
          />
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '0.5rem' }}>
            <button 
              type="submit" 
              disabled={isSubmitting || !answer.trim()}
              className="btn-primary"
              style={{ backgroundColor: '#f59e0b', color: 'white', padding: '0.5rem 1.5rem', border: 'none', borderRadius: '0.25rem', cursor: (isSubmitting || !answer.trim()) ? 'not-allowed' : 'pointer', fontWeight: '600', opacity: (isSubmitting || !answer.trim()) ? 0.7 : 1 }}
            >
              {isSubmitting ? 'Sending...' : 'Submit Answer'}
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
