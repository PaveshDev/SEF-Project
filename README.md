# Waste-to-Value

Local starter repository for a four-member university project. The React and Flutter apps display **Waste-to-Value — project skeleton**. The ASP.NET controller endpoint `GET /health` reports process health with `database: "not_checked"` and works without database credentials.

```text
frontend/                  React + Vite (JavaScript/JSX), React Router, Axios
  src/app/                 Home page and prewired router
  src/shared/              Shared UI reservations, styles, HTTP client
  src/modules/             items, recovery, partners, collections
backend/
  WasteToValue.sln          One ASP.NET Core controller API project
  .config/                 Local EF tool manifest
  src/WasteToValue.Api/
    Controllers/           Health only
    Shared/                Shared extension-point reservations
    Infrastructure/Persistence/  Empty AppDbContext, Migrations/, Seeds/
    Modules/               Items, Recovery, Partners, Collections
  tests/                   Four member folders and Shared (empty)
mobile/                    Flutter scaffold: Android, iOS, web
  lib/app/                 Home screen and prewired router
  lib/core/                Configuration, Dio, theme reservation
  lib/shared/              Widget reservation
  lib/features/            items, recovery, partners, collections
  test/                    Four member folders and shared (empty)
agents/                    Four ownership reservations and empty shared folders
database/                  Documentation only
docs/                      Setup, collaboration, ownership, dependency/migration policies
  members/member-{1..4}/   Personal documentation and empty AI log templates
.github/                   CI, PR template, inactive CODEOWNERS example
```

Otherwise-empty folders contain `.gitkeep`. Both future clients call ASP.NET; neither connects directly to Neon or a future AI service. Neon will be configured later through backend environment variables or .NET user secrets. No database connection is needed to run the skeleton.

Start with [setup and verification](docs/setup.md), [exact versions and dependency policy](docs/dependency-policy.md), [ownership](docs/ownership.md), [team Git workflow](docs/team-workflow.md), and [migration policy](docs/migration-policy.md). Member 1 owns items, Member 2 recovery, Member 3 partners, and Member 4 collections across the stack. The leader coordinates shared changes. Folder ownership reduces overlap; it does not guarantee conflict-free merges.

There are no business components, CRUD endpoints, authentication flows, database entities/tables/migrations, sample business data, dashboards, matching/valuation/collection workflows, or agent implementations. No agent framework or project license has been selected. Empty test directories reserve future tests; no placeholder test suite is claimed to pass.
