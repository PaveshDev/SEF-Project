# Recovery Module - Pending Fixes Checklist
**Date:** 2026-09-10  
**For:** Integration Team  
**Status:** 187 tests passing, awaiting dependencies

---

## QUICK REFERENCE: What's Ready vs What's Not

### ✅ READY FOR IMMEDIATE USE
- Backend: All controllers, services, validation
- React: Complete workflow with CRUD
- Flutter: Complete workflow with tests
- Database: Schema defined (needs 19 column additions)
- Tests: 187 passing

### ⏳ AWAITING INTEGRATION DEPENDENCIES
- Authentication system
- Items assessment API
- Partners matching API
- Collections pickup API
- Durable storage for idempotency
- Database migration applied
- Agent/Gemini hosting

---

## PART 1: CRITICAL BACKEND FIXES (Pre-Integration Testing)

### 1. Add Missing Database Columns to EF Model

**File:** `backend/src/WasteToValue.Api/Infrastructure/Persistence/AppDbContext.cs`

**Action:** Update entity mappings to include these columns:

**RecoveryCase entity:**
```csharp
modelBuilder.Entity<RecoveryCase>(entity =>
{
    // ... existing mappings ...
    entity.Property(e => e.AssessmentVersion)
        .IsRequired()
        .HasDefaultValue(1);
    entity.Property(e => e.ItemRevision)
        .IsRequired()
        .HasDefaultValue(1);
    entity.Property(e => e.Revision)
        .IsRequired()
        .HasDefaultValue(1);
});
```

**RecoveryOption entity:**
```csharp
modelBuilder.Entity<RecoveryOption>(entity =>
{
    // ... existing mappings ...
    entity.Property(e => e.AssessmentVersion)
        .IsRequired()
        .HasDefaultValue(1);
    entity.Property(e => e.CaseRevision)
        .IsRequired();
    entity.Property(e => e.EstimatedShortfall)
        .HasColumnType("numeric(12,2)")
        .HasDefaultValue(0m);
    entity.Property(e => e.IntegrationSnapshot)
        .HasColumnType("jsonb")
        .HasDefaultValue("{}");
    entity.Property(e => e.RequiresPartner)
        .HasDefaultValue(false);
    entity.Property(e => e.RequiresPickup)
        .HasDefaultValue(false);
});
```

**RecoveryProposal entity:**
```csharp
modelBuilder.Entity<RecoveryProposal>(entity =>
{
    // ... existing mappings ...
    entity.Property(e => e.AgentRunId);  // nullable
    entity.Property(e => e.CaseRevision)
        .IsRequired();
    entity.Property(e => e.EstimateSnapshot)
        .HasColumnType("jsonb")
        .HasDefaultValue("{}");
    entity.Property(e => e.InputSnapshot)
        .HasColumnType("jsonb")
        .HasDefaultValue("{}");
    entity.Property(e => e.MatchFreshnessToken);  // nullable
    entity.Property(e => e.MatchVersion);  // nullable
    entity.Property(e => e.OptionVersion)
        .IsRequired()
        .HasDefaultValue(1);
    entity.Property(e => e.PickupFreshnessToken);  // nullable
    entity.Property(e => e.PickupPlanVersion);  // nullable
    entity.Property(e => e.RecommendationOrigin)
        .HasConversion<string>()
        .HasDefaultValue("HUMAN");
});
```

**Create EF Migration:**
```bash
cd backend
dotnet ef migrations add AddRecoveryVersioningColumns -p src/WasteToValue.Api
```

### 2. Update Entity Classes

**Files to Update:**
- `RecoveryCase.cs` - Add properties: AssessmentVersion, ItemRevision, Revision
- `RecoveryOption.cs` - Add properties: AssessmentVersion, CaseRevision, EstimatedShortfall, IntegrationSnapshot, RequiresPartner, RequiresPickup
- `RecoveryProposal.cs` - Add properties: AgentRunId, CaseRevision, EstimateSnapshot, InputSnapshot, MatchFreshnessToken, MatchVersion, OptionVersion, PickupFreshnessToken, PickupPlanVersion, RecommendationOrigin

**Pattern for each property:**
```csharp
public int AssessmentVersion { get; set; } = 1;
public int CaseRevision { get; set; }
public decimal EstimatedShortfall { get; set; }
public string IntegrationSnapshot { get; set; } = "{}";
// ... etc
```

