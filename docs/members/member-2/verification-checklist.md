# Recovery Component — Verification Checklist

**Component:** Component B — Recovery & Recovery Planning Agent  
**Module Owner:** Member 2 (`docs/members/member-2/`)  
**Target Solution:** `backend/LoopWorth.sln`, `frontend/`, `mobile/`  

This verification checklist outlines the pre-submission verification steps for the Recovery component. Automated unit tests that passed during build verification are marked, while live manual integration checks remain available for execution.

---

## 1. Automated Unit Test Verification

- [x] **`RecoveryRequest_DefaultStatusIsDraft`** — Passed via `dotnet test` (Asserts default state is `RecoveryStatus.Draft`).
- [x] **`ApprovalDecision_RecordsAdminAction`** — Passed via `dotnet test` (Asserts admin ID, decision status, and reason tracking).
- [x] **`RecoveryPlan_SupportsStepsAndSafetyNotes`** — Passed via `dotnet test` (Asserts child entity collections for preparation steps and safety notes).
- [x] **`RecoveryPlanningAgent_GeneratesTailoredPlan_ForLaptop`** — Passed via `dotnet test` (Asserts laptop checklist specificity, drive wipe, adapter/dongle detachment, and battery swelling notes; asserts absence of SIM instructions).
- [x] **`RecoveryPlanningAgent_GeneratesTailoredPlan_ForPhone`** — Passed via `dotnet test` (Asserts SIM card tray ejection, iCloud/FRP lock, and glass handling notes).

---

## 2. Backend API Endpoint Availability & Routing

- [ ] `POST /api/recovery` — Endpoint registered and routes to `RecoveryController.Create`.
- [ ] `GET /api/recovery` — Endpoint lists current customer's recovery requests.
- [ ] `GET /api/recovery/{id}` — Endpoint returns detailed request with plan, steps, safety notes, and decisions.
- [ ] `POST /api/recovery/{id}/generate-plan` — Endpoint triggers plan formulation with `RecoveryPlanningAgent`.
- [ ] `PATCH /api/recovery/{id}/checklist` — Endpoint updates completion state for a pre-collection checklist item.
- [ ] `POST /api/recovery/{id}/submit` — Endpoint gatekeeps submission based on checklist verification.
- [ ] `GET /api/recovery/{id}/handover-pass` — Public/anonymous endpoint resolves pass by ID or short code.
- [ ] `GET /api/admin/recovery` — Admin-only endpoint lists all requests with optional status filter.
- [ ] `GET /api/admin/recovery/{id}` — Admin-only endpoint retrieves recovery detail.
- [ ] `POST /api/admin/recovery/{id}/approve` — Admin-only endpoint approves request.
- [ ] `POST /api/admin/recovery/{id}/reject` — Admin-only endpoint rejects request with reason.
- [ ] `POST /api/admin/recovery/{id}/request-revision` — Admin-only endpoint returns request for revisions.
- [ ] `POST /api/admin/recovery/{id}/decision` — Unified admin endpoint supporting decision, reason, handling instructions, and route override.

---

## 3. Business Logic, Validation & State Lifecycle

- [ ] Unassessed item validation: Creating recovery on unassessed item returns HTTP 400.
- [ ] Unassigned route validation: Creating recovery on item without selected route returns HTTP 400.
- [ ] Item ownership enforcement: Accessing or creating recovery for another user's item returns HTTP 403 Forbidden.
- [ ] Conflict prevention: Attempting to create duplicate active recovery for same item returns HTTP 409 Conflict.
- [ ] Submission gate: Submitting with incomplete mandatory checklist items returns HTTP 400 (`requiresChecklistCompletion: true`).
- [ ] Checklist completion: Toggling all mandatory items updates `IsPreparationVerified = true`.
- [ ] State transition lock: Admin decisions are rejected with HTTP 400 if status is not `PendingAdminApproval`.
- [ ] Mandatory rejection/revision notes: Rejecting or requesting revision without a reason string returns HTTP 400.
- [ ] Route override: Admin route override updates both `RecoveryRequest.SelectedRoute` and `Item.SelectedRecoveryRoute`.
- [ ] Non-destructive revisions: Decision history is preserved in `ApprovalDecision` records across revision loops.

---

## 4. Agentic AI & Deterministic Fallback

- [ ] Gemini API integration: Uses `Gemini:RecoveryApiKey` or `GEMINI_RECOVERY_API_KEY`.
- [ ] Device category tailoring: Generates distinct instructions for Laptops, Phones, Tablets, and Peripherals.
- [ ] Negative prompt constraints: Suppresses irrelevant mobile instructions (SIM cards) on PC/laptop items.
- [ ] Hazard awareness: Condition descriptions referencing battery swelling, cracks, or loose hinges trigger hazard-specific safety notes.
- [ ] Deterministic fallback: When Gemini API key is missing or endpoint is unreachable, `GenerateDynamicPreparationPlan` takes over seamlessly.
- [ ] Workflow audit synchronization: Updates `AgentWorkflow` and records `AgentWorkflowStep` execution status (`Running`, `Succeeded`, `Failed`).

---

## 5. Frontend Web (React 19) Integration

- [ ] Route registration: `/recovery`, `/recovery/:id`, `/verify-handover/:id`, and `/admin/recovery` configured in `routes.jsx`.
- [ ] Role protection: Customer routes guarded for `Customer` role; admin portal guarded for `Admin` role.
- [ ] Customer dashboard: `RecoveryListPage.jsx` displays item thumbnails, status badges, and route indicators.
- [ ] Interactive plan page: `RecoveryPlanPage.jsx` renders summary, suitability, steps, hazard notices, and checklist.
- [ ] Live checklist toggle: Checking items sends `PATCH` requests and dynamically enables the submit button upon full completion.
- [ ] Admin approval portal: `AdminRecoveryApprovalsPage.jsx` renders plan detail, route override dropdown, custom instruction input, and decision action dialogs.
- [ ] Public handover verification: `HandoverVerificationPage.jsx` renders verified pass with customer details, pickup slot, and QR code reference.

---

## 6. Mobile (Flutter) Integration

- [ ] Route registration: Paths defined in `mobile/lib/features/recovery/routes.dart`.
- [ ] Data models: Typed JSON parsing in `recovery_model.dart` matches API response DTOs.
- [ ] Service integration: `recovery_api_service.dart` handles JWT headers, GET, POST, and PATCH operations.
- [ ] Mobile screens: `recovery_list_screen.dart`, `recovery_plan_screen.dart`, `recovery_route_screen.dart`, and `admin_approvals_screen.dart` render without layout overflow on phone viewports.

---

## 7. Database Persistence & Integrity (PostgreSQL / EF Core)

- [ ] Entities map to unified `AppDbContext`: `RecoveryRequests`, `RecoveryPlans`, `RecoveryPlanSteps`, `RecoverySafetyNotes`, `ApprovalDecisions`.
- [ ] Cascading deletion / orphan cleanup: Regenerating a plan replaces previous steps and safety notes without orphans.
- [ ] Audit persistence: `ApprovalDecision` rows maintain historical timestamps (`DecidedAt`) and admin IDs.
- [ ] No multiple `DbContext` or parallel migration directories created for the module (complies with `docs/migration-policy.md`).
