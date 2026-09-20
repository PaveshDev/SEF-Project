import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AppShell } from './components/AppShell';
import { MyItems } from './pages/MyItems';
import { AddItem } from './pages/AddItem';
import { ItemDetails } from './pages/ItemDetails';
import { EditItem } from './pages/EditItem';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<AppShell />}>
          <Route index element={<Navigate to="/items" replace />} />
          <Route path="items" element={<MyItems />} />
          <Route path="items/add" element={<AddItem />} />
          <Route path="items/:id/edit" element={<EditItem />} />
          <Route path="items/:id" element={<ItemDetails />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
