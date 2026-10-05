# Collections Validation Scenarios

## Overview

This document catalogues the domain validation rules, input constraints, and error guardrails enforced within the Collections subsystem (`LoopWorth.Api.Controllers.CollectionsController` and `CollectionAgentsController`). Each scenario reflects real validation logic implemented in the backend source code.

---

## Catalog of Verified Validation Scenarios

### 1. Recovery Request Not Approved
- **Trigger**: Customer invokes `POST /api/recovery/{id}/collections` for a recovery request whose status is not `RecoveryStatus.Approved` (e.g. `Submitted`, `Draft`, or `Rejected`).
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "Recovery request must be approved first." }
    ```
- **Reason / Guardrail**: Physical collection logistics incur organizational costs and should only be scheduled once an item has undergone recovery assessment and formal approval.

---

### 2. Missing Partner Selection
- **Trigger**: Customer invokes `POST /api/recovery/{id}/collections` when no `PartnerSelection` entity is associated with the recovery request.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "A recovery partner must be selected before requesting collection." }
    ```
- **Reason / Guardrail**: A collection request requires a designated destination facility (`PartnerId`) for delivery and intake verification.

---

### 3. Unauthorized Customer Preference Submission
- **Trigger**: An authenticated user invokes `POST /api/recovery/{id}/collections` for an approved recovery request owned by another customer (`recovery.CustomerId != userId`), without holding the `Admin` role.
- **Expected System Behaviour**:
  - HTTP Status: `403 Forbidden`
- **Reason / Guardrail**: Enforces strict user privacy and prevents unauthorized scheduling of other users' items.

---

### 4. Admin Reassignment of Confirmed Job
- **Trigger**: Administrator attempts to invoke `POST /api/admin/collections/{id}/assign-agent` or `POST /api/admin/collections/{id}/assign` on a collection whose status is already `Scheduled`, `Collected`, `DeliveredToPartner`, or `Completed`.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "Cannot reassign agent after the collection has been confirmed by the agent." }
    ```
- **Reason / Guardrail**: Once a collection agent has formally accepted an assignment and locked it into their schedule, administrative reassignment is locked to prevent schedule disorientation and customer confusion.

---

### 5. No Available Collection Agents for AI Dispatch
- **Trigger**: Administrator triggers `POST /api/admin/collections/{id}/assign-agent` when no active, available collection agents exist in the customer's administrative district, or all candidate agents have previously declined the order.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "No available collection agents found in customer location for AI dispatch." }
    ```
- **Reason / Guardrail**: Prevents hallucinated or null assignments and flags dispatch bottlenecks for administrative intervention.

---

### 6. Non-Existent Agent ID in Manual Assignment
- **Trigger**: Administrator submits `POST /api/admin/collections/{id}/assign` with an `assignedCollectionAgentId` that does not exist in ASP.NET Identity.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "Specified collection agent does not exist." }
    ```
- **Reason / Guardrail**: Enforces referential integrity between collection requests and registered user accounts.

---

### 7. Accepting an Assignment Intended for Another Agent
- **Trigger**: An authenticated collection agent attempts to call `POST /api/agent/collections/{id}/accept` on a collection where `collection.AssignedCollectionAgentId != userId`.
- **Expected System Behaviour**:
  - HTTP Status: `403 Forbidden`
- **Reason / Guardrail**: Agents may only accept jobs specifically allocated to their user ID.

---

### 8. Accepting Job in Non-Assigned State
- **Trigger**: Agent attempts to call `POST /api/agent/collections/{id}/accept` when the collection status is not `AgentAssigned` (e.g. already `Scheduled`, `Requested`, or `Completed`).
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "Order cannot be accepted in current status: <Status>" }
    ```
- **Reason / Guardrail**: Idempotency and state machine protection; only pending assignments may be accepted.

---

### 9. Single Active In-Progress Job Violation
- **Trigger**: A collection agent with an existing job in `Scheduled` or `Collected` status attempts to accept a second job via `POST /api/agent/collections/{id}/accept`.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "You already have an active pickup in progress. You cannot accept another job until you complete and hand over your current accepted pickup." }
    ```
- **Reason / Guardrail**: Quality-of-service guardrail preventing agent overload, transit bottlenecks, and delivery delays.

---

### 10. Rejecting Job Outside `AgentAssigned` State
- **Trigger**: Agent attempts to call `POST /api/agent/collections/{id}/reject` after the job has already been marked `Scheduled` or `Collected`.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "Order cannot be rejected in current status: <Status>" }
    ```
- **Reason / Guardrail**: An agent cannot unilaterally cancel or decline a job once physical collection has commenced.

---

### 11. Unauthorized Status Progression by Agent
- **Trigger**: A non-admin collection agent attempts to invoke `POST /api/agent/collections/{id}/status` with a status value other than `Scheduled`, `Collected`, `DeliveredToPartner`, or `Completed`.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "Collection agents can only mark items as Scheduled, Collected, DeliveredToPartner, or Completed." }
    ```
- **Reason / Guardrail**: Limits field agent authority strictly to physical operational transitions.

---

### 12. Partner Facility Cross-Intake Attempt
- **Trigger**: A partner user attempts to confirm intake via `POST /api/partner/collections/{id}/receive` on a collection where `collection.PartnerId != partner.Id`.
- **Expected System Behaviour**:
  - HTTP Status: `403 Forbidden`
- **Reason / Guardrail**: Partner facilities can only inspect and confirm packages consigned to their own facility.

---

### 13. Partner Intake Photo Exceeding Size Limit
- **Trigger**: Partner uploads an intake verification image exceeding 10 MB in size via `POST /api/partner/collections/{id}/receive`.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "File size must be less than 10MB." }
    ```
- **Reason / Guardrail**: Mitigates denial-of-service, upload timeouts, and unconstrained file storage growth.

---

### 14. Partner Intake Photo Unsupported File Type
- **Trigger**: Partner uploads a file with an invalid extension (e.g. `.pdf`, `.exe`, or `.gif`) via `POST /api/partner/collections/{id}/receive`.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "Only JPG, PNG, and WebP images are allowed." }
    ```
- **Reason / Guardrail**: File upload security and media format standardization.

---

### 15. Deletion of Agent Profile with In-Progress Jobs
- **Trigger**: Administrator attempts `DELETE /api/admin/collection-agents/{id}` for an agent who has collections in `Scheduled` or `Collected` status.
- **Expected System Behaviour**:
  - HTTP Status: `400 Bad Request`
  - Response Body:
    ```json
    { "error": "Cannot delete agent while they have active collection jobs in progress (Scheduled or Collected). Please complete or reassign those jobs first." }
    ```
- **Reason / Guardrail**: Prevents active custody packages from entering an orphaned state without an accountable agent.
