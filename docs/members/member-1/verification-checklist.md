# Item Verification Checklist

## Item Creation
- [ ] Valid item can be created with Name, Brand, Model, Condition, and Category.
- [ ] Required fields are validated upon creation.
- [ ] Initial Eco-Assessment runs immediately upon creation.

## Item Management
- [ ] Item details can be viewed by the owning Customer.
- [ ] Admin can view all items across all Customers.
- [ ] Item can be updated when not locked in a Recovery Request.
- [ ] Item can be deleted (including associated photos) while in `Draft` status.
- [ ] Photos can be uploaded to an item while in `Draft` status.
- [ ] Photos can be deleted from an item while in `Draft` status.
- [ ] Uploaded photos enforce format (JPG/PNG/WebP) and size (<5MB) limits.

## Item Assessment & Routing
- [ ] Item can be successfully submitted for assessment.
- [ ] `ItemAssessmentAgent` accurately identifies Category Mismatches.
- [ ] `ItemAssessmentAgent` accurately identifies Description Inconsistencies.
- [ ] Item can be successfully assigned a Condition Level and Recommended Route by AI.
- [ ] User can successfully choose a final Recovery Route (Donate or Recycle).
- [ ] User is actively blocked from Donating an item flagged with an environmental hazard.

## Authorization
- [ ] Only the item owner (or Admin) can view the item.
- [ ] Only the item owner can edit, submit, assess, or delete the item.
- [ ] Unauthenticated access to item endpoints is denied.

## Integration
- [ ] Item integrates correctly with `RecoveryRequest` objects (Items cannot be updated if an active Recovery Request exists).

## Validation
- [ ] Invalid values (e.g., missing Name, invalid CategoryId) are rejected on Create and Update.
