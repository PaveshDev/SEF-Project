import { useState } from 'react'

export default function ProposalReviewSection({ review, setReview, tell, Icon, Badge }) {
  const [reviewNotes, setReviewNotes] = useState('')
  const [reviewError, setReviewError] = useState('')

  return <div className="cl-review-layout">
    <section className="cl-panel">
      <div className="cl-panel-heading"><div><h2>Oak dining chair</h2><p className="cl-muted">PU-1042 · Illustrative proposal, version 1</p></div><Badge status="Awaiting review" /></div>
      <div className="cl-panel-body">
        <div className="cl-inline-warning">No agent has run. Feasibility is undetermined until ASP.NET validates every constraint.</div>
        <h3>Recommended option · sample</h3>
        <div className="cl-option">
          <strong>Friday, 11 September · 14:00–16:00</strong>
          <p>Small van · Demo reuse workshop</p>
          <dl className="cl-facts">
            <div><dt>Item weight</dt><dd>8 kg (fixture)</dd></div>
            <div><dt>Vehicle capacity</dt><dd>50 kg (fixture)</dd></div>
            <div><dt>Owner availability</dt><dd>14:00–17:00 (fixture)</dd></div>
            <div><dt>Destination hours</dt><dd>09:00–17:00 (fixture)</dd></div>
            <div><dt>Travel distance / time</dt><dd>Unavailable</dd></div>
            <div><dt>Estimated cost</dt><dd>Unavailable</dd></div>
            <div><dt>Handling</dt><dd>Keep upright; protect finish</dd></div>
            <div><dt>Feasibility</dt><dd>Manual review required</dd></div>
          </dl>
        </div>
        <h3>Decision summary</h3>
        <ul className="cl-check-list">
          <li>Sample vehicle capacity exceeds the item's weight.</li>
          <li>Sample owner availability overlaps the proposed window.</li>
          <li>Destination closes at 17:00; arrival time is unverified.</li>
          <li>Vehicle dimensions, handling support, and actual availability require validation.</li>
        </ul>
        <div className="cl-fallback">
          <strong>Fallback · 16:00–18:00</strong>
          <p>Destination closes at 17:00. This option may exceed opening hours and must not be confirmed without revision.</p>
        </div>
      </div>
    </section>
    <div>
      <section className="cl-panel">
        <div className="cl-panel-heading"><h2>Staff decision preview</h2></div>
        <div className="cl-panel-body">
          <p className="cl-muted">Record a local review preference. This does not approve or confirm a booking.</p>
          <label className="cl-field"><span>Review notes</span><textarea rows={4} value={reviewNotes} maxLength={500} onChange={event => setReviewNotes(event.target.value)} placeholder="Explain the decision or the changes needed…" /></label>
          <div className="cl-review-actions">{['Approve', 'Reject', 'Request Revision'].map(action => <button key={action} className={`cl-button ${action === 'Approve' ? 'cl-primary' : ''}`} onClick={() => { if (action !== 'Approve' && !reviewNotes.trim()) { setReviewError('Add a reason for rejection or revision.'); return } setReviewError(''); setReview({ action, notes: reviewNotes.trim() }); tell(`${action} preference saved locally. Backend validation and authorization are still required.`) }}>{action} · preview</button>)}</div>
          {reviewError && <p className="cl-error" role="alert">{reviewError}</p>}
          {review && <div className="cl-local-record"><strong>Local preference: {review.action}</strong><p>{review.notes || 'No review notes.'}</p><small>Booking remains unconfirmed.</small></div>}
        </div>
      </section>
      <section className="cl-panel cl-audit">
        <div className="cl-panel-heading"><h2>Planned audit trail</h2></div>
        <div className="cl-panel-body">
          <p className="cl-muted">The connected workflow must persist:</p>
          <ol><li>Plan and constraint inputs</li><li>Steps and structured tool results</li><li>Backend validation results</li><li>Authorized human decision</li><li>Final assignment and outcome</li></ol>
          <small className="cl-muted">No persistent audit records exist in this preview.</small>
        </div>
      </section>
    </div>
  </div>
}
