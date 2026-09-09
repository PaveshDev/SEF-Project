import { formatDate, milestones } from '../demoData.js'

export default function OverviewSection({ jobs, slots, calendarDate, setCalendarDate, setSection, setModal, Icon, Badge }) {
  const pending = jobs.filter(job => job.status === 'Awaiting review').length
  const failed = jobs.filter(job => job.status === 'Failed')
  const completed = jobs.filter(job => job.status === 'Handover Verified')
  const active = jobs.filter(job => milestones.slice(0, 4).includes(job.status)).length

  return <>
    <div className="cl-stats">{[
      ['Active pickups', active, 'Scheduled and in progress', 'box', 'blue'],
      ['Awaiting review', pending, 'Needs a staff decision', 'spark', 'amber'],
      ['Needs attention', failed.length, 'Collection needs re-planning', 'alert', 'red'],
      ['Handovers completed', completed.length, 'Verified sample records', 'check', 'green'],
    ].map(([label, value, caption, icon, tone]) => <div className="cl-stat" key={label}><div className="cl-stat-top"><span>{label}</span><span className={`cl-stat-icon cl-${tone}`}><Icon name={icon} size={18} /></span></div><strong>{String(value).padStart(2, '0')}</strong><small>{caption}</small></div>)}</div>
    <div className="cl-overview-grid">
      <section className="cl-panel"><div className="cl-panel-heading"><div><h2>Collection calendar</h2><p className="cl-muted">September 2026 · sample week</p></div><Icon name="calendar" /></div>
        <div className="cl-week" aria-label="Select a date">{['07', '08', '09', '10', '11', '12', '13'].map((day, index) => <button key={day} aria-pressed={calendarDate.endsWith(day)} className={calendarDate.endsWith(day) ? 'is-selected' : ''} onClick={() => setCalendarDate(`2026-09-${day}`)}><span>{['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'][index]}</span><strong>{day}</strong><i className={jobs.some(job => job.date === `2026-09-${day}`) ? 'has-jobs' : ''} /></button>)}</div>
        <div className="cl-agenda">{jobs.filter(job => job.date === calendarDate && job.status !== 'Cancelled').map(job => <button className="cl-agenda-row" key={job.id} onClick={() => setModal({ type: 'detail', id: job.id })}><span className="cl-agenda-time">{job.window.split('–')[0]}</span><span className={`cl-agenda-line ${job.status === 'Failed' ? 'is-failed' : ''}`} /><span className="cl-agenda-item"><strong>{job.item}</strong><small>{job.destination} · {job.vehicle}</small></span><Badge status={job.status} /></button>)}{!jobs.some(job => job.date === calendarDate && job.status !== 'Cancelled') && <div className="cl-empty">No collections on {formatDate(calendarDate)}.</div>}</div>
      </section>
      <section className="cl-proposal-card"><span className="cl-ai-label"><Icon name="spark" size={17} /> COLLECTION PLANNING</span><h2>A plan worth<br />a closer look.</h2><p>A sample chair pickup is ready for a staff review preview.</p><div className="cl-recommendation"><span>ILLUSTRATIVE OPTION</span><strong>Friday, 14:00–16:00</strong><small>Oak dining chair · Small van</small></div><div className="cl-proposal-warning"><Icon name="alert" size={17} /><span>Travel estimate unavailable.<br />Manual review required.</span></div><button className="cl-button" onClick={() => setSection('Proposal review')}>Review sample proposal <Icon name="arrow" size={17} /></button><small className="cl-ai-footnote">Static example · no agent has run</small></section>
    </div>
    {failed.length > 0 && <div className="cl-attention"><span className="cl-attention-icon"><Icon name="alert" /></span><div><strong>A collection needs a new plan</strong><p>{failed[0].item}: {failed[0].failure}</p></div><button className="cl-text-button" onClick={() => setSection('Failed pickups')}>View issue <Icon name="arrow" size={16} /></button></div>}
    <section className="cl-panel"><div className="cl-panel-heading"><div><h2>Pickup activity</h2><p className="cl-muted">A little visibility goes a long way.</p></div><button className="cl-text-button" onClick={() => setSection('Pickup jobs')}>View all jobs <Icon name="arrow" size={16} /></button></div>{renderTable(jobs.slice(0, 4), setModal, Icon, Badge)}</section>
  </>
}

function renderTable(rows, setModal, Icon, Badge) {
  return <div className="cl-table-scroll"><table className="cl-table">
    <thead><tr><th scope="col">Pickup / item</th><th scope="col">Collection window</th><th scope="col">Assignment</th><th scope="col">Status</th><th scope="col"><span className="cl-sr-only">Actions</span></th></tr></thead>
    <tbody>{rows.map(job => <tr key={job.id}>
      <td><div className="cl-item-cell"><span className="cl-item-icon"><Icon /></span><div><strong>{job.item}</strong><small>{job.id} · {job.owner}</small></div></div></td>
      <td><strong>{formatDate(job.date)}</strong><small>{job.window}</small></td>
      <td><strong>{job.collector}</strong><small>{job.vehicle}</small></td>
      <td><Badge status={job.status} /></td>
      <td><button className="cl-text-button" aria-label={`View ${job.id}, ${job.item}`} onClick={() => setModal({ type: 'detail', id: job.id })}>View <Icon name="arrow" size={15} /></button></td>
    </tr>)}</tbody>
  </table>{rows.length === 0 && <div className="cl-empty">No pickups match this view.</div>}</div>
}
