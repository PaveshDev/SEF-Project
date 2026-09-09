export default function CollectionAnalytics({ jobs, Icon }) {
  const total = jobs.length
  const completed = jobs.filter(j => j.status === 'Handover Verified').length
  const failed = jobs.filter(j => j.status === 'Failed').length
  const active = jobs.filter(j => ['Scheduled', 'En Route', 'Collected', 'Delivered'].includes(j.status)).length
  const successRate = total > 0 ? Math.round(((completed) / total) * 100) : 0

  return <section className="cl-panel">
    <div className="cl-panel-heading">
      <div>
        <h2>Collection analytics</h2>
        <p className="cl-muted">Visual summary of collection performance. Travel and cost metrics require routing data.</p>
      </div>
      <Icon name="chart" />
    </div>
    <div className="cl-panel-body">
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '16px', marginBottom: '20px' }}>
        <AnalyticsCard label="Success rate" value={`${successRate}%`} color="#2d674e" />
        <AnalyticsCard label="Active" value={String(active).padStart(2, '0')} color="#627c9c" />
        <AnalyticsCard label="Completed" value={String(completed).padStart(2, '0')} color="#587944" />
        <AnalyticsCard label="Failed" value={String(failed).padStart(2, '0')} color="#b07256" />
      </div>
      <div style={{ display: 'flex', gap: '4px', height: '32px', borderRadius: '6px', overflow: 'hidden', marginBottom: '12px' }}>
        {completed > 0 && <div style={{ flex: completed, background: '#9eb398', transition: 'flex .3s' }} title={`Completed: ${completed}`} />}
        {active > 0 && <div style={{ flex: active, background: '#b5c8d9', transition: 'flex .3s' }} title={`Active: ${active}`} />}
        {failed > 0 && <div style={{ flex: failed, background: '#d4a17c', transition: 'flex .3s' }} title={`Failed: ${failed}`} />}
        {total === 0 && <div style={{ flex: 1, background: '#e9eee3' }} />}
      </div>
      <div style={{ display: 'flex', gap: '20px', fontSize: '10px', color: '#718079' }}>
        <span><i style={{ display: 'inline-block', width: '8px', height: '8px', background: '#9eb398', borderRadius: '2px', marginRight: '5px' }} />Completed</span>
        <span><i style={{ display: 'inline-block', width: '8px', height: '8px', background: '#b5c8d9', borderRadius: '2px', marginRight: '5px' }} />Active</span>
        <span><i style={{ display: 'inline-block', width: '8px', height: '8px', background: '#d4a17c', borderRadius: '2px', marginRight: '5px' }} />Failed</span>
      </div>
      <p className="cl-muted" style={{ marginTop: '16px' }}>Travel distance, duration, cost breakdown, and route analytics are unavailable — no routing data has been collected.</p>
    </div>
  </section>
}

function AnalyticsCard({ label, value, color }) {
  return <div style={{ background: '#f9faf7', border: '1px solid #e3e8e2', borderRadius: '8px', padding: '16px', textAlign: 'center' }}>
    <div style={{ fontSize: '28px', fontWeight: 500, color, letterSpacing: '-1px' }}>{value}</div>
    <div style={{ fontSize: '10px', color: '#718079', marginTop: '4px' }}>{label}</div>
  </div>
}
