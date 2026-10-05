# Partner Matching & Management Workflow

## Overview
The Partner Management and Matching module in the LoopWorth platform connects customers and organizations with verified recycling, refurbishing, and donation partners. It features intelligent AI-assisted partner matching (`PartnerMatchingAgent`), partner directory discovery, performance dashboards, and customer partner selection workflows.

## Key Actors
- **Customer**: Browses verified partners, views AI-driven partner recommendations for items/recovery plans, selects preferred partners, and tracks partner engagement.
- **Partner / Organization**: Manages organization profile, service offerings, accepted waste categories, operating regions, capacity, and reviews matched recovery items.
- **Admin**: Verifies partner registrations, audits compliance, manages partner statuses (Pending, Verified, Suspended), and oversees platform-wide partner analytics.
- **PartnerMatchingAgent (System/AI)**: Evaluates item specifications, material categories, geographic proximity, processing capabilities, capacity, and historical reliability ratings to score and rank optimal partner matches.

## Core Workflows

### 1. Partner Onboarding & Verification
1. **Registration**: Organization submits details including name, business registration, contact details, accepted device categories, recycling/refurbishing capabilities, and operating regions. Initial status is `PendingVerification`.
2. **Verification & Activation**: Admin reviews submitted compliance documents, certifications, and operational capabilities. Admin approves (status `Verified`) or rejects with feedback.
3. **Service & Capacity Configuration**: Verified partners configure accepted material streams, maximum daily/monthly intake quotas, and supported recovery routes (`Recycle`, `Refurbish`, `Donate`).

### 2. AI Partner Matching Process
1. **Trigger**: When an item completes assessment and enters recovery planning, the system invokes `PartnerMatchingAgent`.
2. **Criteria Evaluation**:
   - **Category Compatibility**: Matches item device type and materials against partner accepted lists.
   - **Route Capability**: Validates partner processing licenses for the selected route (e.g. e-waste disassembly vs. refurbish/resale).
   - **Geographic Proximity**: Calculates proximity score between customer pickup location and partner facility.
   - **Capacity & SLA**: Verifies partner current backlog and processing throughput.
   - **Reliability Rating**: Weights past on-time processing and quality ratings.
3. **Recommendation Ranking**: Generates scored match candidates with match rationale and confidence percentages.

### 3. Customer Selection & Dispatch
1. Customer reviews top AI-recommended partners on the web or mobile interface.
2. Customer selects a partner (`PartnerSelection`).
3. Status moves to `PartnerAssigned` and notifies the partner for pickup scheduling / batch intake.

## Business Rules & Constraints
- **Verification Requirement**: Unverified or suspended partners cannot be matched or assigned to customer recovery requests.
- **Category Strictness**: Partners can only receive item streams for categories they are certified to handle.
- **Capacity Limits**: If a partner reaches 100% capacity utilization, the matching engine automatically down-weights them and alerts administrators.
- **Ownership Isolation**: Partners can only view items and match details explicitly assigned to their organization.
