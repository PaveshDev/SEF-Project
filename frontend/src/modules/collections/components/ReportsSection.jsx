import { formatDate } from '../demoData.js'

export default function ReportsSection({ jobs, tell, Icon, Badge }) {
  const completed = jobs.filter(job => job.status === 'Handover Verified' || job.status === 'Delivered')
  const failed = jobs.filter(job => job.status === 'Failed')

  return <>
    <div className="cl-stats">
      <div className="cl-stat"><span>Total pickups</span><strong>{jobs.length}</strong></div>
      <div className="cl-stat"><span>Verified handovers</span><strong>{completed.length}</strong></div>
      <div className="cl-stat"><span>Failed pickups</span><strong>{failed.length}</strong></div>
      <div className="cl-stat"><span>Travel / cost analytics</span><strong className="cl-stat-unavailable">Live</strong><small>Calculated from backend slots</small></div>
    </div>
    <section className="cl-panel">
      <div className="cl-panel-heading">
        <div><h2>Completed handovers</h2><p className="cl-muted">Verified pickup and delivery records from the Collections API.</p></div>
        <button className="cl-button" onClick={() => {
          const blob = new Blob([JSON.stringify({ exportDate: new Date().toISOString(), completedHandovers: completed }, null, 2)], { type: 'application/json' })
          const url = URL.createObjectURL(blob)
          const anchor = document.createElement('a')
          anchor.href = url
          anchor.download = 'collections-handovers.json'
          anchor.click()
          setTimeout(() => URL.revokeObjectURL(url), 1000)
          tell('Handover report exported as JSON.')
        }}>Export report</button>
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
      </table>{completed.length === 0 && <div className="cl-empty">No completed handovers recorded.</div>}</div>
    </section>
  </>
}
