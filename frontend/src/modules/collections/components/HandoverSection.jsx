import { useState } from 'react'
import { milestones } from '../demoData.js'

export default function HandoverSection({ jobs, tell, verifyCode, Icon }) {
  const [handoverId, setHandoverId] = useState(jobs[0]?.id || '')
  const [code, setCode] = useState('')
  const [proofName, setProofName] = useState('')
  const [verifying, setVerifying] = useState(false)
  const [verificationResult, setVerificationResult] = useState(null)

  const activeJob = jobs.find(job => job.id === handoverId) || jobs[0]

  const handleSubmit = async (event) => {
    event.preventDefault()
    if (!activeJob?.id) {
      tell('No pickup selected.')
      return
    }

    if (verifyCode) {
      setVerifying(true)
      try {
        // Submit 6-digit OTP code to real ASP.NET Core API
        const actorId = 'b1000000-0000-0000-0000-000000000001'
        const result = await verifyCode(activeJob.id, code, actorId)
        if (result) {
          setVerificationResult({ success: true, message: 'Handover code verified successfully via API.' })
          tell('Handover verified via API.')
        } else {
          setVerificationResult({ success: false, message: 'Code verification returned null.' })
          tell('Verification returned no response.')
        }
      } catch (err) {
        const errorMsg = err.response?.data?.error || err.message || 'Verification failed.'
        setVerificationResult({ success: false, message: errorMsg })
        tell(`Verification error: ${errorMsg}`)
      } finally {
        setVerifying(false)
      }
    } else {
      tell('Handover service not attached.')
    }
  }

  return <div className="cl-review-layout">
    <section className="cl-panel">
      <div className="cl-panel-heading"><h2>Handover verification</h2></div>
      <form className="cl-panel-body" onSubmit={handleSubmit}>
        <label className="cl-field"><span>Pickup</span>
          <select value={activeJob?.id || ''} onChange={event => { setHandoverId(event.target.value); setCode(''); setVerificationResult(null) }}>
            {jobs.map(job => <option key={job.id} value={job.id}>{job.id} · {job.item}</option>)}
            {jobs.length === 0 && <option value="">No pickups available</option>}
          </select>
        </label>
        <div className="cl-qr-placeholder">
          <Icon name="box" size={38} />
          <strong>QR token / OTP validation</strong>
          <span>Submit the 6-digit security code or scan to verify handover on the backend.</span>
        </div>
        <label className="cl-field"><span>One-time code (6 digits)</span>
          <input value={code} onChange={event => setCode(event.target.value)} inputMode="numeric" pattern="[0-9]{6}" maxLength={6} required autoComplete="off" placeholder="Enter 6 digits" />
        </label>
        <label className="cl-field"><span>Handover proof image (optional)</span>
          <input type="file" accept="image/*" onChange={event => setProofName(event.target.files?.[0]?.name ?? '')} />
        </label>
        {proofName && <p className="cl-muted">Selected proof file: {proofName}</p>}
        {verificationResult && (
          <div className={`cl-notice ${verificationResult.success ? '' : 'cl-notice-error'}`} role="status">
            <span>{verificationResult.message}</span>
          </div>
        )}
        <div className="cl-actions">
          <button type="submit" className="cl-button cl-primary" disabled={verifying || !activeJob?.id}>
            {verifying ? 'Verifying with backend…' : 'Verify handover via API'}
          </button>
        </div>
      </form>
    </section>
    <section className="cl-panel cl-fit">
      <div className="cl-panel-heading"><h2>Collection milestones</h2></div>
      <div className="cl-panel-body">
        <Milestones status={activeJob?.status} />
        <p className="cl-muted">Handover tokens are hashed, short-lived, and strictly verified by the ASP.NET Core API.</p>
      </div>
    </section>
  </div>
}

function Milestones({ status }) {
  const current = milestones.indexOf(status)
  return <ol className="cl-milestones">{milestones.map((step, index) => <li key={step} className={index <= current ? 'is-done' : ''}><span>{index < current ? '✓' : index + 1}</span><div><strong>{step}</strong>{index === current && <small>Current status</small>}</div></li>)}</ol>
}
