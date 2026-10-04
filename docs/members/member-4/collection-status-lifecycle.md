# Collection Status Lifecycle

## Overview

The Collections module tracks each pickup order through an explicit lifecycle governed by the `CollectionStatus` enum in `LoopWorth.Domain.Enums`. State transitions are strictly controlled by role authorization, domain constraints, and audit logging into `CollectionStatusHistories`.

---

## Statuses

The following statuses are implemented in the codebase:

### 1. `Requested`
- **Meaning**: The customer has submitted their preferred collection date and time window for an approved recovery request. The request is currently in the dispatch queue awaiting agent allocation.
- **Actor Responsible**: Customer (`Customer` role) or System fallback when an agent rejects an assignment.
- **Agent Availability Impact**: None.

### 2. `AgentAssigned`
- **Meaning**: A collection agent has been assigned to the request by the AI dispatch engine or an Administrator. The order is now visible in the agent's job queue for review.
- **Actor Responsible**: AI Dispatch Service (`CollectionPlanningAgent`) or Administrator (`Admin` role).
- **Agent Availability Impact**: Agent remains `Available` until they explicitly accept the assignment.

### 3. `Scheduled`
- **Meaning**: The assigned collection agent reviewed and formally accepted the job. The pickup appointment is locked into the schedule.
- **Actor Responsible**: Assigned Collection Agent (`CollectionAgent` role) via `/api/agent/collections/{id}/accept`.
- **Agent Availability Impact**: Agent's `IsAvailable` flag transitions to `false` (marked Busy) if they have no other open jobs. They cannot accept another job while in this state.

### 4. `Collected`
- **Meaning**: The collection agent visited the customer premises, received the e-waste item, verified physical packaging, and commenced transit to the partner facility.
- **Actor Responsible**: Assigned Collection Agent (`CollectionAgent` role) via `/api/agent/collections/{id}/status`.
- **Agent Availability Impact**: Agent remains Busy (`IsAvailable = false`).

### 5. `DeliveredToPartner`
- **Meaning**: The collection agent arrived at the partner organization facility and physically deposited the collected item. The item is awaiting intake inspection by partner staff.
- **Actor Responsible**: Assigned Collection Agent (`CollectionAgent` role) via `/api/agent/collections/{id}/status`.
- **Agent Availability Impact**: Agent remains linked to the order until final receipt.

### 6. `PartnerReceived`
- **Meaning**: Intermediate status acknowledging physical arrival at the partner dock prior to full condition verification.
- **Actor Responsible**: Partner staff (`Partner` role) or Administrator (`Admin` role).

### 7. `Completed`
- **Meaning**: The partner organization completed intake inspection, submitted verification photographs, and recorded condition/defect remarks (or Admin manually completed). The delivery completion transactional email is dispatched to the customer.
- **Actor Responsible**: Partner (`Partner` role) via `/api/partner/collections/{id}/receive` or Administrator (`Admin` role) via `/api/admin/collections/{id}/complete`.
- **Agent Availability Impact**: The assigned agent's active in-progress job count drops. If no other active jobs remain, their profile automatically transitions back to Available (`IsAvailable = true`).

### 8. `Cancelled`
- **Meaning**: The collection order was cancelled prior to physical execution (e.g. customer withdrew recovery request or order became obsolete).
- **Actor Responsible**: Customer or Administrator.
- **Agent Availability Impact**: Any assigned agent is unlinked, and availability is recalculated.

---

## Valid Progression Matrix

| From Status | To Status | Trigger / Endpoint | Authorized Role | Preconditions & Domain Rules |
| :--- | :--- | :--- | :--- | :--- |
| *(None)* | `Requested` | `POST /api/recovery/{id}/collections` | Customer | Recovery must be Approved; Partner must be selected. |
| `Requested` | `AgentAssigned` | `POST /api/admin/collections/{id}/assign-agent` or `assign` | Admin / AI | Agent must exist and be active; AI checks district and schedule. |
| `AgentAssigned` | `Scheduled` | `POST /api/agent/collections/{id}/accept` | Collection Agent | Agent must NOT already have an active job in `Scheduled` or `Collected`. |
| `AgentAssigned` | `Requested` | `POST /api/agent/collections/{id}/reject` | Collection Agent | Declining agent is unassigned and recorded in history to exclude from immediate re-dispatch. |
| `AgentAssigned` | `AgentAssigned` | `POST /api/admin/collections/{id}/assign-agent` | Admin | Reassigns to a different candidate before pickup confirmation. |
| `Scheduled` | `Collected` | `POST /api/agent/collections/{id}/status` | Collection Agent | Must be currently assigned agent. |
| `Collected` | `DeliveredToPartner` | `POST /api/agent/collections/{id}/status` | Collection Agent | Must be currently assigned agent; handed over at facility. |
| `DeliveredToPartner` | `Completed` | `POST /api/partner/collections/{id}/receive` | Partner | Partner must belong to the linked partner facility; optional photo and remarks recorded. |
| `Requested` / `AgentAssigned` / `Scheduled` | `Completed` | `POST /api/admin/collections/{id}/complete` | Admin | Administrative override to finalize delivery. |
| Any active status | `Cancelled` | Cancel endpoint | Customer / Admin | Cannot cancel once physical collection is completed. |

---

## State Transition Diagram

```
                 ┌────────────────────────────────┐
                 │           Requested            │
                 └──────────────┬─────────────────┘
                                │ AI Dispatch / Admin Assign
                                ▼
                 ┌────────────────────────────────┐
        ┌───────►│         AgentAssigned          │◄──────┐
        │        └───────┬────────────────┬───────┘       │
        │ Rejection      │                │               │ Re-dispatch
        │ (Declined)     │ Accept Job     │ Re-assign     │
        │                ▼                └───────────────┘
        │        ┌────────────────┐
        └────────┤   Scheduled    │ (Agent marked Busy)
                 └───────┬────────┘
                         │ Doorstep Pickup
                         ▼
                 ┌────────────────┐
                 │   Collected    │ (In Transit)
                 └───────┬────────┘
                         │ Facility Handover
                         ▼
                 ┌────────────────┐
                 │DeliveredToPartn│
                 └───────┬────────┘
                         │ Partner Intake & Inspection
                         ▼
                 ┌────────────────┐
                 │   Completed    │ (Agent freed; Brevo email sent)
                 └────────────────┘
```

---

## Invariant Rules

1. **State Immutability of History**:
   - `CollectionStatusHistory` entries are strictly append-only.
   - Status transitions never mutate past history records; each transition writes a new record with UTC timestamp and actor identifier.

2. **Re-assignment Protection**:
   - Once an agent advances a collection to `Scheduled`, the admin endpoints prevent reassigning the agent to ensure accountability.

3. **Concurrency and Availability**:
   - Availability is enforced through `SyncAgentAvailabilityAsync`.
   - Agents cannot hold more than one in-progress order (`Scheduled` or `Collected`), preventing double-booking and ensuring prompt execution.
