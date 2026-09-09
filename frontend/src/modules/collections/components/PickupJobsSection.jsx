import { formatDate, milestones } from '../demoData.js'

export default function PickupJobsSection({ jobs, query, setQuery, filter, setFilter, setModal, Icon, Badge }) {
  const visibleJobs = jobs.filter(job => (filter === 'All statuses' || job.status === filter) && `${job.id} ${job.item} ${job.owner}`.toLowerCase().includes(query.toLowerCase()))

  return <section className="cl-panel">
    <div className="cl-toolbar">
      <label className="cl-field"><span>Search pickups</span><input type="search" placeholder="Item, pickup ID, or owner" value={query} onChange={event => setQuery(event.target.value)} /></label>
      <label className="cl-field"><span>Status</span><select value={filter} onChange={event => setFilter(event.target.value)}>{['All statuses', 'Draft', 'Awaiting review', ...milestones, 'Failed', 'Cancelled'].map(status => <option key={status}>{status}</option>)}</select></label>
      <span className="cl-muted">{visibleJobs.length} pickups</span>
    </div>
    <div className="cl-table-scroll"><table className="cl-table">
      <thead><tr><th scope="col">Pickup / item</th><th scope="col">Collection window</th><th scope="col">Assignment</th><th scope="col">Status</th><th scope="col"><span className="cl-sr-only">Actions</span></th></tr></thead>
      <tbody>{visibleJobs.map(job => <tr key={job.id}>
        <td><div className="cl-item-cell"><span className="cl-item-icon"><Icon /></span><div><strong>{job.item}</strong><small>{job.id} · {job.owner}</small></div></div></td>
        <td><strong>{formatDate(job.date)}</strong><small>{job.window}</small></td>
        <td><strong>{job.collector}</strong><small>{job.vehicle}</small></td>
        <td><Badge status={job.status} /></td>
        <td><button className="cl-text-button" aria-label={`View ${job.id}, ${job.item}`} onClick={() => setModal({ type: 'detail', id: job.id })}>View <Icon name="arrow" size={15} /></button></td>
      </tr>)}</tbody>
    </table>{visibleJobs.length === 0 && <div className="cl-empty">No pickups match this view.</div>}</div>
  </section>
}
