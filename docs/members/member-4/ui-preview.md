# Pickup & Handover UI preview

## Scope and entry points

The supplied Member 4 specification describes the eventual full component. This delivery implements its presentation layer using fictional, explicitly labeled session data, in accordance with `AGENTS.md`'s current restriction on business logic, migrations, authentication, and agent implementations. No real pickup, assignment, approval, or handover is executed.

- Branch: `feature/member-4-collections`.
- React: `/collections`, registered in the existing module route array.
- Flutter: `/collections`, registered in the existing module route list.
- The shared home screens remain the project skeleton. Open the web route directly. On Flutter, start the app with the initial route, for example `flutter run --route=/collections`, or navigate to `/collections` through GoRouter during integration.
- All changes are in Member 4's frontend, mobile, and documentation directories. No new dependency or shared-file change is required by this code.

Once the existing toolchain and locked dependencies are available, run `npm ci` and `npm run dev` from `frontend/`, then open the dev server's `/collections` path. For mobile, restore the existing dependencies with `flutter pub get --enforce-lockfile` before running the application. Shared navigation and cross-client routing demonstrations need a separately reviewed integration change.

## Delivered screens and interactions

| Area | React staff preview | Flutter collector/owner preview |
| --- | --- | --- |
| Overview | Counts derived from local fixtures, sample weekly calendar, activity table | Summary cards, next pickups, failed collection notice |
| Pickup requests | Create, read, edit, delete local drafts; search and status filters | Searchable sample job list with status filters and details |
| Availability | Add, edit, delete local collection slots; capacity and collector fields | Pick, edit, or clear a preferred date/time window |
| Assignments | Edit a scheduled fixture's collector and vehicle display fields | Read sample assigned vehicle and handling requirements |
| Proposal review | Static recommendation, constraints, fallback, review notes and decision preferences | Same illustrative constraints and decision preferences |
| Failed collections | View changed constraint, inspect slots, draft revision notes | Draft failure notes; explain the future re-planning sequence |
| Status/history | Read sample history; a local cancellation preview appends a reason | View milestone state; preview a status submission without changing it |
| Handover | Six-digit input format validation and a local proof filename selection | Six-digit input format validation and a local proof note |
| Reports | Fictional completed handovers, count cards, JSON export labeled as demo | Completed sample pickup details |

Both clients explicitly report that QR scanning and real handover verification are unavailable. No code value can change a pickup to Handover Verified. Proof data is not uploaded. The completed lamp handover is an existing fictional fixture, not a result of verifying a code.

Travel distance, duration, and cost are unavailable rather than invented. The 14:00–16:00 recommendation is a static illustration, not an agent output. Its arrival time is unknown. The 16:00–18:00 fallback conflicts with the sample destination closing time of 17:00 and requires revision. Edited slots do not recompute these examples.

## State and validation boundaries

React state resets on refresh or leaving the route. Flutter availability and failure notes stay in memory while the collections screen exists; other detail input is discarded when the detail closes. The two clients do not synchronize. The owner/collector selector is a presentation preview with no authorization semantics.

Required fields, nonblank reasons, time-window order, positive capacity, and the illustrative six-digit code format are UI input checks only. Slot conflicts, item/vehicle dimensions, capacity allocation, destination arrival, code expiry, authorization, and allowed status transitions are not validated here. Neither client treats its local checks as a backend decision.

## Validation performed on 8 September 2026

| Check | Actual result |
| --- | --- |
| `git diff --check` | Passed for tracked changes; new files also checked for whitespace separately |
| `node --check frontend/src/modules/collections/demoData.js` | Passed |
| `npm ci --no-audit --no-fund` | Blocked: registry returned HTTP 403 for the pinned `vite-8.2.2.tgz` after an initial sandbox/cache failure |
| `npm run lint` | Blocked: `oxlint` unavailable because dependencies could not be installed |
| `npm run build` | Blocked: `vite` unavailable because dependencies could not be installed |
| Isolated JSX syntax/format tooling | Blocked: registry also returned HTTP 403 for `@babel/parser`; no project manifest or lockfile changed |
| Flutter analyzer / Dart formatter | Blocked: `flutter` and `dart` are not on PATH |
| Browser/mobile execution | Not performed; frontend dependencies and mobile tooling are unavailable |

