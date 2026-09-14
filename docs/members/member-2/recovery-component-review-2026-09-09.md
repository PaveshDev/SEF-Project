# Recovery component review — 2026-09-09

The component is partially implemented and is not ready for a claim that all CRUD, validation, and agent workflows work correctly. Existing automated checks pass, but targeted browser and agent diagnostics exposed defects. Application source was not changed during this review.

Scope: Member 2 Recovery backend, React frontend, Flutter frontend, and agent. Branch: `feature/member-2-recovery`. Screenshots cover React desktop (1440×1000), tablet (768×1024), mobile (390×844), and the Flutter web build at those sizes. Native Android/iOS were not tested.

[Open screenshot gallery](screenshots-2026-09-09/index.html) · [Screenshot index](screenshots-2026-09-09/README.md) · [Database readiness](database-readiness-check-2026-09-09.md)

## Highest-priority findings

### 1. Agent valuation excludes the returned pickup cost — reproduced

The current agent calculates valuation with zero pickup/repair costs before fetching pickup feasibility, then attaches the pickup result without recalculating valuation. A targeted diagnostic using the existing test fixtures and an immediately successful fake reasoning provider returned:

| Scenario | Expected | Actual |
| --- | --- | --- |
| Resell, proceeds low LKR 100, pickup quote LKR 20 | Include LKR 20 pickup; net LKR 80 | Pickup valuation LKR 0; net LKR 100; `AwaitingApproval` |
| Pickup quote LKR 75, budget LKR 50 | Reject/exclude the option | `AwaitingApproval`, accepted reasoning |
| Tool returns a confirmed assessment for another item | Reject the mismatched assessment | `AwaitingApproval`, accepted reasoning |

Each scenario made one immediate fake reasoning call and returned no agent error. These are not Gemini timeout failures. No real Gemini call, database write, or production agent run occurred. Future real proposal adapters may reject inconsistent proposals, but the current agent itself does not enforce these boundaries correctly.

Source: `agents/recovery/RecoveryPlannerAgent.cs`, assessment checks around lines 208–212, valuation around 221–227, pickup selection/attachment around 251–259. Diagnostic source: [AgentDiagnostic.cs](ui-audit-2026-09-09/AgentDiagnostic.cs). The diagnostic references the existing compiled Recovery test fixtures; its temporary project was built outside the repository.

The agent also selects references using currency/date without carrying route/category/condition fields in its reference contract. That requires reconciliation with the stricter backend planner before integration; it was identified by source inspection, not a separate live test.

### 2. Production agent execution is absent

`agents/recovery/README.md` and the project files confirm the orchestration is compiled into the test project only. The API registers a Gemini reasoning provider, but ordinary CRUD and deterministic planning do not invoke the agent. Runtime hosting, durable workflow storage, real tool adapters, verified identity, and durable command execution remain missing. Avoiding a Gemini timeout cannot supply those integrations.

Existing tests cover strict reasoning output parsing, transient retry behavior, timeouts, cancellation, invalid responses, fallback behavior, and the human-approval boundary. They use fake providers/HTTP handlers. No live Gemini behavior is verified.

### 3. React edit silently removes other preferred routes — reproduced

Opened a case containing `['Reuse', 'Donate']`, changed only its objective, and intercepted the update request. The frontend submitted `preferredRoutes: ['Reuse']` with `expectedVersion: 1`. The form initializes only the first route and always sends a one-element array. An accepted update would remove the other preference.

Source: `frontend/src/modules/recovery/pages/RecoveryPage.jsx:33` and `:52`. The diagnostic intercepted the request; nothing was saved.

### 4. React loses the existing proposal when a case is reopened — reproduced

Using simulated API responses, planning, option selection, proposal creation, and decision submission worked during one visit. After returning to All cases and reopening the same case, the UI displayed “Create a proposal from a selected option to review it here.” Its requests fetched only the case and options; it did not fetch the existing proposal.

`RecoveryWorkflow.jsx` initializes `proposalId` as an empty string, and the case DTO does not include a proposal ID. The same initialization prevents recovery of a pending proposal on a fresh visit. This needs an API/UI contract for locating existing proposals, not just local component state.

### 5. The Agent monitor is a static display

The React `AgentPanel` always displays assessment/references/dependencies completed and “Awaiting approval.” It does this even while the actual API returns `identity_unavailable`, with no running agent or selected case. This screen must not be presented as proof of real agent execution.

### 6. CRUD coverage is incomplete in the clients

| Capability | Backend endpoint/service | React UI | Flutter UI/service |
| --- | --- | --- | --- |
| Create/list/get cases | Present | Present | Present |
| Update case inputs | Present | Present, route-loss defect | Absent |
| Delete/cancel cases | Present, business-history restrictions | Absent | Absent |
| Plan/replan and read options | Present | Present | Present |
| Submit/read/decide proposals | Present | Present, reopening defect | Present; live success unverified |
| Refresh proposal through refresh endpoint | Present | Not called | Not called |
| List value references | Present | Present | Service exists; no equivalent curator screen found |
| Create value reference | Present | Present; hardcodes Good/Reuse/LKR/current time | Absent |
| Update/delete/verify value references | Present | Absent | Absent |

