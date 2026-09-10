# Recovery fixes and integration handoff

Implementation started 9 September 2026; verification continued 10 September 2026.

Recovery-owned fixes have been implemented. This is **not yet a working authenticated end-to-end deployment**. The team confirmed that Items, Partners, Collections and authentication implementations have not been committed. Sharing a SQL reference and folder layout does not provide those runtime contracts.

## Changes implemented

| Area | Result |
| --- | --- |
| Case forms, React and Flutter | Preserve all preferred routes; validate nonempty UUIDs, objective, currency, money and future deadlines. Flutter includes time selection. |
| Case actions | Edit active inputs, cancel eligible cases and delete eligible cases without retained history. Both clients send expected versions for edit/cancel. Deletion explains history restrictions and requires confirmation. |
| Editing and replanning | Editing Draft, Planning, AwaitingInputs, AwaitingApproval or RevisionRequested creates a new case revision; non-draft edits move to RevisionRequested. Old options and pending proposals become stale. Replan creates another revision and new options. Approved/completed cases stay locked. |
| Proposal recovery | Owned `GET /api/recovery-cases/{caseId}/proposals` returns history newest first. Both clients discover proposals when reopening a case. History selectors and explicit revalidation are available. |
| Proposal creation | Correct Location header uses `/api/recovery-proposals/{id}` routing. Expiry must be future and no later than the case deadline. Flutter caps short or long relative expiry choices at that deadline. |
| Decisions | Revision requests require a nonblank comment in the domain as well as the UI. Existing owner, human, version and expiry checks remain. |
| References | Both clients support authorized curator create, edit, verify and delete, with condition, route, currency, observation timestamp and source inputs. Verified references are immutable. Search, pagination, permission loading and retry/error states are present. |
| Capabilities | `GET /api/recovery/access` exposes human/curator capabilities through the configured identity accessor. No fabricated identity was installed. |
| React accessibility | Native modal dialog, initial focus, keyboard containment, Escape dismissal, focus restoration and field error associations. Smaller headings and responsive layouts retain the existing design. |
| Request recovery | React retains uncertain mutation keys in memory and session storage using a payload digest. Flutter retains keys for identical uncertain requests during the service lifetime and retains the submitted proposal expiry for retries. Confirmed saves are distinguished from failed refreshes. |
| Search ordering | React ignores older list responses; Flutter disables busy interactions and retains filters/page during refresh. |
| Response validation | Flutter rejects unknown enums, malformed essential identifiers/versions/timestamps and invalid money. Both clients refuse incomplete or invalid option estimates. |
| Financial policy | Low proceeds minus repair and pickup determines the conservative outcome. Net value and shortfall are separate nonnegative amounts. Donation proceeds are zero. Total costs are calculated on the backend, not recomputed with floating point in Flutter. |
| Reference selection | Category, condition, route, currency, verification and observation window are filtered before pagination. Equal timestamps have deterministic ID ordering. Planning and approval use the same 30-day reference age. |
| Query validation | Accepted sorts, pagination bounds, search length, enum values, currency and observation range are checked. |
| Agent integrity | Reloaded assessment identity/revisions must match. References carry verified category/condition/route metadata. Pickup must match the accepted partner and satisfy identity, version, token, schedule, currency, budget and deadline checks. Actual pickup cost is recalculated before final option persistence. |
| Agent persistence contract | `FinalizeRecoveryOptionsAsync` must preserve authoritative IDs and return the full final estimate, including shortfall. A missing implementation fails safely. Matching/pickup operation IDs are stable for the same run/revision/route/step. |
| Agent approval | Resume requires an injected authoritative approval verifier. The supplied verifier uses the owned proposal service and checks the actual recorded decision and proposal revision/case. A client-supplied approval alone is insufficient. |
| Agent UI | React explicitly displays “Agent integration unavailable.” No simulated running/approval progress is presented as a production agent. |

## Verification

