# Database documentation only

Neon PostgreSQL is the planned shared integration database. It is not configured, contacted, or changed by this scaffold. Members may each use a separate local PostgreSQL database later.

The only future migration directory is `backend/src/WasteToValue.Api/Infrastructure/Persistence/Migrations/`. This directory contains only `.gitkeep`; there are no entities, tables, migrations, or model snapshots yet. Do not add SQL schema scripts or another migration system under `database/`.

Agree on the first entities and API contracts, then have the integrator generate and review the initial EF migration before integrated CRUD development. See [migration policy](../docs/migration-policy.md) and [configuration](../docs/setup.md).
