# Partner Matching Agent Guide (`PartnerMatchingAgent`)

## Agent Purpose
The `PartnerMatchingAgent` is an intelligent orchestration agent within the LoopWorth infrastructure. Its primary responsibility is analyzing recovered items, geographic locations, partner specializations, capacities, and compliance certifications to generate optimal partner recommendations.

## Technical Architecture
- **Location**: `backend/src/LoopWorth.Infrastructure/Agents/PartnerMatchingAgent.cs`
- **Interface**: `IPartnerMatchingAgent`
- **Agent Reservation**: `agents/partners/`

## Inputs & Execution Context
```json
{
  "itemId": "guid",
  "categoryName": "Laptops & Computers",
  "recoveryRoute": "Recycle",
  "hazardFlag": false,
  "pickupPostalCode": "10001",
  "preferredRadiusKm": 25
}
```

## Matching Algorithm & Scoring Weights
1. **Category & Route Fit (40%)**:
   - Verification that the partner holds certified recycling/refurbishing permits for the exact device category.
2. **Geographic Proximity (25%)**:
   - Distance calculation between the customer's pickup address and partner facility.
3. **Capacity & Availability (20%)**:
   - Current utilization percentage against daily quota.
4. **Reliability & Rating (15%)**:
   - Weighted average of past recovery completion ratings and SLA adherence.

## Output Structure
```json
{
  "partnerId": "guid",
  "partnerName": "GreenTech E-Waste Processors",
  "matchScore": 94.5,
  "confidence": "High",
  "rationale": "High capacity for computer hardware with 98% on-time processing SLA within 8 km.",
  "estimatedProcessingDays": 3
}
```

## Error Handling & Fallbacks
- If no partners match within the requested geographic radius, the agent expands the radius incrementally up to regional hubs.
- If all partners are at capacity, the request is flagged for administrator queue review.
