# Collection Agent Operational Guide

## Overview

This guide serves as an operational manual for registered LoopWorth Collection Agents (`CollectionAgent` role). It outlines the day-to-day procedures for receiving dispatch assignments, accepting or rejecting jobs, conducting doorstep pickups, managing physical custody, and executing partner facility handovers.

---

## Agent Portal Access & Authentication

1. **Credentials**: Collection agents log in using their registered email and secure password via the LoopWorth web application (`/login`) or mobile application.
2. **Role Profile**: Upon authentication, the JWT token includes the role claim `"CollectionAgent"`.
3. **Assigned Territories**: Each agent is registered with a designated Sri Lankan administrative district (`ServiceArea`) and specific municipal/divisional town areas (`TownArea`). Dispatch assignments strictly prioritize these zones.

---

## Step-by-Step Field Workflow

```text
[1. View Assigned Jobs] (/agent/collections)
           │
     ┌─────┴────────────────────────┐
     ▼                              ▼
[2a. Accept Job]             [2b. Reject Job]
  - Locks into Schedule        - Returns to Requested pool
  - Agent set to Busy          - Reason note recorded
     │
     ▼
[3. Doorstep Pickup]
  - Arrive at Customer Address
  - Verify Packaging & Item
  - Update status to Collected
     │
     ▼
[4. Transit & Partner Handover]
  - Transport package safely
  - Arrive at Partner Intake Bay
  - Update status to DeliveredToPartner
     │
     ▼
[5. Facility Intake Completion]
  - Partner conducts verification & photo upload
  - Status becomes Completed
  - Agent availability automatically restored to Available
```

---

### Step 1: Review Assigned Jobs
- **Interface**: Navigate to `/agent/jobs` or `/agent/collections`.
- **API Request**: `GET /api/agent/collections`
- **Details Displayed**:
  - Customer contact name, telephone number, and residential address.
  - Customer administrative district and town.
  - Scheduled pickup date and allocated time window (e.g. 09:00 - 12:00).
  - Target partner facility name and address.
  - Item details (category, brand, model, packaging description, and reference photos).

---

### Step 2: Accept or Reject the Job

#### Option A: Accepting the Job
- **Action**: Click **Accept Job** in the agent portal.
- **API Request**: `POST /api/agent/collections/{id}/accept`
- **System Guardrail**:
  - The system checks if the agent already has an active pickup in progress (`Scheduled` or `Collected`).
  - If the agent is currently engaged on another job, acceptance is blocked with HTTP 400: *"You already have an active pickup in progress. You cannot accept another job until you complete and hand over your current accepted pickup."*
- **Outcome**:
  - Collection status updates to `Scheduled`.
  - Agent profile availability flag automatically updates to `IsAvailable = false` (marked Busy).
  - The appointment is confirmed for field execution.

#### Option B: Rejecting the Job
- **Action**: If vehicle breakdown, sickness, or localized travel issues prevent pickup, click **Decline Job** and provide an explanatory reason.
- **API Request**: `POST /api/agent/collections/{id}/reject`
- **Payload**:
  ```json
  {
    "reason": "Road closure / severe localized flooding on route."
  }
  ```
- **Outcome**:
  - Collection status reverts to `Requested`.
  - `AssignedCollectionAgentId` is set to `null`.
  - The refusal reason is recorded in `CollectionStatusHistories`.
  - The dispatch engine automatically notes the refusal and excludes this agent from immediate re-dispatch for this order.

---

### Step 3: Conduct Doorstep Customer Pickup
1. **Arrival**: Arrive at the customer residence within the scheduled window.
2. **Customer Identity Verification**: Verify customer name and inspect the e-waste item against the collection manifest.
3. **Packaging Inspection**: Ensure items (especially fragile screens or lithium-ion batteries) are properly secured.
4. **Mark as Collected**:
   - In the portal, click **Mark as Collected**.
   - **API Request**: `POST /api/agent/collections/{id}/status`
   - **Payload**:
     ```json
     {
       "status": "Collected",
       "note": "Package received from customer doorstep in good physical order."
     }
     ```
   - **Outcome**: Status transitions to `Collected`. Customer dashboard updates in real time to indicate transit is underway. Agent remains in Busy state.

---

### Step 4: Facility Transit & Handover
1. **Transport**: Transport the package directly to the assigned partner facility (e.g. Cleantech Colombo, Ceylon Waste Management).
2. **Facility Check-in**: Present the collection reference number or digital handover pass at the facility receiving dock.
3. **Mark as Delivered**:
   - Click **Handover to Partner**.
   - **API Request**: `POST /api/agent/collections/{id}/status`
   - **Payload**:
     ```json
     {
       "status": "DeliveredToPartner",
       "note": "Delivered to receiving dock bay 2. Awaiting partner intake verification."
     }
     ```
   - **Outcome**: Status transitions to `DeliveredToPartner`. Custody is officially transferred to the partner facility.

---

### Step 5: Partner Intake & Agent Release
1. **Intake Inspection**: Partner facility staff inspect the item, take proof photographs, and record condition remarks via `POST /api/partner/collections/{id}/receive`.
2. **Release of Agent**:
   - Once the partner confirms intake, the collection status advances to `Completed`.
   - The system triggers `SyncAgentAvailabilityAsync`.
   - The agent's active in-progress job count drops to 0, and `IsAvailable` automatically resets to `true` (Available).
   - The agent is now free to accept their next assigned pickup order.

---

## Key Operating Rules for Agents

1. **Strict FIFO Execution**: Never attempt to hold multiple concurrent orders in transit; complete one doorstep-to-facility delivery cycle before starting another.
2. **Clear Rejection Reasons**: Always provide honest, descriptive rejection reasons so dispatchers can quickly reassign jobs to backup agents.
3. **Contact Accuracy**: Keep phone and vehicle contact details up to date via your administrator to ensure customers can reach you during doorstep arrivals.
