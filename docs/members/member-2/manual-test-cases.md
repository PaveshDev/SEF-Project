# Recovery Component — Manual Test Cases

**Component:** Component B — Recovery & Recovery Planning Agent  
**Module Owner:** Member 2 (`docs/members/member-2/`)  
**Target Services:** `RecoveryController`, `RecoveryPlanningAgent`, `RecoveryPlanPage`, `AdminRecoveryApprovalsPage`, `HandoverVerificationPage`  

---

## Test Suite Overview

| Test ID | Category | Scenario Summary | Target Level |
| :--- | :--- | :--- | :--- |
| **TC-REC-01** | Functional / Creation | Create recovery request for an assessed item with valid route | Backend API / UI |
| **TC-REC-02** | Validation / Guardrail | Reject recovery request creation if item is not yet assessed | Backend API |
| **TC-REC-03** | Validation / Conflict | Prevent duplicate active recovery requests for the same item | Backend API |
| **TC-REC-04** | Security / Auth | Reject recovery request creation by a user who does not own the item | Backend API |
| **TC-REC-05** | Agentic AI / Plan | Generate tailored recovery plan for a Laptop (Verify no phone steps) | Backend / AI Agent |
| **TC-REC-06** | Agentic AI / Plan | Generate tailored recovery plan for a Smartphone with glass hazard | Backend / AI Agent |
| **TC-REC-07** | Resilience / Fallback | Fallback to deterministic plan when Gemini API key is missing/invalid | Backend / Fallback Engine |
| **TC-REC-08** | Governance / Checklist | Block submission when mandatory pre-collection checklist is incomplete | Backend / Frontend |
| **TC-REC-09** | Functional / Checklist | Complete all mandatory checklist items and verify submission success | Backend / Frontend |
| **TC-REC-10** | Admin / Approval | Admin approves recovery request with custom handling instructions | Admin Web / Backend |
| **TC-REC-11** | Admin / Route Override | Admin overrides recovery route from Donate to Recycle during review | Admin Web / Backend |
| **TC-REC-12** | Admin / Rejection | Admin rejects recovery request; verify mandatory reason is required | Admin Web / Backend |
| **TC-REC-13** | Admin / Revision | Admin requests revision; customer updates plan and re-submits | End-to-End Workflow |
| **TC-REC-14** | Public / Logistics | Access verifiable handover pass by ID and by short reference code | Public API / Web |
| **TC-REC-15** | Error Handling | Query non-existent recovery ID returns HTTP 404 Not Found | Backend API |

---

## Detailed Manual Test Cases

### TC-REC-01: Create Valid Recovery Request for Assessed Item
* **Test Scenario:** A customer creates a recovery request for an item that has completed initial condition assessment.
* **Preconditions:**
  * User is authenticated as a customer (`Customer` role).
  * An item exists in the database owned by the user with `Status = ItemStatus.Assessed` and `SelectedRecoveryRoute = RecoveryRoute.Donate` or `Recycle`.
* **Steps:**
  1. Send `POST /api/recovery` with body `{ "itemId": "<valid-assessed-item-id>" }`.
  2. Inspect the HTTP response status code and response body.
* **Test Data:**
  * `ItemId`: `<valid-assessed-item-id>`
* **Expected Result:**
  * HTTP `201 Created` with a `Location` header pointing to `/api/recovery/{id}`.
  * Response body contains `id`, `itemId`, `selectedRoute` matching the item, and `status: "Draft"`.
  * No plan is attached yet (`plan: null`).
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-02: Prevent Recovery Request Creation for Unassessed Item
* **Test Scenario:** Ensure recovery creation is blocked if an item has not been assessed by Agent 1.
* **Preconditions:**
  * User is authenticated.
  * An item exists owned by the user, but its status is `ItemStatus.Draft` (unassessed).
* **Steps:**
  1. Send `POST /api/recovery` with body `{ "itemId": "<unassessed-item-id>" }`.
  2. Check status code and error message.
* **Test Data:**
  * `ItemId`: `<unassessed-item-id>`
* **Expected Result:**
  * HTTP `400 Bad Request`.
  * Response payload: `{ "error": "Item must be assessed first." }`.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-03: Prevent Duplicate Active Recovery Requests
* **Test Scenario:** Verify that an item cannot have multiple concurrent active recovery requests.
* **Preconditions:**
  * An active recovery request already exists for item `A` (in `Draft`, `PlanGenerated`, or `PendingAdminApproval` state).
* **Steps:**
  1. Send `POST /api/recovery` referencing item `A` again.
* **Test Data:**
  * `ItemId`: `<already-active-item-id>`
