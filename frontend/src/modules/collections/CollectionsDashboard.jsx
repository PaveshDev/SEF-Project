import { useEffect, useRef, useState } from 'react'
import { useCollections } from './hooks/useCollections.js'
import { formatDate, milestones } from './demoData.js'
import OverviewSection from './components/OverviewSection.jsx'
import PickupJobsSection from './components/PickupJobsSection.jsx'
import CollectionSlotsSection from './components/CollectionSlotsSection.jsx'
import ProposalReviewSection from './components/ProposalReviewSection.jsx'
import FailedPickupsSection from './components/FailedPickupsSection.jsx'
import HandoverSection from './components/HandoverSection.jsx'
import ReportsSection from './components/ReportsSection.jsx'
import CollectionAnalytics from './components/CollectionAnalytics.jsx'
import './collections.css'

const sections = ['Overview', 'Pickup jobs', 'Collection slots', 'Proposal review', 'Failed pickups', 'Handover', 'Reports']
const icons = ['grid', 'box', 'calendar', 'spark', 'alert', 'check', 'chart']

function Icon({ name = 'box', size = 20 }) {
  const paths = {
    grid: <><rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" /><rect x="3" y="14" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" /></>,
    box: <><path d="m3 7 9-4 9 4-9 4-9-4Zm0 0v10l9 4 9-4V7M12 11v10M7 5l10 4" /></>,
    calendar: <><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M16 3v4M8 3v4M3 11h18M8 15h2M14 15h2" /></>,
    spark: <path d="m12 3 2.5 6.5L21 12l-6.5 2.5L12 21l-2.5-6.5L3 12l6.5-2.5L12 3Z" />,
    alert: <><path d="m12 3 10 18H2L12 3ZM12 9v5M12 17v1" /></>,
    check: <><circle cx="12" cy="12" r="9" /><path d="m8 12 3 3 5-6" /></>,
    chart: <path d="M4 3v18h17M8 16v-5M13 16V7M18 16V4" />,
    arrow: <path d="M5 12h14m-5-5 5 5-5 5" />,
    leaf: <><path d="M19 3C7 2 2 8 6 15s15 3 13-12ZM5 21l10-12" /></>,
  }
  return <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">{paths[name]}</svg>
}

function Badge({ status }) {
  const tone = ['Handover Verified', 'Collected', 'Delivered'].includes(status) ? 'green' : status === 'Failed' || status === 'Cancelled' ? 'red' : status === 'Awaiting review' || status === 'Draft' ? 'amber' : 'blue'
  return <span className={`cl-badge cl-${tone}`}><span />{status}</span>
}

function Modal({ title, onClose, children }) {
  const dialog = useRef(null)
  useEffect(() => {
    const node = dialog.current
    node.showModal()
    return () => node.close()
  }, [])
  return <dialog className="cl-modal" ref={dialog} onCancel={onClose} onClick={event => { if (event.target === event.currentTarget) onClose() }}>
    <div className="cl-panel-heading"><h2>{title}</h2><button className="cl-icon-button" onClick={onClose} aria-label="Close dialog">×</button></div>
    {children}
  </dialog>
}

function Field({ label, children }) {
  return <label className="cl-field"><span>{label}</span>{children}</label>
}

function JobForm({ job, onSave, onClose }) {
  const [error, setError] = useState('')
  return <Modal title={job ? 'Edit pickup request' : 'Create pickup request'} onClose={onClose}>
    <p className="cl-muted">All pickup details are stored in the ASP.NET Core backend and verified by the system.</p>
    <form onSubmit={event => {
      event.preventDefault()
      const data = Object.fromEntries(new FormData(event.currentTarget))
      if (['item', 'owner', 'address', 'destination'].some(key => !data[key].trim())) { setError('Enter an item, owner, address, and destination.'); return }
      if (data.end <= data.start) { setError('End time must be after start time.'); return }
      onSave({ ...data, item: data.item.trim(), window: `${data.start}–${data.end}` })
    }}>
      <div className="cl-form-grid">
        <Field label="Item"><input name="item" required maxLength={100} defaultValue={job?.item} /></Field>
        <Field label="Owner"><input name="owner" required maxLength={100} defaultValue={job?.owner} /></Field>
        <Field label="Pickup address"><input name="address" required maxLength={240} defaultValue={job?.address} /></Field>
        <Field label="Destination"><input name="destination" required maxLength={150} defaultValue={job?.destination} /></Field>
        <Field label="Preferred date"><input type="date" name="date" required defaultValue={job?.date ?? '2026-09-11'} /></Field>
        <Field label="Handling notes"><input name="handling" maxLength={300} defaultValue={job?.handling} /></Field>
        <Field label="Available from"><input name="start" type="time" required defaultValue={job?.window?.split('–')[0] ?? '09:00'} /></Field>
        <Field label="Available until"><input name="end" type="time" required defaultValue={job?.window?.split('–')[1] ?? '11:00'} /></Field>
      </div>
      {error && <p role="alert" className="cl-error">{error}</p>}
      <div className="cl-actions"><button type="button" className="cl-button" onClick={onClose}>Cancel</button><button className="cl-button cl-primary">Save pickup request</button></div>
    </form>
  </Modal>
}

