# Recovery schema-change request

Owner: Member 2. Status: implementation supplied for integrator review; no migration generated or applied.

Affected owned tables: recovery_cases, value_references, recovery_options, recovery_proposals, proposal_decisions. Starting reference: database/WasteToValue-Initial-Schema.sql. This document does not authorize running that SQL or changing shared Neon.

## Mapping differences

| Table | Changes relative to the SQL reference |
| --- | --- |
| recovery_cases | Add positive revision, item_revision, assessment_version. Retain active-item partial unique index and owner/status index. |
| value_references | Keep reference fields and monetary precision; add explicit valid condition checks. Observation timestamps normalize to UTC. |
| recovery_options | Add case_revision, assessment_version, requires_partner, requires_pickup, estimated_shortfall, integration_snapshot jsonb. Preserve unknown amounts as nullable with no automatic zero defaults. Enforce complete estimates for VALIDATED/SELECTED, nonnegative amounts, value bounds, exact conservative net/shortfall formulas, and pickup implies partner. |
| recovery_proposals | Add case_revision, option_version, match_version/token, pickup_plan_version/token, input_snapshot jsonb, estimate_snapshot jsonb, recommendation_origin, optional agent_run_id. Match/pickup identifiers become nullable for routes that do not need them, with paired identifier/version/token constraints. Retain unique case/revision and add alternate key (id, revision). |
| proposal_decisions | Composite FK (recovery_proposal_id, proposal_revision) references exact proposal revision. Keep unique proposal/revision/actor/type. Scope idempotency key uniqueness by decided_by instead of globally. No update/delete application operation. |

Monetary columns use numeric(12,2); currency is uppercase character(3). Enum values are explicitly persisted in the SQL reference's uppercase naming convention. Mutable version columns are EF concurrency tokens and increment through domain operations. IDs use PostgreSQL gen_random_uuid defaults; the repository obtains generated IDs before downstream requests.

Own-entity foreign keys use restrictive deletes to retain history: options to cases, proposals to cases/options, and decisions to proposal revisions. This deliberately tightens the reference's cascade behavior for case/options. Planning revisions are snapshots, not foreign keys to a mutable case revision or option version.

## Cross-module work still required

No Item, Assessment, Partner, Match, CollectionSlot, PickupPlan, User, or Category EF entity was created. Their external FK mappings cannot be configured correctly until their actual mapped types and shared user-key strategy exist. Current Recovery configurations therefore contain scalar identifiers for those references, not fabricated shadow entities.

The integrator must add the agreed external relationships in a separate integration change. Verify the full case -> option -> match -> pickup chain; individual foreign keys alone do not enforce that chain. Define provider freshness tokens over all relevant eligibility/capacity inputs and use consistent transaction/isolation semantics at approval.

Persisted general idempotency receipts are a separate shared-infrastructure dependency. No receipt entity or second schema-management mechanism was created in Recovery. Decision idempotency alone is insufficient for case creation, planning, submission, cancellation, and reference commands.

Clarify the authoritative role of proposal_decisions versus shared agent approvals. An agent approval record must never independently authorize a Recovery proposal.

## Backfill and compatibility

Live database deployment state is unknown. Do not apply these mappings directly to an assumed-empty Neon database. Establish the actual schema and migration history first.

Do not invent historical assessment/option versions, evidence, or cost values. Existing proposals without reconstructable snapshots must be marked stale and regenerated through an approved backfill. Review whether old zeros meant known zero or unknown before converting draft amounts. Resolve duplicate active cases and scoped receipt-key conflicts deliberately.

Summary DTOs are newly defined Recovery-owned contracts, not implementations or existing promises from Members 1, 3, or 4. Their adapters, authentication strategy, and durable executor require reviewed integration. Repair quotation sourcing, value-reference age policy, self-delivery variants, and verified fulfillment integration remain unresolved.

## Integrator verification

Test fresh schema creation and upgrade from the last committed migration on an isolated PostgreSQL database/Neon development branch. Cover active-case races, exact-revision decisions, duplicate operations, stale/expired approval, negative/cross-currency values, unknown costs, relational chain integrity, transactional rollback, and retry behavior.

Generate one ordered EF migration with the corresponding reviewed model changes and snapshot. Never edit a generated snapshot manually, rewrite an applied migration, create duplicate foreign-module tables, or execute the SQL reference and equivalent EF initial migration on the same database.
