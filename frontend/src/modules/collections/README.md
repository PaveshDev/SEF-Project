# Collections

Member 4's staff UI preview is available at `/collections` through the existing `collectionsRoutes` extension. The shared home page remains the skeleton.

The responsive dashboard uses the `useCollections` hook, which attempts to load data from the ASP.NET backend API (`/api/collections/`) on mount. When the API is reachable, all CRUD operations (pickups, slots, handover, agent proposals) are executed through the backend and state is updated from the API response. When the API is unreachable, the hook falls back to local demo fixtures and all changes stay in React memory.

The dashboard displays a connection indicator: a green dot and "API" label when connected, or an amber dot and "Demo" label when using local data. The banner text updates accordingly.

Styles stay in `collections.css`; fixtures are in `demoData.js`. Both proposal options are static illustrations independent of edited slots. Missing travel estimates and costs remain unavailable. The sample week is 7–13 September 2026; jobs outside it remain visible in the pickup table.

## API service layer

`services/collectionsApi.js` wraps all backend HTTP calls with axios. Each function catches network errors and falls back gracefully:
- **Slots**: `fetchSlots`, `createSlot`, `updateSlot`, `deleteSlot`
- **Pickups**: `fetchPickups`, `fetchPickup`, `createPickup`, `updatePickup`, `deletePickup`, `fetchPickupEvents`, `reschedulePickup`
- **Handover**: `verifyHandoverCode`, `submitHandoverProof`, `fetchHandoverProofs`
- **Agent**: `prepareCollectionPlan`, `rescheduleAgent`, `fetchProposal`, `approveProposal`

The API base URL defaults to `http://localhost:5062` and can be overridden with the `VITE_API_URL` environment variable.

See the [Member 4 delivery notes](../../../../docs/members/member-4/ui-preview.md) for usage, scope, checks, and integration work. Coordinate shared-file changes with the leader; see [ownership](../../../../docs/ownership.md).
