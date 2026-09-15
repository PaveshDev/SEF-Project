import React, { useState } from 'react';

export function PhotoManager({ photos, onAddPhoto }) {
  const [imageUrl, setImageUrl] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!imageUrl.trim()) return;
    
    setIsSubmitting(true);
    try {
      await onAddPhoto({ imageUrl, photoOrder: photos?.length || 0 });
      setImageUrl('');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="photo-manager">
      <div className="photo-gallery" style={{ marginBottom: '24px' }}>
        {photos?.length === 0 && <p style={{ color: '#6b7280', gridColumn: '1 / -1' }}>No photos added yet.</p>}
        {photos?.map(photo => (
          <img key={photo.id} src={photo.imageUrl} alt="Item" className="photo-thumbnail" />
        ))}
      </div>

      <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '12px' }}>
        <input
          type="url"
          placeholder="Enter image URL..."
          value={imageUrl}
          onChange={(e) => setImageUrl(e.target.value)}
          required
          style={{ flex: 1, padding: '10px 16px', borderRadius: '8px', border: '1px solid #d1d5db', fontFamily: 'inherit' }}
        />
        <button type="submit" className="btn-secondary" disabled={isSubmitting || !imageUrl.trim()}>
          {isSubmitting ? 'Adding...' : 'Add Photo'}
        </button>
      </form>
    </div>
  );
}
