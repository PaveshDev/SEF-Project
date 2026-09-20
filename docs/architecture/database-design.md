# Database Design (Member 1 Foundation)

This document describes the shared database entities, properties, and relationships established by Member 1. Do NOT generate EF Core migrations directly against the shared Neon database; wait for team integration.

## Entities

### `ApplicationUser` (IdentityUser)
- `Id` (string, PK)
- `Email`, `UserName`, etc. (from ASP.NET Core Identity)

### `Category`
- `Id` (Guid, PK)
- `Code` (string, Unique Index)
- `Name` (string)
- `IsActive` (bool)
- **Relationships:**
  - `Items`: `ICollection<Item>` (1 -> many)

### `Item`
- `Id` (Guid, PK)
- `CustomerId` (string, references `ApplicationUser.Id`)
- `Name` (string)
- `CategoryId` (Guid, FK to `Category.Id`)
- `Brand` (string, nullable)
- `Model` (string, nullable)
- `ConditionDescription` (string, nullable)
- `Status` (string - stored as string mapped from `ItemStatus` enum)
- `SelectedRecoveryRoute` (string, nullable - mapped from `RecoveryRoute` enum)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime, nullable)
- **Relationships:**
  - `Images`: `ICollection<ItemImage>` (1 -> many, Cascade Delete)
  - `Assessments`: `ICollection<ItemAssessment>` (1 -> many, Cascade Delete)
  - `Workflows`: `ICollection<AgentWorkflow>` (1 -> many, SetNull on Delete)

### `ItemImage`
- `Id` (Guid, PK)
- `ItemId` (Guid, FK to `Item.Id`)
- `ImageUrl` (string)
- `SortOrder` (int)
- `IsPrimary` (bool)
- `CreatedAt` (DateTime)

### `ItemAssessment`
- `Id` (Guid, PK)
- `ItemId` (Guid, FK to `Item.Id`)
- `ConditionLevel` (string - enum)
- `RecommendedRoute` (string - enum)
- `AlternativeRoute` (string, nullable - enum)
- `Explanation` (string)
- `ConfidenceLevel` (string - enum)
- `CreatedAt` (DateTime)
- **Note:** Preserves assessment history (1 -> many). The latest assessment can be retrieved by ordering by `CreatedAt` descending.

### `AgentWorkflow`
- `Id` (Guid, PK)
- `CustomerId` (string)
- `ItemId` (Guid, nullable, FK to `Item.Id`)
- `Objective` (string) - *e.g., "Complete recovery workflow for Item {ItemId}"*
- `CurrentStage` (string)
- `Status` (string)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime, nullable)
- `CompletedAt` (DateTime, nullable)
- **Relationships:**
  - `Steps`: `ICollection<AgentWorkflowStep>` (1 -> many, Cascade Delete)

### `AgentWorkflowStep`
- `Id` (Guid, PK)
- `WorkflowId` (Guid, FK to `AgentWorkflow.Id`)
- `AgentName` (string) - *e.g., "ItemAssessmentAgent", "RecoveryPlanningAgent"*
- `StepName` (string)
- `ExecutionStatus` (string)
- `InputSummary` (string, nullable)
- `OutputSummary` (string, nullable)
- `ValidationStatus` (string, nullable)
- `ErrorMessage` (string, nullable)
- `StartedAt` (DateTime, nullable)
- `CompletedAt` (DateTime, nullable)
