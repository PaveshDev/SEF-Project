# Member 2 Recovery Readiness Audit

**Audit date:** 2026-09-08  
**Branch:** `feature/member-2-recovery`  
**Commit:** `283a64bebf37c537d9938159612882e083865123`

## Validation Results

- Backend Release build: **passed**
- Genuine Recovery xUnit tests: **163 passed, 0 failed, 0 skipped**
- Custom Recovery checks: **9 passed**
- React lint: **passed**
- React production build: **passed**
- Flutter format check: **passed; 0 files changed**
- Flutter analysis: **passed**
- Flutter web build: **passed**
- Flutter tests: **blocked** because `flutter_test` is absent from `mobile/pubspec.yaml`
- Existing test warnings: one nullable warning and three xUnit blocking-operation warnings

## Endpoint Compatibility

| Capability | Backend | React | Flutter |
|---|---|---:|---:|
| List/create cases | GET/POST `/api/recovery-cases` | Yes | Yes |
| Update case | PUT `/api/recovery-cases/{id}` | Present, runtime defect | No |
| Plan/replan | POST `.../plan`, `.../replan` | Yes | Yes |
| List options | GET `.../{id}/options` | No | No |
| Cancel/delete case | POST/DELETE | No | No |
| List/create references | GET/POST `/api/value-references` | Yes | List only |
| Reference CRUD/verify | GET/PUT/DELETE/POST verify | No | No |
| Read/decide proposal | GET/POST decisions | Yes | Yes |
| Submit/refresh proposal | POST proposals/refresh | No | No |

HTTP verbs and implemented route paths are otherwise correct.

## Findings

1. **React case update is broken at runtime.** `RecoveryPage.jsx` calls `updateCase`, but does not import it from `recoveryApi.js`. The production build and current lint configuration do not catch this.

2. **The proposal workflow is incomplete in both clients.** Planning returns `Case`, `Options`, and `UnavailableInputs`; it does not return `proposal` or `proposalId`. Both clients look for proposal data after planning, but neither calls the proposal submission endpoint. Users cannot complete option selection and proposal creation through the current UI.

3. **Client response models are partial.** Flutter omits several backend response fields, including case revision on options, evidence, integration snapshots, proposal case/option identifiers, and match/pickup identifiers. Existing displayed fields are compatible, but the models do not represent the full DTOs.

4. **Flutter generic error handling exposes raw exception text.** `RecoveryController` uses `error.toString()` for non-Dio exceptions. Backend Problem Details handling is sanitized, but this client fallback should be replaced with a fixed safe message.

5. **Production CRUD is intentionally unavailable until integration.** `UnavailableRecoveryIntegrations` fails closed for identity, durable idempotency, assessment, matching, and pickup services. CRUD is independent of Gemini in code, but cannot operate in the production API until those adapters are supplied.

## Safety Checks

- Enum JSON serialization is consistent through explicit backend converters and client mappings.
- IDs, versions, and proposal revisions are compatible: `Guid` maps to strings, and versions/revisions are integers.
- Backend dates use `DateTimeOffset`; client request deadlines normalize to UTC. Flutter malformed proposal dates fall back to `DateTime.now()`, which is a defensive limitation.
- Gemini is registered only as the reasoning provider used by the planner.
- Deterministic valuation remains authoritative; Gemini only ranks or recommends existing options.
- Human approval remains mandatory.
- Missing production adapters fail safely with sanitized errors.
- No literal secrets or connection strings were found.
- No migration or model snapshot exists; only the shared migrations `.gitkeep` is tracked.
- No build outputs or `memory/` files are tracked.
- The audited commit changes only Member 2 ownership paths. No shared or other-member files are changed.

## Remaining Integration Requirements

- Production compilation and hosting of the Recovery agent
- Durable workflow and idempotency stores
- Authenticated identity/accessor integration
- Assessment, matching, pickup, and schema integration
- Reviewed EF migration and model snapshot
- Flutter test dependency configuration
- Client proposal workflow completion

## Worktree Confirmation

No repository source files were modified, staged, committed, pushed, merged, deleted, or branch-switched during the audit. The pre-existing untracked file `memory/gemini-integration-plan.md` remains unchanged.
