# Collections Manual Test Cases

This document defines manual test cases covering the complete Collections logistics lifecycle implemented across the LoopWorth backend and web client.

---

## TC-01 — Customer Schedules Collection for Approved Item

- **Description**: Customer submits preferred pickup date and time window for an approved recovery request with an assigned partner.
- **Preconditions**:
  - Customer is authenticated with a valid JWT token (`Customer` role).
  - Recovery request exists in `RecoveryStatus.Approved`.
  - A partner selection exists for the recovery request (`PartnerSelection` entity present).
- **Steps**:
  1. Navigate to `/recovery/{id}/schedule` in the web application.
  2. Select a valid future pickup date (e.g. tomorrow).
  3. Select a pickup time slot (e.g. 09:00:00 to 12:00:00).
  4. Submit the form (`POST /api/recovery/{id}/collections`).
- **Expected Result**:
  - HTTP 201 Created is returned with collection request payload.
  - Initial collection status is set to `Requested`.
  - An entry is created in `CollectionStatusHistories` noting customer preference submission.
- **Verification Status**: Not yet verified

---

## TC-02 — Prevent Collection Creation for Unapproved Recovery Request

- **Description**: Verifies that collections cannot be created for draft, rejected, or pending recovery requests.
- **Preconditions**:
  - Customer is authenticated.
  - Recovery request exists in `RecoveryStatus.Submitted` or `RecoveryStatus.Draft`.
- **Steps**:
  1. Attempt to invoke `POST /api/recovery/{id}/collections` for the unapproved request ID.
- **Expected Result**:
  - HTTP 400 Bad Request is returned.
  - Error message specifies: `"Recovery request must be approved first."`
  - No collection request record is inserted into the database.
- **Verification Status**: Not yet verified

---

## TC-03 — Admin Views All Collection Requests

- **Description**: Administrator inspects the live collection queue with optional status filtering.
- **Preconditions**:
  - Administrator is authenticated with `Admin` role (`loopworthadmin@gmail.com`).
- **Steps**:
  1. Send `GET /api/admin/collections` with Bearer token.
  2. Send `GET /api/admin/collections?status=Requested`.
- **Expected Result**:
  - HTTP 200 OK returned.
  - Response contains a list of collection orders with customer location, partner facility, scheduled times, assigned agent, and status history.
  - Filter parameter correctly limits response to items matching the requested status.
- **Verification Status**: Not yet verified

---

## TC-04 — AI Automated Agent Dispatch (District & Town Matching)

- **Description**: Verifies that the AI dispatch engine matches agents according to district, specific town coverage (`TownArea`), and conflict-free schedules.
- **Preconditions**:
  - Administrator is authenticated.
  - A collection request exists in `Requested` status for a customer residing in a specific town (e.g. Trincomalee District, Kinniya town).
- **Steps**:
  1. Send `POST /api/admin/collections/{id}/assign-agent`.
  2. Inspect the returned collection request payload and status history note.
- **Expected Result**:
  - HTTP 200 OK returned.
  - `AssignedCollectionAgentId` is set to an active agent whose `ServiceArea` matches the district and whose `TownArea` covers the specified town (e.g. Kamal Jeyaratnam for Kinniya).
  - Status transitions to `AgentAssigned`.
  - Status history notes automated assignment via AI agent.
- **Verification Status**: Not yet verified

---

## TC-05 — Collection Agent Views and Accepts Assigned Job

- **Description**: Assigned collection agent reviews their job queue and confirms acceptance.
- **Preconditions**:
  - Collection agent is authenticated (`CollectionAgent` role).
  - Agent has a job assigned in `AgentAssigned` status.
  - Agent has no currently active jobs in `Scheduled` or `Collected`.
- **Steps**:
  1. Send `GET /api/agent/collections` to inspect pending jobs.
  2. Send `POST /api/agent/collections/{id}/accept` for the assigned order.
- **Expected Result**:
  - HTTP 200 OK returned.
  - Collection status updates from `AgentAssigned` to `Scheduled`.
  - Agent profile availability updates to `IsAvailable = false` (marked Busy).
  - Status history entry logs agent acceptance and busy state.
- **Verification Status**: Not yet verified

---

## TC-06 — Prevention of Concurrent In-Progress Jobs for Collection Agent

- **Description**: Verifies that an agent cannot accept a second job while an existing job is in `Scheduled` or `Collected` state.
- **Preconditions**:
  - Collection agent already has one job in `Scheduled` or `Collected` status.
  - A second job has been allocated to the agent in `AgentAssigned` status.
- **Steps**:
  1. Send `POST /api/agent/collections/{second_id}/accept` with the agent's credentials.
- **Expected Result**:
  - HTTP 400 Bad Request returned.
  - Error message specifies: `"You already have an active pickup in progress. You cannot accept another job until you complete and hand over your current accepted pickup."`
  - Second collection status remains `AgentAssigned`.
