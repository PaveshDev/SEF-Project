import React from 'react';

export function ConcurrencyAlert({ error, onReload }) {
  if (!error) return null;

  return (
    <div className="concurrency-alert" style={{ border: '1px solid red', padding: '10px', margin: '10px 0', backgroundColor: '#fff0f0' }}>
      <h3 style={{ color: 'red', marginTop: 0 }}>Update Conflict</h3>
      <p>{error}</p>
      <button onClick={onReload} type="button">Reload Latest Version</button>
    </div>
  );
}
