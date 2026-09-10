import { useCallback, useEffect, useEffectEvent, useMemo, useRef, useState } from 'react'
import { createCase, listCases, updateCase } from '../services/recoveryApi.js'
import '../styles/recovery.css'
import RecoveryWorkflow from './RecoveryWorkflow.jsx'
import { apiErrorMessage } from '../services/recoveryWorkflow.js'

import CaseForm from './CaseForm.jsx'
import ReferencePanel from './ReferencePanel.jsx'
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

function CaseCard({ item, onSelect, onPlan }) {
  return <article className="case-card" onClick={() => onSelect(item)}>
    <div className="case-card-top"><span className={statusClass(item.status)}>{item.status}</span><span className="case-id">#{item.id?.slice(0, 8)}</span></div>
    <h3>{item.objective}</h3>
    <p className="muted">{item.preferredRoutes?.join(' · ') || 'No route selected'} · {item.currency}</p>
    <div className="case-card-meta"><span>Revision {item.revision}</span><span>{formatMoney(item.maximumPickupCost, item.currency)} budget</span></div>
    <button className="text-button" onClick={(event) => { event.stopPropagation(); onPlan(item) }}>Open planning →</button>
  </article>
}

function AgentPanel() {
  return <section className="panel"><h2>Planner agent monitor</h2><p role="status">Agent integration unavailable.</p><p className="muted">Production hosting and durable run status are awaiting shared integration. You can review available case options and proposal history from Cases.</p></section>
}

export default function RecoveryPage() {
  const [activeTab, setActiveTab] = useState('cases')
  const [cases, setCases] = useState(fallbackPage)
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
  const requestSequence = useRef(0)
  const loadCases = useCallback(async () => {
    const sequence = ++requestSequence.current
    setLoadState('loading')
    try { const result = await listCases(params); if (sequence !== requestSequence.current) return; setCases(result); setLoadState('ready'); setError('') }
    catch (failure) { if (sequence !== requestSequence.current) return; setLoadState('error'); setError(apiErrorMessage(failure, 'The Recovery API is unavailable.')) }
  }, [params])
  const loadCasesEvent = useEffectEvent(loadCases)
  useEffect(() => { queueMicrotask(loadCasesEvent) }, [params])
  async function saveCase(id, payload, version) { const result = id ? await updateCase(id, { expectedVersion: version, inputs: payload.inputs }) : await createCase(payload); setFormOpen(false); if (id) setSelected(result); await loadCases(); return result }
  if (selected) return <main className="recovery-shell"><RecoveryWorkflow key={selected.id + ":" + selected.version} item={selected} editing={formOpen} onEdit={(item) => { setEditingCase(item); setFormOpen(true) }} onBack={() => { setSelected(null); loadCases() }} />{formOpen && <CaseForm selected={editingCase || selected} onSaved={saveCase} onCancel={() => { setFormOpen(false); setEditingCase(null) }} />}</main>
  return <main className="recovery-shell"><header className="recovery-header"><div><span className="eyebrow">Member 2 · Recovery planning</span><h1>Recovery planning</h1><p>Turn confirmed assessments into transparent, approval-ready recovery decisions.</p></div><button className="button-primary" onClick={() => setFormOpen(true)}>+ New recovery case</button></header><nav className="tab-bar" aria-label="Recovery views">{[['cases', 'Cases'], ['references', 'Value references'], ['proposal', 'Proposal review'], ['agent', 'Agent monitor']].map(([key, label]) => <button key={key} className={activeTab === key ? 'active' : ''} onClick={() => setActiveTab(key)}>{label}</button>)}</nav>{activeTab === 'cases' && <section><div className="toolbar"><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1) }} placeholder="Search objectives" aria-label="Search recovery cases" /><select value={status} onChange={(event) => { setStatus(event.target.value); setPage(1) }} aria-label="Filter by status"><option value="">All statuses</option>{statuses.map((item) => <option key={item}>{item}</option>)}</select><select value={sort} onChange={(event) => setSort(event.target.value)} aria-label="Sort cases"><option value="createdAt">Newest</option><option value="updatedAt">Recently updated</option><option value="status">Status</option></select></div>{loadState === 'error' && <ErrorState message={error} onRetry={loadCases} />}{loadState === 'loading' && <div className="state">Loading cases…</div>}{loadState === 'ready' && !cases.items.length && <EmptyState title="No recovery cases yet" body="Create a case to begin planning from a confirmed assessment." />}{loadState === 'ready' && <div className="case-grid">{cases.items.map((item) => <CaseCard key={item.id} item={item} onSelect={setSelected} onPlan={setSelected} />)}</div>}<div className="pagination"><span>{cases.totalCount || 0} cases</span><button disabled={page <= 1} onClick={() => setPage(page - 1)}>Previous</button><span>Page {page} of {cases.totalPages || 1}</span><button disabled={page >= (cases.totalPages || 1)} onClick={() => setPage(page + 1)}>Next</button></div></section>}{activeTab === 'references' && <ReferencePanel />}{activeTab === 'proposal' && <><EmptyState title="Select a recovery case" body="Open a case to review its current proposal and previous revisions." /><button className="button-muted" onClick={() => setActiveTab('cases')}>Browse cases</button></>}{activeTab === 'agent' && <AgentPanel />}{formOpen && !selected && <CaseForm onSaved={saveCase} onCancel={() => setFormOpen(false)} />}</main>
}
