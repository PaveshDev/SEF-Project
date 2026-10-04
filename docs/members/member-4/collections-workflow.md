# Collections Workflow

## Overview

The Collections subsystem in LoopWorth manages the logistics lifecycle of collecting verified, customer-submitted electronic waste and delivering it safely to designated partner recycling or refurbishment facilities across Sri Lanka. The workflow bridges customer collection scheduling, agentic AI route/agent dispatching, collection agent doorstep pickup, partner facility intake verification, and automated customer transactional notifications.

---

## Actors

The Collections workflow involves five distinct roles and automated services identified in the codebase:

1. **Customer (`Customer` role)**
   - Schedules preferred pickup date and morning/afternoon time window once an item recovery request is approved and a partner is selected.
   - Monitors live pickup status, assigned agent details, and historical status updates through the customer dashboard.

2. **Collection Agent (`CollectionAgent` role)**
   - Assigned to specific Sri Lankan districts and designated town coverage zones.
   - Reviews assigned pickup orders via the agent portal.
   - Accepts or declines incoming jobs (providing reasons for rejections).
   - Executes doorstep pickups, marks items as collected, transports them, and completes delivery handover to partner facilities.

3. **Administrator (`Admin` role)**
   - Oversees all pending, active, and completed collection requests across the national network.
   - Manages collection agent profiles, service areas, and availability.
   - Initiates or re-runs AI automated agent dispatch, or manually overrides agent assignments and schedules.
   - Resends delivery confirmation emails or marks collections complete if administrative escalation is necessary.

4. **Partner (`Partner` role)**
   - Recycling or refurbishment facility linked to the customer's recovery request.
   - Inspects physical items upon arrival at the facility.
   - Submits formal intake confirmation including facility photographs, condition verification (e.g. transit damages or defects), and handover remarks.

5. **AI Planning & Notification Agents (`CollectionPlanningAgent` & `DeliveryNotificationAgent`)**
   - **CollectionPlanningAgent**: Evaluates customer district and town against registered collection agents, checks calendar availability, verifies current active workloads, and selects the optimal agent.
   - **DeliveryNotificationAgent**: Composes a tailored transactional completion summary email sent to the customer via Brevo upon partner intake.

---

## End-to-End Workflow

```
[Customer Item Approved & Partner Selected]
                     │
                     ▼
[1. Customer Schedules Pickup Preference]
   - Endpoint: POST /api/recovery/{id}/collections
   - Initial Status: Requested
                     │
                     ▼
[2. AI Agent Dispatch / Admin Assignment]
   - Endpoints: POST /api/collections/{id}/plan
               POST /api/admin/collections/{id}/assign-agent
   - Logic: Prioritizes agents by District, TownArea match, conflict-free schedule, lowest active jobs
   - Status: AgentAssigned
                     │
         ┌───────────┴───────────┐
         ▼                       ▼
  [Agent Accepts Job]     [Agent Declines Job]
   - POST .../accept       - POST .../reject (reason recorded)
   - Status: Scheduled     - Status returns to Requested
   - Agent set to Busy     - Re-queued for alternative AI dispatch
         │
         ▼
[3. Doorstep Pickup Execution]
   - Endpoint: POST /api/agent/collections/{id}/status (Status = Collected)
   - Note: Agent picks up packaged item from customer
   - Status: Collected
                     │
                     ▼
[4. Transport & Partner Handover]
   - Endpoint: POST /api/agent/collections/{id}/status (Status = DeliveredToPartner)
   - Item handed over at partner facility gate
   - Status: DeliveredToPartner
                     │
                     ▼
[5. Partner Intake Verification]
   - Endpoint: POST /api/partner/collections/{id}/receive
   - Partner uploads proof photo, inspects condition, inputs defect feedback
   - Status: Completed
   - Agent availability restored to Available
                     │
                     ▼
[6. Automated Customer Delivery Notification]
   - Handled via TriggerDeliveryCompletionEmailAsync with Brevo SMTP
   - DeliveryEmailSent recorded in database
```

