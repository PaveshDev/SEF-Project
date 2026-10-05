# Collections API Reference

## Overview

This reference documents the real REST API endpoints powering the Collections subsystem in LoopWorth (`LoopWorth.Api.Controllers.CollectionsController` and `LoopWorth.Api.Controllers.CollectionAgentsController`). All endpoints require authentication (`[Authorize]`) via a Bearer JWT token unless otherwise indicated, and are strictly scoped by user role and domain validations.

---

## Customer Endpoints

### 1. Create or Update Pickup Preference
- **HTTP Method**: `POST`
- **Route**: `/api/recovery/{id:guid}/collections`
- **Required Role**: `Customer` (or `Admin`)
- **Purpose**: Creates a new collection request for an approved recovery request, or updates pickup preferences if an active non-cancelled collection request already exists for the given recovery ID.
- **Preconditions**:
  - Recovery request must exist and have `RecoveryStatus.Approved`.
  - A partner selection must already exist for the recovery request (`PartnerSelection` entity).
  - Calling user must be the recovery item owner (`CustomerId == userId`) or an `Admin`.
- **Request Body** (`CreateCollectionDto`):
  ```json
  {
    "preferredPickupDate": "2026-10-15T00:00:00Z",
    "preferredStartTime": "09:00:00",
    "preferredEndTime": "12:00:00"
  }
  ```
- **Expected Behaviour**:
  - If no collection request exists: creates `CollectionRequest` in status `Requested`, appends history note `"Pickup preference submitted by customer. Awaiting Admin dispatch."`, saves to database, and returns `HTTP 201 Created` with the `CollectionRequestDto`.
  - If a collection request already exists (and is not `Cancelled`): updates date/time preferences, sets status back to `Requested`, appends history note `"Pickup preference updated by customer. Awaiting Admin dispatch."`, and returns `HTTP 201 Created`.

---

### 2. Generate / Re-generate AI Plan
- **HTTP Method**: `POST`
- **Route**: `/api/collections/{id:guid}/plan`
- **Required Role**: `Customer` (must be request owner) or `Admin`
- **Purpose**: Triggers `CollectionPlanningAgent` (or deterministic fallback) to analyze customer location (district/town), candidate agent availability, and conflict schedules, suggesting the optimal pickup date, time, and agent.
- **Request Body**: None
- **Expected Behaviour**:
  - Invokes internal dispatch engine `DispatchWithAiInternalAsync`.
  - Populates `SuggestedPickupDate`, `SuggestedStartTime`, `SuggestedEndTime`, and `SuggestedCollectionAgentId`.
  - If candidates are available, assigns the agent, sets status to `AgentAssigned`, updates `AgentWorkflowStep`, logs to `CollectionStatusHistories`, and returns `HTTP 200 OK` with updated `CollectionRequestDto`.

---

### 3. List Customer Collections
- **HTTP Method**: `GET`
- **Route**: `/api/collections`
- **Required Role**: Authenticated user (`Customer`)
- **Purpose**: Retrieves all collection requests associated with the authenticated customer account.
- **Important Inputs**: None (uses JWT `ClaimTypes.NameIdentifier`).
- **Expected Behaviour**:
  - Returns `HTTP 200 OK` with an array of `CollectionRequestDto` objects ordered descending by `CreatedAt`.
  - Includes nested `PartnerName`, scheduled dates, assigned agent name, item summary, and complete chronological `StatusHistory`.

---

### 4. Get Collection Request by ID
- **HTTP Method**: `GET`
- **Route**: `/api/collections/{id:guid}`
- **Required Role**: Request Owner (`Customer`), Assigned Agent (`CollectionAgent`), or `Admin`
- **Purpose**: Retrieves full details of a specific collection request including status history and related item details.
- **Preconditions**: Caller must be the customer who created the request, the currently assigned agent, or an administrator; otherwise returns `HTTP 403 Forbidden`.
- **Expected Behaviour**:
  - Returns `HTTP 200 OK` with `CollectionRequestDto`.
  - Returns `HTTP 404 Not Found` if the collection ID does not exist.

---

## Admin Endpoints

### 5. List All Collections (Admin)
- **HTTP Method**: `GET`
- **Route**: `/api/admin/collections`
- **Required Role**: `Admin`
- **Purpose**: Oversees all collection requests across the platform with optional status filtering.
- **Query Parameters**:
  - `status` (optional string): filter by `CollectionStatus` enum name (e.g. `Requested`, `AgentAssigned`, `Scheduled`, `Collected`, `DeliveredToPartner`, `Completed`).
- **Expected Behaviour**:
  - Returns `HTTP 200 OK` with list of `CollectionRequestDto` objects sorted descending by `CreatedAt`.

---

