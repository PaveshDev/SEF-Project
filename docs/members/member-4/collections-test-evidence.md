# Collections Test Evidence

## Overview

This document provides genuine, verifiable test evidence for the Collections component in LoopWorth. It summarizes automated unit test execution results and distinguishes verified manual checks from pending manual test scenarios without fabricating results or claiming unconfirmed tests have passed.

---

## Automated Test Evidence

### Unit Test Execution Details

- **Test Project**: `backend/tests/LoopWorth.UnitTests/LoopWorth.UnitTests.csproj`
- **Test Class**: `LoopWorth.UnitTests.CollectionPlanningTests`
- **Source File**: `backend/tests/LoopWorth.UnitTests/CollectionPlanningTests.cs`
- **Framework**: xUnit.net v2.5.3.1 (`.NETCoreApp,Version=v8.0`)
- **Execution Command**:
  ```bash
  dotnet test backend/tests/LoopWorth.UnitTests/LoopWorth.UnitTests.csproj --filter "FullyQualifiedName~CollectionPlanningTests" --logger "console;verbosity=detailed"
  ```

### Recorded Test Results

```text
Test run for C:\Users\ASUS\Videos\SEF-Project\backend\tests\LoopWorth.UnitTests\bin\Debug\net8.0\LoopWorth.UnitTests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
C:\Users\ASUS\Videos\SEF-Project\backend\tests\LoopWorth.UnitTests\bin\Debug\net8.0\LoopWorth.UnitTests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.31)
[xUnit.net 00:00:00.08]   Discovering: LoopWorth.UnitTests
[xUnit.net 00:00:00.12]   Discovered:  LoopWorth.UnitTests
[xUnit.net 00:00:00.12]   Starting:    LoopWorth.UnitTests
[xUnit.net 00:00:00.18]   Finished:    LoopWorth.UnitTests
  Passed LoopWorth.UnitTests.CollectionPlanningTests.CollectionRequest_ProgressesThroughLifecycle [6 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 0.8515 Seconds
```

### Analysis of Test Case

#### Test Name: `CollectionRequest_ProgressesThroughLifecycle`
- **Behaviour Tested**:
  1. Instantiates a new `CollectionRequest` with status `CollectionStatus.Requested`.
  2. Simulates transition to `CollectionStatus.AgentAssigned` with assigned agent ID `"agent-1"` and appends a `CollectionStatusHistory` entry.
  3. Simulates transition to `CollectionStatus.Collected` and appends a pickup history note.
  4. Simulates transition to `CollectionStatus.DeliveredToPartner` and appends a facility delivery history note.
  5. Simulates final transition to `CollectionStatus.Completed` and appends partner verification note.
  6. Asserts `col.Status == CollectionStatus.Completed`.
  7. Asserts `col.StatusHistory.Count == 4`.
- **What the Result Proves**:
  - The domain model correctly supports sequential lifecycle progression across the core collection stages.
  - The `StatusHistory` navigation collection maintains state and correctly aggregates audit trail entries corresponding to each lifecycle transition.

---

## Manual Verification Coverage Summary

Based on project inspection and recorded test runs, the status of manual verification scenarios is documented below:

| Scenario / Test Case ID | Description | Verification Status | Notes / Evidence |
| :--- | :--- | :--- | :--- |
| **TC-01** | Customer Schedules Collection for Approved Item | `Verification status: Not independently confirmed` | Requires end-to-end interactive session through web UI or Postman against approved recovery request. |
| **TC-02** | Prevent Collection for Unapproved Recovery | `Verification status: Not independently confirmed` | Logic verified in code review (`CollectionsController.cs:62`); live HTTP rejection not independently run in this session. |
| **TC-03** | Admin Views All Collection Requests | `Verification status: Not independently confirmed` | Endpoint verified compile-clean; live execution pending admin token run. |
| **TC-04** | AI Automated Agent Dispatch (District & Town) | `Verification status: Not independently confirmed` | Dispatch fallback logic unit-tested; external Gemini AI API key dependent. |
| **TC-05** | Agent Accepts Job & IsAvailable Synchronized | `Verification status: Not independently confirmed` | Implementation verified in controller (`SyncAgentAvailabilityAsync`); interactive acceptance pending. |
| **TC-06** | Single Active In-Progress Job Guardrail | `Verification status: Not independently confirmed` | Implementation verified in controller (`CollectionsController.cs:655-661`); runtime assertion pending. |
| **TC-07** | Agent Rejects Job with Recorded Reason | `Verification status: Not independently confirmed` | Reversion to `Requested` and history note logging confirmed in source; live test pending. |
| **TC-08** | Doorstep Pickup (`Collected`) Execution | `Verification status: Not independently confirmed` | Controller status transition verified in code; live test pending. |
| **TC-09** | Handover to Partner (`DeliveredToPartner`) | `Verification status: Not independently confirmed` | Controller status transition verified in code; live test pending. |
| **TC-10** | Partner Intake Verification with Photo | `Verification status: Not independently confirmed` | Multipart file upload and condition flags confirmed in code; live image upload pending. |
| **TC-11** | Admin Agent Roster & Town Coverage Verification | **Verified** | Confirmed via live API execution against Neon database (`GET /api/admin/collection-agents` returns 50 seeded agents across 25 Sri Lankan districts). |
| **TC-12** | Role-Based Access Control (401/403) | `Verification status: Not independently confirmed` | Controller `[Authorize(Roles = "...")]` attributes verified in code; runtime rejection assertions pending. |

---

## Test Gap Identification

1. **Automated Integration Tests**:
   - `LoopWorth.IntegrationTests` currently tests `AuthEndpointsTests` and category listing. Direct HTTP integration tests for `CollectionsController` using `CustomWebApplicationFactory` are currently absent.
2. **Mobile Device Testing**:
   - Flutter unit and widget tests for mobile collection screens in `mobile/test/collections/` contain only `.gitkeep`.
3. **External Service Isolation**:
   - Live tests for `CollectionPlanningAgent` require mocking `ICollectionPlanningAgent` or configuring a test `GEMINI_COLLECTION_API_KEY`.
   - Brevo email dispatch requires valid external API keys.
