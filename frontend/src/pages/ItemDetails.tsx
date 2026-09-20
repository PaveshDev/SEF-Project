import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import api from '../api';
import { PrimaryButton, SecondaryButton, StatusChip } from '../components/SharedUI';

export const ItemDetails = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [item, setItem] = useState<any>(null);
  const [assessment, setAssessment] = useState<any>(null);

  const fetchItem = () => {
    api.get(/items/\).then(res => setItem(res.data));
    api.get(/items/\/assessment).then(res => setAssessment(res.data)).catch(() => {});
  };

  useEffect(() => {
    fetchItem();
  }, [id]);

  const handleAssess = async () => {
    try {
      await api.post(/items/\/assess);
      fetchItem();
    } catch (err) {
      alert("Assessment failed or item not submitted yet.");
    }
  };

  const handleRouteSelect = async (route) => {
    await api.post(/items/\/select-route, { selectedRoute: route });
    fetchItem();
  };

  if (!item) return <div>Loading...</div>;

  return (
    <div className="max-w-4xl mx-auto flex flex-col gap-6">
      <div className="card p-6 flex justify-between items-start">
        <div>
          <h2 className="text-3xl font-bold mb-2">{item.name}</h2>
          <div className="flex gap-2 mb-4">
            <StatusChip status={item.status} />
            {item.selectedRecoveryRoute && <span className="bg-primary text-white px-2 py-1 rounded-full text-xs font-semibold">Selected: {item.selectedRecoveryRoute}</span>}
          </div>
          <p className="text-text-muted mb-4">{item.conditionDescription}</p>
        </div>
        <div className="flex gap-2">
          {item.status === 'Draft' && <Link to={/items/\/edit}><SecondaryButton>Edit Draft</SecondaryButton></Link>}
          {(item.status === 'Submitted' || item.status === 'AssessmentPending') && <PrimaryButton onClick={handleAssess}>Run AI Assessment</PrimaryButton>}
        </div>
      </div>
      
      {assessment && (
        <div className="card p-6 bg-primary-light border-primary/20">
          <h3 className="text-xl font-semibold mb-4 text-primary-dark">AI Assessment Result</h3>
          <div className="grid grid-cols-2 gap-4 mb-4">
            <div><strong>Condition:</strong> {assessment.conditionLevel}</div>
            <div><strong>Confidence:</strong> {assessment.confidenceLevel}</div>
            <div><strong>Recommended Route:</strong> {assessment.recommendedRoute}</div>
            {assessment.alternativeRoute && <div><strong>Alternative:</strong> {assessment.alternativeRoute}</div>}
          </div>
          <p className="mb-6 bg-white p-3 rounded border border-border-main">{assessment.explanation}</p>
          
          {!item.selectedRecoveryRoute && (
            <div>
              <h4 className="font-semibold mb-2">Select Your Preferred Route:</h4>
              <div className="flex gap-4">
                {['Reuse', 'Donate', 'Recycle'].map(route => (
                  <PrimaryButton key={route} onClick={() => handleRouteSelect(route)}>{route}</PrimaryButton>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
