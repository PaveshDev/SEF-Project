import React from 'react';

export const PrimaryButton = ({ children, onClick, type = 'button', disabled = false, className = '' }) => (
  <button type={type} onClick={onClick} disabled={disabled} className={tn-primary disabled:opacity-50 }>
    {children}
  </button>
);

export const SecondaryButton = ({ children, onClick, type = 'button', disabled = false, className = '' }) => (
  <button type={type} onClick={onClick} disabled={disabled} className={tn-secondary disabled:opacity-50 }>
    {children}
  </button>
);

export const FormField = ({ label, id, value, onChange, type = 'text', required = false, as = 'input', rows = 3 }) => (
  <div className="mb-4">
    <label htmlFor={id} className="label">{label} {required && <span className="text-status-error">*</span>}</label>
    {as === 'textarea' ? (
      <textarea id={id} value={value} onChange={onChange} required={required} rows={rows} className="input-field" />
    ) : (
      <input id={id} type={type} value={value} onChange={onChange} required={required} className="input-field" />
    )}
  </div>
);

export const StatusChip = ({ status }) => {
  let bg = 'bg-gray-100 text-gray-800';
  if (status === 'Submitted' || status === 'Assessed') bg = 'bg-primary-light text-primary-dark';
  if (status === 'AssessmentPending') bg = 'bg-yellow-100 text-status-warning';
  
  return <span className={px-2 py-1 rounded-full text-xs font-semibold }>{status}</span>;
};
