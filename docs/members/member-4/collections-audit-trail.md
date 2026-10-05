# Collection Audit Trail

## Overview

Traceability and non-repudiation in electronic waste logistics are critical for compliance, security, and dispute resolution. In LoopWorth, every lifecycle transition of a collection request is recorded as an immutable audit event in the `CollectionStatusHistories` table.

---

## Relationship Model

The relationship between the primary collection entity and its audit log is a one-to-many parent-child relationship:

```text
CollectionRequest (1)
       │
       ▼
CollectionStatusHistory (N)
       │
       ├── Status change 1: Requested (Customer submission)
       ├── Status change 2: AgentAssigned (AI dispatch or Admin allocation)
       ├── Status change 3: Scheduled (Agent acceptance)
       ├── Status change 4: Collected (Doorstep pickup)
       ├── Status change 5: DeliveredToPartner (Facility handover)
       └── Status change 6: Completed (Partner intake inspection & verification)
```

---

## Entity Schemas (Verified from Source)

### 1. `CollectionRequest` Entity
Defined in `LoopWorth.Domain.Entities.CollectionRequest`:

| Field | Type | Description |
| :--- | :--- | :--- |
| `Id` | `Guid` | Unique primary key identifier. |
| `RecoveryRequestId` | `Guid` | Foreign key referencing the parent recovery request. |
| `PartnerId` | `Guid` | Foreign key referencing the assigned partner facility. |
| `CustomerId` | `string` | Identity user ID of the customer requesting collection. |
| `PreferredPickupDate` | `DateTime?` | Customer's requested pickup date. |
| `PreferredStartTime` | `TimeSpan?` | Start of customer's preferred pickup window. |
| `PreferredEndTime` | `TimeSpan?` | End of customer's preferred pickup window. |
| `SuggestedPickupDate` | `DateTime?` | Date recommended by `CollectionPlanningAgent`. |
| `SuggestedStartTime` | `TimeSpan?` | Window start recommended by AI. |
| `SuggestedEndTime` | `TimeSpan?` | Window end recommended by AI. |
| `SuggestedCollectionAgentId` | `string?` | Agent ID suggested by AI dispatch. |
| `ScheduledPickupDate` | `DateTime?` | Confirmed/scheduled pickup date. |
| `ScheduledStartTime` | `TimeSpan?` | Confirmed window start. |
| `ScheduledEndTime` | `TimeSpan?` | Confirmed window end. |
| `AssignedCollectionAgentId` | `string?` | Identity user ID of the assigned collection agent. |
| `Status` | `CollectionStatus` | Current operational state enum value. |
| `CreatedAt` | `DateTime` | UTC timestamp of initial request creation. |
| `UpdatedAt` | `DateTime` | UTC timestamp of most recent state update. |
| `PartnerPhotoUrl` | `string?` | Stored intake image URL uploaded by partner facility. |
| `PartnerFeedback` | `string?` | Intake remarks and condition notes from partner. |
| `PartnerConfirmedAt` | `DateTime?` | UTC timestamp when partner acknowledged receipt. |
| `PartnerReceivedConditionOk` | `bool?` | Intake verification flag (`true` if undamaged). |
| `DeliveryEmailSent` | `bool` | Flag indicating customer transactional email dispatch. |
| `DeliveryEmailSentAt` | `DateTime?` | UTC timestamp of email dispatch. |
| `DeliveryEmailSubject` | `string?` | Subject line of generated delivery email. |
| `StatusHistory` | `ICollection<CollectionStatusHistory>` | Navigation collection of historical audit records. |

---

### 2. `CollectionStatusHistory` Entity
Defined in `LoopWorth.Domain.Entities.CollectionStatusHistory`:

| Field | Type | Description |
| :--- | :--- | :--- |
| `Id` | `Guid` | Unique primary key of the audit log entry. |
| `CollectionRequestId` | `Guid` | Foreign key referencing the parent collection request. |
| `Status` | `CollectionStatus` | The status enum value at the time of this transition. |
| `ChangedByUserId` | `string?` | Identity user ID of the actor initiating the transition. |
| `Note` | `string?` | Contextual explanation, reason code, or handover remark. |
| `ChangedAt` | `DateTime` | Exact UTC timestamp of the status transition. |
| `CollectionRequest` | `CollectionRequest` | Navigation reference to the parent collection request. |