* **Expected Result:**
  * HTTP `409 Conflict`.
  * Response payload: `{ "error": "An active recovery request already exists for this item." }`.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-04: Enforce Item Ownership Authorization
* **Test Scenario:** Verify that Customer B cannot initiate recovery on an item owned by Customer A.
* **Preconditions:**
  * Item belongs to Customer A (`customerId = "user-A"`).
  * JWT Bearer token belongs to Customer B (`customerId = "user-B"`).
* **Steps:**
  1. Send `POST /api/recovery` with Customer B's token referencing Customer A's item ID.
* **Expected Result:**
  * HTTP `403 Forbidden`.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-05: AI Recovery Plan Generation — Laptop Specificity
* **Test Scenario:** Verify that `RecoveryPlanningAgent` produces laptop-tailored instructions and does not hallucinate smartphone instructions.
* **Preconditions:**
  * A recovery request exists in `Draft` state for a laptop (e.g., ASUS ZenBook or MacBook).
  * The item's condition notes report battery swelling and trackpad lifting.
* **Steps:**
  1. Send `POST /api/recovery/{id}/generate-plan`.
  2. Inspect the resulting `RecoveryPlanDto`.
* **Expected Result:**
  * HTTP `200 OK`.
  * `plan.summary` explicitly mentions "laptop".
  * `plan.steps` contains instructions for disk wipe / BitLocker, AC adapter/charger disconnection, and external storage dongle removal.
  * `plan.steps` does **not** mention "SIM card" or "eSIM".
  * `plan.safetyNotes` contains battery swelling precautions.
  * `plan.checklist` contains 5 category-appropriate items (e.g., Data Backup, Disk Encryption, External Storage, Battery & Charger, Packaging).
  * Request `status` transitions to `"PlanGenerated"`.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-06: AI Recovery Plan Generation — Smartphone with Glass Hazard
* **Test Scenario:** Verify that plan generation for a smartphone with shattered glass includes phone-specific SIM and glass safety handling.
* **Preconditions:**
  * A recovery request exists for a smartphone (e.g. iPhone 13 Pro Max) with condition "Shattered rear glass, screen working".
* **Steps:**
  1. Send `POST /api/recovery/{id}/generate-plan`.
* **Expected Result:**
  * HTTP `200 OK`.
  * `plan.steps` includes physical SIM card tray ejection and account unlinking (iCloud / Find My / Google FRP).
  * `plan.safetyNotes` explicitly identifies broken glass and cautions handling splinters.
  * `plan.checklist` items include cloud unlinking and SIM removal.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-07: Offline Deterministic Fallback on Gemini Unavailability
* **Test Scenario:** Verify system resilience when the Gemini API is unreachable or times out.
* **Preconditions:**
  * Configure backend with invalid Gemini API key or disconnect network to Google AI endpoints.
* **Steps:**
  1. Send `POST /api/recovery/{id}/generate-plan`.
* **Expected Result:**
  * API does not crash; returns HTTP `200 OK`.
  * Output generated by `GenerateDynamicPreparationPlan`.
  * Plan summary, steps, safety notes, and checklist are correctly populated based on item keyword heuristics.
  * `AgentWorkflowStep` logs a fallback execution.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-08: Block Submission on Incomplete Checklist
* **Test Scenario:** Verify that customers cannot bypass mandatory safety preparation before admin review.
* **Preconditions:**
  * Recovery request is in `PlanGenerated` state.
  * Mandatory items in `plan.checklist` have `isCompleted = false` (`isPreparationVerified = false`).
* **Steps:**
  1. Send `POST /api/recovery/{id}/submit`.
* **Expected Result:**
  * HTTP `400 Bad Request`.
  * Response body:
    ```json
    {
      "error": "Please complete all mandatory pre-collection preparation checklist steps before submitting for admin review.",
      "requiresChecklistCompletion": true
    }
    ```
  * Request `status` remains `"PlanGenerated"`.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-09: Checklist Toggle and Successful Submission
* **Test Scenario:** Toggle all mandatory checklist items to completed and successfully submit for admin review.
* **Preconditions:**
  * Recovery request in `PlanGenerated` state.
* **Steps:**
  1. For each mandatory step in `plan.checklist`, send:
     `PATCH /api/recovery/{id}/checklist` with `{ "stepId": "<id>", "isCompleted": true }`.
  2. Verify that `isPreparationVerified` returns `true` once the last mandatory item is marked complete.
  3. Send `POST /api/recovery/{id}/submit`.
