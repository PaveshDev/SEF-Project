import React, { useState } from 'react';

export function ConditionAnswersForm({ existingAnswers, onSubmit }) {
  // Using static questions as mentioned in backend Phase 3,
  // since the backend currently returns a static list of condition questions.
  const staticQuestions = [
    { questionCode: 'POWER', questionText: 'Does the item power on?' },
    { questionCode: 'DAMAGE', questionText: 'Is there any visible physical damage?' }
  ];

  const [answers, setAnswers] = useState(() => {
    const initialMap = {};
    staticQuestions.forEach(q => {
      const existing = existingAnswers?.find(a => a.questionCode === q.questionCode);
      initialMap[q.questionCode] = existing?.answer || '';
    });
    return initialMap;
  });

  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = (code, value) => {
    setAnswers(prev => ({ ...prev, [code]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      const payload = {
        answers: staticQuestions.map(q => ({
          questionCode: q.questionCode,
          questionText: q.questionText,
          answer: answers[q.questionCode] || ''
        }))
      };
      await onSubmit(payload);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="condition-answers">
      <h3>Condition Questions</h3>
      <form onSubmit={handleSubmit}>
        {staticQuestions.map(q => (
          <div key={q.questionCode} style={{ marginBottom: '10px' }}>
            <label style={{ display: 'block', fontWeight: 'bold' }}>{q.questionText}</label>
            <textarea
              value={answers[q.questionCode]}
              onChange={(e) => handleChange(q.questionCode, e.target.value)}
              required
              rows={2}
              style={{ width: '100%', maxWidth: '400px' }}
            />
          </div>
        ))}
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : 'Save Answers'}
        </button>
      </form>
    </div>
  );
}