### 6. Admin AI Dispatch / Re-dispatch Agent
- **HTTP Method**: `POST`
- **Route**: `/api/admin/collections/{id:guid}/assign-agent`
- **Required Role**: `Admin`
- **Purpose**: Triggers automated AI agent assignment for a pending request, or reassigns an alternative agent if one was previously declined or reassigned.
- **Preconditions**: Collection request status must be `Requested` or `AgentAssigned`. Cannot reassign after the collection has been confirmed by the agent (returns `HTTP 400 Bad Request`).
- **Request Body**: None
- **Expected Behaviour**:
  - Automatically excludes previously assigned or declining agents.
  - Queries active, available agents matching the customer's district and town.
  - Evaluates calendar conflicts and assigns the best candidate.
  - Transitions status to `AgentAssigned` and logs history entry.
  - Returns `HTTP 200 OK` with updated `CollectionRequestDto`.

---

### 7. Admin Manual Agent Assignment
- **HTTP Method**: `POST`
- **Route**: `/api/admin/collections/{id:guid}/assign`
- **Required Role**: `Admin`
- **Purpose**: Manually assigns or overrides a collection agent and explicitly specifies pickup date and time window.
- **Preconditions**: Status must be `Requested` or `AgentAssigned`. Agent must exist in Identity.
- **Request Body** (`AdminAssignCollectionDto`):
  ```json
  {
    "assignedCollectionAgentId": "agent-user-guid-or-id",
    "scheduledPickupDate": "2026-10-16T00:00:00Z",
    "scheduledStartTime": "10:00:00",
    "scheduledEndTime": "13:00:00"
  }
  ```
- **Expected Behaviour**:
  - Sets `AssignedCollectionAgentId`, `ScheduledPickupDate`, `ScheduledStartTime`, and `ScheduledEndTime`.
  - Sets status to `AgentAssigned`.
  - Appends `CollectionStatusHistory` with assigned agent name and scheduled window.
  - Synchronizes old and new agent availability via `SyncAgentAvailabilityAsync`.
  - Returns `HTTP 200 OK` with updated `CollectionRequestDto`.

---

### 8. Admin Complete Collection
- **HTTP Method**: `POST`
- **Route**: `/api/admin/collections/{id:guid}/complete`
- **Required Role**: `Admin`
- **Purpose**: Administratively marks a collection as completed (e.g. following external facility confirmation or manual dispute resolution).
- **Request Body** (`UpdateCollectionStatusDto`, optional):
  ```json
  {
    "note": "Confirmed received by partner. Collection completed by administrator."
  }
  ```
- **Expected Behaviour**:
  - Sets status to `Completed`, records history note.
  - Completes linked `AgentWorkflow` stage if active.
  - Frees assigned agent availability if no other active jobs remain.
  - Dispatches transactional completion email to customer via Brevo.
  - Returns `HTTP 200 OK` with updated `CollectionRequestDto`.

---

### 9. Admin Send / Resend Delivery Completion Email
- **HTTP Method**: `POST`
- **Route**: `/api/admin/collections/{id:guid}/send-delivery-email`
- **Required Role**: `Admin`
- **Purpose**: Explicitly sends or resends the customer transactional delivery completion email generated via `DeliveryNotificationAgent` through Brevo SMTP.
- **Expected Behaviour**:
  - Resets `DeliveryEmailSent` flag and invokes `TriggerDeliveryCompletionEmailAsync`.
  - Returns `HTTP 200 OK` with `{ success: true, sentAt, subject }` or `HTTP 400 Bad Request` if SMTP dispatch fails.

---

### 10. Admin Collection Agents Management
- **Routes**:
  - `GET /api/admin/collection-agents`: Lists all registered collection agent profiles with live dynamic availability (`IsAvailable`).
  - `GET /api/admin/collection-agents/{id:guid}`: Returns single agent profile details.
  - `POST /api/admin/collection-agents`: Creates a new collection agent user account in role `CollectionAgent` and creates `CollectionAgentProfile` with `serviceArea`, `townArea`, and validated 10-digit phone number.
  - `PUT /api/admin/collection-agents/{id:guid}`: Updates agent phone, district territory, town vicinity, and active status.
  - `DELETE /api/admin/collection-agents/{id:guid}`: Deletes agent profile and Identity user. Guardrail: blocked if agent has active jobs in `Scheduled` or `Collected` status (`HTTP 400 Bad Request`).
- **Required Role**: `Admin`

---

## Collection Agent Endpoints

### 11. List Assigned Agent Jobs
- **HTTP Method**: `GET`
- **Route**: `/api/agent/collections`
- **Required Role**: `CollectionAgent`
- **Purpose**: Retrieves all jobs currently assigned to the authenticated collection agent ordered by scheduled pickup date.
- **Expected Behaviour**:
  - Returns `HTTP 200 OK` with `CollectionRequestDto[]` filtered by `AssignedCollectionAgentId == userId`.

---

### 12. Accept Assigned Job
- **HTTP Method**: `POST`
- **Route**: `/api/agent/collections/{id:guid}/accept`
- **Required Role**: `CollectionAgent`
- **Purpose**: Agent formally accepts an assigned job, locking the appointment into their schedule.
- **Preconditions**:
  - Calling user must be the assigned agent (`AssignedCollectionAgentId == userId`).
  - Current status must be `AgentAssigned` (otherwise returns `HTTP 400 Bad Request`).
  - Agent must NOT already have an active job in progress (`Scheduled` or `Collected`). If an active job exists, returns `HTTP 400 Bad Request`: *"You already have an active pickup in progress. You cannot accept another job until you complete and hand over your current accepted pickup."*
