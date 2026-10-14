# Partner Module API Reference

## Base Path
`/api/partners`

---

## Endpoints

### 1. Get All Partners (Directory / Admin)
- **Method**: `GET`
- **Path**: `/api/partners`
- **Query Params**:
  - `status` (optional): Filter by `Verified`, `PendingVerification`, `Suspended`
  - `category` (optional): Filter by accepted category
  - `page`, `pageSize`: Pagination parameters
- **Response**: `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "EcoRecycle Hub",
    "contactEmail": "info@ecorecycle.com",
    "phoneNumber": "+1-800-555-0199",
    "operatingRegion": "Northern District",
    "status": "Verified",
    "capacityDaily": 150,
    "currentLoad": 42,
    "rating": 4.8
  }
]
```

---

### 2. Get Partner by ID
- **Method**: `GET`
- **Path**: `/api/partners/{id}`
- **Response**: `200 OK` with partner detail object, or `404 Not Found`.

---

### 3. Register New Partner
- **Method**: `POST`
- **Path**: `/api/partners`
- **Body**:
```json
{
  "name": "CircularTech Innovations",
  "contactEmail": "contact@circulartech.org",
  "phoneNumber": "+1-800-555-0123",
  "operatingRegion": "Metro West",
  "acceptedCategories": ["Smartphones", "Batteries", "Tablets"],
  "capacityDaily": 80
}
```
- **Response**: `201 Created`

---

### 4. Trigger AI Partner Matching
- **Method**: `POST`
- **Path**: `/api/partners/match`
- **Body**:
```json
{
  "itemId": "11111111-2222-3333-4444-555555555555",
  "recoveryPlanId": "66666666-7777-8888-9999-000000000000"
}
```
- **Response**: `200 OK` with sorted list of scored partner recommendations.

---

### 5. Customer Selects Partner
- **Method**: `POST`
- **Path**: `/api/partners/select`
- **Body**:
```json
{
  "recoveryPlanId": "66666666-7777-8888-9999-000000000000",
  "selectedPartnerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerNotes": "Preferred morning pickup"
}
```
- **Response**: `200 OK` with confirmation of assignment.
