# Waste-to-Value working rules

- Read `docs/ownership.md` and determine the requested module from the task and branch. If they disagree, clarify the intended scope while continuing independent inspection.
- Keep normal feature edits within that module's folders across the stack.
- Use the prewired route arrays and registration extensions. Module folders are ownership placeholders, not completed features.
- Do not reformat, rename, or refactor another member's files as part of unrelated work. Preserve existing changes.
- Coordinate necessary shared-file and cross-module changes through a separate reviewed integration change. The leader coordinates shared files after bootstrap.
- Never create a second DbContext, migration directory, or independent schema-management tool for a member module. Follow `docs/migration-policy.md`.
- Do not edit generated lockfiles or model snapshots manually. Use the corresponding package manager or EF tooling.
- Keep logs and member documentation in the member's own directory. Keep credentials and private data out of logs and commits.
- Follow the user's explicit task authorization; ownership guidance is not permission to overwrite others' work.
- Never use force pushes, `reset --hard`, automatic "ours/theirs" conflict resolution, or migrations against the shared database as routine feature work.
- Both clients communicate with ASP.NET. Neither client connects directly to Neon or a future AI service.
- Run relevant available checks and report actual results and blockers. Do not introduce placeholder tests or claim absent tests pass.
- Until explicitly requested later, keep business logic, schema/migrations, authentication, and agent implementations absent.
