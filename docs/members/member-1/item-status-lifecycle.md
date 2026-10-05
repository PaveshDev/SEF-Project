# Item Status Lifecycle

The lifecycle of an Item in LoopWorth is governed by the `ItemStatus` enumeration. The system progresses through sequential statuses based on Customer actions and AI agent assessments.

## Actual Statuses
The exact `ItemStatus` values defined in `LoopWorth.Domain.Enums.ItemStatus` are:
- `Draft`
- `Submitted`
- `AssessmentPending`
- `Assessed`

## Meaning of Statuses
- **Draft**: The item has been created and is editable. The Customer can modify item details, manage uploaded photos, and run/re-run eco-assessments. The item can also be deleted safely in this state.
- **Submitted**: The Customer has completed filling in item details (and photos) and requested an assessment. The item is now awaiting the AI assessment processing.
- **AssessmentPending**: The Assessment agent workflow has formally picked up the item and is actively analyzing the item details against its category and condition.
- **Assessed**: The `ItemAssessmentAgent` has successfully completed its analysis, confirmed the category matches the device identity, and proposed a Condition Level and Recommended Route. The Customer can now proceed to select their final route (Donate/Recycle).

## Valid Transitions and Triggers
- `Draft` -> `Submitted`: Triggered by the Customer calling the `POST /api/items/{id}/submit` endpoint.
- `Submitted` -> `AssessmentPending`: Triggered when the Assessment workflow is initiated via `POST /api/items/{id}/assess`.
- `AssessmentPending` -> `Assessed`: Triggered automatically upon successful AI analysis from the `ItemAssessmentAgent`.
- `AssessmentPending` -> `Draft`: Forced fallback if the `ItemAssessmentAgent` detects a Category or Description Mismatch, requiring the Customer to fix the details.
- `Assessed` -> `Draft`: Triggered if the Customer edits the item details (via `PUT /api/items/{id}`), clearing prior assessments to ensure accurate re-evaluation.

## Restrictions
- **Deletions**: Only items in the `Draft` status can be deleted.
- **Photo Management**: Photos can only be uploaded or deleted while the item is in `Draft` status.
- **Route Selection**: A Customer can only choose `Donate` if the item is in the `Assessed` status. (No route can be selected before assessment).
- **Submitting**: Only items currently in `Draft` status can be submitted.
- **Assessing**: Items must be either `Submitted`, `AssessmentPending`, or `Draft` to trigger an assessment.
