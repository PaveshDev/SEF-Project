import React from 'react';

export const Input = ({ label, id, className = '', ...props }) => {
  return (
    <div className="form-group">
      {label && <label htmlFor={id} className="form-label">{label}</label>}
      <input id={id} className={`form-input ${className}`} {...props} />
    </div>
  );
};

export const Select = ({ label, id, options, className = '', ...props }) => {
  return (
    <div className="form-group">
      {label && <label htmlFor={id} className="form-label">{label}</label>}
      <select id={id} className={`form-select ${className}`} {...props}>
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
    </div>
  );
};
