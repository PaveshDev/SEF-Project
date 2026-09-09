import { useState } from 'react'
import { milestones } from '../demoData.js'

export default function HandoverSection({ jobs, tell, Icon }) {
  const [handoverId, setHandoverId] = useState(jobs[0]?.id || 'PU-1041')
  const [code, setCode] = useState('')
  const [proofName, setProofName] = useState('')

  return <div className="cl-review-layout">
    <section className="cl-panel">
      <div className="cl-panel-heading"><h2>Handover verification preview</h2></div>
      <form className="cl-panel-body" onSubmit={event => { event.preventDefault(); tell('Code format accepted for preview only. Verification is unavailable: connect the ASP.NET handover endpoint. No status was changed.') }}>
        <label className="cl-field"><span>Pickup</span><select value={handoverId} onChange={event => { setHandoverId(event.target.value); setCode('') }}>{jobs.map(job => <option key={job.id} value={job.id}>{job.id} · {job.item}</option>)}</select></label>
        <div className="cl-qr-placeholder">
          <Icon name="box" size={38} />
          <strong>QR scanning is not connected</strong>
          <span>Camera integration will submit the scanned token to ASP.NET.</span>
        </div>
        <label className="cl-field"><span>One-time code (demo format: 6 digits)</span><input value={code} onChange={event => setCode(event.target.value)} inputMode="numeric" pattern="[0-9]{6}" maxLength={6} required autoComplete="off" placeholder="Enter 6 digits" /></label>
        <label className="cl-field"><span>Handover proof (local filename preview only)</span><input type="file" accept="image/*" onChange={event => setProofName(event.target.files?.[0]?.name ?? '')} /></label>
        {proofName && <p className="cl-muted">Selected: {proofName}. Nothing is uploaded.</p>}
        <div className="cl-actions"><button className="cl-button cl-primary">Preview code submission</button></div>
      </form>
    </section>
    <section className="cl-panel cl-fit">
      <div className="cl-panel-heading"><h2>Collection milestones</h2></div>
      <div className="cl-panel-body">
        <Milestones status={jobs.find(job => job.id === handoverId)?.status} />
        <p className="cl-muted">Actual codes must be job-bound, short-lived, and verified by the backend. A browser entry cannot verify a handover.</p>
      </div>
    </section>
  </div>
}

function Milestones({ status }) {
  const current = milestones.indexOf(status)
  return <ol className="cl-milestones">{milestones.map((step, index) => <li key={step} className={index <= current ? 'is-done' : ''}><span>{index < current ? '✓' : index + 1}</span><div><strong>{step}</strong>{index === current && <small>Current sample status</small>}</div></li>)}</ol>
}
