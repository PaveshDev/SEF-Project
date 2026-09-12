import React from 'react';

export const Badge = ({ children, variant = 'neutral', className = '', ...props }) => {
  return (
    <span className={`badge badge-${variant} ${className}`} {...props}>
      {children}
    </span>
  );
};
