# Local setup

Use separate terminals for each application. Commands below use ordinary tool names; if your local coding-agent policy requires RTK, prefix shell commands with `rtk` (or `rtk proxy` for commands it does not filter). On Windows, use `npm.cmd` if PowerShell blocks `npm.ps1`.

## Tools

Install Node **24.18.0** with npm **11.12.1**, .NET SDK **8.0.424**, and Flutter stable **3.24.5** (bundled Dart **3.5.4**). The baseline intentionally matches the SDKs found on the scaffold machine. `global.json`, `.nvmrc`, the frontend manifest, and CI record these choices. See [exact dependencies](dependency-policy.md) for coordinated upgrade guidance.

Use the official [Node downloads](https://nodejs.org/en/download), [.NET 8 downloads](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), and [Flutter SDK archive](https://docs.flutter.dev/install/archive). Put the selected versions on PATH. Verify:

```sh
node --version
npm --version
dotnet --version
flutter --version
flutter doctor -v
```

For Android, install Android Studio, Android SDK/emulator tools, and **JDK 17**. Flutter generated Gradle **8.3**, Android Gradle Plugin **8.1.0**, and Kotlin **1.8.22**. The scaffold machine has Java **24.0.2**, which Flutter warned may conflict with Gradle 8.3. Before Android builds, select JDK 17 in your local Flutter/Android tool configuration and verify `flutter doctor -v`. If needed, `flutter config --jdk-dir=<LOCAL_JDK_17_DIRECTORY>` changes your Flutter SDK configuration; this task has not run it. See [Gradle Java compatibility](https://docs.gradle.org/current/userguide/compatibility.html#java). iOS requires macOS and Xcode; it cannot be built on this Windows machine. The generated `com.example` package identity must be replaced in a coordinated change before release.

## Web

From the repository root:

```sh
cd frontend
npm ci
```

Copy `.env.example` to `.env.local` (`Copy-Item .env.example .env.local` in PowerShell, or `cp .env.example .env.local` in a POSIX shell). It contains:

```dotenv
VITE_API_BASE_URL=http://localhost:5080
```

```sh
npm run dev
```

Open **http://localhost:5173**. The port is fixed to match API development CORS; Vite fails clearly if it is occupied. The shared Axios client reads `VITE_API_BASE_URL` and defaults to the same local API address. Restart Vite after configuration changes. All `VITE_` values are public browser bundle configuration; never put secrets there.

## ASP.NET API

From the repository root:

```sh
cd backend
dotnet tool restore
dotnet restore WasteToValue.sln --locked-mode
dotnet run --project src/WasteToValue.Api --launch-profile http
```

Open **http://localhost:5080/health**. Expected JSON: `{"status":"ok","database":"not_checked"}`. The controller confirms the API process responds; it does not check PostgreSQL. Development Swagger is at **http://localhost:5080/swagger**, with the document at `/swagger/v1/swagger.json`. The only API route is `/health`.

No database credentials are required for startup or the home pages. Future database operations resolve the context lazily; if `ConnectionStrings:DefaultConnection` is absent, resolution throws an explicit missing-configuration error. No connection, `EnsureCreated`, seed execution, or startup migration is performed by the scaffold.

For future database work, use one of these actual configuration mechanisms in private local setup:

```powershell
# PowerShell: applies only to the current terminal and its child processes.
$env:ConnectionStrings__DefaultConnection = 'Host=<NEON_HOST>;Database=<DATABASE>;Username=<USER>;Password=<PASSWORD>;SSL Mode=VerifyFull'
dotnet run --project src/WasteToValue.Api --launch-profile http
```

Or use .NET user secrets from `backend/`:

```sh
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<NEON_HOST>;Database=<DATABASE>;Username=<USER>;Password=<PASSWORD>;SSL Mode=VerifyFull" --project src/WasteToValue.Api
```

Replace placeholders privately, never in tracked files or logs. These are Npgsql key/value connection strings, not a `postgresql://` URL. User secrets are local development storage, not production secret management. ASP.NET **does not automatically load `.env` files**; `backend/.env.example` is reference material only. The double underscore environment key maps to the colon-delimited .NET configuration key. For isolated local PostgreSQL later, use your own local host/database credentials. Read [migration policy](migration-policy.md) before any schema work; shared Neon is updated only by the coordinated integration process.

Development CORS permits only **http://localhost:5173** (React) and **http://localhost:5174** (Flutter web). `127.0.0.1`, other ports, and other origins are different origins. No production CORS policy is enabled. Coordinate future deployed origins through an integration change.

## Flutter

From the repository root:

```sh
cd mobile
flutter pub get --enforce-lockfile
flutter run -d chrome --web-hostname localhost --web-port 5174 --dart-define=API_BASE_URL=http://localhost:5080
```

If Chrome is unavailable, use `-d web-server` with the same flags and open **http://localhost:5174** manually. The home screen makes no API requests. `lib/core/network/api_client.dart` exposes the Dio client configured by the public, compile-time `API_BASE_URL`. Rebuild/restart after changing `--dart-define`; it is not a secret store.

The chosen initial web setup is loopback HTTP: clients at ports 5173/5174 and API at 5080. For local desktop-browser HTTPS, explicitly trust the ASP.NET development certificate on your machine, then select the HTTPS launch profile:

```sh
dotnet dev-certs https --trust
dotnet run --project backend/src/WasteToValue.Api --launch-profile https
```

The HTTPS API address is **https://localhost:7080**. Configure the clients to use that address if selected. This task has not installed or trusted certificates. Do not disable certificate validation.

Native device networking differs:

| Client | Meaning of localhost | Development API addressing |
| --- | --- | --- |
| Browser on the API machine | The API machine | `http://localhost:5080` for the initial web setup, or trusted `https://localhost:7080` |
| Android emulator | The emulator itself | `10.0.2.2` reaches the host, but a localhost TLS certificate is not valid for that address |
| iOS simulator | Usually the Mac host | Run/forward the API on that Mac and use a certificate the simulator trusts |
| Physical device | The device itself | Use a reachable host/LAN address or development hostname, not localhost |

For future Android/physical-device API calls, use a team-provided HTTPS development endpoint with a certificate valid for its hostname and trusted by the device. A trusted HTTPS forwarding endpoint to the local API is one option; no tunnel/service is provisioned by this scaffold. Alternatively configure a development hostname, matching certificate, device trust, and API binding deliberately. Pass the resulting address with:

```sh
flutter devices
flutter run -d <DEVICE_ID> --dart-define=API_BASE_URL=https://<TRUSTED_DEVELOPMENT_API_HOST>
```

Do not simply replace localhost with an IP in an HTTPS URL: certificate hostnames must match. No certificate bypass, Android cleartext override, iOS ATS exception, or weakened release network setting is included. Android has the standard Internet permission so future Dio calls can work. Release clients must use the real HTTPS ASP.NET endpoint. Neither client connects directly to Neon or an agent service.

## Checks

From `frontend/`: `npm ci`, `npm run lint`, `npm run build`.

From the repository root: `dotnet restore backend/WasteToValue.sln --locked-mode` and `dotnet build backend/WasteToValue.sln --configuration Release --no-restore`.

From `mobile/`: `flutter pub get --enforce-lockfile`, `flutter analyze --no-pub`, and optionally `flutter build web --no-pub`.

There is no separate JavaScript type-check script (this is JS/JSX), frontend test script, .NET test project, or Flutter test suite. Member test folders are empty reservations; add meaningful tests and CI commands with future implementations. CI restores/builds the API, installs/lints/builds React, and analyzes Flutter; it needs no database or AI credentials. GitHub execution is pending the leader's reviewed push.

Local verification results are recorded in [verification](verification.md). If restore fails on your machine, check access to the npm registry, NuGet, pub.dev, and Flutter artifact servers, then rerun the commands above. Do not fabricate or hand-edit missing lockfiles.
