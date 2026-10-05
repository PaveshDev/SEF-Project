# Collections Role Permissions

## Overview

The Collections subsystem in LoopWorth enforces strict Role-Based Access Control (RBAC) across ASP.NET Identity roles and entity ownership checks. Every operation verifies both the caller's JWT role claims and contextual constraints (such as item ownership or facility association).

---

## Role Permissions Matrix

| Operation | Customer | Admin | Collection Agent | Partner |
| :--- | :---: | :---: | :---: | :---: |
| **Create / Update Pickup Preference** | Allowed (Own items) | Allowed | Denied | Denied |
| **View Collection Details** | Allowed (Own items) | Allowed (All) | Allowed (Assigned) | Allowed (Linked Partner) |
| **List Collections** | Allowed (Own items) | Allowed (All) | Allowed (Assigned) | Allowed (Linked Partner) |
| **Trigger AI Collection Planning** | Allowed (Own items) | Allowed (All) | Denied | Denied |
| **Auto-Dispatch Agent with AI** | Denied | Allowed | Denied | Denied |
| **Manual Agent Assignment / Reschedule** | Denied | Allowed | Denied | Denied |
| **Accept Assigned Pickup Job** | Denied | Denied | Allowed (Assigned Agent) | Denied |
| **Reject Assigned Pickup Job** | Denied | Denied | Allowed (Assigned Agent) | Denied |
| **Mark Item Collected (Doorstep)** | Denied | Allowed (Override) | Allowed (Assigned Agent) | Denied |
| **Mark Delivered to Partner** | Denied | Allowed (Override) | Allowed (Assigned Agent) | Denied |
| **Confirm Partner Receipt (Intake)** | Denied | Denied | Denied | Allowed (Linked Partner) |
| **Admin Direct Complete** | Denied | Allowed | Denied | Denied |
| **Resend Delivery Completion Email** | Denied | Allowed | Denied | Denied |
| **Manage Agent Roster (CRUD)** | Denied | Allowed | Denied | Denied |
| **View Audit / Status History** | Allowed (Own items) | Allowed (All) | Allowed (Assigned) | Allowed (Linked Partner) |

---

## Detailed Role Responsibilities & Guardrails

### 1. Customer (`Customer` role)
- **Permissions**:
  - `POST /api/recovery/{id}/collections`: Allowed only if the authenticated user is the item owner (`recovery.CustomerId == userId`) or an Admin. The recovery request must be in `Approved` status and have a confirmed `PartnerSelection`.
  - `POST /api/collections/{id}/plan`: Can initiate AI planning for their own collection requests.
  - `GET /api/collections`: Retrieves only the collection requests created by the calling customer.
  - `GET /api/collections/{id}`: Can inspect collection details and the complete chronological `StatusHistory` for their own items.
- **Guardrails**:
  - Customers cannot assign, accept, or reject collection agents.
  - Customers cannot alter the operational status (`Collected`, `DeliveredToPartner`, `Completed`).
  - Attempting to access another user's collection returns `HTTP 403 Forbidden`.

---

### 2. Administrator (`Admin` role)
- **Permissions**:
  - `GET /api/admin/collections`: Full visibility over all collection orders across Sri Lanka with status filtering (`?status=...`).
  - `POST /api/admin/collections/{id}/assign-agent`: Triggers AI agent dispatch or re-dispatch.
  - `POST /api/admin/collections/{id}/assign`: Overrides agent assignment, scheduled date, and time slot manually.
  - `POST /api/admin/collections/{id}/complete`: Administratively finalizes collections, triggering workflow completion and Brevo completion email.
  - `POST /api/admin/collections/{id}/send-delivery-email`: Dispatches or resends transactional delivery emails via Brevo SMTP.
  - `GET|POST|PUT|DELETE /api/admin/collection-agents`: Manages national collection agent profiles, district territories, and contact details.
- **Guardrails**:
  - Admin cannot reassign an agent once the collection has been accepted and locked into `Scheduled` status by the agent (returns `HTTP 400 Bad Request`).
  - Admin cannot delete an agent profile if that agent has active in-progress jobs (`Scheduled` or `Collected`).

---

### 3. Collection Agent (`CollectionAgent` role)
- **Permissions**:
  - `GET /api/agent/collections`: Retrieves only jobs assigned to the caller (`AssignedCollectionAgentId == userId`).
  - `POST /api/agent/collections/{id}/accept`: Confirms and accepts an assigned job, locking it into `Scheduled` status and marking the agent profile `IsAvailable = false`.
  - `POST /api/agent/collections/{id}/reject`: Declines an assigned job with an optional reason note, returning the request to `Requested` status for re-dispatch.
  - `POST /api/agent/collections/{id}/status`: Advances status from `Scheduled` to `Collected`, and from `Collected` to `DeliveredToPartner`.
- **Guardrails**:
  - **Single Active Job Rule**: An agent cannot accept a job if they already have an active job in `Scheduled` or `Collected` status.
  - Cannot accept or update jobs assigned to other agents (`AssignedCollectionAgentId != userId` returns `HTTP 403 Forbidden`).
  - Cannot accept jobs not in `AgentAssigned` status (returns `HTTP 400 Bad Request`).
  - Restricted from directly altering partner intake verifications (`PartnerReceivedConditionOk`, `PartnerPhotoUrl`).

---

### 4. Partner (`Partner` role)
- **Permissions**:
  - `GET /api/partner/collections`: Lists incoming and intake-verified collections linked to the partner organization.
  - `POST /api/partner/collections/{id}/receive`: Confirms physical package receipt, uploads facility intake verification photos (under 10MB, JPG/PNG/WebP), records defect/damage notes, and verifies package condition (`ConditionOk`).
- **Guardrails**:
  - The calling user account must be linked to the `Partner` organization record associated with the collection request (`collection.PartnerId == partner.Id`); otherwise returns `HTTP 403 Forbidden`.
  - Partners cannot reassign agents, alter scheduled dates, or bypass intake verification.
