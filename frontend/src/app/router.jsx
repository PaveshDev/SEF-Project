import { createBrowserRouter } from 'react-router'
import App from './App.jsx'
import { itemsRoutes } from '../modules/items/routes.jsx'
import { recoveryRoutes } from '../modules/recovery/routes.jsx'
import { partnersRoutes } from '../modules/partners/routes.jsx'
import { collectionsRoutes } from '../modules/collections/routes.jsx'

export const router = createBrowserRouter([
  { path: '/', element: <App /> },
  ...itemsRoutes,
  ...recoveryRoutes,
  ...partnersRoutes,
  ...collectionsRoutes,
])
