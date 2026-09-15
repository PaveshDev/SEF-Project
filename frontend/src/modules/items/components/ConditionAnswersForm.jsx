import React, { useState } from 'react';

const parseAnswer = (rawAnswer) => {
  if (!rawAnswer) return { choice: '', details: '' };
  const choiceMatch = rawAnswer.match(/^(Yes|No|Unknown)/i);
  if (choiceMatch) {
    const choice = choiceMatch[1];
    const standardizedChoice = choice.charAt(0).toUpperCase() + choice.slice(1).toLowerCase();
    const details = rawAnswer.substring(choice.length).replace(/^(\s*-\s*|\s*\nDetails:\s*|\s*\n\s*)/i, '').trim();
    return { choice: standardizedChoice, details };
  }
  return { choice: '', details: rawAnswer };
};

const formatAnswer = (choice, details) => {
  if (!details) return choice;
  return `${choice}\nDetails: ${details}`;
};

export function ConditionAnswersForm({ existingAnswers, onSubmit }) {
  const staticQuestions = [
    { questionCode: 'POWER', questionText: 'Does the item power on?' },
    { questionCode: 'DAMAGE', questionText: 'Is there any visible physical damage?' }
  ];

  const [answers, setAnswers] = useState(() => {
    const initialMap = {};
    staticQuestions.forEach(q => {
      const existing = existingAnswers?.find(a => a.questionCode === q.questionCode);
      initialMap[q.questionCode] = parseAnswer(existing?.answer || '');
    });
    return initialMap;
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  const handleChoiceChange = (code, choice) => {
    setAnswers(prev => ({
      ...prev,
      [code]: { ...prev[code], choice }
    }));
  };

  const handleDetailsChange = (code, details) => {
    setAnswers(prev => ({
      ...prev,
      [code]: { ...prev[code], details }
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    setSuccessMessage('');
    setErrorMessage('');
    try {
      const payload = {
        answers: staticQuestions.map(q => {
          const ans = answers[q.questionCode] || { choice: '', details: '' };
          return {
            questionCode: q.questionCode,
            questionText: q.questionText,
            answer: formatAnswer(ans.choice, ans.details)
          };
        })
      };
      await onSubmit(payload);
      setSuccessMessage('Condition answers saved successfully.');
      setTimeout(() => setSuccessMessage(''), 4000);
    } catch (err) {
      setErrorMessage(err.response?.data?.detail || err.message || 'Failed to save condition answers.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="condition-answers">
      <form onSubmit={handleSubmit} className="item-form">
        {staticQuestions.map(q => {
          const currentAnswer = answers[q.questionCode] || { choice: '', details: '' };
          return (
            <div key={q.questionCode} className="form-group" style={{ marginBottom: '24px' }}>
              <label style={{ marginBottom: '12px', display: 'block', fontWeight: '600' }}>
                {q.questionText}
              </label>
              
              <div className="segmented-control" style={{ display: 'flex', gap: '8px', marginBottom: '12px' }}>
                {['Yes', 'No', 'Unknown'].map(option => (
                  <label 
                    key={option} 
                    style={{
                      flex: 1,
                      textAlign: 'center',
                      padding: '10px',
                      border: `1px solid ${currentAnswer.choice === option ? '#3b82f6' : '#e5e7eb'}`,
                      backgroundColor: currentAnswer.choice === option ? '#eff6ff' : '#ffffff',
                      color: currentAnswer.choice === option ? '#1d4ed8' : '#374151',
                      borderRadius: '8px',
                      cursor: 'pointer',
                      fontWeight: currentAnswer.choice === option ? '600' : '400',
                      transition: 'all 0.2s'
                    }}
                  >
                    <input
                      type="radio"
                      name={`question-${q.questionCode}`}
                      value={option}
                      checked={currentAnswer.choice === option}
                      onChange={() => handleChoiceChange(q.questionCode, option)}
                      style={{ display: 'none' }}
                      required
                    />
                    {option}
                  </label>
                ))}
              </div>

              <div className="additional-details">
                <label htmlFor={`details-${q.questionCode}`} style={{ fontSize: '13px', color: '#6b7280', marginBottom: '4px', display: 'block' }}>
                  Additional details (optional)
                </label>
                <textarea
                  id={`details-${q.questionCode}`}
                  value={currentAnswer.details}
                  onChange={(e) => handleDetailsChange(q.questionCode, e.target.value)}
                  rows={2}
                  placeholder="Provide any additional context..."
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #d1d5db', fontSize: '14px' }}
                />
              </div>
            </div>
          );
        })}
        <div className="form-actions">
          {successMessage && (
            <div className="success-alert" style={{ color: '#15803d', backgroundColor: '#f0fdf4', border: '1px solid #bbf7d0', padding: '12px', borderRadius: '8px', marginBottom: '16px' }} role="status">
              {successMessage}
            </div>
          )}
          {errorMessage && (
            <div className="error-alert" role="alert" style={{ marginBottom: '16px' }}>
              {errorMessage}
            </div>
          )}
          <button type="submit" className="btn-primary" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Save Answers'}
          </button>
        </div>
      </form>
    </div>
  );
}
