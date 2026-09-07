# Recovery

Member 2 owns this module and `backend/tests/Recovery/`. Recovery entities, validation, valuation/planning/decision services, controllers, and Recovery-owned integration/agent contracts are implemented. `AddRecoveryModule` is already wired in Program.cs; it registers the module and explicit unavailable defaults for missing integrations.

Real identity, durable idempotency/transaction coordination, upstream adapters, and integrated database schema are still required. Recovery endpoints fail closed until those dependencies exist. No migration, foreign-module entity, AI runtime, or client UI is implemented by this change.

See [implementation and API details](../../../../../docs/members/member-2/recovery-design.md), [schema-change request](../../../../../docs/members/member-2/schema-change-request.md), [ownership](../../../../../docs/ownership.md), and [migration policy](../../../../../docs/migration-policy.md).
