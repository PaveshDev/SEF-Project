import { useEffect, useRef, useState } from 'react'
import { apiErrorMessage, localDateInput } from '../services/recoveryWorkflow.js'
import { caseErrors, fieldErrors, humanize, routes } from '../services/recoveryValidation.js'

export default function CaseForm({ selected, onSaved, onCancel }) {
  const [values, setValues] = useState(() => ({ itemId: selected?.itemId ?? '', objective: selected?.objective ?? '', currency: selected?.currency ?? 'LKR', budget: selected?.maximumPickupCost ?? '', preferredRoutes: [...(selected?.preferredRoutes ?? ['Reuse'])], deadline: localDateInput(selected?.deadline) }))
  const [errors, setErrors] = useState({})
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const dialog = useRef(null)
  const returnFocus = useRef(document.activeElement)
  const saving = useRef(false)
  useEffect(() => {
    const node = dialog.current
    const opener = returnFocus.current
    node.showModal()
    return () => { node.close(); queueMicrotask(() => { if (opener?.isConnected) opener.focus() }) }
  }, [])
  const change = (key, value) => setValues(old => ({ ...old, [key]: value }))
  const props = key => ({ value: values[key], onChange: e => change(key, key === 'currency' ? e.target.value.toUpperCase() : e.target.value), 'aria-invalid': !!errors[key], 'aria-describedby': errors[key] ? `case-${key}-error` : undefined })
  const issue = key => errors[key] && <small className="field-error" id={`case-${key}-error`}>{errors[key]}</small>
  async function submit(event) {
    event.preventDefault()
    if (saving.current) return
    const invalid = caseErrors(values)
    setErrors(invalid)
    if (Object.keys(invalid).length) return
    saving.current = true; setBusy(true); setError('')
    try {
      await onSaved(selected?.id ?? null, { itemId: values.itemId, inputs: { objective: values.objective.trim(), preferredRoutes: values.preferredRoutes, currency: values.currency, maximumPickupCost: values.budget === '' ? null : Number(values.budget), deadline: values.deadline ? new Date(values.deadline).toISOString() : null } }, selected?.version)
    } catch (failure) { setErrors(fieldErrors(failure)); setError(apiErrorMessage(failure, 'The save could not be confirmed. Retry the same inputs to recover the submission.')) }
    finally { saving.current = false; setBusy(false) }
  }
  return <dialog ref={dialog} className="form-panel" aria-labelledby="case-form-title" onCancel={event => { event.preventDefault(); if (!saving.current) onCancel() }}>
    <form onSubmit={submit} noValidate>
      <div className="panel-heading"><h2 id="case-form-title">{selected ? 'Edit recovery case' : 'New recovery case'}</h2><button type="button" className="icon-button" aria-label="Close form" disabled={busy} onClick={onCancel}>×</button></div>
      <fieldset disabled={busy} className="form-fields">
        <label>Item identifier<input autoFocus={!selected} disabled={!!selected} {...props('itemId')} />{issue('itemId')}</label>
        <p className="muted">Use the identifier of your item with a confirmed assessment. The Items selector is awaiting the team integration.</p>
        <label>Objective<textarea aria-label="Objective" autoFocus={!!selected} maxLength={1000} {...props('objective')} />{issue('objective')}</label>
        <fieldset><legend>Preferred routes — choose all that apply</legend>{routes.map(route => <label className="route-choice" key={route}><input type="checkbox" checked={values.preferredRoutes.includes(route)} onChange={event => change('preferredRoutes', event.target.checked ? [...values.preferredRoutes, route] : values.preferredRoutes.filter(x => x !== route))} />{humanize(route)}</label>)}{issue('preferredRoutes')}</fieldset>
        <div className="form-grid"><label>Currency<input maxLength={3} {...props('currency')} />{issue('currency')}</label><label>Maximum pickup cost (optional)<input inputMode="decimal" {...props('budget')} />{issue('budget')}</label></div>
        <label>Deadline (your local time, optional)<input type="datetime-local" {...props('deadline')} />{issue('deadline')}</label>
      </fieldset>
      {error && <p className="inline-error" role="alert">{error}</p>}
      <div className="form-actions"><button type="button" className="button-muted" disabled={busy} onClick={onCancel}>Cancel</button><button className="button-primary" disabled={busy}>{busy ? 'Saving…' : 'Save case'}</button></div>
    </form>
  </dialog>
}
