# Member 2 client workflow implementation review

The attached readiness-fix prompt was partially implemented when this review began. The existing update import was present, but the workflow was not functional. The remaining fixes are now implemented and verified as described below.

Branch: `feature/member-2-recovery`. Starting commit: `283a64b` (`feat(recovery): complete Member 2 recovery and Gemini workflow`). Branch, worktree status and latest commit were checked before editing. No branch switch, commit or push was performed.

1. **Exact files changed in this session**

   Source files:

   - `frontend/src/modules/recovery/pages/RecoveryPage.jsx`
   - `frontend/src/modules/recovery/pages/RecoveryWorkflow.jsx` (new)
   - `frontend/src/modules/recovery/services/recoveryApi.js`
   - `frontend/src/modules/recovery/services/recoveryWorkflow.js` (new)
   - `frontend/src/modules/recovery/styles/recovery.css`
   - `mobile/lib/features/recovery/models/recovery_models.dart`
   - `mobile/lib/features/recovery/providers/recovery_controller.dart`
   - `mobile/lib/features/recovery/screens/recovery_screen.dart`
   - `mobile/lib/features/recovery/services/recovery_service.dart`

   Review artifacts, all new:

   - `docs/members/member-2/client-workflow-fixes-2026-09-08.md` (this report)
   - `docs/members/member-2/react-workflow-390.png`
   - `docs/members/member-2/react-workflow-768.png`
   - `docs/members/member-2/react-workflow-1440.png`
   - `docs/members/member-2/flutter-workflow-390.png`
   - `docs/members/member-2/flutter-workflow-768.png`
   - `docs/members/member-2/flutter-workflow-1440.png`

   The five source files already modified at the start were completed in place. The existing untracked readiness audit and `memory/` were preserved.

2. **React update-case fix**

   `updateCase` is exported from the module API service, imported by `RecoveryPage`, and uses `PUT /api/recovery-cases/{id}` with `{ expectedVersion, inputs }`. Inputs contain objective, preferredRoutes, currency, maximumPickupCost and deadline. The returned case replaces the selected case and refreshes its workflow; the case list also refreshes. The edit form uses the latest loaded case version. Item ID cannot be edited because the update DTO does not accept it. The deadline converts between local input time and UTC without shifting an unchanged deadline. A synchronous request guard and disabled submit prevent duplicate saves. Sanitized errors remain visible.

3. **React proposal workflow**

   Opening a case loads its current details and persisted options. Planning/replanning replaces the current case/version and options using the actual planning response. Missing returned options are fetched from the options endpoint. Planning's unavailable inputs are displayed. Selection, proposal creation, and human review are separate steps. The selected option must be current, validated/selected, have an estimate and satisfy its required integration snapshots. Submission also requires expiry and explanation. Requests lock competing actions. Validation failures block further submission until reload/replan and a fresh selection.

   Creation retains the returned proposal ID, loads its details, and refreshes the case/options. If that read fails, reload retries the retained ID rather than creating another proposal. Decisions use the loaded proposal ID, version and revision. Missing/invalid expiry, expiry in the past, mismatched case revision, and non-awaiting-approval statuses disable decisions. Successful decisions refresh the proposal, case and options; returning to the list reloads it.

4. **Flutter proposal workflow**

   The controller owns one selected case/option and coordinates all requests. Case opening, planning/replanning, option selection, creation, proposal read, decision and refresh follow the same API contract as React. The user selects a real option, explanation and expiry duration; no manual option ID, proposal ID, version or revision fields are required. A single request guard prevents competing requests and duplicate submission. Revision requests require a comment. The misplaced create-case handler was moved into its widget State, and the form responds to controller loading/error state. Proposal cards expose separate accessible text fields rather than merging their entire contents into field labels.

5. **Exact proposal endpoint and payload**

   `POST /api/recovery-cases/{caseId}/proposals`, with the existing `Idempotency-Key` header:

   ```text
   {
     expectedVersion: currentCase.version,
     optionId: selectedOption.id,
     optionVersion: selectedOption.version,
     match: requiresPartner
       ? { id: integration.match.matchId,
           version: integration.match.version,
           freshnessToken: integration.match.freshnessToken }
       : null,
     pickup: requiresPickup
       ? { id: integration.pickup.pickupPlanId,
           version: integration.pickup.version,
           freshnessToken: integration.pickup.freshnessToken }
       : null,
     expiresAt: user-selected expiry as an ISO UTC string,
     explanation: user-entered explanation
   }
   ```

   A match must be eligible, accepted and linked to the option; a required pickup must be feasible and linked to that match. The details endpoint is `GET /api/recovery-proposals/{proposalId}`. Decisions use `POST /api/recovery-proposals/{proposalId}/decisions` with `{ expectedVersion: proposal.version, proposalRevision: proposal.revision, decision, comment }` and the existing idempotency header.

