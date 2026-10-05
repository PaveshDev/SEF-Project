# Member 1: Items

Own the items folders across web, API, mobile, agents, and corresponding tests. Branch: `feature/member-1-items`.

Read [ownership](../../ownership.md), [team workflow](../../team-workflow.md), and [migration policy](../../migration-policy.md). Record your notes and AI usage in this directory. Add future work and decisions as they happen; no feature work has been completed by this skeleton.

## Responsibility
I am responsible for the Item Management functionality, including Item CRUD operations, Item status progression, photo management, and integration with the AI agents for condition assessment and eco-hazard detection.

## Item Features
- **Item Creation and Editing**: Customers can create and manage their electronic waste items.
- **Photo Management**: Customers can upload and delete images of their items.
- **AI Assessment Integration**: Items undergo rigorous AI assessment (`ItemAssessmentAgent` and `EcoImpactAgent`) to determine categories, damage, and appropriate recovery routes.
- **Eco-Hazard Awareness**: Automatic detection and handling of hazardous devices, preventing unsafe donation options.

## Documentation and Testing Work
- Created `item-workflow.md` documenting the full lifecycle of an item.
- Created `item-status-lifecycle.md` outlining the states (`Draft`, `Submitted`, `AssessmentPending`, `Assessed`).
- Drafted `manual-test-cases.md` detailing specific workflows for manual verification.
- Developed `verification-checklist.md` to ensure all functionality is thoroughly checked.

## Related Project Areas
- **Recovery Request & Collections**: Items feed directly into the Recovery logic, integrating through the `RecoveryRequest` and `CollectionRequest` entities.
- **AI Agents**: The Item domain deeply relies on the `ItemAssessmentAgent` and `EcoImpactAgent`.

## Verification
- Pending manual and unit testing of implemented controller endpoints, domain services, and agent integrations.

## Known Limitations / Remaining Work
- End-to-end integration testing with Partner matching still needs complete verification.
- Further polish of frontend and mobile UI components for photo uploads and agent validations.
