# Item Manual Test Cases

These test cases reflect the actual Item Management functionality confirmed on the `Item-New` branch.

## TC-I01 — Create Item Successfully
**Preconditions**: Customer is logged in. Valid `CategoryId` exists.
**Steps**:
1. Call `POST /api/items` with valid `Name`, `Brand`, `Model`, `ConditionDescription`, and `CategoryId`.
2. Wait for the automatic Eco-Assessment to complete.
**Expected Result**:
- Status code 201 Created.
- Item is created with `Status = Draft`.
- `EcoHazardLevel` and `EcoHazardAcknowledged` are appropriately populated based on condition.
**Verification Status**: Not yet verified

## TC-I02 — Create Item Missing Fields
**Preconditions**: Customer is logged in.
**Steps**:
1. Call `POST /api/items` omitting `ConditionDescription`.
**Expected Result**:
- Status code 400 BadRequest.
- Error message indicating condition description is required.
**Verification Status**: Not yet verified

## TC-I03 — View Customer Items
**Preconditions**: Customer is logged in and owns at least one item.
**Steps**:
1. Call `GET /api/items`.
**Expected Result**:
- Status code 200 OK.
- Returns paginated list of only the items belonging to the requesting Customer.
**Verification Status**: Not yet verified

## TC-I04 — View All Items as Admin
**Preconditions**: Admin user is logged in. Multiple customers have items.
**Steps**:
1. Call `GET /api/items`.
**Expected Result**:
- Status code 200 OK.
- Returns paginated list of items across all customers.
**Verification Status**: Not yet verified

## TC-I05 — Update Item Details
**Preconditions**: Customer owns an item in `Draft` status.
**Steps**:
1. Call `PUT /api/items/{id}` providing updated `Name` and `ConditionDescription`.
**Expected Result**:
- Status code 200 OK.
- Item details are updated.
- Previous `ItemAssessments` are cleared.
- Status is explicitly set to `Draft`.
- Eco-Assessment is re-run and updated.
**Verification Status**: Not yet verified

## TC-I06 — Upload Item Photo
**Preconditions**: Customer owns an item in `Draft` status.
**Steps**:
1. Call `POST /api/items/{id}/photos/upload` with a valid `<5MB` JPG/PNG image file.
**Expected Result**:
- Status code 200 OK.
- Image is saved to file storage and record is created in DB linked to the Item.
**Verification Status**: Not yet verified

## TC-I07 — Upload Photo to Assessed Item
**Preconditions**: Customer owns an item in `Assessed` status.
**Steps**:
1. Call `POST /api/items/{id}/photos/upload` with a valid image file.
**Expected Result**:
- Status code 400 BadRequest.
- Error stating photos can only be added when item is in Draft status.
**Verification Status**: Not yet verified

## TC-I08 — Delete Item
**Preconditions**: Customer owns an item in `Draft` status with uploaded photos.
**Steps**:
1. Call `DELETE /api/items/{id}`.
**Expected Result**:
- Status code 204 NoContent.
- Item is deleted from DB.
- Associated image files are removed from physical storage.
**Verification Status**: Not yet verified

## TC-I09 — Submit Item
**Preconditions**: Customer owns an item in `Draft` status with a condition description.
**Steps**:
1. Call `POST /api/items/{id}/submit`.
**Expected Result**:
- Status code 204 NoContent.
- Item status transitions to `Submitted`.
**Verification Status**: Not yet verified

## TC-I10 — Agent Category Validation Rejection
**Preconditions**: Customer submits an item named "iPhone 13" but categorized as "Laptop".
**Steps**:
1. Call `POST /api/items/{id}/assess`.
2. Wait for `ItemAssessmentAgent` response.
**Expected Result**:
- Status code 422 UnprocessableEntity.
- Returns `isCategoryMismatch = true`.
- Item status returns to `Draft`.
**Verification Status**: Not yet verified

## TC-I11 — Select Route (Donate) for Eco-Hazardous Item
**Preconditions**: Customer owns an `Assessed` item that was flagged with High Eco-Hazard.
**Steps**:
1. Call `POST /api/items/{id}/select-route` with `{"SelectedRoute": "Donate"}`.
**Expected Result**:
- Status code 400 BadRequest.
- Error stating hazardous items cannot be accepted for Donation and must be Recycled.
**Verification Status**: Not yet verified
