# Recovery Planner Agent

Member 2 owns this agent boundary. No agent framework is installed or approved in the repository.
`RecoveryPlannerAgent.cs` therefore provides framework-neutral, test-only orchestration contracts:
typed input/output and workflow-state models, an explicit allow-list, deterministic input validation,
timeout/retry handling, structured failures, approval pause state, and auditable execution summaries.

The orchestration accepts only typed tool results. It cannot inspect images, invent prices, create or
verify partners, invent acceptance or pickup availability, reserve collection slots, approve its own
recommendation, call arbitrary tools, or persist hidden chain-of-thought. Deterministic valuation is
delegated to `ValueEstimationService`; matching and pickup use the Recovery-owned gateway contracts.

The source is compiled only into the Member 2 Recovery test project for Phase 4B verification. A future
runtime integration must provide a reviewed agent framework or host, durable workflow-state persistence,
real tool implementations, transaction/idempotency coordination, and an explicit registration boundary.
No production adapter, endpoint, AI dependency, migration, or shared-file change is included here.