- **Verification Status**: Not yet verified

---

## TC-07 — Collection Agent Rejects Job with Reason

- **Description**: Assigned agent declines an assigned pickup, returning it to the pool for re-dispatch.
- **Preconditions**:
  - Collection agent has an assigned order in `AgentAssigned` status.
- **Steps**:
  1. Send `POST /api/agent/collections/{id}/reject` with JSON body:
     ```json
     { "reason": "Vehicle mechanical issue" }
     ```
- **Expected Result**:
  - HTTP 200 OK returned.
  - `AssignedCollectionAgentId` is set to `null`.
  - Status transitions back to `Requested`.
  - Status history records the declining agent's ID and refusal reason.
  - The declined agent is excluded from immediate automated re-dispatch for this order.
- **Verification Status**: Not yet verified

---

## TC-08 — Doorstep Pickup Execution (`Collected`)

- **Description**: Agent collects the item from the customer address and records transit.
- **Preconditions**:
  - Collection request is in `Scheduled` status.
  - Assigned collection agent is authenticated.
- **Steps**:
  1. Send `POST /api/agent/collections/{id}/status` with JSON body:
     ```json
     { "status": "Collected", "note": "Picked up sealed electronics package from customer doorstep." }
     ```
- **Expected Result**:
  - HTTP 200 OK returned.
  - Status updates to `Collected`.
  - Agent remains in Busy state (`IsAvailable = false`).
  - Status history records pickup note and timestamp.
- **Verification Status**: Not yet verified

---

## TC-09 — Delivery Handover to Partner Facility (`DeliveredToPartner`)

- **Description**: Agent delivers the package to the partner facility.
- **Preconditions**:
  - Collection request is in `Collected` status.
  - Assigned collection agent is authenticated.
- **Steps**:
  1. Send `POST /api/agent/collections/{id}/status` with JSON body:
     ```json
     { "status": "DeliveredToPartner", "note": "Package deposited at partner intake receiving bay." }
     ```
- **Expected Result**:
  - HTTP 200 OK returned.
  - Status updates to `DeliveredToPartner`.
  - Status history records handover note.
- **Verification Status**: Not yet verified

---

## TC-10 — Partner Intake Verification with Photo and Condition Inspection

- **Description**: Partner facility confirms receipt, verifies packaging condition, records defects, and uploads intake photo.
- **Preconditions**:
  - Collection request is in `DeliveredToPartner` status.
  - User belonging to the linked Partner organization is authenticated (`Partner` role).
- **Steps**:
  1. Send `POST /api/partner/collections/{id}/receive` as `multipart/form-data`:
     - `Photo`: image file (JPG, PNG, or WebP < 10MB)
     - `Feedback`: `"Package received intact; minor cosmetic wear on casing."`
     - `ConditionOk`: `true`
- **Expected Result**:
  - HTTP 200 OK returned.
  - Photo is stored in `partner-receipts` storage folder.
  - Status updates to `Completed`.
  - `PartnerConfirmedAt`, `PartnerFeedback`, and `PartnerReceivedConditionOk` are persisted.
  - Assigned collection agent availability automatically resets to `IsAvailable = true`.
  - Background customer delivery confirmation email is triggered.
- **Verification Status**: Not yet verified

---

## TC-11 — Admin Collection Agent Roster and Town Coverage Verification

- **Description**: Administrator inspects the registered collection agents, district coverage, specific town areas, and live availability.
- **Preconditions**:
  - Backend API service running on `http://localhost:5080`.
  - Administrator authenticated (`loopworthadmin@gmail.com`).
- **Steps**:
  1. Send authenticated `GET /api/admin/collection-agents`.
  2. Verify total agent count and inspect coverage across districts (e.g. Trincomalee, Colombo, Jaffna).
- **Expected Result**:
  - HTTP 200 OK returned.
  - Returns collection agents covering all 25 districts of Sri Lanka.
  - Each agent profile includes `serviceArea`, `townArea`, `isAvailable`, `isActive`, and contact details.
- **Verification Status**: Passed (Genuinely verified via live API execution against Neon PostgreSQL)

---

## TC-12 — Role-Based Access Control on Collection Endpoints

- **Description**: Unauthorized roles cannot invoke protected collection endpoints.
- **Preconditions**:
  - Customer, Partner, and unauthenticated users exist.
- **Steps**:
  1. Send `GET /api/admin/collections` without token.
  2. Send `GET /api/admin/collections` with Customer JWT token.
  3. Send `POST /api/agent/collections/{id}/accept` with Customer JWT token.
- **Expected Result**:
  - Step 1: HTTP 401 Unauthorized.
  - Step 2: HTTP 403 Forbidden.
  - Step 3: HTTP 403 Forbidden.
- **Verification Status**: Not yet verified