### 3. Update Repository Queries

**File:** `RecoveryPlanningService.cs`

**Current Code (Line ~103-104):**
```csharp
var references = (await repository.ListReferencesAsync(new ValueReferenceQuery(null,
    assessment.Condition, route, value.Currency, PageSize: 1, ...
```

**Issue:** PageSize: 1 is too small; should fetch reasonable batch

**Fix:**
```csharp
const int ReferencePageSize = 20;  // Reasonable batch size
var references = (await repository.ListReferencesAsync(new ValueReferenceQuery(null,
    assessment.Condition, route, value.Currency, PageSize: ReferencePageSize, ...
```

**Also verify in `EfRecoveryRepository.cs`:**
- Reference queries include: `WHERE category_id = @categoryId AND condition_grade = @condition AND route_type = @route AND currency = @currency AND is_verified = true AND observed_at >= @minDate AND observed_at <= @maxDate`
- Queries apply `ORDER BY observed_at DESC, id ASC` (tie-breaker for pagination)
- Pagination uses `LIMIT/OFFSET` correctly

### 4. Verify Financial Consistency

**Files:** ValueEstimationService.cs, RecoveryPlanningService.cs, ProposalDecisionService.cs

**Check:** All three use the same calculation method:
```csharp
var estimate = valuation.Calculate(
    marketValueLow: route == RecoveryRoute.Donate ? 0 : reference.ValueLow,
    marketValueHigh: route == RecoveryRoute.Donate ? 0 : reference.ValueHigh,
    repairCost: 0,  // TODO: Get from verified repair quotation
    pickupCost: pickup?.EstimatedCost ?? 0,
    currency: value.Currency,
    pickupCurrency: pickup?.Currency ?? value.Currency
);
```

**Verification:**
- ✅ Donation sets proceeds to zero
- ✅ Market value preserved for other routes
- ✅ Repair cost handled separately (not fabricated)
- ✅ Pickup cost included from actual gateway
- ✅ Shortfall calculated when net < 0
- ⏳ Must match in: agent planning, API planning, revalidation

---

## PART 2: FRONTEND VALIDATION & ACCESSIBILITY

### React Form Improvements

**File:** `frontend/src/modules/recovery/components/CaseForm.jsx` (or equivalent)

**1. Dialog Accessibility Checklist:**
- [ ] Wrap form in `<div role="dialog" aria-modal="true" aria-labelledby="form-title">`
- [ ] Set `aria-label` on modal container
- [ ] Implement focus trap (FocusTrap library or custom)
- [ ] Close on Escape: `onKeyDown={(e) => e.key === 'Escape' && onClose()`
- [ ] Restore focus to trigger element when closed
- [ ] Prevent body scroll when modal open: `document.body.style.overflow = 'hidden'`

**2. Error Field Mapping:**
```javascript
const mapBackendError = (error) => {
  const fieldErrors = {};
  if (error.response?.data?.errors) {
    error.response.data.errors.forEach(err => {
      fieldErrors[err.field] = err.message;  // "ItemId", "Objective", etc.
    });
  }
  return fieldErrors;
};
```

**3. Visual Improvements:**
- [ ] Use human-readable status labels (map enum to display text)
- [ ] Add inline help text for complex fields
- [ ] Improve label grouping and hierarchy
- [ ] Test responsive behavior at 320px, 768px, 1024px widths

### Flutter Form Improvements

**File:** `mobile/lib/features/recovery/controllers/recovery_controller.dart`

**1. Response Validation:**
```dart
class RecoveryCaseStatus {
  static const validValues = ['DRAFT', 'PLANNING', 'AWAITING_INPUTS', ...];
  
  static RecoveryCaseStatus? tryParse(String? value) {
    if (value == null || !validValues.contains(value)) {
      return null;  // Invalid
    }
    return RecoveryCaseStatus._(value);
  }
}

// In controller:
final status = RecoveryCaseStatus.tryParse(response.status);
if (status == null) {
  throw Exception('Unknown status: ${response.status}');
}
```

**2. Automated Tests:**
```dart
// File: mobile/test/features/recovery/recovery_controller_test.dart

void main() {
  group('RecoveryController', () {
    late RecoveryController controller;
    late MockRecoveryService mockService;
    
    setUp(() {
      mockService = MockRecoveryService();
      controller = RecoveryController(mockService);
    });
    
    testWidgets('createCase with valid inputs', (tester) async {
      // Test case creation
    });
    
    testWidgets('updateInputs creates new revision', (tester) async {
      // Test input update
    });
    
    testWidgets('submitProposal validates expiry', (tester) async {
      // Test proposal submission
    });
  });
}
```