- ASP.NET/agent suite: **187 tests passed** after the transition and approval-authority changes. Tests use in-memory/fake integration infrastructure; they do not establish PostgreSQL atomicity or production agent hosting.
- Existing compiler/analyzer warnings remain in older tests: one nullable-value warning and three xUnit warnings about blocking tasks.
- React lint and production build: passed. Flutter analysis: passed with no issues. Flutter web release build passed, with a missing CupertinoIcons font-family warning from the shared dependency configuration; no missing Recovery icons were observed in the browser captures.
- React validation suite: five tests. Run `rtk proxy node --test src/modules/recovery/tests/validation.test.js` inside `frontend`.
- React browser regression harness: route preservation, proposal reopening, revision comment validation, malformed input rejection, keyboard containment, Escape/focus restoration and unavailable agent state. Captures at 1440, 768 and 390 pixels are labeled simulated data. See `capture-web.js` and `screenshots/`.
- Flutter model/controller/widget suite: six tests passed through the isolated harness. It covers malformed response rejection, form route preservation, proposal reopening, retained filters, confirmed-save/failed-refresh handling and uncertain mutation retry identity.
- Run the Flutter tests from the repository root with `rtk proxy powershell -NoProfile -File docs/members/member-2/fixes-2026-09-09/run-flutter-tests.ps1`. The harness depends on this actual mobile package and uses `flutter_test` in a temporary package. The checked-in test source has a `.dart.template` suffix until shared manifest integration, so ordinary project analysis does not fail on an undeclared test dependency.
- Native Android/device testing is blocked: the environment exposes Windows, Chrome and Edge, but no connected Android device. Browser screenshots cannot establish native networking, keyboard inset or platform navigation behavior.
- .NET checks used SDK **8.0.425**, running from the parent directory with absolute project paths because the original repository pin was 8.0.424. On final inspection, the working tree already contained a separate change updating `global.json` to 8.0.425; that change was preserved.

- React CRUD browser check: reference error/retry, create/update/verify/immutable behavior/delete, case cancellation and eligible deletion passed using simulated responses (`check-web-crud.js`).
- React lost-response browser check: uncertain request retains its key across reload; a subsequent new operation gets a new key (`check-web-retries.js`).
- Flutter browser check: editing preserves both routes and the changed objective; proposal creation/reopening succeeds with simulated responses; reference forms display validation. Captures use the Flutter web accessibility bridge plus keyboard input. Native Android behavior is not covered.

Open [the screenshot gallery](index.html) for 62 screenshots: 36 React previews, 24 Flutter previews, and two live unavailable states. Preview screenshots explicitly use simulated API data, without database or Gemini execution. The final React capture rerun passed all workflow and keyboard checks with no page errors or horizontal overflow at 1440, 768 and 390 pixels.

## Running locally

From the repository root, start React:

```powershell
rtk proxy npm.cmd --prefix frontend run dev
```

With the repository-pinned .NET SDK installed, start ASP.NET:

```powershell
rtk proxy dotnet run --project backend/src/WasteToValue.Api --launch-profile http
```

Flutter (run inside `mobile`):

```powershell
rtk proxy flutter run -d web-server --web-hostname localhost --web-port 5174 --dart-define=API_BASE_URL=http://localhost:5080
```

Open React at `http://localhost:5173/recovery` and Flutter at `http://localhost:5174/#/recovery`. API port: `5080`. Use Flutter port 5174 because the existing development CORS policy permits localhost ports 5173 and 5174. Port 5177 used by the simulated Flutter capture script does not permit live API calls. Until the shared identity accessor and other integrations are registered, the real application displays the explicit unavailable state. The screenshot scripts are tests, not an alternate production backend.

The final live Flutter check serves the existing release build on port 5174 using `rtk proxy python -m http.server 5174 --bind 127.0.0.1 --directory mobile/build/web` from the repository root. Rebuild with `rtk proxy flutter build web --dart-define=API_BASE_URL=http://localhost:5080` inside `mobile` after changing Flutter code.

## Shared build change prepared for review

`shared-build-integration.patch` adds the existing agent sources to the production API assembly, removes duplicate test-only source inclusion and declares Flutter's test SDK dependency. It has not been applied to shared manifests. On this CRLF checkout, check it with:

```powershell
rtk proxy git apply --check --ignore-space-change docs/members/member-2/fixes-2026-09-09/shared-build-integration.patch
```

After the integrator applies it, run the package manager to regenerate the Flutter lockfile and rename the Recovery test template to `readiness_test.dart`. Production source compilation alone does not host the agent or provide persistence and real tools. Review and implement the runtime composition below before enabling it.

## Contracts still required from the team

