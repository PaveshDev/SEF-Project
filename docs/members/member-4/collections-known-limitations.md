# Collections Known Limitations

## Overview

This document identifies genuine current limitations in the Collections subsystem of LoopWorth. In accordance with project integrity standards, deliberate security controls (such as the single active job constraint and re-assignment locks) are distinguished from actual implementation gaps or technical boundaries.

---

## Status Classification of Subsystem Capabilities

| Capability Area | Current Status | Description |
| :--- | :--- | :--- |
| **ASP.NET Core Collections API** | **Implemented** | Complete REST endpoints for customer scheduling, admin dispatch, agent actions, and partner intake verification. |
| **Agent Availability Sync & Concurrency Locks** | **Implemented** | Automated synchronization of `IsAvailable` flag based on active jobs in `Scheduled` or `Collected` state. |
| **Audit Logging (`CollectionStatusHistories`)** | **Implemented** | Append-only audit trail logging timestamps, notes, and user identifiers for all status transitions. |
| **AI Logistics Dispatch & Fallback Heuristics** | **Implemented** | `CollectionPlanningAgent` integrates with Gemini API, accompanied by a deterministic fallback covering district and town vicinity. |
| **React Web Collections Module** | **Implemented** | Customer scheduling, admin dispatch dashboards, agent job queues, and roster management pages. |
| **Mobile Flutter Collection Screens** | **Partially Implemented** | Screen widgets and models exist under `mobile/lib/features/collections/`; however, live camera hardware QR scanning on physical devices is not independently tested. |
| **Automated Test Coverage** | **Partially Implemented** | Unit tests exist for domain lifecycle transitions (`CollectionPlanningTests.cs`); integration HTTP tests and mobile widget tests are currently absent. |
| **Real-Time GPS Vehicle Telematics** | **Not Implemented** | The platform tracks discrete milestone statuses (`Requested`, `Scheduled`, `Collected`, `DeliveredToPartner`, `Completed`), not continuous GPS telemetry. |
| **Autonomous AI Background Dispatch** | **Planned** | AI dispatch is triggered on-demand by administrators or customers rather than running as a scheduled autonomous background cron daemon. |

---

## Current Limitations

### 1. Human-in-the-Loop Admin Dispatch Dependency
- **Details**: Although `CollectionPlanningAgent` computes optimal route windows and candidate agents, dispatching is primarily triggered via `POST /api/admin/collections/{id}/assign-agent` or customer preference submission. The system does not run an autonomous background worker daemon that continuously scans for unassigned requests and automatically auto-commits assignments without human supervision.
- **Impact**: In a high-volume production scenario, pending requests in `Requested` status could experience latency if administrative personnel are delayed in triggering batch dispatching.
- **Status**: Partially Implemented (On-demand dispatch implemented; autonomous background polling planned).

---

### 2. Mobile Hardware Camera & Scanner Verification
- **Details**: While mobile code contains `qr_scanner_screen.dart` and `handover_pass_screen.dart`, live execution and hardware permission handling (e.g., iOS camera permission dialogs, Android camera runtime permissions, barcode lighting conditions) have only been verified in standard emulator environments without physical hardware testing.
- **Impact**: Edge cases such as low-light barcode capture or camera permission denial on physical devices must be handled gracefully during field operations.
- **Status**: Partially Implemented.

---

### 3. Missing Mobile Unit and Widget Test Suite
- **Details**: The directory `mobile/test/collections/` contains only `.gitkeep`. There are no automated widget tests verifying Flutter UI rendering, form validation, or Dio API client responses for collection screens.
- **Impact**: Regressions in mobile UI state or model serialization could pass CI without detection until manual smoke testing.
- **Status**: Planned (Placeholder directory created).

---

### 4. Integration Test Suite for Collections Endpoints
- **Details**: Automated tests in `LoopWorth.UnitTests` verify domain model state transitions (`CollectionPlanningTests.cs`), but `LoopWorth.IntegrationTests` currently lacks an end-to-end HTTP integration test class for `CollectionsController` (e.g. testing database transactions, multipart upload handling, and role authorization headers with `CustomWebApplicationFactory`).
- **Impact**: Changes to database configuration or middleware pipeline must be validated manually or via post-deployment smoke tests.
- **Status**: Planned.

---

### 5. External API Key Dependencies in Local Development
- **Details**: Full AI dispatch through Google Gemini and automated delivery confirmation emails through Brevo rely on external service API keys (`GEMINI_COLLECTION_API_KEY` and Brevo SMTP settings).
- **Impact**: When running in offline or sandbox environments without valid API credentials, the system gracefully falls back to deterministic rule-based agent matching and logs email dispatch warnings.
- **Status**: Implemented with graceful fallback.

---

## Possible Future Improvements

1. **Autonomous Background Worker**:
   - Implement an ASP.NET Core `IHostedService` or Quartz.NET background job that periodically sweeps `Requested` collection orders and automatically initiates AI dispatch based on configurable business rules.

2. **Expanded Automated Test Coverage**:
   - Add integration tests under `backend/tests/LoopWorth.IntegrationTests/CollectionsEndpointTests.cs` using in-memory or test database containers.
   - Implement Flutter widget tests under `mobile/test/collections/` mocking the Dio API service.

3. **Geospatial Proximity Calculation**:
   - Integrate geographic coordinate calculation (latitude/longitude distances) to augment the current district and town string-matching algorithm.

4. **Offline Mobile Synchronization**:
   - Provide local SQLite / Hive caching in the Flutter app so collection agents operating in areas with intermittent cellular connectivity can record pickups locally and synchronize upon reconnection.
