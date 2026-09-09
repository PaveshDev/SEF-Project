import { useEffect, useRef, useState } from 'react'
import { decideProposal, getCase, getOptions, getProposal, planCase, submitProposal } from '../services/recoveryApi.js'
import { apiErrorMessage, optionUnavailable, proposalPayload, validDate } from '../services/recoveryWorkflow.js'

const money = (value, currency = 'LKR') => value == null ? 'Unavailable' : `${currency} ${Number(value).toLocaleString()}`

export default function RecoveryWorkflow({ item: initialCase, onEdit, onBack, editing }) {
  const [item, setItem] = useState(initialCase)
  const [options, setOptions] = useState([])
  const [unavailableInputs, setUnavailableInputs] = useState([])
  const [selectedId, setSelectedId] = useState('')
  const [proposalId, setProposalId] = useState('')
  const [proposal, setProposal] = useState(null)
  const [busy, setBusy] = useState(true)
  const [blocked, setBlocked] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [expiry, setExpiry] = useState('')
  const [explanation, setExplanation] = useState('')
  const [decision, setDecision] = useState('Approved')
  const [comment, setComment] = useState('')
  const running = useRef(false)

  useEffect(() => {
    let ignore = false
    Promise.all([getCase(initialCase.id), getOptions(initialCase.id)]).then(([current, values]) => {
      if (!ignore) { setItem(current); setOptions(Array.isArray(values) ? values : []); setBusy(false) }
    }).catch((failure) => {
      if (!ignore) { setError(apiErrorMessage(failure)); setBlocked(true); setBusy(false) }
    })
    return () => { ignore = true }
  }, [initialCase.id])

  async function refresh(id = proposalId) {
    const [current, values, loaded] = await Promise.all([getCase(item.id), getOptions(item.id), id ? getProposal(id) : null])
    if (current.id !== item.id || (loaded && (loaded.id !== id || loaded.caseId !== current.id))) throw new Error('Invalid workflow response')
    setItem(current); setOptions(Array.isArray(values) ? values : []); setProposal(loaded)
    setSelectedId(''); setBlocked(false)
  }

  async function run(action) {
    if (running.current || busy || editing) return
    running.current = true; setBusy(true); setError(''); setSuccess('')
    try { await action() } catch (failure) {
      setBlocked(true); setSelectedId(''); setError(apiErrorMessage(failure))
    } finally { running.current = false; setBusy(false) }
  }

  async function plan() {
    setOptions([]); setSelectedId(''); setProposal(null); setProposalId(''); setUnavailableInputs([])
    const result = await planCase(item.id, { expectedVersion: item.version }, item.status !== 'Draft')
    if (result.case?.id !== item.id) throw new Error('Invalid planning response')
    setItem(result.case)
    setUnavailableInputs(Array.isArray(result.unavailableInputs) ? result.unavailableInputs.filter((value) => typeof value === 'string') : [])
    setOptions(Array.isArray(result.options) ? result.options : await getOptions(item.id))
    setBlocked(false); setSuccess('Planning completed. Select an eligible option to prepare a proposal.')
  }

  const selected = options.find((option) => option.id === selectedId)
  const expiryDate = validDate(expiry)
  const canCreate = !busy && !editing && !blocked && !proposalId && selected && !optionUnavailable(selected, item) && expiryDate > new Date() && explanation.trim()
  const proposalExpiry = validDate(proposal?.expiresAt)
  const canDecide = !busy && !editing && !blocked && proposal?.id === proposalId && proposal?.caseId === item.id && proposal?.caseRevision === item.revision && proposal?.version > 0 && proposal?.revision > 0 && proposal?.status === 'AwaitingApproval' && proposalExpiry > new Date()

  async function create(event) {
    event.preventDefault()
    if (!canCreate) return
    await run(async () => {
      const created = await submitProposal(item.id, proposalPayload(item, selected, expiry, explanation))
      if (!created?.id || created.caseId !== item.id || created.optionId !== selected.id) throw new Error('Invalid proposal response')
      setProposalId(created.id)
      await refresh(created.id)
      setSuccess('Proposal created and loaded for review.')
    })
  }

  async function decide(event) {
    event.preventDefault()
    if (!canDecide || (decision === 'RevisionRequested' && !comment.trim())) return
    await run(async () => {
      await decideProposal(proposal.id, { expectedVersion: proposal.version, proposalRevision: proposal.revision, decision, comment: comment.trim() || null })
      await refresh(proposal.id)
      setSuccess('Decision submitted. Proposal, case and options refreshed.')
    })
  }

  return <section className="detail-layout" aria-busy={busy}>
    <button className="back-button" disabled={busy || editing} onClick={onBack}>← All cases</button>
    <div className="detail-header"><div><span className="status">{item.status}</span><h2>{item.objective}</h2><p className="muted">Case {item.id} · revision {item.revision} · version {item.version}</p></div><button className="button-muted" disabled={busy || editing} onClick={() => onEdit(item)}>Edit inputs</button></div>
    <div className="metric-row"><div><span>Preferred routes</span><strong>{item.preferredRoutes?.join(' · ')}</strong></div><div><span>Pickup budget</span><strong>{money(item.maximumPickupCost, item.currency)}</strong></div><div><span>Deadline</span><strong>{validDate(item.deadline)?.toLocaleString() || 'Open'}</strong></div></div>
    {busy && <p role="status">Loading Recovery workflow…</p>}
    {error && <p className="inline-error" role="alert">{error} Reload workflow before continuing.</p>}
    {success && <p className="inline-success" role="status">{success}</p>}
    <section className="panel"><div className="panel-heading"><div><span className="eyebrow">1 · Recovery option selection</span><h2>Recovery options</h2></div><div className="workflow-actions"><button className="button-muted" disabled={busy || editing} onClick={() => run(() => refresh())}>Reload workflow</button><button className="button-primary" disabled={busy || editing || blocked} onClick={() => run(plan)}>{item.status === 'Draft' ? 'Plan case' : 'Replan'}</button></div></div>
      {unavailableInputs.length > 0 && <div role="status"><strong>Unavailable planning inputs</strong><ul>{unavailableInputs.map((input, index) => <li key={index}>{input}</li>)}</ul></div>}
      {!options.length && !busy && <p className="muted">No options available. Run planning when the required inputs are available.</p>}
      <div className="option-grid">{options.map((option, index) => {
        const reason = optionUnavailable(option, item)
        return <article className={`option-card ${selectedId === option.id ? 'option-selected' : ''}`} key={option.id || index}>
          <div className="case-card-top"><strong>{option.route || 'Unknown route'}</strong><span className="status">{option.status || 'Unavailable'}</span></div>
          <p>{money(option.estimate?.valueLow, option.estimate?.currency)} – {money(option.estimate?.valueHigh, option.estimate?.currency)}</p>
          <p className="muted">Repair {money(option.estimate?.repairCost, option.estimate?.currency)} · Pickup {money(option.estimate?.pickupCost, option.estimate?.currency)} · Net {money(option.estimate?.netValue, option.estimate?.currency)}</p>
          <p className="muted">Match: {option.integration?.match?.response || 'Unavailable'} · Pickup: {option.integration?.pickup?.feasibility || 'Unavailable'}</p>
          <details><summary>Evidence ({option.evidence?.length || 0})</summary><ul>{option.evidence?.map((entry, i) => <li key={i}>{entry.sourceName || 'Unnamed source'} · {validDate(entry.observedAt)?.toLocaleDateString() || 'Date unavailable'}</li>)}</ul></details>
          {reason && <p className="muted">{reason}</p>}
          <button className="text-button" aria-pressed={selectedId === option.id} disabled={busy || editing || blocked || !!proposalId || !!reason} onClick={() => setSelectedId(option.id)}>{selectedId === option.id ? 'Selected' : `Select ${option.route || 'option'}`}</button>
        </article>
      })}</div>
    </section>
    <section className="panel"><span className="eyebrow">2 · Proposal creation</span><h2>Prepare proposal</h2><p>{proposalId ? 'This proposal has been submitted for review.' : selected ? `Selected route: ${selected.route}` : 'Select a validated option above.'}</p>
      <form onSubmit={create}><label className="proposal-comment">Proposal expiry<input type="datetime-local" required value={expiry} disabled={busy || !!proposalId} onChange={(event) => setExpiry(event.target.value)} /></label><label className="proposal-comment">Explanation<textarea required maxLength={4000} value={explanation} disabled={busy || !!proposalId} onChange={(event) => setExplanation(event.target.value)} /></label><button className="button-primary" disabled={!canCreate}>Create proposal</button></form>
      {proposalId && <p role="status">Created proposal {proposalId}. Replan to prepare another option.</p>}
    </section>
    <section className="panel"><span className="eyebrow">3 · Human approval</span><h2>Proposal review</h2>
      {!proposal && <p className="muted">{proposalId ? 'Proposal details could not be loaded. Use Reload workflow to retry.' : 'Create a proposal from a selected option to review it here.'}</p>}
      {proposal && <><span className="status">{proposal.status}</span><div className="proposal-summary"><span>Proposal {proposal.id}</span><span>Case {proposal.caseId}</span><span>Option {proposal.optionId}</span><span>Revision {proposal.revision} · Version {proposal.version}</span><span>Expires {proposalExpiry?.toLocaleString() || 'Date unavailable'}</span><span>Match {proposal.matchId || 'Not linked'}</span><span>Pickup {proposal.pickupPlanId || 'Not linked'}</span></div>
        <p>{proposal.explanation || 'No explanation supplied.'}</p><p>Value {money(proposal.estimate?.valueLow, proposal.estimate?.currency)} – {money(proposal.estimate?.valueHigh, proposal.estimate?.currency)} · Net {money(proposal.estimate?.netValue, proposal.estimate?.currency)}</p>
        {!canDecide && !busy && <p className="muted">Decisions require a current proposal awaiting approval with an available future expiry.</p>}
        <form onSubmit={decide}><label className="proposal-comment">Decision comment<textarea value={comment} onChange={(event) => setComment(event.target.value)} required={decision === 'RevisionRequested'} maxLength={2000} disabled={!canDecide} /></label><div className="proposal-form"><select aria-label="Decision" value={decision} disabled={!canDecide} onChange={(event) => setDecision(event.target.value)}><option>Approved</option><option>Rejected</option><option>RevisionRequested</option></select><button className="button-primary" disabled={!canDecide || (decision === 'RevisionRequested' && !comment.trim())}>Submit decision</button></div></form>
      </>}
    </section>
  </section>
}
