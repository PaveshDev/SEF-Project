import { Outlet, Link } from 'react-router';

export default function App() {
  return (
    <div>
      <header>
        <h1>Waste-to-Value — project skeleton</h1>
        <nav>
          <Link to="/items">Items</Link>
        </nav>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  )
}
