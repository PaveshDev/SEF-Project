# Member 4 — Collections

Own the collections folders across web, API, mobile, agents, and corresponding tests. Branch: `Collection-New` (and `feature/member-4-collections`).

Reference documentation:
- [Ownership Policy](../../ownership.md)
- [Team Workflow](../../team-workflow.md)
- [Migration Policy](../../migration-policy.md)

---

## Responsibility

Member 4 is responsible for the end-to-end logistics lifecycle for electronic waste collection within LoopWorth:
- Enabling customers to schedule doorstep pickup preferences for approved item recovery requests.
- Dispatching collection agents across Sri Lanka's 25 administrative districts using AI-driven and deterministic heuristics.
- Facilitating collection agent workflows (reviewing, accepting, declining, updating doorstep collection, and delivering to partner facilities).
- Supporting partner facility intake inspections (intake proof photos, condition verification, defect notes).
- Triggering customer delivery completion notifications.
- Documenting, verifying, and testing the collections subsystem.

---

## Collections Features Worked On

1. **Customer Pickup Scheduling**:
   - Web interface allowing customers to pick date and morning/afternoon slots for approved items.
   - Validation ensuring partner selection and approval prerequisites are met prior to booking.

2. **Collection Agent Management & National Coverage**:
   - Database seeding and management of 50 collection agents covering all 25 districts of Sri Lanka (2 agents per district).
   - District sub-region coverage (`TownArea`) dividing municipal and divisional secretariat areas between agents for localized matching.
   - Live availability tracking (`IsAvailable`, active job counters, calendar conflict detection).

3. **Intelligent Agent Dispatch & Dispatch Guardrails**:
   - Integration with `CollectionPlanningAgent` prioritizing district matching, town substring matching (`TownArea.Contains(customerTown)`), and calendar conflict freedom.
   - Admin management portal for initiating automated AI dispatch, manual agent overrides, and viewing agent candidate availability.
   - Single in-progress job constraint preventing agents from accepting multiple concurrent active orders.

4. **Agent & Partner Execution Workflows**:
   - Agent job acceptance (`Scheduled`) and rejection (`Requested` with recorded refusal reason for re-dispatch).
   - Doorstep pickup marking (`Collected`) and facility handover (`DeliveredToPartner`).
   - Partner intake verification (`Completed`) with photo upload and condition assessment.
   - Customer completion email integration via Brevo.

---

## Documentation and Testing Contribution

- **[Collections Workflow Documentation](collections-workflow.md)**: Comprehensive guide detailing actors, sequence diagrams, process phases, and core business rules.
- **[Collection Status Lifecycle](collection-status-lifecycle.md)**: Complete enumeration of statuses (`Requested`, `AgentAssigned`, `Scheduled`, `Collected`, `DeliveredToPartner`, `PartnerReceived`, `Completed`, `Cancelled`), allowed transitions, and state invariant rules.
- **[Manual Test Cases](manual-test-cases.md)**: 12 manual test specifications covering customer booking, AI dispatch, agent accept/reject, handover, partner intake, RBAC, and error handling.
- **[Verification Checklist](verification-checklist.md)**: Rigorous checklist distinguishing verified capabilities from pending manual test execution.
- **[AI Usage Log](ai-usage-log.md)**: Honest audit trail recording AI assistance used during documentation and code inspection.

---

## Files / Areas Related to My Work

### Backend (API & Infrastructure)
- `backend/src/WasteToValue.Api/Modules/Collections/` — Module registration placeholder.
- `backend/src/LoopWorth.Api/Controllers/CollectionsController.cs` — Core Collections API endpoints.
- `backend/src/LoopWorth.Api/Controllers/CollectionAgentsController.cs` — Agent profile & roster management.
- `backend/src/LoopWorth.Domain/Entities/CollectionRequest.cs` — Primary collection domain entity.
- `backend/src/LoopWorth.Domain/Entities/CollectionAgentProfile.cs` — Agent coverage & availability entity.
- `backend/src/LoopWorth.Domain/Entities/CollectionStatusHistory.cs` — Audit history entity.
- `backend/src/LoopWorth.Domain/Enums/CollectionStatus.cs` — Status enum.
- `backend/src/LoopWorth.Infrastructure/Agents/CollectionPlanningAgent.cs` — AI dispatch heuristic engine.
- `backend/src/LoopWorth.Infrastructure/Data/DbInitializer.cs` — National collection agent seed data.

### Frontend (Web)
- `frontend/src/modules/collections/pages/CustomerCollectionsPage.jsx` — Customer collection history.
- `frontend/src/modules/collections/pages/SchedulePickupPage.jsx` — Customer scheduling page.
- `frontend/src/modules/collections/pages/AdminCollectionsPage.jsx` — Admin collection dispatcher view.
- `frontend/src/modules/collections/pages/AdminCollectionAgentsPage.jsx` — Admin agent roster management.
- `frontend/src/modules/collections/pages/CollectionAgentJobsPage.jsx` — Agent queue and status actions.
- `frontend/src/modules/collections/routes.jsx` — Collections route registrations.

### Mobile
- `mobile/lib/features/collections/` — Flutter screens, models, and service interfaces for mobile collection management.

### Documentation
- `docs/members/member-4/` — Member 4 personal documentation, workflow guides, test cases, and AI log.

---

## Verification

- **API Execution**: Confirmed backend runs cleanly on `http://localhost:5080`.
- **Roster & Seeding**: Verified live via `GET /api/admin/collection-agents` that 53 collection agents (50 seeded across 25 districts + test accounts) are registered with valid district and town area assignments.
- **Frontend Startup**: Confirmed web development server compiles and serves on `http://localhost:5173`.

---

## Known Limitations / Remaining Work

1. **Automated Unit & Integration Test Suite**:
   - `backend/tests/Collections/` requires expanded automated test coverage matching the manual test cases defined in `manual-test-cases.md`.
2. **Mobile QR Code Scanning Verification**:
   - Live hardware camera testing on physical Android/iOS devices for agent-to-partner handover QR code scanning remains to be performed.
3. **Brevo Production SMTP Validation**:
   - Live email delivery receipt in customer inboxes requires valid external Brevo API key configuration in deployment environments.