---

## Status Changes & Event Triggers

Every state change appends a new `CollectionStatusHistory` record:

1. **Preference Submitted / Updated** (`Status = Requested`)
   - Actor: Customer (`ChangedByUserId = customerId`)
   - Note: `"Pickup preference submitted by customer. Awaiting Admin dispatch."` or `"Pickup preference updated by customer. Awaiting Admin dispatch."`

2. **Agent Assigned via AI Dispatch** (`Status = AgentAssigned`)
   - Actor: Admin or System dispatcher (`ChangedByUserId = adminId`)
   - Note: Includes assigned agent name, customer target area, and verification confirmation (e.g. `"Assigned to collection agent Amal Perera via Agentic AI for customer area (Colombo) - verified free on 2026-10-15 at 09:00-12:00."`).

3. **Agent Assignment Rejected** (`Status = Requested`)
   - Actor: Collection Agent (`ChangedByUserId = agentUserId`)
   - Note: Records decline reason (e.g. `"Agent Nimal Silva declined the order (Vehicle mechanical issue). Returned to Admin queue for AI re-dispatch."`).
   - Impact: Used by dispatch algorithm to exclude this agent from immediate automated re-dispatch.

4. **Agent Accepted Job** (`Status = Scheduled`)
   - Actor: Collection Agent (`ChangedByUserId = agentUserId`)
   - Note: `"Collection agent Amal Perera confirmed and accepted the order. Pickup scheduled. Agent duty status: Busy."`

5. **Item Picked Up at Customer Doorstep** (`Status = Collected`)
   - Actor: Collection Agent (`ChangedByUserId = agentUserId`)
   - Note: Custom transit remarks or default note `"Item has been picked up by our collection agent."`

6. **Item Deposited at Facility Gate** (`Status = DeliveredToPartner`)
   - Actor: Collection Agent (`ChangedByUserId = agentUserId`)
   - Note: `"Item has been delivered and handed over to <PartnerName>. Awaiting partner facility intake verification."`

7. **Partner Intake Inspection Completed** (`Status = Completed`)
   - Actor: Partner Facility Staff (`ChangedByUserId = partnerUserId`)
   - Note: Records condition verification and remarks: `"Partner confirmed item receipt at facility. Condition verified. Remarks: <Feedback>"` or `"Damages or defects detected upon intake. Remarks: <Feedback>"`.

8. **Admin Manual Completion** (`Status = Completed`)
   - Actor: Administrator (`ChangedByUserId = adminId`)
   - Note: `"Confirmed received by partner. Collection completed by administrator."`

---

## Traceability & Immutability

1. **Append-Only Immutability**:
   - `CollectionStatusHistory` records are strictly append-only. There are no update or delete endpoints exposed for history records.
   - Any correction or escalation results in an additional chronological entry, preserving the complete sequence of events.

2. **Actor Attribution**:
   - Every event explicitly stores `ChangedByUserId`, enabling end-to-end attribution back to the exact authenticated user account.

3. **Temporal Ordering**:
   - Entries record UTC timestamps (`DateTime.UtcNow`). When mapped to `CollectionRequestDto`, history items are sorted by `ChangedAt` ascending, providing an unambiguous timeline.

---

## Why the Audit Trail Matters

1. **Custody Handover Accountability**:
   - Electronic waste often contains hazardous substances and data storage media. The audit trail clearly documents who possessed the item at each milestone (Customer → Agent → Partner).

2. **Conflict & Damage Dispute Resolution**:
   - When items arrive with damage, the audit trail alongside the partner's intake photograph and `PartnerReceivedConditionOk` flag determines whether defects occurred before pickup or in transit.

3. **Re-dispatch Optimization**:
   - Rejection history records are parsed during automated dispatch to prevent re-assigning a collection request to an agent who already declined it.
