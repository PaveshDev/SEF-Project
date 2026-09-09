import { formatDate } from '../demoData.js'

export default function CollectionSlotsSection({ slots, setModal, Icon }) {
  return <section className="cl-panel">
    <div className="cl-panel-heading">
      <div><h2>Available collection slots</h2><p className="cl-muted">Preview availability, vehicle capacity, and staff assignments.</p></div>
      <button className="cl-button cl-primary" onClick={() => setModal({ type: 'slot' })}>+ Add slot</button>
    </div>
    <div className="cl-slot-grid">{slots.map(slot => <article className="cl-slot-card" key={slot.id}>
      <span className="cl-eyebrow">{slot.id} · {formatDate(slot.date)}</span>
      <h3>{slot.start}–{slot.end}</h3>
      <p>{slot.vehicle} · {slot.capacity} kg capacity</p>
      <p className="cl-muted">{slot.collector}</p>
      <div className="cl-actions">
        <button className="cl-button" onClick={() => setModal({ type: 'slot', slot })}>Edit</button>
        <button className="cl-text-button cl-danger" onClick={() => setModal({ type: 'deleteSlot', slot })}>Delete</button>
      </div>
    </article>)}</div>
    {slots.length === 0 && <div className="cl-empty">No demo slots. Add one to preview availability.</div>}
  </section>
}
