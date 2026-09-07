# Recovery implementation

Implemented on `feature/member-2-recovery` following the approved design. This document describes implemented behavior and the remaining integration boundaries; it does not claim that the shared Neon schema or other members' services exist.

## Scope

The Recovery module owns RecoveryCase, ValueReference, RecoveryOption, RecoveryProposal, and ProposalDecision. Their private setters and named operations enforce transitions. EF discovers their five configurations through the existing AppDbContext assembly scan. There is no second context, migration, schema execution, external entity, or agent runtime.

Routes are Reuse, Donate, RepairThenReuse, Resell, and Recycle. All amounts use decimal/numeric(12,2), reject negatives and excess precision, and require one currency. Draft options preserve unknown values as null. Validated options require a complete estimate and identified reference evidence.

The conservative financial calculation is:

- Total cost = repair cost + pickup cost.
- Net value = max(lower estimated value - total cost, 0).
- Shortfall = max(total cost - lower estimated value, 0).

Responses include both net value and shortfall, plus the original low/high range. Donation can have a zero monetary return and a positive shortfall; no loss is silently hidden. No currency conversion is performed.

## Planning behavior

Only a verified human owner creates a case, using a current confirmed assessment. The case pins assessment/item versions. Active uniqueness is checked by the service and represented by a partial unique database index, including Approved cases.

Planning selects the latest verified reference for the matching category, condition, route, and currency (stable ID tie-break). A reference freshness-age policy is not yet agreed; no age cutoff is invented. Future policy changes should be coordinated.

The initial workflow handles owner reuse without a partner or pickup. Other routes use managed partner transfer and pickup; self-delivery is not implemented. Matching must return an eligible accepted match. Pickup must be feasible, in the same currency, within budget/deadline, and in the future. Missing sources produce explicit unavailable-input reasons. A case waits for inputs if no option can be validated.

RepairThenReuse remains AwaitingInputs until a verified repair quotation source is agreed. Gross value references do not establish repair costs, and no agent-estimated repair amount is trusted as a quotation.

Options preserve a sanitized matching/pickup snapshot for clients to select exact IDs, versions, and freshness tokens when submitting a proposal. Those snapshots are projections, not foreign EF entities.

## Proposals and decisions

Submitting freezes the selected option, estimate/evidence, case revision, and upstream versions/tokens. Proposal revisions are unique per case. Simple reuse has no dummy partner, match, slot, or pickup.

Approval revalidates the current assessment, option version, value-reference snapshots, matching, pickup, budget, currency, and deadline. Expiry is checked again after asynchronous lookups. Only a verified human owner may decide; an agent cannot acquire approval permission through output data.

A pending proposal can be explicitly refreshed. If expired or demonstrably stale, it becomes Expired/Stale and the case moves to RevisionRequested. An unavailable provider is not treated as proof of staleness. GET requests never mutate status.

There is no generic status setter or normal delete endpoint. Approved cases cannot be normally cancelled. Domain completion/fulfillment transitions exist, but there is no public completion shortcut or invented Collections fulfillment adapter.

## API

All paths are prefixed by `/api/recovery`.

| Method | Path | Purpose |
| --- | --- | --- |
| POST | /cases | Create owned case |
| GET | /cases | Latest 100 owned cases |
| GET | /cases/{caseId} | Owned case |
| PUT | /cases/{caseId}/inputs | Replace draft/revision-requested inputs |
| POST | /cases/{caseId}/planning | Plan or retry awaiting/failed planning |
| GET | /cases/{caseId}/options | Options for current case revision |
| POST | /cases/{caseId}/proposals | Submit an exact validated option |
| POST | /cases/{caseId}/cancel | Pre-approval cancellation |
| GET | /proposals/{proposalId} | Owned proposal |
| POST | /proposals/{proposalId}/decisions | Human owner decision |
| POST | /proposals/{proposalId}/refresh | Explicit stale/expiry reconciliation |
| GET | /value-references | Verified references, or curator-visible references |
| POST | /value-references | Human curator creates reference |
| POST | /value-references/{referenceId}/verify | Human curator verifies reference |

Mutations require an Idempotency-Key header. Edits and decisions carry expected versions; decisions also carry proposal revision. Monetary totals, statuses, owner IDs, and curator verification are not caller-controlled fields.

Recovery's exception filter is local to its controllers and returns sanitized Problem Details with code and traceId. Standard ASP.NET model-binding failures continue to use its built-in validation Problem Details. Database details and credentials are not returned.

## Integrator-supplied boundaries

- IAssessmentGateway, IMatchingGateway, IPickupPlanningGateway: implement the Recovery-owned DTO contracts with actual owner-module services; honor cancellation and explicit Success/Unavailable/NotFound/Invalid/Stale outcomes.
- IRecoveryActorAccessor: supply verified user identity, human/agent classification, and curator capability. Do not infer authority from request headers or body fields.
- IRecoveryCommandExecutor: supply durable operation receipts and transaction coordination. Authorize every request before receipt disclosure, including replay. Scope keys by actor and operation; reject changed payload hashes. Serialize concurrent duplicates and commit Recovery changes plus the recorded response atomically.

The executor must begin its transaction before authorization reads and avoid stale tracked entities. Approval needs serializable, consistent cross-module revalidation. Matching/pickup database writes must enlist in the same scoped AppDbContext unit of work. An adapter with external committed side effects requires an agreed durable continuation strategy before it can be connected.

The default registrations deliberately return unavailable and never execute mutation callbacks. The current API therefore returns 503 identity_unavailable for Recovery requests until shared identity is supplied. If identity is connected but durable command infrastructure is absent, writes return 503 idempotency_unavailable. This is intentional: no in-memory production idempotency or simulated services are installed.

Register real implementations before AddRecoveryModule (TryAdd preserves them), or replace the specific service descriptor in the reviewed integration change. Do not register foreign modules against RecoveryPlanningService, which would introduce dependency cycles.

## Agent boundary

Input/output DTOs and validation are implemented in Recovery only. Inputs use sanitized facts and verified evidence. Outputs identify the exact contract version, run, and case revision; recommend allowed routes; and cite supplied references. Unknown output fields, including invented approval fields, are rejected on deserialization.

No AI framework, prompt, tool, orchestration, persistence service, or endpoint is created. Client UI implementation is outside this backend change.

## Verification

Run from the repository root:

```powershell
rtk proxy dotnet build backend/WasteToValue.sln --configuration Release --no-restore
rtk proxy powershell -NoProfile -File backend/tests/Recovery/run-checks.ps1
```

The second command generates a temporary SDK console project under ignored backend/tests/Recovery/.artifacts and runs the checked-in executable assertions. It reuses the API dependencies and aligns EF Relational with the version in the existing API lockfile. It does not change a tracked project/solution/manifest/lockfile or add a test framework.

The test doubles live only under backend/tests/Recovery. Command replay tests validate service participation in the executor contract; they do not prove a production durable receipt store or database isolation. EF tests construct metadata without opening a database. Real PostgreSQL constraint/concurrency tests and owner-module integration tests remain blocked until their infrastructure exists.
