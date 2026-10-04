# Collections Verification Checklist

This checklist tracks the implementation and verification status of the Collections feature set across customer, admin, collection agent, and partner interfaces.

> **Note**: In accordance with the verification policy, checkboxes are marked `[x]` ONLY for items verified through genuine execution against the running system. Unverified items remain `[ ]`.

---

## 1. Environment & Infrastructure Verification

- [x] Backend API compiles with 0 errors and executes on `http://localhost:5080`
- [x] Frontend React application compiles and runs on `http://localhost:5173`
- [x] Neon PostgreSQL database connection established and migrations applied
- [x] Identity & JWT authentication functional for Admin (`loopworthadmin@gmail.com`)

---

## 2. Collection Agent Management & National Seeding

- [x] 50 collection agents seeded across all 25 districts of Sri Lanka (2 per district)
- [x] Collection agent profiles populated with distinct municipal/town area coverage (`TownArea`)
- [x] Admin endpoint `GET /api/admin/collection-agents` returns complete active roster with service areas and contact details
- [ ] Admin can update collection agent availability manually through web portal
- [ ] Admin can add a new collection agent profile through web interface

---

## 3. Customer Pickup Scheduling Functions

- [ ] Customer can navigate to `/recovery/:id/schedule` after item approval
- [ ] Customer can select preferred pickup date and time window
- [ ] System prevents scheduling if recovery request is not yet approved
- [ ] System prevents scheduling if partner has not been selected
- [ ] Form submission generates collection order in `Requested` status
- [ ] Updating preference on existing collection request modifies existing order without creating duplicates
- [ ] Customer can view scheduled pickups and live status in `/collections`

---

## 4. Intelligent Dispatch & Admin Allocation

- [x] Planning agent algorithm ranks agents by district, town substring match, calendar conflicts, and active job count
- [ ] Automated AI plan generation succeeds via `POST /api/collections/:id/plan`
- [ ] Admin can trigger AI assignment via `POST /api/admin/collections/:id/assign-agent`
- [ ] Admin can manually override and assign a collection agent via `POST /api/admin/collections/:id/assign`
- [ ] Reassigning agent is blocked once order has been confirmed (`Scheduled`) by agent
- [ ] Admin can filter collection requests by status (`Requested`, `AgentAssigned`, etc.)

---

## 5. Collection Agent Operations

- [ ] Agent can log in and view assigned pickups at `/agent/jobs`
- [ ] Agent can accept assigned job, moving status to `Scheduled`
- [ ] Accepting job synchronizes agent availability to Busy (`IsAvailable = false`)
- [ ] Agent is blocked from accepting a second job while an existing job is in `Scheduled` or `Collected` status
- [ ] Agent can reject assigned job with reason; order returns to `Requested` with refusal note
- [ ] Rejected agent is excluded from immediate automated re-dispatch
- [ ] Agent can update status to `Collected` upon doorstep pickup
- [ ] Agent can update status to `DeliveredToPartner` upon facility handover

---

## 6. Partner Facility Intake & Inspection

- [ ] Partner organization can view incoming handovers at `/partner/collections`
- [ ] Partner can upload intake verification photo (< 10MB; JPG, PNG, WebP)
- [ ] Partner can record condition check (`ConditionOk`) and defect/damage remarks
- [ ] Submitting intake advances collection status to `Completed`
- [ ] Completing intake automatically frees collection agent availability (`IsAvailable = true`)

---

## 7. Customer Delivery Notification & Completion

- [ ] Automated completion email generation triggers upon partner intake
- [ ] Transactional email dispatched via Brevo with item details and partner remarks
- [ ] Email dispatch records `DeliveryEmailSent = true` and timestamp in database
- [ ] Admin can manually resend delivery email via `POST /api/admin/collections/:id/send-delivery-email`
- [ ] Completed collection is archived in customer and partner history views
