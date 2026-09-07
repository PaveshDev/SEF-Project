# One coordinated EF migration history

There is one `AppDbContext`, currently empty, and one future migration directory:

`backend/src/WasteToValue.Api/Infrastructure/Persistence/Migrations/`

There are no `DbSet` properties, entity classes, seed execution, migrations, or snapshots. `ApplyConfigurationsFromAssembly` provides the extension point for future module mappings. The application does not call `EnsureCreated`, migrate at startup, or connect to a database from its health check.

Before integrated CRUD development, the team must agree on the first entities and contracts. The integrator then generates the initial migration with the repository-local EF tool, reviews it, and tests it against an isolated database. Do not generate the initial migration merely to populate this skeleton.

For later model changes:

1. Coordinate the model changes with the leader. Serialize migration generation from the latest `main`, including any preceding integration changes.
2. Use only the existing context, EF tool, and migration directory. Generate migrations and snapshots with EF; never edit snapshots manually.
3. Commit the migration and generated snapshot with their corresponding model/configuration changes. Review SQL effects and test upgrade behavior against an isolated database before merging.
4. Each member may use a separate local PostgreSQL database. Only the coordinated integration process updates shared Neon after review. Its credentials remain in private configuration.
5. If branches overlap, reconcile models deliberately and regenerate only unapplied local migrations when appropriate. Separate module folders do not eliminate snapshot conflicts.

Never delete or rewrite a migration already applied to the shared database; prefer a corrective migration. Do not create tables manually in the Neon console, add independent schema tools, or run shared-database updates as routine feature work. `database/` is documentation only.
