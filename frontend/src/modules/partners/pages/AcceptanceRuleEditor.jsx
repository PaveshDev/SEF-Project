import React, { useState, useEffect } from 'react';
import { Card } from '../../../shared/components/Card';
import { Badge } from '../../../shared/components/Badge';
import { getRules } from '../api/rulesApi';

export function AcceptanceRuleEditor() {
  const [rules, setRules] = useState([]);
  
  useEffect(() => {
    getRules().then(setRules).catch(console.error);
  }, []);

  return (
    <main>
      <div style={{ marginBottom: '2rem' }}>
        <h1>Acceptance Rules</h1>
        <p>Manage rules dictating what items are accepted by partners.</p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1.5rem' }}>
        {rules.map(r => (
          <Card key={r.id}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }}>
              <h3 style={{ margin: 0, fontSize: '1.1rem' }}>Route: {r.routeType}</h3>
              <Badge variant={r.isActive ? 'success' : 'danger'}>
                {r.isActive ? 'Active' : 'Inactive'}
              </Badge>
            </div>
            <p style={{ margin: '0.25rem 0' }}><strong>Min Condition:</strong> {r.minimumCondition}</p>
            <p style={{ margin: '0.25rem 0' }}><strong>Restrictions:</strong> {r.restrictions}</p>
          </Card>
        ))}
        {rules.length === 0 && <p>No rules defined yet.</p>}
      </div>
    </main>
  );
}
