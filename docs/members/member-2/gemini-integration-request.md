# Recovery Gemini integration boundary

The provider implementation and its Recovery-owned dependency injection are complete. Production agent hosting remains an integration dependency. No shared change below was implemented.

## Configuration

Configuration names only:

- Gemini__ApiKey
- Gemini__Model
- Gemini__TimeoutSeconds
- Gemini__MaxRetries
- Gemini__Enabled

The normal .NET IConfiguration hierarchy reads the corresponding colon-separated names. The standard environment provider maps double underscores to that hierarchy; local user secrets can use the same hierarchy. No appsettings, environment file, model selection, deployed configuration, or real credential was added. The provider fails closed when configuration is disabled, missing, malformed, or outside its bounded timeout/retry policy.

## Exact shared integration requested

The API project currently does not compile agents/recovery. The genuine Recovery test project already links that source. A reviewed integration change must first include it in the production compilation, either through the team's chosen agent assembly or this API project item:

```xml
<ItemGroup>
  <Compile Include="../../../agents/recovery/**/*.cs"
           Link="Agents/Recovery/%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```

This is a requested project-manifest change, not an edit made by this implementation. No Program.cs change is necessary for provider registration: its existing AddRecoveryModule call already registers IRecoveryReasoningProvider.

After the production compilation decision, coordinate the runtime composition:

- Register RecoveryPlannerAgent and RecoveryPlannerToolset through the Recovery extension. Supply the registered IRecoveryReasoningProvider and the existing ValueEstimationService.
- Supply a durable IRecoveryWorkflowStore, an authenticated IRecoveryActorAccessor, and the team's durable IRecoveryCommandExecutor. Require persistence in production; the legacy optional store path exists for standalone contract tests.
- Supply IRecoveryPlannerToolset adapters using existing Recovery application services and the approved assessment, matching, and pickup gateways. SaveRecoveryOptionsAsync must return authoritative option IDs and versions.
- CreateProposalDraftAsync must invoke the existing validated proposal submission service. It must reload current case/option versions and integrations, apply RecoveryProposalValidator, and require the existing authorized human decision flow. Treat model prose as an explanation only. Use the first validated ranked option as a proposal candidate; never interpret text as an operation.
- Reconcile planner draft values and current persisted option estimates through ValueEstimationService, especially after pickup pricing becomes available. The existing test-only planner valuation/persistence contracts are not a substitute for that production adapter.
- The host must handle caller cancellation and the explicit workflow_state_unavailable reconciliation result if durable failure recording itself is unavailable. Do not report such a run as successful.

Existing identity, command-store, upstream, and schema integration dependencies remain. Any storage or migration work needs the separate integration process; no migration or database connection was performed here.

## External API contract

The adapter follows Google's [generateContent REST contract](https://ai.google.dev/api/generate-content) and [structured JSON output documentation](https://ai.google.dev/gemini-api/docs/structured-output), checked through Context7 and official documentation. It fixes the Google endpoint, uses the key header, disables redirects and HTTP client loggers, and offers no model tools or URL execution. No Gemini SDK or dependency change was needed.