### Detailed Sequence of Steps

1. **Prerequisite Validation**:
   - The recovery request must be in `RecoveryStatus.Approved`.
   - A recovery partner must already be selected (`PartnerSelection` entity must exist).
   - If a non-cancelled collection request already exists for the recovery request, the customer's submission updates the existing preference instead of creating a duplicate.

2. **Customer Preference Submission**:
   - The customer selects a preferred pickup date and time window (`PreferredStartTime` to `PreferredEndTime`).
   - The request is saved with `CollectionStatus.Requested`, and an initial entry is written to `CollectionStatusHistories`.

3. **Intelligent Agent Dispatch**:
   - The dispatch system determines the target district and town from customer profile data (falling back to the partner's service area).
   - Candidate collection agents must have active profiles (`IsActive = true`, `IsAvailable = true`).
   - Time-overlap conflicts with existing scheduled pickups are calculated.
   - The AI agent or deterministic fallback ranks candidates by availability at the time window, town name matching (`c.TownArea.Contains(customerTown)`), and active job count.
   - The collection request is updated to `CollectionStatus.AgentAssigned`.

4. **Agent Confirmation / Refusal**:
   - The assigned agent views their queue at `/agent/collections`.
   - **Accept**: The agent confirms acceptance. The status advances to `CollectionStatus.Scheduled`. The agent profile is synchronized to busy (`IsAvailable = false`).
   - **Reject**: The agent provides a refusal reason. The agent is unassigned, added to an exclusion set in history, and the status reverts to `CollectionStatus.Requested` for alternative dispatch.

5. **Doorstep Collection**:
   - On the scheduled date, the agent arrives at the customer's address, inspects packaging, and marks the item as `CollectionStatus.Collected`.

6. **Partner Handover**:
   - The agent delivers the item to the partner organization.
   - The agent updates the status to `CollectionStatus.DeliveredToPartner`.

7. **Partner Intake & Inspection**:
   - The partner organization accesses `/partner/collections`.
   - The partner conducts intake inspection, uploads an intake photograph (up to 10MB; JPG, PNG, or WebP), indicates whether the item arrived undamaged (`ConditionOk`), and provides optional remarks.
   - The collection advances to `CollectionStatus.Completed`.
   - The assigned agent's availability is recalculated (`SyncAgentAvailabilityAsync`) and freed if no other in-progress jobs remain.

8. **Customer Delivery Email**:
   - Upon completion, the system triggers `TriggerDeliveryCompletionEmailAsync`.
   - The `DeliveryNotificationAgent` creates an HTML confirmation email detailing the item received, partner facility remarks, and condition outcome, dispatched via Brevo.

---

## Important Business Rules

1. **Single Active In-Progress Job per Agent**:
   - A collection agent cannot accept a new pickup if they already have an active job in `Scheduled` or `Collected` status.
   - Attempting to accept a second job returns HTTP 400 with: *"You already have an active pickup in progress. You cannot accept another job until you complete and hand over your current accepted pickup."*

2. **Re-assignment Lock**:
   - An administrator cannot reassign a collection request once it has been confirmed and scheduled by the agent (allowed only when status is `Requested` or `AgentAssigned`).

3. **Geographic Zone Prioritization**:
   - Sri Lanka's 25 districts each feature two dedicated collection agents covering distinct sub-regions/towns.
   - The system prioritizes agents whose `TownArea` substring matches the customer's specified town.

4. **Status Progression Guardrails**:
   - Non-admin collection agents are restricted to updating statuses only to `Scheduled`, `Collected`, `DeliveredToPartner`, or `Completed`.
   - Reverting completed or cancelled collections through the agent endpoint is blocked.

5. **Audit Trail**:
   - Every status modification creates an immutable `CollectionStatusHistory` row capturing `Status`, `ChangedByUserId`, `Note`, and `ChangedAt`.
