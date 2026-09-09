# Collection Agent — Design Document

## Objective

The Collection Agent plans the most feasible pickup arrangement for an approved recovery proposal by evaluating multiple operational constraints and proposing the best available option.

## Agent Type

Multi-step Agentic AI with deterministic backend validation and human approval before high-impact collection actions.

## Inputs

| Input | Source |
|---|---|
| Approved recovery proposal | Recovery module (cross-module contract) |
| Item size, weight, handling requirements | Item assessment data |
| Pickup location | Owner-provided address |
| Owner availability window | Owner profile / pickup request |
| Destination opening hours | Partner module (cross-module contract) |
| Available collection slots | `ReadCollectionSlots` tool |
| Vehicle capacity | `CheckVehicleCapacity` tool |
| Previous failed-pickup information | Pickup events history |

## Allow-Listed Tools

| Tool | Purpose | Input | Output |
|---|---|---|---|
| `ReadCollectionSlots` | Find available time windows | Date range, service area | List of slots with capacity |
| `CheckVehicleCapacity` | Verify vehicle can handle the item | Vehicle class, item weight/dimensions | Pass/fail with reason |
| `ReadHandlingRules` | Check special handling requirements | Item category, handling class | Required equipment, person count |
| `GetTravelEstimate` | Obtain distance/time from routing API | Origin address, destination address | Distance, duration, or unavailable |
| `ProposePickup` | Submit a ranked collection proposal | Slot ID, vehicle, time window, constraints | Structured proposal |
| `ProposeReschedule` | Submit a revised plan after failure | Failed pickup ID, new slot, reason | Revised proposal |

All tool inputs must be validated. Tools must return structured outputs. The agent must not bypass backend business rules.

## Multi-Step Workflow

```
1. Receive approved recovery proposal
2. ReadCollectionSlots → find suitable time windows
3. CheckVehicleCapacity → verify vehicle can handle item
4. ReadHandlingRules → check special requirements
5. Compare owner availability with destination opening hours
6. GetTravelEstimate → obtain distance/time from routing API
7. Evaluate and rank feasible options
8. ProposePickup → recommend best option + fallback
9. PAUSE_FOR_APPROVAL → wait for human decision
10. If approved → confirm booking via backend
11. If rejected → record decision and stop
12. If revision requested → go to step 2 with new constraints
```

## Dynamic Re-Planning (Innovation)

If the original arrangement fails (e.g., assigned vehicle becomes unavailable):

```
1. Detect failure event
2. Identify changed constraint (e.g., vehicle unavailable)
3. ReadCollectionSlots → search alternative slots
4. CheckVehicleCapacity → verify alternative vehicles
5. GetTravelEstimate → calculate new travel time
6. Compare alternatives against remaining constraints
7. ProposeReschedule → propose revised collection plan
8. PAUSE_FOR_APPROVAL → request human approval
```

This demonstrates genuine multi-step Agentic AI rather than a simple chatbot.

## Agent Output (Structured Proposal)

```json
{
  "proposalId": "uuid",
  "pickupRequestId": "uuid",
  "recommended": {
    "proposedStart": "2026-09-11T14:00:00+05:30",
    "proposedEnd": "2026-09-11T16:00:00+05:30",
    "collectionMethod": "Pickup from owner address",
    "requiredVehicleType": "Small van",
    "estimatedTravelDistance": "12.4 km",
    "estimatedTravelTime": "35 min",
    "estimatedCost": 1500.00,
    "currency": "LKR",
    "handlingRequirements": ["Keep upright", "Protect finish"]
  },
  "fallback": {
    "proposedStart": "2026-09-11T16:00:00+05:30",
    "proposedEnd": "2026-09-11T18:00:00+05:30",
    "collectionMethod": "Pickup from owner address",
    "requiredVehicleType": "Small van",
    "estimatedTravelDistance": null,
    "estimatedTravelTime": null,
    "estimatedCost": null,
    "currency": "LKR",
    "handlingRequirements": ["Keep upright", "Protect finish"]
  },
  "feasibilityStatus": "FEASIBLE | INFEASIBLE | MANUAL_REVIEW",
  "reasonForRecommendation": "...",
  "constraintChecks": [
    { "constraint": "Vehicle capacity", "passed": true, "detail": "..." },
    { "constraint": "Owner availability", "passed": true, "detail": "..." },
    { "constraint": "Destination hours", "passed": true, "detail": "..." },
    { "constraint": "Travel feasibility", "passed": false, "detail": "Unavailable" }
  ],
  "createdAt": "2026-09-11T10:00:00Z"
}
```

## Human Approval Gate

The Collection Agent **cannot** directly confirm a pickup. An authorized staff member must:
- **Approve** → backend confirms the booking
- **Reject** → workflow ends, decision recorded
- **Request Revision** → agent re-plans with updated constraints

## Audit Trail

The workflow persists:
1. Plan and constraint inputs
2. Steps and structured tool results
3. Backend validation results
4. Authorized human decision
5. Final assignment and outcome

All records stored in `agent_workflows`, `agent_steps`, `agent_tool_calls`, and `approvals` tables.

## External Service Failure Handling

If the routing service fails:
- Tool returns: "Travel estimate unavailable"
- Feasibility status: `MANUAL_REVIEW`
- The system must **never invent a travel estimate**
- External-service failures, invalid responses, and timeouts are handled safely

## Current Implementation Status

The current `CollectionAgentService` is a **placeholder orchestrator** that:
- Simulates the multi-step workflow with deterministic demo data
- Produces correctly structured proposals
- Returns `MANUAL_REVIEW` feasibility (no routing service connected)
- Supports the approval gate pattern

**Not yet implemented:**
- AI/LLM framework integration
- Real tool execution
- Persistent workflow state
- Actual constraint evaluation against live data