- **Expected Behaviour**:
  - Transitions status to `Scheduled`.
  - Appends history note: `"Collection agent <Name> confirmed and accepted the order. Pickup scheduled. Agent duty status: Busy."`
  - Updates agent profile availability: `IsAvailable = false`.
  - Returns `HTTP 200 OK` with updated `CollectionRequestDto`.

---

### 13. Reject Assigned Job
- **HTTP Method**: `POST`
- **Route**: `/api/agent/collections/{id:guid}/reject`
- **Required Role**: `CollectionAgent`
- **Purpose**: Agent declines an assigned job, providing a refusal reason, and returns it to the pool for re-dispatch.
- **Preconditions**:
  - Calling user must be the assigned agent.
  - Current status must be `AgentAssigned`.
- **Request Body** (`RejectCollectionJobDto`, optional):
  ```json
  {
    "reason": "Vehicle mechanical issue; unable to reach vicinity."
  }
  ```
- **Expected Behaviour**:
  - Clears `AssignedCollectionAgentId = null`.
  - Reverts status to `Requested`.
  - Logs history entry recording agent rejection reason (used by AI dispatch to exclude this agent).
  - Recalculates agent availability.
  - Returns `HTTP 200 OK` with updated `CollectionRequestDto`.

---

### 14. Update Status (Doorstep Pickup & Handover)
- **HTTP Method**: `POST`
- **Route**: `/api/agent/collections/{id:guid}/status`
- **Required Role**: `CollectionAgent` (or `Admin`)
- **Purpose**: Advances the physical execution of the pickup from `Scheduled` to `Collected`, and from `Collected` to `DeliveredToPartner` (or `Completed`).
- **Preconditions**:
  - Calling user must be the assigned agent (or `Admin`).
  - Non-admin agents are restricted to target statuses: `Scheduled`, `Collected`, `DeliveredToPartner`, or `Completed`.
- **Request Body** (`UpdateCollectionStatusDto`):
  ```json
  {
    "status": "Collected",
    "note": "Package received from customer doorstep."
  }
  ```
- **Expected Behaviour**:
  - If `status == Collected`: status becomes `Collected`, logs history, agent remains busy.
  - If `status == DeliveredToPartner`: status becomes `DeliveredToPartner`, logs facility handover note, awaits partner facility verification.
  - If `status == Completed`: marks completed, updates linked workflow, frees agent availability, triggers Brevo delivery completion email.
  - Returns `HTTP 200 OK` with updated `CollectionRequestDto`.

---

## Partner Endpoints

### 15. List Incoming Partner Collections
- **HTTP Method**: `GET`
- **Route**: `/api/partner/collections`
- **Required Role**: `Partner`
- **Purpose**: Allows partner facility staff to inspect all incoming handovers and intake history associated with their facility.
- **Preconditions**: Authenticated user must be linked to a registered `Partner` entity by `UserId` or matching `Email`.
- **Expected Behaviour**:
  - Returns `HTTP 200 OK` with list of `CollectionRequestDto` objects linked to `partner.Id`, ordered descending by `CreatedAt`.

---

### 16. Partner Confirm Receipt & Intake Verification
- **HTTP Method**: `POST`
- **Route**: `/api/partner/collections/{id:guid}/receive`
- **Required Role**: `Partner`
- **Content-Type**: `multipart/form-data`
- **Purpose**: Partner facility confirms receipt of the physical item from the collection agent, uploads an intake proof photo, verifies condition (damages/defects), and completes the collection lifecycle.
- **Preconditions**:
  - Authenticated user must belong to the partner facility linked to the collection request (`collection.PartnerId == partner.Id`).
  - Optional intake photo must be under 10MB and have file extension `.jpg`, `.jpeg`, `.png`, or `.webp`.
- **Form Data Fields**:
  - `Photo` (file, optional): Intake verification image.
  - `Feedback` (string, optional): Remarks on condition, packaging, or detected defects.
  - `ConditionOk` (boolean, default: `true`): Indicates whether item arrived undamaged.
- **Expected Behaviour**:
  - If photo uploaded: saved to `partner-receipts` storage folder via `IFileStorageService`.
  - Sets `Status = Completed`.
  - Sets `PartnerPhotoUrl`, `PartnerFeedback`, `PartnerConfirmedAt`, and `PartnerReceivedConditionOk`.
  - Appends `CollectionStatusHistory` with verification outcome.
  - Marks linked `AgentWorkflow` stage as `Completed`.
  - Restores assigned agent's `IsAvailable` flag to `true` (via `SyncAgentAvailabilityAsync`).
  - Triggers automated Brevo transactional delivery confirmation email to the customer.
  - Returns `HTTP 200 OK` with updated `CollectionRequestDto`.