The installed Node version is 24.15.0; the repository requests 24.18.0. npm reported this engine mismatch. Do not interpret the source/whitespace checks as a successful build, UI test, or Dart analysis. No placeholder tests were added.

## Manual checks once the toolchain is restored

1. Open `/collections` on desktop and a narrow viewport. Navigate every section using keyboard focus. Open and dismiss dialogs using their buttons and Escape.
2. Select calendar days with and without fixtures. Search jobs and combine the search with status filters, including an empty result.
3. Create/edit/delete a draft. Try blank/whitespace fields and an end time before the start. Confirm the date's actual year is displayed in the jobs table.
4. Add/edit/delete a slot and reject an invalid time window. Confirm changing a slot never silently creates a real booking or recalculates the sample proposal.
5. Open a scheduled pickup. Preview assignment changes and cancellation with a reason. Inspect local history.
6. Open the proposal. Require notes for rejection/revision; confirm each choice is labeled as a local preference and the booking stays unconfirmed.
7. Draft a revised request for the failed refrigerator collection. Confirm no agent-generated plan is claimed.
8. Try invalid and valid six-digit handover entries. Neither must verify the job. Confirm selecting proof only displays a filename on web, and nothing uploads.
9. Export the demo report and inspect its `demo: true` and `persistent: false` markers.
10. On Flutter, exercise both presentation modes, job filtering, availability editing, detail navigation, failure notes, and returning to the list. Check narrow layouts and larger text settings.
11. Refresh/reopen the clients and confirm session-only changes disappear. No synchronized cross-client state should be claimed.

## Remaining full-component implementation

The complete attached specification remains future work. It needs explicit authorization for backend and agent development and a reviewed integration agreement for shared contracts, persistence, and authentication.

1. Agree on the approved recovery-proposal input with Member 2 and destination/handling information with the relevant owners. Define principal entities: Pickup Request, Collection Slot, Pickup Assignment, Handover Record, Reschedule Request, Collection Status History. Keep all persistence in the existing `AppDbContext`; the leader coordinates the first migration under `docs/migration-policy.md`.
2. Implement ASP.NET-owned draft/slot CRUD and recorded confirm, cancel, reschedule, collection, delivery, and handover operations. Enforce authorization, version/concurrency checks, allowed transitions, slot/capacity allocation, and audit records on the server.
3. Add a routing adapter behind ASP.NET with validated coordinates, timeouts, error handling, and structured distance/time responses. Missing or invalid responses must produce "Travel estimate unavailable — manual review required" and no fabricated values.
4. Implement the Collection Agent with the allow-listed tools `ReadCollectionSlots`, `CheckVehicleCapacity`, `ReadHandlingRules`, `GetTravelEstimate`, `ProposePickup`, and `ProposeReschedule`. Validate every tool input and return structured results. Evaluate all constraints, rank feasible options, provide a fallback, and re-plan after changed constraints. Tools must not bypass backend rules or confirm a pickup.
5. Persist the agent plan, each step, tool results, validation results, approval status, and final outcome. Resume after an authorized human approves, rejects, or requests revision. Revalidate before confirming the booking to account for changed availability.
6. Implement job-bound, expiring, single-use QR/one-time tokens and server-controlled handover verification, plus proof storage. Coordinate camera permissions and any mobile dependencies through integration review.
7. Connect both clients through their shared ASP.NET clients, replace fixtures, and implement loading, empty, validation, authorization, stale-data, and external-failure states. Neither client may access Neon or the agent/routing service directly.
8. Demonstrate a real persisted cross-platform workflow: trigger in one client, review in the other, confirm on ASP.NET, collect/deliver, verify handover, then read the updated outcome and audit history from both clients. Validate against an isolated database before any shared-database integration.
