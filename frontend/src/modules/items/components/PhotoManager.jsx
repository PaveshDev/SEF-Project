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
      <h3>Photos</h3>
      <div className="photos-list" style={{ display: 'flex', gap: '10px', flexWrap: 'wrap', marginBottom: '10px' }}>
        {photos?.length === 0 && <p>No photos added yet.</p>}
        {photos?.map(photo => (
          <div key={photo.id} style={{ border: '1px solid #ccc', padding: '5px' }}>
            <img src={photo.imageUrl} alt="Item" style={{ width: '100px', height: '100px', objectFit: 'cover' }} />
          </div>
        ))}
      </div>

      <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '10px' }}>
        <input
          type="url"
          placeholder="Image URL"
          value={imageUrl}
          onChange={(e) => setImageUrl(e.target.value)}
          required
        />
        <button type="submit" disabled={isSubmitting || !imageUrl.trim()}>
          {isSubmitting ? 'Adding...' : 'Add Photo'}
        </button>
      </form>
    </div>
  );
}