* **Expected Result:**
  * HTTP `204 NoContent` on submission.
  * Querying `GET /api/recovery/{id}` shows `status: "PendingAdminApproval"` and a populated `submittedAt` timestamp.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-10: Admin Approval with Custom Handling Instructions
* **Test Scenario:** Administrator reviews a pending recovery request, adds custom warehouse handling notes, and approves.
* **Preconditions:**
  * Request is in `PendingAdminApproval` state.
  * Caller has `Admin` role JWT token.
* **Steps:**
  1. Send `POST /api/admin/recovery/{id}/approve` with payload:
     ```json
     {
       "reason": "All device preparation criteria satisfied.",
       "customHandlingInstructions": "Fragile OLED panel; transport in static-shield padded box."
     }
     ```
* **Expected Result:**
  * HTTP `200 OK` returning `ApprovalDecisionDto`.
  * `decision: "Approved"`.
  * Request `status` transitions to `"Approved"`.
  * An `ApprovalDecision` row is persisted with `AdminId` and the custom instructions.
  * `RecoveryPlan.AdminHandlingInstructions` reflects the submitted instructions.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-11: Admin Route Override During Approval
* **Test Scenario:** Administrator determines that an item submitted as "Donate" has unrepairable motherboard failure and overrides route to "Recycle".
* **Preconditions:**
  * Request is in `PendingAdminApproval` with `selectedRoute = "Donate"`.
  * Caller is Admin.
* **Steps:**
  1. Send `POST /api/admin/recovery/{id}/decision` with:
     ```json
     {
       "decision": "Approved",
       "reason": "Board damage prevents donation; re-routed to certified material recycler.",
       "routeOverride": "Recycle"
     }
     ```
* **Expected Result:**
  * HTTP `200 OK`.
  * `recovery.SelectedRoute` is updated to `RecoveryRoute.Recycle`.
  * Associated `item.SelectedRecoveryRoute` is updated to `RecoveryRoute.Recycle`.
  * `ApprovalDecision.OverriddenRoute` records `"Recycle"`.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-12: Admin Rejection with Mandatory Reason
* **Test Scenario:** Admin rejects a recovery request and verifies that rejection is blocked if reason is missing.
* **Preconditions:**
  * Request is in `PendingAdminApproval`.
* **Steps:**
  1. Send `POST /api/admin/recovery/{id}/reject` with empty reason `{ "reason": "" }`.
  2. Verify error response.
  3. Send `POST /api/admin/recovery/{id}/reject` with valid reason `{ "reason": "Suspected hazardous non-e-waste chemical leakage." }`.
* **Expected Result:**
  * Step 1 returns HTTP `400 Bad Request` (`"Reason is required for rejection."`).
  * Step 3 returns HTTP `200 OK`.
  * Request `status` transitions to `"Rejected"`.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-13: Admin Revision Loop and Customer Resubmission
* **Test Scenario:** Admin requests revision, and customer rectifies and resubmits.
* **Preconditions:**
  * Request in `PendingAdminApproval`.
* **Steps:**
  1. Admin sends `POST /api/admin/recovery/{id}/request-revision` with `{ "reason": "Please package battery charger with the device." }`.
  2. Verify request status is now `"RevisionRequested"`.
  3. Customer opens `RecoveryPlanPage.jsx` and views the revision feedback.
  4. Customer updates checklist step or regenerates plan.
  5. Customer sends `POST /api/recovery/{id}/submit`.
* **Expected Result:**
  * Step 1 returns HTTP `200 OK` and moves status to `"RevisionRequested"`.
  * Step 5 succeeds and returns status to `"PendingAdminApproval"`.
  * Historical `ApprovalDecision` records for both rounds remain preserved in audit query.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-14: Public Handover Pass Verification Query
* **Test Scenario:** Field courier accesses the public handover pass verification endpoint without credentials.
* **Preconditions:**
  * Recovery request is in `Approved` state.
* **Steps:**
  1. Send unauthenticated `GET /api/recovery/{id}/handover-pass`.
  2. Send unauthenticated `GET /api/recovery/LPW-PASS-{shortCode}/handover-pass`.
* **Expected Result:**
  * Both requests return HTTP `200 OK` with `HandoverPassDto`.
  * Payload includes `isApproved: true`, customer address, verified preparation status, device photos, and admin handling instructions.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail

---

### TC-REC-15: Query Non-Existent Recovery Request
* **Test Scenario:** Query an invalid or missing Recovery ID.
* **Steps:**
  1. Send `GET /api/recovery/00000000-0000-0000-0000-000000000000`.
* **Expected Result:**
  * HTTP `404 Not Found`.
* **Status:** [ ] Untested / [ ] Pass / [ ] Fail
