import { useCallback, useEffect, useEffectEvent, useMemo, useRef, useState } from 'react'
import { createCase, createReference, listCases, listReferences, updateCase } from '../services/recoveryApi.js'
import '../styles/recovery.css'
import RecoveryWorkflow from './RecoveryWorkflow.jsx'
import { apiErrorMessage, localDateInput } from '../services/recoveryWorkflow.js'

const routes = ['Reuse', 'Donate', 'RepairThenReuse', 'Resell', 'Recycle']
const statuses = ['Draft', 'Planning', 'AwaitingInputs', 'AwaitingApproval', 'Approved', 'Rejected', 'RevisionRequested', 'Completed', 'Failed', 'Cancelled']

const fallbackPage = { items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 }

function formatMoney(value, currency = 'LKR') {
  if (value === null || value === undefined) return 'Not estimated'
  return `${currency} ${Number(value).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function statusClass(status = '') {
  return `status status-${status.toLowerCase().replaceAll('_', '-')}`
}

function ErrorState({ message, onRetry }) {
  return <div className="state state-error" role="alert"><strong>Could not load Recovery data.</strong><span>{message}</span><button onClick={onRetry}>Retry</button></div>
}

function EmptyState({ title, body }) {
  return <div className="state state-empty"><strong>{title}</strong><span>{body}</span></div>
}

function CaseForm({ selected, onSaved, onCancel }) {
  const [objective, setObjective] = useState(selected?.objective || '')
  const [currency, setCurrency] = useState(selected?.currency || 'LKR')
  const [budget, setBudget] = useState(selected?.maximumPickupCost ?? '')
  const [route, setRoute] = useState(selected?.preferredRoutes?.[0] || 'Reuse')
  const [itemId, setItemId] = useState(selected?.itemId || '')
  const [deadline, setDeadline] = useState(() => localDateInput(selected?.deadline))
  const [state, setState] = useState('idle')
  const [error, setError] = useState('')

  const saving = useRef(false)

  async function submit(event) {
    event.preventDefault()
    if (saving.current) return
    saving.current = true
    setState('loading')
    setError('')
    try {
      const payload = {
        itemId,
        inputs: {
          objective,
          preferredRoutes: [route],
          currency,
          maximumPickupCost: budget === '' ? null : Number(budget),
          deadline: deadline ? new Date(deadline).toISOString() : null,
        },
      }
      const result = selected ? await onSaved(selected.id, payload, selected.version) : await onSaved(null, payload)
      if (result) setState('success')
    } catch (requestError) {
      setState('error')
      setError(apiErrorMessage(requestError, 'Check the fields and try again.'))
    } finally {
      saving.current = false
    }
  }

  return <form className="form-panel" onSubmit={submit}>
    <div className="panel-heading"><div><span className="eyebrow">{selected ? 'Edit case' : 'New case'}</span><h2>{selected ? 'Refine recovery intent' : 'Start a recovery case'}</h2></div><button type="button" className="icon-button" aria-label="Close form" disabled={state === 'loading'} onClick={onCancel}>×</button></div>
    <label>Item identifier<input required disabled={!!selected} value={itemId} onChange={(event) => setItemId(event.target.value)} placeholder="UUID from Items" /></label>
    <label>Objective<textarea aria-label="Objective" required maxLength="1000" value={objective} onChange={(event) => setObjective(event.target.value)} placeholder="What should happen to this item?" /></label>
    <div className="form-grid">
      <label>Preferred route<select value={route} onChange={(event) => setRoute(event.target.value)}>{routes.map((item) => <option key={item}>{item}</option>)}</select></label>
      <label>Currency<input required maxLength="3" value={currency} onChange={(event) => setCurrency(event.target.value.toUpperCase())} /></label>
      <label>Pickup budget<input type="number" min="0" step="0.01" value={budget} onChange={(event) => setBudget(event.target.value)} placeholder="Optional" /></label>
      <label>Deadline<input type="datetime-local" value={deadline} onChange={(event) => setDeadline(event.target.value)} /></label>
    </div>
    {state === 'error' && <p className="inline-error" role="alert">{error}</p>}
    {state === 'success' && <p className="inline-success" role="status">Case saved.</p>}
    <div className="form-actions"><button type="button" className="button-muted" disabled={state === 'loading'} onClick={onCancel}>Cancel</button><button className="button-primary" disabled={state === 'loading'}>{state === 'loading' ? 'Saving…' : 'Save case'}</button></div>
  </form>
}

function CaseCard({ item, onSelect, onPlan }) {
  return <article className="case-card" onClick={() => onSelect(item)}>
    <div className="case-card-top"><span className={statusClass(item.status)}>{item.status}</span><span className="case-id">#{item.id?.slice(0, 8)}</span></div>
    <h3>{item.objective}</h3>
    <p className="muted">{item.preferredRoutes?.join(' · ') || 'No route selected'} · {item.currency}</p>
    <div className="case-card-meta"><span>Revision {item.revision}</span><span>{formatMoney(item.maximumPickupCost, item.currency)} budget</span></div>
    <button className="text-button" onClick={(event) => { event.stopPropagation(); onPlan(item) }}>Open planning →</button>
  </article>
}

function ReferencePanel({ onCreated }) {
  const [sourceName, setSourceName] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [valueLow, setValueLow] = useState('')
  const [valueHigh, setValueHigh] = useState('')
  const [state, setState] = useState('idle')
  async function submit(event) { event.preventDefault(); setState('loading'); try { await createReference({ categoryId, condition: 'Good', route: 'Reuse', valueLow: Number(valueLow), valueHigh: Number(valueHigh), currency: 'LKR', sourceName, observedAt: new Date().toISOString() }); setState('success'); onCreated() } catch { setState('error') } }
  return <section className="panel reference-panel"><div className="panel-heading"><div><span className="eyebrow">Curator workspace</span><h2>Value references</h2></div><span className="role-chip">Authorized staff</span></div><p className="muted">Add verified market evidence. References are kept separate from social and environmental benefits.</p><form className="reference-form" onSubmit={submit}><input required value={categoryId} onChange={(event) => setCategoryId(event.target.value)} placeholder="Category UUID" aria-label="Category identifier" /><input required value={sourceName} onChange={(event) => setSourceName(event.target.value)} placeholder="Source name" aria-label="Source name" /><input required type="number" min="0" step="0.01" value={valueLow} onChange={(event) => setValueLow(event.target.value)} placeholder="Low value" aria-label="Low value" /><input required type="number" min="0" step="0.01" value={valueHigh} onChange={(event) => setValueHigh(event.target.value)} placeholder="High value" aria-label="High value" /><button className="button-primary" disabled={state === 'loading'}>{state === 'loading' ? 'Adding…' : 'Add reference'}</button></form>{state === 'success' && <p className="inline-success" role="status">Reference submitted for verification.</p>}{state === 'error' && <p className="inline-error" role="alert">Reference could not be saved. Staff authorization may be required.</p>}</section>
}

function AgentPanel() {
  return <section className="panel agent-panel"><div className="panel-heading"><div><span className="eyebrow">Auditable workflow</span><h2>Planner agent monitor</h2></div><span className="status status-awaitingapproval">Awaiting approval</span></div><div className="workflow"><span className="done">Assessment confirmed</span><span className="done">References checked</span><span className="done">Alternatives valued</span><span className="done">Dependencies checked</span><span className="current">Human approval</span></div><p className="muted">The planner can recommend routes and prepare a proposal. It cannot approve itself, invent prices, or reserve pickup.</p></section>
}

export default function RecoveryPage() {
  const [activeTab, setActiveTab] = useState('cases')
  const [cases, setCases] = useState(fallbackPage)
  const [references, setReferences] = useState(fallbackPage)
  const [selected, setSelected] = useState(null)
  const [editingCase, setEditingCase] = useState(null)
  const [formOpen, setFormOpen] = useState(false)
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [sort, setSort] = useState('createdAt')
  const [page, setPage] = useState(1)
  const [loadState, setLoadState] = useState('loading')
  const [error, setError] = useState('')
  const params = useMemo(() => ({ search: search || undefined, status: status || undefined, sortBy: sort, sortDirection: 'desc', page, pageSize: 12 }), [search, status, sort, page])
  const loadCases = useCallback(async () => { setLoadState('loading'); try { setCases(await listCases(params)); setLoadState('ready'); setError('') } catch (requestError) { setLoadState('error'); setError(apiErrorMessage(requestError, 'The Recovery API is unavailable.')) } }, [params])
  const loadReferences = useCallback(async () => { try { setReferences(await listReferences({ page: 1, pageSize: 12, sortBy: 'observedAt', sortDirection: 'desc' })) } catch { setReferences(fallbackPage) } }, [])
  const loadCasesEvent = useEffectEvent(loadCases)
  const loadReferencesEvent = useEffectEvent(loadReferences)
  useEffect(() => { queueMicrotask(loadCasesEvent) }, [params])
  useEffect(() => { if (activeTab === 'references') queueMicrotask(loadReferencesEvent) }, [activeTab])
  async function saveCase(id, payload, version) { const result = id ? await updateCase(id, { expectedVersion: version, inputs: payload.inputs }) : await createCase(payload); setFormOpen(false); if (id) setSelected(result); await loadCases(); return result }
  if (selected) return <main className="recovery-shell"><RecoveryWorkflow key={selected.id + ":" + selected.version} item={selected} editing={formOpen} onEdit={(item) => { setEditingCase(item); setFormOpen(true) }} onBack={() => { setSelected(null); loadCases() }} />{formOpen && <CaseForm selected={editingCase || selected} onSaved={saveCase} onCancel={() => { setFormOpen(false); setEditingCase(null) }} />}</main>
  return <main className="recovery-shell"><header className="recovery-header"><div><span className="eyebrow">Member 2 · Recovery planning</span><h1>Recovery command center</h1><p>Turn confirmed assessments into transparent, approval-ready recovery decisions.</p></div><button className="button-primary" onClick={() => setFormOpen(true)}>+ New recovery case</button></header><nav className="tab-bar" aria-label="Recovery views">{[['cases', 'Cases'], ['references', 'Value references'], ['proposal', 'Proposal review'], ['agent', 'Agent monitor']].map(([key, label]) => <button key={key} className={activeTab === key ? 'active' : ''} onClick={() => setActiveTab(key)}>{label}</button>)}</nav>{activeTab === 'cases' && <section><div className="toolbar"><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1) }} placeholder="Search objectives" aria-label="Search recovery cases" /><select value={status} onChange={(event) => { setStatus(event.target.value); setPage(1) }} aria-label="Filter by status"><option value="">All statuses</option>{statuses.map((item) => <option key={item}>{item}</option>)}</select><select value={sort} onChange={(event) => setSort(event.target.value)} aria-label="Sort cases"><option value="createdAt">Newest</option><option value="updatedAt">Recently updated</option><option value="status">Status</option></select></div>{loadState === 'error' && <ErrorState message={error} onRetry={loadCases} />}{loadState === 'loading' && <div className="state">Loading cases…</div>}{loadState === 'ready' && !cases.items.length && <EmptyState title="No recovery cases yet" body="Create a case to begin planning from a confirmed assessment." />}{loadState === 'ready' && <div className="case-grid">{cases.items.map((item) => <CaseCard key={item.id} item={item} onSelect={setSelected} onPlan={setSelected} />)}</div>}<div className="pagination"><span>{cases.totalCount || 0} cases</span><button disabled={page <= 1} onClick={() => setPage(page - 1)}>Previous</button><span>Page {page} of {cases.totalPages || 1}</span><button disabled={page >= (cases.totalPages || 1)} onClick={() => setPage(page + 1)}>Next</button></div></section>}{activeTab === 'references' && <><ReferencePanel onCreated={loadReferences} />{references.items?.length ? <div className="reference-table">{references.items.map((reference) => <div className="reference-row" key={reference.id}><strong>{reference.sourceName}</strong><span>{reference.currency} {reference.valueLow}–{reference.valueHigh}</span><span>{reference.route}</span><span>{reference.observedAt ? new Date(reference.observedAt).toLocaleDateString() : 'Unknown date'}</span><span className={statusClass(reference.isVerified ? 'Verified' : 'Pending')}>{reference.isVerified ? 'Verified' : 'Pending'}</span></div>)}</div> : <EmptyState title="No references visible" body="Authorized references will appear here after the API responds." />}</>}{activeTab === 'proposal' && <EmptyState title="Select a recovery case" body="Open a case, plan its options, and create a proposal to review." />}{activeTab === 'agent' && <AgentPanel />}{formOpen && !selected && <CaseForm onSaved={saveCase} onCancel={() => setFormOpen(false)} />}</main>
}
