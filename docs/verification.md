# Local verification

Verified on Windows on 2026-09-07 using the versions in [dependency policy](dependency-policy.md).

| Check | Result |
| --- | --- |
| Official Vite, dotnet, and Flutter scaffold generation | Completed in the existing workspace root |
| npm install and final `npm ci` | Passed; genuine package-lock.json, audit reported zero vulnerabilities |
| `npm run lint` | Passed with Oxlint |
| `npm run build` | Passed with Vite 8.2.2 |
| NuGet restore with `--locked-mode` | Passed; genuine packages.lock.json |
| Repository-local dotnet-ef 8.0.30 install | Passed; generated tool manifest |
| API Release build | Passed, zero warnings and errors |
| Running `/health` | HTTP 200, `status: ok`, `database: not_checked` |
| Development Swagger document | Loaded successfully; only `/health` is listed |
| Development CORS preflight | Both documented localhost origins allowed; unapproved origin received no allow-origin header |
| Missing database configuration | EF context inspection with no connection string and Production configuration failed with the explicit missing-configuration message, before any connection |
| React browser verification with Playwright MCP | Correct home text at widths 375, 768, 1440; no horizontal overflow; no page errors or failed requests in the final check |
| Shared Axios client | Manual browser invocation received the health JSON through CORS; no API call was added to the home page |
| Flutter pub restore with `--enforce-lockfile` | Passed; genuine pubspec.lock |
| `flutter analyze --no-pub` | Passed, no issues found |
| Dart formatting check | Passed, 9 files and no changes |
| Flutter debug web run | Compiled and served successfully on localhost:5174 |
| Flutter release web build | Passed, with the font warning described below |
| Flutter browser verification with Playwright MCP | Correct accessible text at widths 375, 768, 1440; no horizontal overflow or browser errors; loaded network requests succeeded |
| Scaffold/documentation inventory | All 86 explicitly listed request paths exist; relative Markdown links resolve |
| CI YAML validation | Parsed successfully; main push/PR triggers, three jobs, and read-only permissions verified |

The two home screens contain only static skeleton text, so there are no forms, actions, loading states, or business interactions to exercise. Tab navigation was checked; there are no application controls or focus traps. Flutter semantics were enabled for accessibility inspection, and its rendered desktop screenshot was visually inspected.

Flutter 3.24.5's web-server debug run emits a developer-extension/DWDS warning. Its release web build emits a missing CupertinoIcons font-family warning after the unused generated Cupertino icon dependency was removed. The scaffold does not render icons; analysis, compilation, and the displayed text passed. No unrelated package was added to silence these SDK messages.

Android native build/device execution was not run: the installed Java 24.0.2 is outside the generated Gradle 8.3 compatibility range. Select JDK 17 and verify the Android toolchain using the [setup commands](setup.md) before building. iOS build/device execution is unavailable on Windows and requires macOS/Xcode. Native HTTPS connectivity and certificate trust remain unconfigured and untested.

No business test suites exist. No frontend test/type-check script, .NET test project, or Flutter tests were reported as passing. GitHub-hosted CI has not run because no remote or push was performed. No database was contacted, and database connectivity, schemas, migrations, and agent implementations were not tested or created.

The local Git repository is on `main`, with no commits, staging, or remote. Sandbox Git inspection required a command-scoped `safe.directory` setting because the sandbox account differs from the workspace owner; no global Git configuration or identity was changed.
