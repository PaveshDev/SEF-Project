# Member 2: Recovery

Own the recovery folders across web, API, mobile, agents, and corresponding tests. Branch: `feature/member-2-recovery`.

Read [ownership](../../ownership.md), [team workflow](../../team-workflow.md), and [migration policy](../../migration-policy.md). Record your notes and AI usage in this directory.

The approved Recovery backend implementation is described in [recovery-design.md](recovery-design.md). The [schema-change request](schema-change-request.md) identifies required integrator work. Identity, durable command execution, real upstream adapters, and schema deployment remain explicit integration dependencies.

The approved Gemini reasoning implementation keeps CRUD independent of AI and preserves deterministic valuation and human approval. See [behavior and validation](gemini-validation.md), the [production integration request](gemini-integration-request.md), and [changed files](changed-files.md). The provider is registered in the API; agent hosting remains pending because agent source is currently compiled only by the Recovery tests.
