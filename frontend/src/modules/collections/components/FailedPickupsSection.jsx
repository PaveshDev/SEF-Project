import { formatDate } from '../demoData.js'

export default function FailedPickupsSection({ jobs, setSection, setModal, Icon, Badge }) {
  const failed = jobs.filter(job => job.status === 'Failed')

  return <div className="cl-failure-list">{failed.map(job => <section className="cl-panel" key={job.id}>
    <div className="cl-panel-heading"><div><h2>{job.item}</h2><p className="cl-muted">{job.id} · {formatDate(job.date)} · {job.window}</p></div><Badge status="Failed" /></div>
    <div className="cl-panel-body">
      <div className="cl-inline-warning"><strong>Changed constraint:</strong> {job.failure}</div>
      <h3>Re-planning preview</h3>
      <p>Search alternative slots, recheck capacity and handling, compare owner and destination hours, then obtain a fresh travel estimate before staff review.</p>
      <p className="cl-muted">No revised plan has been generated. Routing and the Collection Agent are not connected.</p>
      {job.rescheduleNote && <div className="cl-local-record"><strong>Local revision request</strong><p>{job.rescheduleNote}</p></div>}
      <div className="cl-actions">
        <button className="cl-button" onClick={() => setSection('Collection slots')}>Inspect demo slots</button>
        <button className="cl-button cl-primary" onClick={() => setModal({ type: 'reschedule', id: job.id })}>Draft revision request</button>
      </div>
    </div>
  </section>)}{failed.length === 0 && <div className="cl-empty">No failed pickups in the demo.</div>}</div>
}
