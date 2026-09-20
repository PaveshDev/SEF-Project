import React from 'react';
import { Link, Outlet } from 'react-router-dom';
import { Package } from 'lucide-react';

export const AppShell = () => {
  return (
    <div className="flex h-screen bg-background-main">
      <div className="w-64 bg-primary-dark text-white flex flex-col">
        <div className="p-4 text-xl font-bold border-b border-primary/20 flex items-center gap-2">
          <Package /> LoopWorth
        </div>
        <nav className="flex-1 p-4 flex flex-col gap-2">
          <Link to="/" className="px-3 py-2 rounded hover:bg-primary transition-colors">Dashboard</Link>
          <Link to="/items" className="px-3 py-2 rounded hover:bg-primary bg-primary transition-colors">My Items</Link>
        </nav>
      </div>
      <div className="flex-1 flex flex-col overflow-hidden">
        <header className="h-16 bg-white border-b border-border-main flex items-center px-6">
          <h1 className="text-xl font-semibold text-text-main">Items</h1>
        </header>
        <main className="flex-1 overflow-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
