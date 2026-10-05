# Member 3: Partner Module Manual Test Cases

| Test ID | Module / Screen | Test Description | Preconditions | Steps to Execute | Expected Result | Status |
|---|---|---|---|---|---|---|
| **TC-PRT-01** | Partner Directory (Web/Mobile) | View verified partner directory with filters | System has seeded partners | 1. Open Partner Directory<br>2. Filter by Category "Smartphones" | Only verified partners supporting smartphones display | Passed |
| **TC-PRT-02** | Partner Matching (Web) | AI generates partner recommendations for item | Assessed item exists | 1. Navigate to Customer Partner Matching page<br>2. Request AI match | Top scored partner recommendations displayed with match rationale | Passed |
| **TC-PRT-03** | Partner Selection (Mobile) | Customer selects recommended partner | Matches rendered | 1. Tap partner card on mobile screen<br>2. Confirm selection | PartnerSelection saved and confirmation screen shown | Passed |
| **TC-PRT-04** | Admin Partners (Web) | Admin approves pending partner registration | Partner in `PendingVerification` | 1. Login as Admin<br>2. Navigate to Admin Partners<br>3. Click "Verify" | Status updates to `Verified` and appears on public directory | Passed |
| **TC-PRT-05** | Partner Dashboard | View active partner capacity & assigned recovery tasks | Logged in as partner organization | 1. Access Partner Dashboard<br>2. View daily capacity & item intake queue | Metrics reflect real-time assigned items and capacity | Passed |
| **TC-PRT-06** | Capacity Overflow Prevention | Prevent assignment when partner is at 100% capacity | Partner load is 100% | 1. Request matching for new item<br>2. Review recommendations | Fully utilized partner is down-ranked or excluded | Passed |
