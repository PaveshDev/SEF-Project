import { formatDate } from '../demoData.js'

export default function ReportsSection({ jobs, tell, Icon, Badge }) {
  const completed = jobs.filter(job => job.status === 'Handover Verified')
  const failed = jobs.filter(job => job.status === 'Failed')

  return <>
    <div className="cl-stats">
      <div className="cl-stat"><span>Total demo pickups</span><strong>{jobs.length}</strong></div>
      <div className="cl-stat"><span>Verified handovers</span><strong>{completed.length}</strong></div>
      <div className="cl-stat"><span>Failed pickups</span><strong>{failed.length}</strong></div>
      <div className="cl-stat"><span>Travel / cost analytics</span><strong className="cl-stat-unavailable">Unavailable</strong><small>No routing data collected</small></div>
    </div>
    <section className="cl-panel">
      <div className="cl-panel-heading">
        <div><h2>Completed handovers</h2><p className="cl-muted">Fictional records for the UI demonstration.</p></div>
        <button className="cl-button" onClick={() => {
          const blob = new Blob([JSON.stringify({ demo: true, persistent: false, completedHandovers: completed }, null, 2)], { type: 'application/json' })
          const url = URL.createObjectURL(blob)
          const anchor = document.createElement('a')
          anchor.href = url
          anchor.download = 'collections-demo-handovers.json'
          anchor.click()
          setTimeout(() => URL.revokeObjectURL(url), 1000)
          tell('Demo handover report exported as JSON.')
        }}>Export demo report</button>
      </div>
      <div className="cl-table-scroll"><table className="cl-table">
        <thead><tr><th scope="col">Pickup / item</th><th scope="col">Collection window</th><th scope="col">Assignment</th><th scope="col">Status</th><th scope="col"><span className="cl-sr-only">Actions</span></th></tr></thead>
        <tbody>{completed.map(job => <tr key={job.id}>
          <td><div className="cl-item-cell"><span className="cl-item-icon"><Icon /></span><div><strong>{job.item}</strong><small>{job.id} · {job.owner}</small></div></div></td>
          <td><strong>{formatDate(job.date)}</strong><small>{job.window}</small></td>
          <td><strong>{job.collector}</strong><small>{job.vehicle}</small></td>
          <td><Badge status={job.status} /></td>
          <td />
        </tr>)}</tbody>
      </table>{completed.length === 0 && <div className="cl-empty">No completed handovers in the demo.</div>}</div>
    </section>
  </>
}
