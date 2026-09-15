# Database and integration readiness check

Date: 2026-09-09  
Branch: `feature/member-2-recovery`

## Result

The supplied Neon connection authenticated successfully using the backend's installed Npgsql library with `SSL Mode=VerifyFull`. PostgreSQL reported version 18.6. Metadata queries ran in a transaction with `transaction_read_only=on`, followed by rollback. No application records were read, and no schema, seed data, or application data was changed. Credentials were supplied through hidden input to a temporary diagnostic process outside the repository; they were not saved in source, application configuration, or this report.

The first diagnostic connection using startup `Options` returned PostgreSQL protocol error `08P01`. Retrying without those startup options succeeded; read-only mode and the statement timeout were set inside the transaction before metadata queries. The precise cause of the initial error was not independently verified.

The connected database contains no non-system tables. All 30 tables in `database/WasteToValue-Initial-Schema.sql` are absent, and no `__EFMigrationsHistory` table exists. This result applies to the supplied database endpoint, not every database or branch in the team's Neon project.

## Current code versus SQL reference

The current `AppDbContext` discovers five Recovery entity mappings through `ApplyConfigurationsFromAssembly`. The other three backend module registration extensions are empty. The migration directory contains only `.gitkeep`.

The SQL reference lacks these 19 columns required by the compiled Recovery EF model:

| Table | Missing columns |
| --- | --- |
| `recovery_cases` | `assessment_version`, `item_revision`, `revision` |
| `recovery_options` | `assessment_version`, `case_revision`, `estimated_shortfall`, `integration_snapshot`, `requires_partner`, `requires_pickup` |
| `recovery_proposals` | `agent_run_id`, `case_revision`, `estimate_snapshot`, `input_snapshot`, `match_freshness_token`, `match_version`, `option_version`, `pickup_freshness_token`, `pickup_plan_version`, `recommendation_origin` |

Additional differences require review: the SQL requires proposal match/pickup IDs whereas the model permits null; option monetary amounts differ in nullability; the model uses a composite proposal/revision foreign key for decisions; decision idempotency uniqueness differs; and option-to-case delete behavior differs. The column comparison is not a complete schema equivalence check. No SQL execution or database constraint/write tests were performed.

## Actual verification

- `dotnet test backend/tests/Recovery/WasteToValue.Recovery.Tests/WasteToValue.Recovery.Tests.csproj --no-restore --verbosity minimal`: 163 passed, zero failed, zero skipped. Compilation emitted one nullable warning and three xUnit blocking-operation warnings.
- These tests include test doubles and model metadata checks; they do not prove live PostgreSQL persistence or integrated operation across all four modules.
- Live `GET /api/recovery-cases`: HTTP 503 with `identity_unavailable`.
- Recovery production registration still supplies unavailable implementations for verified identity, durable command/idempotency execution, assessment, matching, and pickup planning.
- The older September 8 audit is partly outdated: React now imports `updateCase`, and `RecoveryWorkflow.jsx` calls `submitProposal`. This inspection does not establish successful end-to-end UI behavior.

## Migration decision and sequence

Do not apply the SQL reference unchanged or generate the shared initial migration from this Recovery-only model. The database plan names EF migrations as the source of truth and requires agreeing on the user/Identity key strategy before initialization.

1. The team integrator combines the actual shared and member entity mappings on an integration branch and resolves differences with the SQL specification, including authentication IDs and durable operation storage.
2. Generate the initial migration with the existing repository-local EF tool, existing `AppDbContext`, and existing migration directory. Review its generated SQL, constraints, and model snapshot.
3. Apply and test it on an isolated local PostgreSQL database or separate Neon development branch, including persistence, concurrency, foreign-key enforcement, and rollback behavior.
4. Integrate verified identity and the real assessment, matching, pickup, and durable command adapters; add development fixtures only to the isolated environment.
5. Test the full flow: authenticated item submission, confirmed assessment, recovery planning, option selection, partner matching/pickup feasibility, proposal creation, human approval, and pickup completion. Also test rejection, stale revisions, duplicate commands, and service failures.
6. After review and isolated validation, the nominated integrator applies the migration to shared Neon using the designated direct administrative connection and records the result. Do not run both the reference SQL and an equivalent initial EF migration against that database.

Database connectivity is verified; shared initialization and full integrated workflow testing remain incomplete. No backend credential configuration was persisted or server restarted as part of this check.

References: [database plan](../../../database/WasteToValue-Database-Plan.md), [migration policy](../../migration-policy.md), [Npgsql TLS documentation](https://www.npgsql.org/doc/security.html), [EF migration overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/).