| Dependency | Required integration |
| --- | --- |
| Authentication | Verified nonempty user identity, agreed key mapping to `app_users`, human/agent distinction and curator role. Supply `IRecoveryActorAccessor`; both clients continue to call ASP.NET only. |
| Items | Owned item/category selectors and `IAssessmentGateway`: current confirmed assessment, matching owner/item/assessment IDs, revisions, category, condition and function. Manual UUID entry remains explicitly marked until the selector contract exists. |
| Partners | `IMatchingGateway` returning accepted eligible matches linked to authoritative Recovery option IDs, versions and freshness tokens, plus revalidation. |
| Collections | `IPickupPlanningGateway` with real quotes, currency, schedules, deadline/budget enforcement, stable operation receipts and revalidation. |
| Repair | Verified repair-quotation contract. RepairThenReuse remains unavailable; zero repair cost is never substituted. |
| Transactions/idempotency | Durable `IRecoveryCommandExecutor` with actor/operation/key uniqueness, payload conflict checks, stored response replay, transaction rollback and restart recovery. Existing unavailable defaults remain. |
| Agent host | Register tools, `IRecoveryWorkflowStore`, reasoning provider, approval verifier and lifecycle coordinator. Implement authorized start/status/result endpoints and reconciliation after crashes or failed state writes. |
| Agent provenance | Internal proposal submission must reload authoritative data, use the ordinary business validators and record Agent origin/run ID. Public human submission must not accept spoofed provenance. |
| Fulfillment | Agree an idempotent approval handoff and an authorized completion callback carrying actual fulfillment evidence. Approval must not mark a collection complete. No invented Collections endpoint or duplicate fulfillment logic was added. |
| Cross-module consistency | Protect approval against assessment, match, pickup or quote changes between validation and commit using the team's transaction/integration design. |
| Gemini | Supply enabled/model/key/timeout/retry settings privately on ASP.NET. Live provider behavior has not been verified; existing tests use fake HTTP responses. A timely Gemini answer still cannot override failed deterministic validation. |
| Persistent mobile retries | Current Flutter retry identity survives retries within its service instance. App-process restart recovery needs the team's approved local persistence and identity/session policy. Server durable receipts are also still required. |

## Database reconciliation — do not apply the reference SQL unchanged

The earlier authorized read-only audit found the supplied Neon database reachable but without application tables or EF migration history. **This implementation did not modify the shared database or generate/apply a migration.** `database/WasteToValue-Initial-Schema.sql` remains a reference, not a second schema-management path.

These current Recovery model columns are absent from that SQL:

These properties already exist in the Recovery model; do not add duplicate properties or copy illustrative migration code from older handoff/checklist drafts. Use the actual module interfaces and configurations as the integration contract. The reference lookup intentionally requests one newest qualifying reference after all evidence filters; increasing its page size is not a required fix.

| Table | Missing columns |
| --- | --- |
| `recovery_cases` | `assessment_version`, `item_revision`, `revision` |
| `recovery_options` | `assessment_version`, `case_revision`, `estimated_shortfall`, `integration_snapshot`, `requires_partner`, `requires_pickup` |
| `recovery_proposals` | `agent_run_id`, `case_revision`, `estimate_snapshot`, `input_snapshot`, `match_freshness_token`, `match_version`, `option_version`, `pickup_freshness_token`, `pickup_plan_version`, `recommendation_origin` |

The integrator must also reconcile all of the following before generating the initial migration:

1. Actual shared authentication/user key mappings and all four modules' entity configurations.
2. Foreign keys to Items, assessments, users, categories, matches and pickup plans. This branch cannot correctly map entities absent from teammates' commits.
3. Nullable match/pickup linkage for owner reuse; the SQL currently requires both on every proposal.
4. Nullable draft financial estimates; the SQL's zero defaults would misrepresent unknown estimates.
5. Restrictive option history deletion; the SQL currently cascades case deletion to options while EF restricts it.
6. Composite proposal/revision decision integrity, active-case uniqueness and durable command receipt uniqueness.
7. Money precision, nonnegative surplus/shortfall balance, status conversions, positive revisions and optimistic concurrency.
8. Durable workflow state and command receipts, with safe failure/retry metadata and indexes for actual queries.
9. Isolated development fixtures for users, confirmed assessments, references, accepted partners and available pickups.

Generate the initial migration from the **single reconciled AppDbContext** into `backend/src/WasteToValue.Api/Infrastructure/Persistence/Migrations/`. Test its SQL, constraints, upgrade behavior, concurrent writes, rollback and restart-safe replay against an isolated PostgreSQL database first. The nominated integrator can then review and apply the shared rollout. Do not execute both reference SQL and an equivalent EF initial migration.

## Acceptance still blocked

Real database CRUD, cross-owner authorization through actual authentication, PostgreSQL concurrency/atomicity, durable idempotency after restart, live Gemini, production agent restart/resume, real fulfillment and the complete authenticated item → assessment → recovery → proposal → decision → fulfillment flow must be tested after the missing integrations land. The local tests and screenshots are evidence for Recovery code and client behavior, not evidence that those external integrations already work.