---

## PART 3: DATABASE MIGRATION PROCEDURE

### For Database Integrator

**Step 1: Create Migration**
```bash
cd backend
dotnet ef migrations add AddMissingRecoveryColumns \
  -p src/WasteToValue.Api \
  -o Infrastructure/Persistence/Migrations
```

**Step 2: Verify Migration**
- Review the generated migration file
- Ensure all 19 columns are included
- Check default values match entity definitions

**Step 3: Test on Isolated Database**
```bash
# Create test database (PostgreSQL)
createdb WasteToValue_Test

# Apply migration
cd backend
ASPNETCORE_ENVIRONMENT=Testing dotnet ef database update \
  -p src/WasteToValue.Api

# Run schema validation tests
dotnet test tests/Recovery --filter "Schema"
```

**Step 4: Validate Constraints**
- [ ] Active-case unique index works
- [ ] Proposal revision unique index works
- [ ] Foreign keys enforce referential integrity
- [ ] Check constraints validate money values

**Step 5: Apply to Shared Neon Database**
- [ ] Get DBA approval
- [ ] Schedule maintenance window
- [ ] Apply migration through CI/CD or manual script
- [ ] Verify application connections successful

---

## PART 4: GATEWAY IMPLEMENTATION CONTRACTS

### Member 1: Items Assessment Gateway

**Location:** `backend/src/WasteToValue.Api/Modules/Items/Interfaces/IAssessmentGateway.cs`

**Implementation Required:**
```csharp
public interface IAssessmentGateway
{
    Task<GatewayResult<AssessmentSummary>> GetCurrentAsync(
        Guid itemId, 
        CancellationToken cancellationToken);
}

public class AssessmentSummary
{
    public Guid AssessmentId { get; set; }
    public int Version { get; set; }
    public int ItemRevision { get; set; }
    public bool IsConfirmed { get; set; }
    public Guid? CategoryId { get; set; }
    public string Condition { get; set; }  // "EXCELLENT", "GOOD", etc.
    public string Function { get; set; }   // "FUNCTIONAL", etc.
    public string ServiceArea { get; set; }
}
```

**Validations Required:**
- Item exists and belongs to authenticated user
- Assessment is current (not superseded)
- Assessment is confirmed
- Return error if: item not found, not owned, assessment stale

### Member 3: Partners Matching Gateway

**Location:** `backend/src/WasteToValue.Api/Modules/Partners/Interfaces/IMatchingGateway.cs`

**Implementation Required:**
```csharp
public interface IMatchingGateway
{
    Task<GatewayResult<IReadOnlyList<MatchSummary>>> FindMatchesAsync(
        MatchRequest request,
        CancellationToken cancellationToken);
}

public class MatchSummary
{
    public Guid MatchId { get; set; }
    public int Version { get; set; }
    public Guid RecoveryOptionId { get; set; }
    public MatchEligibility Eligibility { get; set; }  // ELIGIBLE, INELIGIBLE
    public PartnerResponse Response { get; set; }     // ACCEPTED, REJECTED, PENDING
    public string FreshnessToken { get; set; }  // For revalidation
}

public enum MatchEligibility { Eligible, Ineligible, RequiresReview }
public enum PartnerResponse { Accepted, Rejected, Pending, Expired }
```

**Validations Required:**
- Category must accept route
- Condition must meet minimum
- Partner must be verified
- Filter to: eligible partners, accepted responses

### Member 4: Collections Pickup Gateway

**Location:** `backend/src/WasteToValue.Api/Modules/Collections/Interfaces/IPickupPlanningGateway.cs`

**Implementation Required:**
```csharp
public interface IPickupPlanningGateway
{
    Task<GatewayResult<PickupPlanSummary>> PlanAsync(
        PickupPlanRequest request,
        CancellationToken cancellationToken);
}

public class PickupPlanSummary
{
    public Guid PickupPlanId { get; set; }
    public int Version { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Currency { get; set; }  // "LKR", etc.
    public DateTime ProposedStart { get; set; }
    public DateTime ProposedEnd { get; set; }
    public string FeasibilityStatus { get; set; }  // FEASIBLE, INFEASIBLE
    public string FreshnessToken { get; set; }  // For revalidation
}
```

