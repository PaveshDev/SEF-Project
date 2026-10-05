# Partner Status Lifecycle & State Transitions

## 1. Partner Organization Status Lifecycle

```mermaid
stateDiagram-v2
    [*] --> PendingVerification: Organization Registered
    PendingVerification --> Verified: Admin Approval & Compliance Check
    PendingVerification --> Rejected: Compliance Failure / Incomplete
    Verified --> Suspended: Policy Violation / High Error Rate
    Suspended --> Verified: Re-audit Passed
    Verified --> Inactive: Partner Requested Deactivation
    Inactive --> Verified: Reactivation Request Approved
    Rejected --> [*]
```

### Partner Statuses:
- **`PendingVerification`**: Initial state after organization sign-up. Cannot receive item matches.
- **`Verified`**: Fully approved partner active on the directory, eligible for AI matching and dispatch.
- **`Suspended`**: Temporarily barred from receiving new assignments due to compliance or SLA issues.
- **`Inactive`**: Dormant or seasonally paused partner account.
- **`Rejected`**: Registration declined by platform administration.

---

## 2. Partner Match & Selection Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Proposed: AI Matching Agent Generates Candidate
    Proposed --> Recommended: High Confidence Match Filter
    Recommended --> Selected: Customer Selects Partner
    Recommended --> Expired: Customer Selects Alternative
    Selected --> Accepted: Partner Confirms Intake Capacity
    Selected --> Declined: Partner Declines (Capacity Exceeded)
    Accepted --> InProgress: Items In Transit / Processing
    InProgress --> Completed: Items Processed & Certified
```

### Match States:
- **`Proposed`**: Candidate scored by `PartnerMatchingAgent`.
- **`Recommended`**: Ranked in top recommendations shown to customer.
- **`Selected`**: Chosen by customer for pickup or drop-off fulfillment.
- **`Accepted`**: Partner accepted the assignment into their processing queue.
- **`Completed`**: Partner marked recovery/recycling processing as finished.
