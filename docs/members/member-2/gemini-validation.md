# Recovery reasoning validation

## Implemented behavior

RecoveryPlannerAgent calls IRecoveryReasoningProvider after deterministic valuation, authoritative option persistence, and the existing matching/pickup steps. Its request projects only case/category, assessment condition/function/evidence, eligible option IDs and routes, deterministic financial values, matching/pickup summaries, and relevant constraints. Owner identity and full upstream payloads are excluded.

RecoveryReasoningValidator independently validates the provider response at the agent boundary. It requires a complete, unique permutation of supplied option IDs, a matching first recommended route, valid confidence, bounded summaries, and supplied evidence references. Every accepted recommendation requires human review. Ranking reorders existing alternative objects; it cannot replace their valuation or recipient.

The Gemini response has exactly the ten approved nonfinancial fields. Strict parsing rejects missing, unknown, duplicate, mistyped, malformed, empty, oversized, incomplete, blocked, thought-part, and tool-call outputs. The request contains no callable tools. User prose remains data, including instructions embedded in assessment evidence.

The HTTP adapter uses a total configurable timeout, caller cancellation, bounded transient-only retries, limited streamed reads, and fixed safe errors. Authentication, validation, redirects, permanent failures, and invalid output are not retried. Logs contain no HTTP headers, prompt bodies, response bodies, or exception details.

Ordinary Recovery CRUD and deterministic planning have no reasoning-provider dependency. ValueEstimationService and the existing proposal validators were not changed.

## Failure and approval

Provider failure records a fixed safe code in workflow ErrorSummaries and a failed recommendation ValidationResult. A valid existing deterministic path retains its existing route order and produces no fabricated recommendation. Stale fallback values are excluded. If no valid fallback remains, the result is Failed with IntegrationUnavailable.

Both accepted recommendations and successful fallback proposals pause in WaitingForApproval. A false approval-request result is a failure. Early tool failures are finalized, and caller cancellation records workflow_cancelled before propagating cancellation. A persistence failure is explicitly reported as workflow_state_unavailable and requires host reconciliation.

The test-only workflow store persists outcome metadata, route ordering, and safe failures. It has no prompt or hidden-reasoning property. Model explanations are returned with the draft/result, not written into workflow state.

## Checks performed

- Backend Release build: passed with zero warnings and errors.
- Genuine Recovery xUnit project: 163 passed, zero failed/skipped.
- Separate custom runner: passed its current 9 assertions. Historical larger assertion totals in earlier notes are not this runner's current output.
- Four existing warnings remain in unchanged tests: CS8629 in RecoveryCaseTransitionTests and three xUnit1031 warnings in RecoveryIntegrationContractTests.
- Automated tests made zero real Gemini calls. Agent tests use fake providers; provider transport tests terminate in a fake HttpMessageHandler. A DI-selection test resolves the configured provider without calling it. No user secrets or environment configuration are loaded by the test setup.
- Credential-pattern scan: no matches in Member 2 agent, backend, test, documentation, React, or Flutter source. Generated build artifacts are excluded. The scan reports filenames only and is a pattern check, not a guarantee against every possible secret format.
- git diff --check: passed.
- Baseline content hashes confirm changes are confined to the four authorized folders; pre-existing DTO, React, Flutter, and memory changes were preserved.
- No Program.cs, AppDbContext, appsettings, project/package manifest, tracked lockfile, migration, model snapshot, connection string, or other-member file changed. No Neon connection, live Gemini call, commit, or push occurred.

The existing custom runner generated ignored SDK project/build files under backend/tests/Recovery/.artifacts; it did not change tracked manifests.

## Remaining integration

The API provider is registered, but the planner remains compiled only by the test project. Production hosting, durable workflow state, and real application-service tool adapters are required before runtime use. See [the exact integration request](gemini-integration-request.md). Live Gemini behavior and durable-store reconciliation are not verified by these offline tests.
