import { ItemsListPage } from './pages/ItemsListPage';
import { CreateItemPage } from './pages/CreateItemPage';
import { ItemDetailsPage } from './pages/ItemDetailsPage';
import { EditItemPage } from './pages/EditItemPage';

export const itemsRoutes = [
  { path: '/items', element: <ItemsListPage /> },
  { path: '/items/new', element: <CreateItemPage /> },
  { path: '/items/:id', element: <ItemDetailsPage /> },
  { path: '/items/:id/edit', element: <EditItemPage /> }
];