**Validations Required:**
- Cost must be within budget
- Deadline must be feasible
- Planned time must be within service window
- Return INFEASIBLE if: area not covered, time window too tight, fleet unavailable

---

## PART 5: DURABLE STORAGE IMPLEMENTATION

### Integrator: Command Execution & Idempotency

**Interface Already Defined:**
`backend/src/WasteToValue.Api/Modules/Recovery/Interfaces/IRecoveryCommandExecutor.cs`

**Implement Using EF:**
```csharp
public class EfRecoveryCommandExecutor : IRecoveryCommandExecutor
{
    public async Task<TResult> ExecuteAsync<TResult>(
        RecoveryCommand command,
        Func<CancellationToken, Task> preCheckAsync,
        Func<CancellationToken, Task<TResult>> executeAsync,
        CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Check idempotency: same key = same result
            var existing = await context.CommandReceipts
                .FirstOrDefaultAsync(r => r.IdempotencyKey == command.Key, cancellationToken);
            if (existing != null)
            {
                return JsonSerializer.Deserialize<TResult>(existing.ResultJson);
            }
            
            // Pre-check (authorization, dependencies)
            await preCheckAsync(cancellationToken);
            
            // Execute business logic
            var result = await executeAsync(cancellationToken);
            
            // Store receipt
            var receipt = new CommandReceipt
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = command.Key,
                Actor = command.Actor,
                Operation = command.Operation,
                Payload = JsonSerializer.Serialize(command.Payload),
                Result = JsonSerializer.Serialize(result),
                CreatedAt = DateTime.UtcNow
            };
            context.CommandReceipts.Add(receipt);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

**Database Table Required:**
```sql
CREATE TABLE command_receipts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    idempotency_key varchar(100) NOT NULL UNIQUE,
    actor_id uuid NOT NULL,
    operation varchar(50) NOT NULL,
    payload jsonb NOT NULL,
    result jsonb NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_command_receipts_key ON command_receipts(idempotency_key);
```

---

## PART 6: TESTING CHECKLIST

### Unit Tests (Should Already Pass)
```bash
cd backend/tests/Recovery
dotnet test
# Expected: 187 passed
```

### Integration Tests (Needs Real DB)
```bash
cd backend/tests/Recovery
dotnet test --filter "Integration"
```

**Coverage Required:**
- [ ] Case CRUD against real database
- [ ] Option creation and status transitions
- [ ] Proposal submission and decision
- [ ] Reference list with pagination
- [ ] Authorization (different owners isolated)
- [ ] Optimistic concurrency (version conflicts)
- [ ] Idempotency (duplicate key returns same result)
- [ ] Atomicity (failed operations leave clean state)

### Frontend Tests (React)
```bash
cd frontend
npm test -- --coverage
```

**Validation Required:**
- [ ] Form fields accept valid input
- [ ] Form fields reject invalid input
- [ ] Error messages mapped from backend
- [ ] API errors displayed with retry
- [ ] Navigation works after operations
- [ ] Keyboard accessibility (Tab, Escape)

### Mobile Tests (Flutter)
```bash
cd mobile
flutter test --coverage
```

**Validation Required:**
- [ ] Case creation and editing
- [ ] Reference browser pagination
- [ ] Proposal lifecycle
- [ ] Form validation
- [ ] Native device behavior (if possible)

---

## PART 7: SIGN-OFF CRITERIA

### Before Merging to Main
- [ ] All 187 backend tests pass
- [ ] New EF migration generates without errors
- [ ] React form accessibility verified
- [ ] Flutter automated tests passing
- [ ] Code review approved

### Before Deploying to Staging
- [ ] Database migration applied successfully
- [ ] All four gateway integrations complete
- [ ] Durable storage tests passing
- [ ] End-to-end workflow tested with real data
- [ ] Authentication working

### Before Deploying to Production
- [ ] Comprehensive test suite all passing
- [ ] Performance verified (query optimization complete)
- [ ] Logging and monitoring configured
- [ ] Rollback procedure documented
- [ ] Team sign-off from all members

---

**Prepared by:** Member 2  
**Last Updated:** 2026-09-10  
**Questions?** See [HANDOFF-2026-09-10.md](HANDOFF-2026-09-10.md) for full context
