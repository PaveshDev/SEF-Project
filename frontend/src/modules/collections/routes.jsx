import CollectionsDashboard from './CollectionsDashboard.jsx'

export const collectionsRoutes = [
  { path: '/collections', element: <CollectionsDashboard /> },
  { path: '/collections/pickups/:id', element: <CollectionsDashboard /> },
  { path: '/collections/proposals/:id', element: <CollectionsDashboard /> },
]