function SlotForm({ slot, onSave, onClose }) {
  const [error, setError] = useState('')
  return <Modal title={slot ? 'Edit collection slot' : 'Add collection slot'} onClose={onClose}>
    <p className="cl-muted">Collection slots define vehicle capacity and time windows for scheduling pickups.</p>
    <form onSubmit={event => {
      event.preventDefault()
      const data = Object.fromEntries(new FormData(event.currentTarget))
      if (data.end <= data.start) { setError('End time must be after start time.'); return }
      onSave(data)
    }}>
      <div className="cl-form-grid">
        <Field label="Date"><input name="date" type="date" required defaultValue={slot?.date ?? '2026-09-11'} /></Field>
        <Field label="Vehicle"><select name="vehicle" defaultValue={slot?.vehicle ?? 'Small van'}><option>Small van</option><option>Cargo van</option><option>Truck</option></select></Field>
        <Field label="Start"><input name="start" type="time" required defaultValue={slot?.start ?? '09:00'} /></Field>
        <Field label="End"><input name="end" type="time" required defaultValue={slot?.end ?? '11:00'} /></Field>
        <Field label="Capacity (kg)"><input name="capacity" type="number" min="1" max="10000" step="1" required defaultValue={slot?.capacity ?? '50'} /></Field>
        <Field label="Collector"><select name="collector" defaultValue={slot?.collector ?? 'Unassigned'}><option>Unassigned</option><option>Demo collector A</option><option>Demo collector B</option></select></Field>
      </div>
      {error && <p role="alert" className="cl-error">{error}</p>}
      <div className="cl-actions"><button type="button" className="cl-button" onClick={onClose}>Cancel</button><button className="cl-button cl-primary">Save collection slot</button></div>
    </form>
  </Modal>
}

