import React, { useState } from 'react';

export function ClarificationList({ clarifications, onAnswerClarification }) {
  if (!clarifications || clarifications.length === 0) return null;

  return (
    <div className="clarification-list" style={{ marginTop: '20px' }}>
      <h3>Pending Clarifications</h3>
      {clarifications.map(clarification => (
        <ClarificationItem 
          key={clarification.id} 
          clarification={clarification} 
          onAnswer={onAnswerClarification} 
        />
      ))}
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
      await onAnswer(clarification.id, answer);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div style={{ border: '1px solid #f59e0b', padding: '10px', marginBottom: '10px', borderRadius: '4px', backgroundColor: '#fffbeb' }}>
      <p style={{ fontWeight: 'bold' }}>Question: {clarification.questionText}</p>
      
      {clarification.answer ? (
        <p><strong>Your Answer:</strong> {clarification.answer}</p>
      ) : (
        <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '10px' }}>
          <input
            type="text"
            placeholder="Type your answer here..."
            value={answer}
            onChange={(e) => setAnswer(e.target.value)}
            required
            style={{ flex: 1 }}
          />
          <button type="submit" disabled={isSubmitting || !answer.trim()}>
            {isSubmitting ? 'Sending...' : 'Submit Answer'}
          </button>
        </form>
      )}
    </div>
  );
}
