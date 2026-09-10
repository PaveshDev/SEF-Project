import { useCallback, useEffect, useEffectEvent, useRef, useState } from 'react'
import { createReference, deleteReference, getAccess, listReferences, updateReference, verifyReference } from '../services/recoveryApi.js'
import { apiErrorMessage, localDateInput } from '../services/recoveryWorkflow.js'
import { fieldErrors, humanize, referenceErrors, routes } from '../services/recoveryValidation.js'

const empty = () => ({ categoryId: '', condition: 'Good', route: 'Reuse', currency: 'LKR', valueLow: '', valueHigh: '', sourceName: '', sourceReference: '', observedAt: localDateInput(new Date()) })
export default function ReferencePanel() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [route, setRoute] = useState('')
  const [data, setData] = useState(null)
  const [curator, setCurator] = useState(false)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [selected, setSelected] = useState(null)
  const [values, setValues] = useState(empty)
  const [errors, setErrors] = useState({})
  const sequence = useRef(0)
  const running = useRef(false)
  const load = useCallback(async () => {
    const current = ++sequence.current
    setLoading(true); setError('')
    try {
      const [result, access] = await Promise.all([listReferences({ page, pageSize: 12, search: search || undefined, route: route || undefined }), getAccess()])
      if (current !== sequence.current) return
      setData(result); setCurator(access.canManageValueReferences === true)
    } catch (failure) { if (current === sequence.current) { setError(apiErrorMessage(failure)); setCurator(false) } }
    finally { if (current === sequence.current) setLoading(false) }
  }, [page, search, route])
  const loadEvent = useEffectEvent(load)
  useEffect(() => { queueMicrotask(loadEvent) }, [load])
  const change = (key, value) => setValues(old => ({ ...old, [key]: value }))
  async function run(action, message) {
    if (running.current) return
    running.current = true; setBusy(true); setError(''); setNotice('')
    try { await action(); setNotice(message); await load() }
    catch (failure) { setErrors(fieldErrors(failure)); setError(apiErrorMessage(failure, 'The operation could not be confirmed. Reload, or retry the same submission.')) }
    finally { running.current = false; setBusy(false) }
  }
  async function save(event) {
    event.preventDefault()
    const invalid = referenceErrors(values); setErrors(invalid)
    if (Object.keys(invalid).length) return
    await run(async () => {
      const payload = { ...values, sourceName: values.sourceName.trim(), sourceReference: values.sourceReference.trim() || null, observedAt: new Date(values.observedAt).toISOString(), valueLow: Number(values.valueLow), valueHigh: Number(values.valueHigh) }
      if (selected) await updateReference(selected.id, { ...payload, expectedVersion: selected.version })
      else await createReference(payload)
      setSelected(null); setValues(empty())
    }, 'Reference saved. Verified evidence is immutable.')
  }
  const field = (key, label, type = 'text', maxLength) => <label>{label}<input type={type} maxLength={maxLength} disabled={key === 'categoryId' && !!selected} value={values[key]} aria-invalid={!!errors[key]} aria-describedby={errors[key] ? `reference-${key}-error` : undefined} onChange={e => change(key, key === 'currency' ? e.target.value.toUpperCase() : e.target.value)} />{errors[key] && <small className="field-error" id={`reference-${key}-error`}>{errors[key]}</small>}</label>
  return <section className="panel reference-panel"><h2>Value references</h2>
    <p className="muted">Verified evidence supports estimates. Only authorized curators can change unverified references. Category lookup is awaiting the Items integration.</p>
    <div className="toolbar"><input aria-label="Search references" placeholder="Search sources" value={search} disabled={busy} onChange={e => { setSearch(e.target.value); setPage(1) }} /><select aria-label="Reference route" value={route} disabled={busy} onChange={e => { setRoute(e.target.value); setPage(1) }}><option value="">All routes</option>{routes.map(x => <option key={x} value={x}>{humanize(x)}</option>)}</select></div>
    {notice && <p role="status" className="inline-success">{notice}</p>}
    {error && <div role="alert" className="inline-error">{error} <button disabled={busy} onClick={load}>Retry loading references</button></div>}
    {loading && <p role="status">Loading references and permissions…</p>}
    {!loading && !error && !data?.items?.length && <p>No references match these filters.</p>}
    {!loading && !error && <><div className="reference-table">{data?.items?.map(item => <article className="reference-row" key={item.id}><strong>{item.sourceName}</strong><span>{item.currency} {item.valueLow}–{item.valueHigh}</span><span>{humanize(item.route)} · {item.condition}</span><span>{new Date(item.observedAt).toLocaleString()}</span><span>{item.isVerified ? 'Verified' : 'Unverified'}</span>{curator && !item.isVerified && <div className="workflow-actions"><button disabled={busy} onClick={() => { setSelected(item); setValues({ ...item, sourceReference: item.sourceReference ?? '', observedAt: localDateInput(item.observedAt) }); setErrors({}) }}>Edit</button><button disabled={busy} onClick={() => { if (window.confirm('Verify this evidence? It will become immutable.')) run(() => verifyReference(item.id, item.version), 'Reference verified.') }}>Verify</button><button disabled={busy} onClick={() => { if (window.confirm('Delete this unverified reference?')) run(() => deleteReference(item.id), 'Reference deleted.') }}>Delete</button></div>}</article>)}</div><div className="pagination"><button disabled={busy || page <= 1} onClick={() => setPage(page - 1)}>Previous</button><span>Page {page} of {data?.totalPages || 1}</span><button disabled={busy || page >= (data?.totalPages || 1)} onClick={() => setPage(page + 1)}>Next</button></div></>}
    {curator && <form onSubmit={save} noValidate><h3>{selected ? 'Edit unverified reference' : 'Add reference'}</h3><fieldset disabled={busy} className="form-fields"><div className="form-grid reference-fields">
      {field('categoryId', 'Category identifier')}{field('sourceName', 'Source name', 'text', 200)}{field('sourceReference', 'Source reference (optional)', 'text', 1000)}
      <label>Condition<select value={values.condition} onChange={e => change('condition', e.target.value)}>{['Excellent', 'Good', 'Fair', 'Poor', 'Unsafe', 'Unknown'].map(x => <option key={x}>{x}</option>)}</select></label>
      <label>Route<select value={values.route} onChange={e => change('route', e.target.value)}>{routes.map(x => <option key={x} value={x}>{humanize(x)}</option>)}</select></label>
      {field('currency', 'Currency', 'text', 3)}{field('valueLow', 'Low value')}{field('valueHigh', 'High value')}{field('observedAt', 'Observed at (local time)', 'datetime-local')}
    </div><div className="form-actions">{selected && <button type="button" onClick={() => { setSelected(null); setValues(empty()); setErrors({}) }}>Cancel editing</button>}<button className="button-primary">{busy ? 'Saving…' : 'Save reference'}</button></div></fieldset></form>}
  </section>
}
