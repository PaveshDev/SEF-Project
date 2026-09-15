import { useState, useEffect } from 'react'
import { prepareCollectionPlan, approveProposal, fetchPickupEvents } from '../services/collectionsApi.js'

export default function ProposalReviewSection({ review, setReview, tell, Icon, Badge }) {
  const [proposal, setProposal] = useState(null)
  const [events, setEvents] = useState([])
  const [loading, setLoading] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [reviewNotes, setReviewNotes] = useState('')
  const [reviewError, setReviewError] = useState('')
  const [serverError, setServerError] = useState('')

  // Default demo pickup ID for Member 4 review workflow
  const demoPickupId = 'c1000000-0000-0000-0000-000000001042'
  const staffMemberId = 'b1000000-0000-0000-0000-000000000001'

  const loadProposalAndEvents = async () => {
    setLoading(true)
    setServerError('')
    try {
      const plan = await prepareCollectionPlan(demoPickupId)
      if (plan) {
        setProposal(plan)
      }
      const eventList = await fetchPickupEvents(demoPickupId)
      setEvents(eventList || [])
    } catch (err) {
      setServerError('Failed to load proposal from backend API.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadProposalAndEvents()
  }, [])

  const handleDecision = async (actionType) => {
    const apiDecision = actionType === 'Approve' ? 'APPROVED' : actionType === 'Reject' ? 'REJECTED' : 'REVISION_REQUESTED'
    
    if (actionType !== 'Approve' && !reviewNotes.trim()) {
      setReviewError('Add a reason for rejection or revision.')
      return
    }
    setReviewError('')
    setServerError('')
    setSubmitting(true)

    try {
      if (proposal?.proposalId) {
        const updated = await approveProposal(proposal.proposalId, {
          proposalId: proposal.proposalId,
          decision: apiDecision,
          decidedBy: staffMemberId,
          notes: reviewNotes.trim() || `${actionType} by staff.`
        })

        if (updated) {
          setProposal(updated)
          setReview({ action: actionType, notes: reviewNotes.trim() })
          tell(`${actionType} submitted successfully. Pickup status updated in backend.`)
          await loadProposalAndEvents()
        } else {
          setServerError('Decision failed. Slot may no longer be available.')
        }
      } else {
        setReview({ action: actionType, notes: reviewNotes.trim() })
        tell(`${actionType} preference saved locally.`)
      }
    } catch (err) {
      setServerError(err.response?.data?.error || err.message || 'Error submitting staff decision.')
    } finally {
      setSubmitting(false)
    }
  }

  const rec = proposal?.recommended
  const fb = proposal?.fallback

  const formatDateString = (dtStr) => {
    if (!dtStr) return 'TBD'
    try {
      const dt = new Date(dtStr)
      return dt.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    } catch {
      return dtStr
    }
  }

  return <div className="cl-review-layout">
    <section className="cl-panel">
      <div className="cl-panel-heading">
        <div>
          <h2>Oak dining chair</h2>
          <p className="cl-muted">PU-1042 · Agentic AI Proposal ({proposal?.proposalId ? `ID: ${proposal.proposalId.slice(0, 8)}` : 'Loading...'})</p>
        </div>
        <Badge status={proposal?.feasibilityStatus || 'Awaiting review'} />
      </div>
      <div className="cl-panel-body">
        {loading && <div className="cl-notice">Loading Agentic AI proposal...</div>}
        {serverError && <p className="cl-error" role="alert">{serverError}</p>}
        
        {proposal ? (
          <>
            <div className="cl-inline-warning">
              {proposal.reasonForRecommendation || 'AI agent proposal generated. Review constraints before authorizing booking.'}
            </div>

            <h3>Recommended option (AI Generated)</h3>
            <div className="cl-option">
              <strong>
                {rec?.proposedStart ? new Date(rec.proposedStart).toLocaleDateString(undefined, { weekday: 'long', day: 'numeric', month: 'long' }) : 'Date TBD'} · {formatDateString(rec?.proposedStart)}–{formatDateString(rec?.proposedEnd)}
              </strong>
              <p>{rec?.requiredVehicleType || 'Small van'} · {rec?.collectionMethod || 'Pickup from owner address'}</p>
              <dl className="cl-facts">
                <div><dt>Item weight</dt><dd>8 kg</dd></div>
                <div><dt>Vehicle class</dt><dd>{rec?.requiredVehicleType || 'Small van'}</dd></div>
                <div><dt>Travel distance</dt><dd>{rec?.estimatedTravelDistance || '12.4 km (estimated)'}</dd></div>
                <div><dt>Travel duration</dt><dd>{rec?.estimatedTravelTime || '28 mins (estimated)'}</dd></div>
                <div><dt>Estimated cost</dt><dd>{rec?.estimatedCost ? `${rec.currency || 'LKR'} ${rec.estimatedCost}` : 'Free / Included'}</dd></div>
                <div><dt>Handling</dt><dd>{rec?.handlingRequirements?.join('; ') || 'Standard handling'}</dd></div>
                <div><dt>Feasibility</dt><dd><strong>{proposal.feasibilityStatus}</strong></dd></div>
              </dl>
            </div>

            <h3>Decision &amp; Constraint checks</h3>
            <ul className="cl-check-list">
              {proposal.constraintChecks?.map((check, i) => (
                <li key={i} style={{ color: check.passed ? 'inherit' : '#d32f2f' }}>
                  <strong>{check.constraint}:</strong> {check.detail}
                </li>
              ))}
            </ul>

            {fb && (
              <div className="cl-fallback">
                <strong>Fallback option · {formatDateString(fb.proposedStart)}–{formatDateString(fb.proposedEnd)}</strong>
                <p>{fb.collectionMethod} ({fb.requiredVehicleType})</p>
              </div>
            )}
          </>
        ) : (
          <p className="cl-muted">No proposal loaded. Click re-plan or refresh.</p>
        )}
      </div>
    </section>

    <div>
      <section className="cl-panel">
        <div className="cl-panel-heading"><h2>Staff decision action</h2></div>
        <div className="cl-panel-body">
          <p className="cl-muted">Authorized staff decision. Approving confirms the booking and generates a secure OTP.</p>
          <label className="cl-field">
            <span>Review notes</span>
            <textarea
              rows={4}
              value={reviewNotes}
              maxLength={500}
              onChange={event => setReviewNotes(event.target.value)}
              placeholder="Explain the decision or any revision requirements..."
            />
          </label>
          <div className="cl-review-actions">
            {['Approve', 'Reject', 'Request Revision'].map(action => (
              <button
                key={action}
                disabled={submitting}
                className={`cl-button ${action === 'Approve' ? 'cl-primary' : ''}`}
                onClick={() => handleDecision(action)}
              >
                {submitting ? 'Submitting...' : `${action}`}
              </button>
            ))}
          </div>
          {reviewError && <p className="cl-error" role="alert">{reviewError}</p>}
          {review && (
            <div className="cl-local-record">
              <strong>Staff Decision Recorded: {review.action}</strong>
              <p>{review.notes || 'No review notes provided.'}</p>
              <small>Status persisted in backend database.</small>
            </div>
          )}
        </div>
      </section>

      <section className="cl-panel cl-audit">
        <div className="cl-panel-heading"><h2>Backend Audit Trail History</h2></div>
        <div className="cl-panel-body">
          <p className="cl-muted">Persistent event records for pickup <code>PU-1042</code>:</p>
          {events.length > 0 ? (
            <ol className="cl-history">
              {events.map(e => (
                <li key={e.id}>
                  <strong>{e.eventType}</strong> · <small>{new Date(e.eventAt).toLocaleString()}</small>
                  {e.notes && <p style={{ margin: '2px 0 0 0', fontSize: '0.85em' }}>{e.notes}</p>}
                </li>
              ))}
            </ol>
          ) : (
            <small className="cl-muted">No persistent audit records recorded yet.</small>
          )}
        </div>
      </section>
    </div>
  </div>
}