6. **Real identifiers**

   Option IDs/versions and integration choices come from `PlanningResponse.options` or `GET /api/recovery-cases/{caseId}/options`. Proposal IDs come exclusively from the proposal creation response; the subsequent GET supplies the decision ID/version/revision. Planning is no longer treated as returning a proposal. No client code generates resource IDs. The existing generation of request idempotency keys remains separate from resource identity. Browser verification used simulated persisted records in Playwright routes, not production data.

7. **Response-model corrections**

   Both clients use case revision/version, option caseRevision/version/status, nullable estimates, evidence, unavailable inputs, integration snapshots, and proposal case/option/match/pickup identifiers and concurrency fields. Flutter now reads the actual `matchId`, `recoveryOptionId`, `eligibility`, `response`, `pickupPlanId`, `matchId` and `feasibility` fields instead of invented `id`/`status` response fields. Missing concurrency metadata cannot enable submissions. Missing amounts display as unavailable/pending instead of invented zero values. Invalid/missing expiry and evidence dates are nullable and display a safe unavailable state. The option response DTO contains no caseId; case association comes from the case-scoped endpoint plus caseRevision, without fabricating a response field.

8. **Flutter error safety**

   Generic errors use the fixed message: “The Recovery request could not be completed. Please try again.” No exception text or Dio transport message is displayed. Sanitized Problem Details title/detail strings remain supported, with a defensive filter for technical/credential content. No logger or dependency was introduced. A malformed case-version response was checked in the browser: the fixed fallback appeared and the diagnostic marker did not.

9. **Backend verification**

   - `dotnet build WasteToValue.sln -c Release --no-restore`: passed, zero warnings/errors.
   - `dotnet test tests/Recovery/WasteToValue.Recovery.Tests/WasteToValue.Recovery.Tests.csproj -c Release --no-restore`: 163 passed, zero failed/skipped.
   - Backend controllers/DTOs were read for the contract; no backend code or shared database was changed.

10. **React verification**

    - `npm run lint`: passed without suppressions.
    - `npm run build`: passed.
    - No test command is configured; no test dependency or placeholder suite was added.
    - Playwright MCP exercised the rendered UI: option selection does not submit; persisted selection creates the exact payload; double-click produces one create/update request; stale options are disabled; revision comments are required; decisions use the loaded concurrency fields; validation rejection requires refresh; failed proposal GET retries the retained ID; invalid expiry disables decisions; update refreshes the selected case; unchanged deadlines preserve UTC time.
    - Screenshots inspected at 390, 768 and 1440 pixels, with no horizontal overflow. Keyboard interaction was checked. No page exceptions occurred. Console/network inspection showed the deliberately simulated 409/503 failures; initial live API access failed because no local API server was running.

11. **Flutter verification**

    - `dart format --output=none --set-exit-if-changed lib/features/recovery`: passed, five files checked, zero changes.
    - `flutter analyze --no-pub`: passed, no issues.
    - `flutter build web --no-pub`: passed. The build reports a missing CupertinoIcons font declaration; the dependency/asset manifests are outside this task's allowed scope.
    - `flutter test` was not run: `flutter_test` is absent from the manifest and the Recovery test directory has no runnable tests. No manifest changes were made to enable tests.
    - Playwright MCP exercised the compiled web app: case load, planning, option selection, exact proposal payload, proposal GET, required revision comment, exact decision payload, case/options/proposal refresh, and unavailable expiry display. A malformed response also verified the fixed generic error. Screenshots inspected at 390, 768 and 1440 pixels; no page exceptions, browser warnings or horizontal overflow were observed in the completed workflow check.
    - Flutter's local preview service-worker cache was cleared to ensure the final compiled assets were inspected. Native Android/iOS builds were not part of the requested checks.

12. **Remaining integration requirements**

    Live deployment still needs authenticated identity/access, durable workflow/idempotency persistence, confirmed assessment inputs, partner matching, pickup planning, and the separately reviewed schema integration documented in the existing readiness audit. Required unavailable integrations continue to fail closed. Browser success cases used contract-shaped fixtures; they do not establish live adapter readiness. Existing agent/Gemini production integration work remains separate. No Neon connection or real Gemini call was made. Framework test infrastructure remains a shared integration task.

13. **Scope confirmation**

    All source edits are inside the Member 2 frontend/mobile modules; all review artifacts are inside Member 2 documentation. No backend, shared file, router, global stylesheet, migration, model snapshot, configuration, dependency manifest/lockfile, memory file or other-member file was edited. Existing untracked audit/memory content was preserved. `git diff --check` passed. No files were staged, committed or pushed. Implementation stops here for review.
