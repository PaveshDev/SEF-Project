import { useEffect, useRef, useState } from 'react'
import { cancelCase, deleteCase, decideProposal, getCase, getOptions, listProposals, planCase, refreshProposal, submitProposal } from '../services/recoveryApi.js'
import { apiErrorMessage, optionUnavailable, proposalPayload, validDate } from '../services/recoveryWorkflow.js'

import { caseActions, humanize } from '../services/recoveryValidation.js'

const money = (value, currency = 'LKR') => value == null ? 'Unavailable' : `${currency} ${Number(value).toLocaleString()}`

export default function RecoveryWorkflow({ item: initialCase, onEdit, onBack, editing }) {
  const [item, setItem] = useState(initialCase)
  const [options, setOptions] = useState([])
  const [unavailableInputs, setUnavailableInputs] = useState([])
  const [selectedId, setSelectedId] = useState('')
  const [history, setHistory] = useState([])
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
    Promise.all([getCase(initialCase.id), getOptions(initialCase.id), listProposals(initialCase.id)]).then(([current, values, proposals]) => {
      if (!ignore) { setItem(current); setOptions(Array.isArray(values) ? values : []); setHistory(proposals); setProposal(proposals[0] ?? null); setProposalId(proposals[0]?.id ?? ''); setBusy(false) }
    }).catch((failure) => {
      if (!ignore) { setError(apiErrorMessage(failure)); setBlocked(true); setBusy(false) }
    })
    return () => { ignore = true }
  }, [initialCase.id])

  async function refresh(id = proposalId) {
    const [current, values, proposals] = await Promise.all([getCase(item.id), getOptions(item.id), listProposals(item.id)])
    if (current.id !== item.id || !Array.isArray(proposals) || proposals.some(p => p.caseId !== item.id)) throw new Error('Invalid workflow response')
    const loaded = proposals.find(p => p.id === id) ?? proposals[0] ?? null
    setItem(current); setOptions(Array.isArray(values) ? values : []); setHistory(proposals); setProposal(loaded); setProposalId(loaded?.id ?? '')
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
    const result = await planCase(item.id, { expectedVersion: item.version }, item.status !== 'Draft')
    if (result.case?.id !== item.id) throw new Error('Invalid planning response')
    setItem(result.case)
    setUnavailableInputs(Array.isArray(result.unavailableInputs) ? result.unavailableInputs.filter((value) => typeof value === 'string') : [])
    setOptions(Array.isArray(result.options) ? result.options : await getOptions(item.id))
    await refresh(''); setSelectedId('')
    setBlocked(false); setSuccess('Planning completed. Select an eligible option to prepare a proposal.')
  }

  const actions = caseActions(item.status)
  const pendingProposal = history.some(p => p.status === 'AwaitingApproval' && p.caseRevision === item.revision)
  const selected = options.find((option) => option.id === selectedId)
  const expiryDate = validDate(expiry)
  const canCreate = !busy && !editing && !blocked && !pendingProposal && selected && !optionUnavailable(selected, item) && expiryDate > new Date() && (!item.deadline || expiryDate <= validDate(item.deadline)) && explanation.trim() && explanation.trim().length <= 4000
  const proposalExpiry = validDate(proposal?.expiresAt)
  const canDecide = !busy && !editing && !blocked && proposal?.id === proposalId && proposal?.caseId === item.id && proposal?.caseRevision === item.revision && proposal?.version > 0 && proposal?.revision > 0 && proposal?.status === 'AwaitingApproval' && proposalExpiry > new Date()

  async function create(event) {
    event.preventDefault()
    if (!canCreate) return
    await run(async () => {
      const created = await submitProposal(item.id, proposalPayload(item, selected, expiry, explanation))
      if (!created?.id || created.caseId !== item.id || created.optionId !== selected.id) throw new Error('Invalid proposal response')
      setProposalId(created.id); setProposal(created); setSuccess('Proposal saved. Reload workflow if the following refresh fails.')
      await refresh(created.id)
      setSuccess('Proposal created and loaded for review.')
    })
  }

  async function decide(event) {
    event.preventDefault()
    if (!canDecide || (decision === 'RevisionRequested' && !comment.trim())) return
    await run(async () => {
      await decideProposal(proposal.id, { expectedVersion: proposal.version, proposalRevision: proposal.revision, decision, comment: comment.trim() || null })
      setSuccess('Decision saved. Reload workflow if the following refresh fails.')
      await refresh(proposal.id)
      setSuccess('Decision submitted. Proposal, case and options refreshed.')
    })
  }

  return <section className="detail-layout" aria-busy={busy}>
    <button className="back-button" disabled={busy || editing} onClick={onBack}>← All cases</button>
    <div className="detail-header"><div><span className="status">{humanize(item.status)}</span><h2>{item.objective}</h2><p className="muted">Case {item.id} · revision {item.revision} · version {item.version}</p></div><button className="button-muted" title={actions.edit ? 'Edit case inputs' : 'This case is closed to input changes.'} disabled={busy || editing || !actions.edit} onClick={() => onEdit(item)}>Edit inputs</button></div>
    <div className="workflow-actions">
      <button className="button-muted" disabled={busy || editing || blocked || !actions.cancel} onClick={() => { if (window.confirm('Cancel this recovery case? Pending proposals will become stale.')) run(async () => { await cancelCase(item.id, item.version); setSuccess('Case cancelled.'); await refresh() }) }}>Cancel case</button>
      <button className="button-muted" title="Cases with options or proposal history must be retained." disabled={busy || editing || blocked || !actions.delete || options.length > 0 || history.length > 0} onClick={() => { if (window.confirm('Permanently delete this case without history?')) run(async () => { await deleteCase(item.id); onBack() }) }}>Delete case</button>
    </div><p className="muted">Editing active inputs creates a new revision and invalidates existing options and pending proposals. Cases with history are retained.</p>
    <div className="metric-row"><div><span>Preferred routes</span><strong>{item.preferredRoutes?.join(' · ')}</strong></div><div><span>Pickup budget</span><strong>{money(item.maximumPickupCost, item.currency)}</strong></div><div><span>Deadline</span><strong>{validDate(item.deadline)?.toLocaleString() || 'Open'}</strong></div></div>
    {busy && <p role="status">Loading Recovery workflow…</p>}
    {error && <p className="inline-error" role="alert">{error} Reload workflow before continuing.</p>}
    {success && <p className="inline-success" role="status">{success}</p>}
    <section className="panel"><div className="panel-heading"><div><span className="eyebrow">1 · Recovery option selection</span><h2>Recovery options</h2></div><div className="workflow-actions"><button className="button-muted" disabled={busy || editing} onClick={() => run(() => refresh())}>Reload workflow</button><button className="button-primary" disabled={busy || editing || blocked || !actions.plan} onClick={() => run(plan)}>{item.status === 'Draft' ? 'Plan case' : 'Replan'}</button></div></div>
      {unavailableInputs.length > 0 && <div role="status"><strong>Unavailable planning inputs</strong><ul>{unavailableInputs.map((input, index) => <li key={index}>{input}</li>)}</ul></div>}
      {!options.length && !busy && <p className="muted">No options available. Run planning when the required inputs are available.</p>}
      <div className="option-grid">{options.map((option, index) => {
        const reason = optionUnavailable(option, item)
        return <article className={`option-card ${selectedId === option.id ? 'option-selected' : ''}`} key={option.id || index}>
          <div className="case-card-top"><strong>{option.route || 'Unknown route'}</strong><span className="status">{option.status || 'Unavailable'}</span></div>
          <p>{money(option.estimate?.valueLow, option.estimate?.currency)} – {money(option.estimate?.valueHigh, option.estimate?.currency)}</p>
          <FinancialBreakdown estimate={option.estimate} />
          <p className="muted">Match: {option.requiresPartner ? option.integration?.match?.response || 'Unavailable' : 'Not required'} · Pickup: {option.requiresPickup ? option.integration?.pickup?.feasibility || 'Unavailable' : 'Not required'}</p>
          <details><summary>Evidence ({option.evidence?.length || 0})</summary><ul>{option.evidence?.map((entry, i) => <li key={i}>{entry.sourceName || 'Unnamed source'} · {validDate(entry.observedAt)?.toLocaleDateString() || 'Date unavailable'}</li>)}</ul></details>
          {reason && <p className="muted">{reason}</p>}
          <button className="text-button" aria-pressed={selectedId === option.id} disabled={busy || editing || blocked || pendingProposal || !!reason} onClick={() => setSelectedId(option.id)}>{selectedId === option.id ? 'Selected' : `Select ${option.route || 'option'}`}</button>
        </article>
      })}</div>
    </section>
    <section className="panel"><span className="eyebrow">2 · Proposal creation</span><h2>Prepare proposal</h2><p>{proposalId ? 'This proposal has been submitted for review.' : selected ? `Selected route: ${selected.route}` : 'Select a validated option above.'}</p>
      <form onSubmit={create}><label className="proposal-comment">Proposal expiry (local time; no later than the case deadline)<input type="datetime-local" required max={item.deadline ? new Date(new Date(item.deadline).getTime() - new Date(item.deadline).getTimezoneOffset() * 60000).toISOString().slice(0, 16) : undefined} value={expiry} disabled={busy || pendingProposal} onChange={(event) => setExpiry(event.target.value)} /></label><label className="proposal-comment">Explanation<textarea required maxLength={4000} value={explanation} disabled={busy || pendingProposal} onChange={(event) => setExplanation(event.target.value)} /></label><button className="button-primary" disabled={!canCreate}>Create proposal</button></form>
      {proposalId && <p role="status">Created proposal {proposalId}. Replan to prepare another option.</p>}
    </section>
    <section className="panel"><span className="eyebrow">3 · Human approval</span><h2>Proposal review</h2>
      {history.length > 0 && <label className="proposal-comment">Proposal history<select aria-label="Proposal history" value={proposalId} disabled={busy} onChange={event => { const loaded = history.find(p => p.id === event.target.value); setProposalId(loaded.id); setProposal(loaded) }}>{history.map(p => <option key={p.id} value={p.id}>Revision {p.revision} · {humanize(p.status)}</option>)}</select></label>}
      {proposal?.status === 'AwaitingApproval' && <button className="button-muted" disabled={busy || editing} onClick={() => run(async () => { const result = await refreshProposal(proposal.id); setProposal(result); setSuccess('Proposal evidence revalidated.'); await refresh(proposal.id) })}>Revalidate proposal</button>}
      {!proposal && <p className="muted">{proposalId ? 'Proposal details could not be loaded. Use Reload workflow to retry.' : 'Create a proposal from a selected option to review it here.'}</p>}
      {proposal && <><span className="status">{proposal.status}</span><div className="proposal-summary"><span>Proposal {proposal.id}</span><span>Case {proposal.caseId}</span><span>Option {proposal.optionId}</span><span>Revision {proposal.revision} · Version {proposal.version}</span><span>Expires {proposalExpiry?.toLocaleString() || 'Date unavailable'}</span><span>Match {proposal.matchId || 'Not linked'}</span><span>Pickup {proposal.pickupPlanId || 'Not linked'}</span></div>
        <p>{proposal.explanation || 'No explanation supplied.'}</p><FinancialBreakdown estimate={proposal.estimate} /><p>Value {money(proposal.estimate?.valueLow, proposal.estimate?.currency)} – {money(proposal.estimate?.valueHigh, proposal.estimate?.currency)} · Net {money(proposal.estimate?.netValue, proposal.estimate?.currency)}</p>
        {!canDecide && !busy && <p className="muted">Decisions require a current proposal awaiting approval with an available future expiry.</p>}
        <form onSubmit={decide}><label className="proposal-comment">Decision comment<textarea value={comment} onChange={(event) => setComment(event.target.value)} required={decision === 'RevisionRequested'} maxLength={2000} disabled={!canDecide} /></label><div className="proposal-form"><select aria-label="Decision" value={decision} disabled={!canDecide} onChange={(event) => setDecision(event.target.value)}><option>Approved</option><option>Rejected</option><option>RevisionRequested</option></select><button className="button-primary" disabled={!canDecide || (decision === 'RevisionRequested' && !comment.trim())}>Submit decision</button></div></form>
      </>}
    </section>
  </section>
}

function FinancialBreakdown({ estimate }) {
  if (!estimate) return <p>Estimate unavailable.</p>
  return <dl className="financial-breakdown">{[['Value / proceeds (low)', estimate.valueLow], ['Value / proceeds (high)', estimate.valueHigh], ['Repair cost', estimate.repairCost], ['Pickup cost', estimate.pickupCost], ['Total costs', estimate.totalCost], ['Net value', estimate.netValue], ['Shortfall', estimate.shortfall]].map(([label, value]) => <div key={label} style={{ display: 'contents' }}><dt>{label}</dt><dd>{money(value, estimate.currency)}</dd></div>)}</dl>
}
