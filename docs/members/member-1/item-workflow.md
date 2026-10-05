# Item Management Workflow

## Overview
Item Management is the core functionality in the LoopWorth platform for handling customer's electronic waste. It handles the lifecycle of an electronic item from its initial registration by a customer through to its condition assessment via the AI agent, and route selection (Donate or Recycle).

## Actors
- **Customer**: Registers their items, uploads photos, submits items for assessment, reviews assessment results, and selects recovery routes (Donate or Recycle).
- **Admin**: Has elevated access to view and assess items across all customers.
- **ItemAssessmentAgent (System/AI)**: Evaluates the item's details, categorizes it, identifies mismatch errors, and determines condition levels and recovery routes.
- **EcoImpactAgent (System/AI)**: Evaluates the environmental impact and hazards associated with the item based on its condition description.

## Item Workflow
1. **Creation**: The Customer creates an item with basic details (Name, Brand, Model, ConditionDescription, Category). Status is `Draft`. An automated `EcoImpactAgent` assessment is immediately run to identify any environmental hazards.
2. **Photo Management**: While in `Draft`, the Customer uploads photos of the item.
3. **Submission**: Customer submits the item. Status moves to `Submitted`.
4. **Assessment**: An Assessment is triggered. Status moves to `AssessmentPending`. The `ItemAssessmentAgent` analyzes the item and verifies if the physical device identity matches the selected category.
   - If mismatch found, it fails with a validation error, and returns status to `Draft`.
   - If successful, records an `ItemAssessment` and updates status to `Assessed`.
5. **Route Selection**: Customer selects a recovery route (`Donate` or `Recycle`). 
   - If environmental hazards are detected (e.g., swollen battery), `Donate` is prohibited, and the Customer must acknowledge the hazard and select `Recycle`.

## Main Item Functions
- **Add item**: Create item details (Name, Brand, Model, Condition, Category).
- **View item**: View item details, including eco-hazard reports and latest assessment result.
- **Update item**: Edit item details (only when not locked in a recovery request).
- **Delete item**: Remove item and associated photos (only when in `Draft` status).
- **Categorize item**: Link item to a standard Category. Automatically verified by the AI agent.
- **Search/filter items**: Search by name, filter by category or status.
- **Upload/Delete Photos**: Manage item images while in `Draft`.
- **Submit for Assessment**: Transition from Draft to Submitted.
- **Trigger Assessment**: Execute AI analysis for condition level and optimal recovery route.
- **Trigger Eco-Assessment**: Execute AI analysis for environmental hazards.
- **Select Recovery Route**: Choose to Donate or Recycle based on assessment limits.
- **Acknowledge Eco Hazard**: Confirm awareness of item safety risks.

## Business Rules
- **Required fields**: Name, Brand, Model, ConditionDescription, and CategoryId must be present upon creation and update.
- **Category restrictions**: AI strict-checks the category against item Name, Brand, and Model. Any discrepancy forces a correction from the Customer.
- **Ownership restrictions**: Only the owning Customer (or an Admin) can View, Edit, Delete, or Manage Photos for the item. Admin bypass applies to viewing and assessing.
- **Locking rules**: Cannot update an item if it is locked into an active/approved Recovery Request.
- **Hazard restrictions**: Environmentally hazardous items (e.g., leaking, swollen) cannot be routed for Donation. They must be Recycled.
