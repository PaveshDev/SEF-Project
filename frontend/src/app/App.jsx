import { Outlet, Link, useLocation } from 'react-router';

export default function App() {
  const location = useLocation();
  const isHome = location.pathname === '/' || location.pathname === '';

  return (
    <div className="app-container">
      <style>{`
        :root {
          --primary: #10b981;
          --primary-dark: #059669;
          --bg-light: #f8fafc;
          --text-main: #0f172a;
          --text-muted: #475569;
        }
        
        body, html {
          margin: 0;
          padding: 0;
          font-family: 'Inter', system-ui, -apple-system, sans-serif;
          background-color: var(--bg-light);
          color: var(--text-main);
          -webkit-font-smoothing: antialiased;
        }

        .app-container {
          display: flex;
          flex-direction: column;
          min-height: 100vh;
        }

        /* Top Navigation for non-home pages */
        .app-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 1rem 2rem;
          background-color: rgba(255, 255, 255, 0.8);
          backdrop-filter: blur(10px);
          border-bottom: 1px solid rgba(0, 0, 0, 0.05);
          position: sticky;
          top: 0;
          z-index: 10;
        }

        .header-logo {
          font-size: 1.25rem;
          font-weight: 800;
          color: var(--text-main);
          text-decoration: none;
          letter-spacing: -0.025em;
        }

        .header-nav .nav-link {
          font-weight: 600;
          color: var(--text-muted);
          text-decoration: none;
          padding: 0.5rem 1rem;
          border-radius: 0.5rem;
          transition: all 0.2s ease;
        }

        .header-nav .nav-link:hover {
          color: var(--primary);
          background: rgba(16, 185, 129, 0.1);
        }

        /* Main Layout */
        .app-main {
          flex: 1;
          display: flex;
          flex-direction: column;
        }

        /* Hero Section (Home Page) */
        .hero-section {
          flex: 1;
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          padding: 2rem;
          text-align: center;
          background: radial-gradient(circle at center, #ffffff 0%, #f1f5f9 100%);
          position: relative;
          overflow: hidden;
        }

        .hero-section::before {
          content: '';
          position: absolute;
          top: -20%;
          left: -10%;
          width: 50%;
          height: 50%;
          background: radial-gradient(circle, rgba(16,185,129,0.15) 0%, transparent 70%);
          border-radius: 50%;
          z-index: 0;
        }

        .hero-section::after {
          content: '';
          position: absolute;
          bottom: -20%;
          right: -10%;
          width: 60%;
          height: 60%;
          background: radial-gradient(circle, rgba(59,130,246,0.1) 0%, transparent 70%);
          border-radius: 50%;
          z-index: 0;
        }

        .hero-content {
          position: relative;
          z-index: 1;
          max-width: 800px;
          display: flex;
          flex-direction: column;
          align-items: center;
        }

        .hero-title {
          font-size: clamp(3rem, 8vw, 5rem);
          font-weight: 900;
          line-height: 1.1;
          margin: 0 0 1.5rem 0;
          letter-spacing: -0.04em;
          background: linear-gradient(135deg, var(--text-main) 0%, #334155 100%);
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
        }

        .hero-subtitle {
          font-size: clamp(1.125rem, 4vw, 1.5rem);
          color: var(--text-muted);
          margin: 0 0 3rem 0;
          line-height: 1.6;
          max-width: 600px;
        }

        .hero-card {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          background: var(--primary);
          color: white;
          text-decoration: none;
          font-size: clamp(1.25rem, 4vw, 1.75rem);
          font-weight: 800;
          padding: 1.5rem 4rem;
          border-radius: 1rem;
          box-shadow: 0 10px 25px -5px rgba(16, 185, 129, 0.4), 0 8px 10px -6px rgba(16, 185, 129, 0.1);
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
          letter-spacing: 0.05em;
          position: relative;
          overflow: hidden;
        }

        .hero-card::after {
          content: '';
          position: absolute;
          top: 0;
          left: -100%;
          width: 50%;
          height: 100%;
          background: linear-gradient(to right, rgba(255,255,255,0) 0%, rgba(255,255,255,0.2) 50%, rgba(255,255,255,0) 100%);
          transform: skewX(-20deg);
          transition: all 0.5s ease;
        }

        .hero-card:hover {
          transform: translateY(-5px) scale(1.02);
          box-shadow: 0 20px 35px -5px rgba(16, 185, 129, 0.5), 0 10px 15px -5px rgba(16, 185, 129, 0.2);
          background: var(--primary-dark);
        }

        .hero-card:hover::after {
          left: 200%;
        }

        /* Responsive adjustments */
        @media (max-width: 640px) {
          .app-header {
            padding: 1rem;
          }
          .hero-section {
            padding: 2rem 1rem;
          }
          .hero-card {
            padding: 1.25rem 3rem;
            width: 100%;
            max-width: 300px;
          }
        }
      `}</style>

      {!isHome && (
        <header className="app-header">
          <Link to="/" className="header-logo">
            Waste-to-Value
          </Link>
          <nav className="header-nav">
            <Link to="/items" className="nav-link">Items</Link>
          </nav>
        </header>
      )}

      <main className="app-main">
        {isHome && (
          <section className="hero-section">
            <div className="hero-content">
              <h1 className="hero-title">Waste-to-Value</h1>
              <p className="hero-subtitle">
                An AI-powered waste-to-value and recovery platform designed to identify, assess, and upcycle materials efficiently.
              </p>
              <Link to="/items" className="hero-card">
                ITEMS
              </Link>
            </div>
          </section>
        )}
        <Outlet />
      </main>
    </div>
  );
}