export default function CollectionsDashboard() {
  const {
    jobs, slots, loading, error: hookError, apiConnected, loadData,
    addJob, updateJob, removeJob, rescheduleJob,
    addSlot, editSlot, removeSlot, verifyCode,
  } = useCollections()

  const [section, setSection] = useState('Overview')
  const [query, setQuery] = useState('')
  const [filter, setFilter] = useState('All statuses')
  const [calendarDate, setCalendarDate] = useState('2026-09-11')
  const [modal, setModal] = useState(null)
  const [notice, setNotice] = useState('')
  const [review, setReview] = useState(null)
  const close = () => setModal(null)
  const tell = message => setNotice(message)
  const selectedJob = modal?.id ? jobs.find(job => job.id === modal.id) : null

  return <div className="cl-app">
    <aside className="cl-sidebar">
      <a href="/collections" className="cl-brand"><span className="cl-brand-mark"><Icon name="leaf" size={25} /></span><span>Waste<span className="cl-brand-light">to</span>Value<small>COLLECTION WORKSPACE</small></span></a>
      <div className="cl-nav-label">WORKSPACE</div>
      <nav aria-label="Collection management">{sections.map((name, index) => <button key={name} className={`cl-nav-item ${section === name ? 'is-active' : ''}`} aria-current={section === name ? 'page' : undefined} onClick={() => { setSection(name); setNotice('') }}><Icon name={icons[index]} /><span>{name}</span>{name === 'Proposal review' && <span className="cl-nav-count">{jobs.filter(j => j.status === 'Awaiting review').length}</span>}{name === 'Failed pickups' && <span className="cl-nav-dot" />}</button>)}</nav>
      <div className="cl-sidebar-bottom"><div className="cl-impact-icon"><Icon name="leaf" /></div><strong>A better next chapter.</strong><p>Help useful items find their way to a new beginning.</p><div className="cl-profile"><span className="cl-avatar">M4</span><div><strong>Member 4</strong><small>Operations Workspace</small></div><span className={`cl-online${apiConnected ? '' : ' cl-offline'}`} title={apiConnected ? 'API connected' : 'API offline'} /></div></div>
    </aside>

    <div className="cl-workspace">
      <header className="cl-topbar"><span>Operations <span className="cl-separator">/</span> <strong>Pickup &amp; handover</strong></span><span className="cl-topbar-date"><Icon name="calendar" size={16} />September 2026 · {apiConnected ? 'API' : 'Offline'}</span></header>
      <main className="cl-main">
        <div className="cl-demo-banner"><span className="cl-demo-tag">{apiConnected ? 'API CONNECTED' : 'BACKEND OFFLINE'}</span> {apiConnected ? 'Connected to ASP.NET backend · Changes persist in the API' : 'Backend API unavailable or database not ready · No fallback demo records displayed.'}</div>
        {loading && <div className="cl-notice" role="status"><span>Loading collections data…</span></div>}
        {hookError && <div className="cl-notice" role="alert"><span>API Error: {hookError}</span><button className="cl-text-button" onClick={loadData} style={{ marginLeft: '1rem', textDecoration: 'underline' }}>Retry</button><button className="cl-icon-button" aria-label="Dismiss" onClick={() => setNotice('')}>×</button></div>}
        <div className="cl-page-heading"><div><p className="cl-eyebrow">PICKUP &amp; HANDOVER MANAGEMENT</p><h1>{section === 'Overview' ? 'Every pickup, a new possibility.' : section}</h1><p className="cl-muted">{section === 'Overview' ? 'Plan collections, keep things moving, and close the loop.' : 'Manage the next step in an item\u2019s journey.'}</p></div><button className="cl-button cl-primary" onClick={() => setModal({ type: 'draft' })}><span aria-hidden="true">+</span> New pickup</button></div>
        {notice && <div className="cl-notice" role="status"><span>{notice}</span><button className="cl-icon-button" aria-label="Dismiss notice" onClick={() => setNotice('')}>×</button></div>}

        {section === 'Overview' && <OverviewSection jobs={jobs} slots={slots} calendarDate={calendarDate} setCalendarDate={setCalendarDate} setSection={setSection} setModal={setModal} Icon={Icon} Badge={Badge} />}
        {section === 'Pickup jobs' && <PickupJobsSection jobs={jobs} query={query} setQuery={setQuery} filter={filter} setFilter={setFilter} setModal={setModal} Icon={Icon} Badge={Badge} />}
        {section === 'Collection slots' && <CollectionSlotsSection slots={slots} setModal={setModal} Icon={Icon} />}
        {section === 'Proposal review' && <ProposalReviewSection review={review} setReview={setReview} tell={tell} Icon={Icon} Badge={Badge} />}
        {section === 'Failed pickups' && <FailedPickupsSection jobs={jobs} setSection={setSection} setModal={setModal} Icon={Icon} Badge={Badge} />}
        {section === 'Handover' && <HandoverSection jobs={jobs} tell={tell} verifyCode={verifyCode} Icon={Icon} />}
        {section === 'Reports' && <><ReportsSection jobs={jobs} tell={tell} Icon={Icon} Badge={Badge} /><CollectionAnalytics jobs={jobs} Icon={Icon} /></>}

        <footer className="cl-footer"><span>Waste-to-Value / Member 4</span><span>Small actions. Lasting value.</span></footer>
      </main>
    </div>

    {modal?.type === 'draft' && <JobForm job={selectedJob} onClose={close} onSave={async data => {
      try {
        if (selectedJob) {
          await updateJob(selectedJob.id, data)
          tell('Pickup updated via API.')
        } else {
          await addJob(data)
          tell('Pickup created via API.')
        }
        close()
      } catch (err) {
        tell(`Error saving pickup: ${err.message}`)
      }
    }} />}

    {modal?.type === 'slot' && <SlotForm slot={modal.slot} onClose={close} onSave={async data => {
      try {
        if (modal.slot) {
          await editSlot(modal.slot.id, data)
          tell('Slot updated via API.')
        } else {
          await addSlot(data)
          tell('Slot created via API.')
        }
        close()
      } catch (err) {
        tell(`Error saving slot: ${err.message}`)
      }
    }} />}

    {modal?.type === 'deleteSlot' && <Modal title="Delete collection slot?" onClose={close}>
      <p>Remove slot {modal.slot.start}–{modal.slot.end} on {formatDate(modal.slot.date)}?</p>
      <div className="cl-actions">
        <button className="cl-button" onClick={close}>Cancel</button>
        <button className="cl-button cl-danger" onClick={async () => {
          try {
            await removeSlot(modal.slot.id)
            close()
            tell('Slot deleted via API.')
          } catch (err) {
            tell(`Error deleting slot: ${err.message}`)
          }
        }}>Delete slot</button>
      </div>
    </Modal>}

    {modal?.type === 'detail' && selectedJob && <Modal title={`${selectedJob.id} · ${selectedJob.item}`} onClose={close}>
      <Badge status={selectedJob.status} />
      <dl className="cl-facts">
        <div><dt>Pickup address</dt><dd>{selectedJob.address}</dd></div>
        <div><dt>Destination</dt><dd>{selectedJob.destination}</dd></div>
        <div><dt>Window</dt><dd>{formatDate(selectedJob.date)} · {selectedJob.window}</dd></div>
        <div><dt>Collector / vehicle</dt><dd>{selectedJob.collector} · {selectedJob.vehicle}</dd></div>
        <div><dt>Handling</dt><dd>{selectedJob.handling || 'Not specified'}</dd></div>
      </dl>
      <h3>Status history</h3>
      <ul className="cl-history">
        {selectedJob.history?.map((entry, index) => <li key={`${index}-${entry}`}>{entry}</li>)}
      </ul>
      <div className="cl-actions">
        {selectedJob.status === 'Draft' && <>
          <button className="cl-button" onClick={() => setModal({ type: 'draft', id: selectedJob.id })}>Edit pickup</button>
          <button className="cl-button cl-danger" onClick={async () => {
            try {
              await removeJob(selectedJob.id)
              close()
              tell('Pickup deleted via API.')
            } catch (err) {
              tell(`Error: ${err.message}`)
            }
          }}>Delete pickup</button>
        </>}
        {selectedJob.status === 'Scheduled' && <>
          <button className="cl-button" onClick={() => setModal({ type: 'assignment', id: selectedJob.id })}>Update assignment</button>
          <button className="cl-button cl-danger" onClick={() => setModal({ type: 'cancel', id: selectedJob.id })}>Cancel pickup</button>
        </>}
        {selectedJob.status === 'Awaiting review' && <button className="cl-button cl-primary" onClick={() => { close(); setSection('Proposal review') }}>Review proposal</button>}
        {selectedJob.status === 'Failed' && <button className="cl-button cl-primary" onClick={() => { close(); setSection('Failed pickups') }}>Review failure</button>}
      </div>
    </Modal>}

    {modal?.type === 'assignment' && selectedJob && <Modal title="Update assignment" onClose={close}>
      <p className="cl-muted">Assign a collector or vehicle class for this pickup.</p>
      <form onSubmit={async event => {
        event.preventDefault()
        const data = Object.fromEntries(new FormData(event.currentTarget))
        try {
          await updateJob(selectedJob.id, data)
          close()
          tell('Assignment updated via API.')
        } catch (err) {
          tell(`Error updating assignment: ${err.message}`)
        }
      }}>
        <Field label="Collector"><select name="collector" defaultValue={selectedJob.collector}><option>Demo collector A</option><option>Demo collector B</option></select></Field>
        <Field label="Vehicle"><select name="vehicle" defaultValue={selectedJob.vehicle}><option>Small van</option><option>Cargo van</option><option>Truck</option></select></Field>
        <div className="cl-actions"><button className="cl-button cl-primary">Save assignment</button></div>
      </form>
    </Modal>}

    {(modal?.type === 'cancel' || modal?.type === 'reschedule') && selectedJob && <Modal title={modal.type === 'cancel' ? 'Cancel pickup' : 'Request reschedule'} onClose={close}>
      <p className="cl-muted">This request will be recorded and processed by the ASP.NET Core Collections service.</p>
      <form onSubmit={async event => {
        event.preventDefault()
        const reason = new FormData(event.currentTarget).get('reason')?.trim()
        if (!reason) return
        try {
          if (modal.type === 'cancel') {
            await updateJob(selectedJob.id, { status: 'Cancelled' })
            tell('Pickup cancelled via API.')
          } else {
            await rescheduleJob(selectedJob.id, { reason, requestedBy: '00000000-0000-0000-0000-000000000000' })
            tell('Reschedule request submitted to API.')
          }
          close()
        } catch (err) {
          tell(`Error: ${err.message}`)
        }
      }}>
        <Field label="Reason / changed constraint">
          <textarea name="reason" required maxLength={500} rows={4} onChange={event => event.target.setCustomValidity(event.target.value.trim() ? '' : 'Enter a reason.')} />
        </Field>
        <div className="cl-actions"><button type="button" className="cl-button" onClick={close}>Back</button><button className="cl-button cl-primary">Submit request</button></div>
      </form>
    </Modal>}
  </div>
}
