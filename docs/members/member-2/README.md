# Member 2: Recovery

Own the recovery folders across web, API, mobile, agents, and corresponding tests. Branch: `feature/member-2-recovery` (active branch: `recovery-new`).

Read [ownership](../../ownership.md), [team workflow](../../team-workflow.md), and [migration policy](../../migration-policy.md). Record your notes and AI usage in this directory.

---

## Recovery Documentation Index

This directory contains the complete technical specifications, lifecycle definitions, and quality verification documents for Component B (Recovery & Recovery Planning Agent):

1. **[Recovery Workflow Documentation](recovery-workflow.md)**  
   End-to-end architecture, request initiation preconditions, Agentic AI (`RecoveryPlanningAgent`) integration, prompt engineering and guardrails, deterministic offline fallback, checklist gatekeeping, administrative review, and downstream handover pass generation.

2. **[Recovery Status Lifecycle Documentation](recovery-status-lifecycle.md)**  
   Detailed breakdown of `RecoveryStatus` enum values (`Draft`, `PlanGenerated`, `PendingAdminApproval`, `Approved`, `Rejected`, `RevisionRequested`), allowed and forbidden state transitions, triggers, and entity relationships.

3. **[Manual Test Cases](manual-test-cases.md)**  
   Comprehensive manual testing catalog (TC-REC-01 through TC-REC-15) covering functional request creation, validation guardrails, ownership security, AI device specificity (laptop vs. phone), offline resilience, mandatory checklist gates, admin approvals/overrides/rejections, and public pass lookups.

4. **[Verification Checklist](verification-checklist.md)**  
   Pre-submission verification checklist detailing automated unit test results (`RecoveryWorkflowTests.cs` — 5/5 passing) and operational checkpoints across backend API, frontend React web, mobile Flutter, and PostgreSQL persistence.

5. **[AI Usage Log](ai-usage-log.md)**  
   Audit trail recording AI assistance used during development and documentation, highlighting human review, adjustments, and verification steps.