Recovery options are workflow-generated, and proposal decisions preserve history; unrestricted CRUD is not appropriate for every object. However, case deletion/cancellation and reference-management endpoints already exist without corresponding client controls.

Backend CRUD is not verified against PostgreSQL. Live reads return HTTP 503 `identity_unavailable`; the earlier read-only database check found no application tables or migration history. The connection was not persisted in backend configuration.

### 7. Backend proposal creation returns a wrong Location path — source finding

`RecoveryCasesController.cs:46` returns `/api/recovery/proposals/{id}`, while the actual controller uses `/api/recovery-proposals/{id}`. A client following the Location header would target an unregistered route. The current React client uses the response body ID, so this does not explain its present 503.

## Validation and frontend behavior

| Check | Observation |
| --- | --- |
| React empty case form | Native required validation blocks submission and focuses Item identifier |
| React negative budget / three decimal places | Browser validation rejects both |
| React malformed UUID / whitespace objective / one-character numeric currency / past deadline | Browser form reports valid; these reach server validation |
| React invalid category UUID / low value greater than high | Browser reference form reports valid |
| Backend invalid UUID / unknown enum / missing required body fields | Actual API returns 400 with the expected binding/validation fields; requests also omitted Idempotency-Key and its validation error was present |
| Backend domain validation | Code validates nonempty identifiers, text bounds, uppercase three-letter currency, money precision/range, future deadlines, allowed routes, assessment ownership/revision, optimistic concurrency, and proposal dependencies |
| React option/approval gating | In preview, unvalidated Donate option was disabled; requesting revision required a comment |
| React API errors | Cases display the actual unavailable error with Retry; reference-list failures are swallowed and displayed as an empty list |
| React form keyboard behavior | Opening leaves focus on New recovery case; next Tab goes to background Cases button; Escape does not close the form |
| React responsiveness | No document horizontal overflow in tested tab/workflow states at 1440, 768, or 390 pixels |
| React form on mobile | Form scrolls internally; bottom actions require scrolling; a separate bottom-of-form screenshot is included |
| Flutter empty case form | Displays two Required field errors; Escape dismisses the bottom sheet |
| Flutter input validation, source inspection | Item ID only checks nonempty; currency only checks length; budget has no validator and invalid text becomes null via `double.tryParse`; same-day deadline selection can represent a past midnight |

Backend domain validation is stronger than frontend validation, but live authorization, database constraints, real transaction rollback, and integration freshness handling remain unverified. Passing isolated validation tests does not establish those runtime guarantees.

The React layout contains the main recovery screens and adapts to the tested widths. Its giant heading, large empty regions, limited field guidance, static agent status, and incomplete action coverage still need product/UX review. No visual redesign was made as part of this audit.

Flutter was launched at `http://localhost:5174/#/recovery`. Its screenshots show the running Flutter web build, not React at a mobile width. Flutter showed the same real identity integration error. Browser diagnostics included one Dart debug-environment warning and the expected API 503; React live errors were the expected API 503 responses. The successful simulated React workflow produced zero page exceptions.

## Checks and their limits

- Recovery xUnit suite: **163 passed, 0 failed, 0 skipped**, run earlier in this same session against unchanged application code. Includes agent and Gemini adapter tests using doubles. Four existing test warnings: one nullable and three blocking-task warnings.
- Custom Recovery runner: **9 assertions passed** during this review.
- React `npm run lint`: **passed**.
- React `npm run build`: **passed**.
- Flutter `flutter analyze --no-pub`: **passed**; no issues.
- Flutter web development compilation/startup: **passed**.
- Additional agent diagnostics: **three defects reproduced**, despite a timely valid fake reasoning response.
- Actual API malformed-request checks: **three HTTP 400 responses**; cases/references reads remain **503**.
- Simulated React flow: list → detail → plan → select → create proposal → approve → refresh rendered successfully. Reopening lost the proposal; objective-only edit dropped a route.
- Frontend has no configured test script or separate type-check script. Flutter has no `flutter_test` dependency; no Flutter automated test suite is claimed to pass. Android/iOS, live Gemini, successful database CRUD, and cross-member end-to-end execution remain unverified.

## Screenshots and reproduction

The gallery separates **live application screenshots** from **simulated React previews**. Preview files have `-preview` in their names and a visible explanatory banner. All API requests in the preview page were intercepted inside Playwright; their success states prove only frontend behavior for supplied responses. The audit did not seed the shared database or run migrations.

Preview capture source: [capture-previews.js](ui-audit-2026-09-09/capture-previews.js). Its synthetic dates and fixtures are fixed for this audit. Run it through Playwright MCP with the React development server running; it creates and closes a separate page with page-scoped request interception.

The screenshot set covers all four React tabs, the new-case form, empty/populated lists, references, case detail/edit, options, proposal creation/review, decision success, and the reopening defect. Flutter coverage includes the reachable case list, create form, and required-field errors. Screens needing real integrated records cannot be demonstrated as live success while identity and schema integration are absent.

Recommended order: fix the reproduced agent and React data-integrity issues, add meaningful regression coverage, complete missing client actions and proposal recovery, integrate identity/schema/stores/adapters, then test successful CRUD and complete workflows against an isolated database before the shared group database.
