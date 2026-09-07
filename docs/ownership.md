# Module ownership

| Member | Web module (including tests) | API module | API tests | Flutter feature | Flutter tests | Agent reservation | Personal documentation |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `frontend/src/modules/items/` | `backend/src/WasteToValue.Api/Modules/Items/` | `backend/tests/Items/` | `mobile/lib/features/items/` | `mobile/test/items/` | `agents/items/` | `docs/members/member-1/` |
| 2 | `frontend/src/modules/recovery/` | `backend/src/WasteToValue.Api/Modules/Recovery/` | `backend/tests/Recovery/` | `mobile/lib/features/recovery/` | `mobile/test/recovery/` | `agents/recovery/` | `docs/members/member-2/` |
| 3 | `frontend/src/modules/partners/` | `backend/src/WasteToValue.Api/Modules/Partners/` | `backend/tests/Partners/` | `mobile/lib/features/partners/` | `mobile/test/partners/` | `agents/partners/` | `docs/members/member-3/` |
| 4 | `frontend/src/modules/collections/` | `backend/src/WasteToValue.Api/Modules/Collections/` | `backend/tests/Collections/` | `mobile/lib/features/collections/` | `mobile/test/collections/` | `agents/collections/` | `docs/members/member-4/` |

After bootstrap, the team leader coordinates root documentation/configuration, `Program.cs`, `AppDbContext`, the one EF migrations directory and future snapshot, shared contracts, API infrastructure/health, core UI/network files, global styles, central routers, CI, native platform configuration, dependency manifests, and lockfiles. Shared test folders and `agents/shared/` are also integration-owned. The leader role is independent of the numbered member roles; assign it as a team.

All four React route arrays and Flutter route lists are already combined by the central routers. All four API registration extensions are already called in `Program.cs`. Add future routes/services through your module's extension points. Entity configurations will be found by `ApplyConfigurationsFromAssembly` in the single context. Do not edit shared files just to add a normal module route or service.

Keep feature CSS under its module. Coordinate necessary shared or cross-module changes through a separate reviewed integration change; preserve other members' changes. Ownership and folder separation reduce overlapping edits but cannot guarantee zero merge conflicts, especially in manifests and EF snapshots.

The inactive [CODEOWNERS example](../.github/CODEOWNERS.example) uses deliberately invalid, clearly marked placeholders. The leader must replace them with real GitHub handles, rename the file to `CODEOWNERS`, and configure available branch-protection/ruleset and review settings on GitHub. Those settings have not been configured. A CODEOWNERS file alone does not enforce merge restrictions.
