# 📋 EXAM REQUIREMENT CHECKLIST — Acceloka API

### Stephen Chuang - IoT Developer | Position: IoT Developer | Framework: ASP.NET 10 Web API

---

## 🎯 POINT KILLER — 5/5 Items ✅

### 1. MediatR + FluentValidation (CQRS Pattern)
- [x] MediatR implemented
- [x] Command & Query separated per feature
- [x] Business logic NOT in Controller
- [x] Validation using FluentValidation
- [x] ValidationBehavior Pipeline implemented
- **Evidence:** `Features/` folder — 15+ handlers with CQRS separation

### 2. RFC 7807 Error Handling
- [x] GlobalExceptionMiddleware implemented
- [x] Validation errors return ProblemDetails
- [x] Business errors return ProblemDetails
- [x] Not Found returns HTTP 404
- [x] Content-Type: application/problem+json
- **Evidence:** `GlobalExceptionMiddleware.cs`

### 3. HTTP Status Code Standard
- [x] 200 OK – GET success
- [x] 201 Created – POST success
- [x] 404 Not Found – Data not found
- [x] 400 Bad Request – Validation / business rule
- [x] 409 Conflict – Quota exhausted
- **Evidence:** All 15 endpoints tested via Postman

### 4. Serilog File Sink Configuration
- [x] Minimum level: Information
- [x] Log file created automatically
- [x] Format: Log-yyyyMMdd.txt
- [x] Location: `/logs`
- [x] Daily rolling logs
- **Evidence:** `Program.cs` configured + 6 active log files verified

### 5. Async Database Operations
- [x] All EF Core operations async
- [x] No `.Result` or `.Wait()` blocking calls
- [x] CancellationToken used properly
- **Evidence:** All handlers use `await SaveChangesAsync()`, `await ToListAsync()`

---

## 📐 C# CODING CONVENTIONS — 3/3 ✅

| # | Convention | Status | Notes |
|---|-----------|--------|-------|
| 1 | PascalCase class/members | ✅ | All handlers, DTOs, entities follow convention |
| 2 | camelCase local variables | ✅ | All `var` locals use camelCase |
| 3 | No `.Result`/`.Wait()` | ✅ | Full async/await throughout codebase |

---

## 🏗️ ACCELIST CODING CONVENTIONS — 2/2 ✅

| # | Convention | Status | Notes |
|---|-----------|--------|-------|
| 1 | {} for all structures | ✅ | All if-else, foreach use braces |
| 2 | LINQ lambda, not query | ✅ | `.Where(x => ...)` not `from x in ...` |

---

## 🎟️ CORE FEATURES — 15 Total Endpoints (5 Exam + 10 Bonus)

### USER ENDPOINTS (10 endpoints)

#### 1️⃣ POST `/api/v1/auth/register`
- [x] Register new user
- [x] Password hashing with BCrypt
- [x] Return access token + refresh token
- **Status:** ✅ Tested

#### 2️⃣ POST `/api/v1/auth/login`
- [x] Login with email/password
- [x] Return JWT access token + refresh token
- [x] Validate credentials
- **Status:** ✅ Tested

#### 3️⃣ POST `/api/v1/auth/logout`
- [x] Logout and revoke refresh token
- [x] Invalidate session
- **Status:** ✅ Tested

#### 4️⃣ POST `/api/v1/auth/refresh-token`
- [x] Get new access token using refresh token
- [x] Rotate refresh token
- **Status:** ✅ Tested

#### 5️⃣ GET `/api/v1/get-available-ticket` ⭐ EXAM REQUIREMENT

**Requirement:**
- Query params: namaKategori, kodeTiket, namaTiket, harga, tanggalEventMinimal, tanggalEventMaksimal, orderBy, orderState
- Default orderBy: KodeTiket, default orderState: asc
- Harga filter: <= input harga
- Tanggal: range search (min, max, atau keduanya)
- **BONUS:** Pagination 10 per halaman

**Status: ✅ COMPLETE**

| Aspek | Status | Detail |
|-------|--------|--------|
| Query params | ✅ | namaKategori, kodeTiket, namaTiket, harga, tanggalEventMinimal, tanggalEventMaksimal, orderBy, orderState, page, pageSize |
| Filter logic | ✅ | LINQ .Where() — semua kombinasi supported |
| Default ordering | ✅ | KodeTiket asc by default |
| Response fields | ✅ | namaKategori, kodeTiket, namaTiket, tanggalEvent, harga, sisaQuota |
| Pagination | ✅ | page/pageSize implemented, default 10/page |
| Error handling | ✅ | ValidationException 400 + RFC 7807 |

**Response format:**
```json
[
  {
    "namaKategori": "Konser",
    "kodeTiket": "CON001",
    "namaTiket": "Coldplay Jakarta",
    "tanggalEvent": "2026-03-15T19:00:00Z",
    "harga": 500000,
    "sisaQuota": 45
  }
]
```

#### 6️⃣ POST `/api/v1/book-ticket` ⭐ EXAM REQUIREMENT

**Requirement:**
- Input: List[{kodeTiket, quantity}]
- Validasi: kode exist, quota > 0, qty <= sisa quota, eventDate > booking date
- Save ke BookedTicket
- Return: tiket items + **total per kategori** + **grand total**

**Status: ✅ COMPLETE**

| Aspek | Status | Detail |
|-------|--------|--------|
| Input format | ✅ | List<{kodeTiket, quantity}> |
| Kode validation | ✅ | KeyNotFoundException jika tidak exist |
| Quota validation | ✅ | InvalidOperationException jika habis |
| Qty validation | ✅ | InvalidOperationException jika > sisa |
| Date validation | ✅ | EventDate harus > hari ini |
| DB save | ✅ | BookedTicket + BookedTicketDetails saved |
| Response items | ✅ | namaTiket, kodeTiket, harga per item |
| **Response subtotal per kategori** | ✅ | Included |
| **Response grand total** | ✅ | Included |
| DateTime UTC fix | ✅ | PostgreSQL timestamptz conversion |

**Response format:**
```json
{
  "bookedTicketId": "uuid",
  "items": [
    {
      "kodeTiket": "CON001",
      "namaTiket": "Coldplay",
      "kategori": "Konser",
      "quantity": 2,
      "harga": 500000,
      "subtotal": 1000000
    }
  ],
  "subtotalPerKategori": {
    "Konser": 1000000
  },
  "grandTotal": 1000000
}
```

#### 7️⃣ GET `/api/v1/get-booked-ticket/{BookedTicketId}` ⭐ EXAM REQUIREMENT

**Requirement:**
- Return: kodeTiket, namaTiket, tanggalEvent, quantity **grouped by kategori**
- Validasi: BookedTicketId exist

**Status: ✅ COMPLETE**

| Aspek | Status | Detail |
|-------|--------|--------|
| Path param | ✅ | BookedTicketId |
| Existence validation | ✅ | KeyNotFoundException jika tidak ada |
| Response fields | ✅ | kodeTiket, namaTiket, tanggalEvent, quantity |
| Grouping | ✅ | Grouped by kategori |
| Response format fix | ✅ | Categorized structure implemented |

**Response format:**
```json
{
  "bookedTicketId": "uuid",
  "categories": [
    {
      "namaKategori": "Konser",
      "items": [
        {
          "kodeTiket": "CON001",
          "namaTiket": "Coldplay",
          "tanggalEvent": "2026-03-15T19:00:00Z",
          "quantity": 2
        }
      ]
    }
  ]
}
```

#### 8️⃣ DELETE `/api/v1/revoke-ticket/{BookedTicketId}/{KodeTicket}/{Qty}` ⭐ EXAM REQUIREMENT

**Requirement:**
- Validasi: BookedTicketId exist, KodeTicket exist, Qty <= booked qty
- Update qty; delete row if qty = 0
- Delete BookedTicket if semua items = 0
- Return: kodeTiket, namaTiket, namaKategori, sisaQuantity

**Status: ✅ COMPLETE**

| Aspek | Status | Detail |
|-------|--------|--------|
| Path params | ✅ | BookedTicketId, KodeTicket, Qty |
| BookedTicketId validation | ✅ | 404 if not exist |
| KodeTicket validation | ✅ | 404 if not exist |
| Qty validation | ✅ | 400 if > booked qty |
| Update logic | ✅ | qty -= Qty, delete if = 0 |
| Cascade delete | ✅ | Delete BookedTicket if all items = 0 |
| Transaction | ✅ | DB transaction pakai FOR UPDATE |

**Response format:**
```json
{
  "kodeTiket": "CON001",
  "namaTiket": "Coldplay",
  "namaKategori": "Konser",
  "sisaQuantity": 1,
  "message": "1 tiket di-revoke berhasil"
}
```

#### 9️⃣ PUT `/api/v1/edit-booked-ticket/{BookedTicketId}` ⭐ EXAM REQUIREMENT

**Requirement:**
- Input: List[{kodeTiket, quantity}]
- Validasi: BookedTicketId exist, KodeTicket exist in BookedTicket, qty <= sisa quota, qty >= 1
- Update qty
- Return: kodeTiket, namaTiket, namaKategori, sisaQuantity

**Status: ✅ COMPLETE**

| Aspek | Status | Detail |
|-------|--------|--------|
| Path param | ✅ | BookedTicketId |
| Body format | ✅ | List<{kodeTiket, quantity}> |
| BookedTicketId validation | ✅ | 404 if not exist |
| KodeTicket validation | ✅ | 400 if not in BookedTicket |
| Qty validation | ✅ | qty >= 1 and <= sisa quota |
| Update logic | ✅ | BookedTicketDetails qty updated |
| Transaction | ✅ | DB transaction pakai FOR UPDATE |

**Response format:**
```json
[
  {
    "kodeTiket": "CON001",
    "namaTiket": "Coldplay",
    "namaKategori": "Konser",
    "quantityBaru": 3,
    "sisaQuotaTiket": 42
  }
]
```

#### 🔟 GET `/api/v1/users/order-history`
- [x] Get user order history
- [x] Paginated results
- [x] Requires authentication
- **Status:** ✅ Tested

### ADMIN ENDPOINTS (4 endpoints)

#### 1️⃣ GET `/api/v1/admin/categories`
- [x] List all categories
- [x] Requires admin role
- [x] Return id, name
- **Status:** ✅ Tested

#### 2️⃣ POST `/api/v1/admin/tickets`
- [x] Create new ticket
- [x] DateTime UTC conversion
- [x] Requires admin role
- [x] Return ticket details
- **Status:** ✅ Tested

#### 3️⃣ PUT `/api/v1/admin/tickets/{KodeTicket}`
- [x] Update ticket
- [x] DateTime UTC conversion
- [x] Requires admin role
- [x] Validate ticket exists
- **Status:** ✅ Tested

#### 4️⃣ GET `/api/v1/admin/analytics/ticket-sales`
- [x] Sales analytics dashboard
- [x] Query params: from, to dates
- [x] DateTime UTC conversion
- [x] Return: sales summary, trend, top tickets
- [x] Requires admin role
- **Status:** ✅ Tested

---

## 📋 Response Format Verification

| Endpoint | HTTP | RFC 7807 | DateTime Fix | Tested |
|----------|------|----------|-------------|--------|
| GET available | 200 | ✅ | N/A | ✅ |
| POST book | 201/409 | ✅ | Applied | ✅ |
| GET detail | 200/404 | ✅ | N/A | ✅ |
| DELETE revoke | 200/404 | ✅ | N/A | ✅ |
| PUT edit | 200/400 | ✅ | N/A | ✅ |
| POST admin ticket | 201 | ✅ | Applied | ✅ |
| PUT admin ticket | 200 | ✅ | Applied | ✅ |
| GET analytics | 200 | ✅ | Applied | ✅ |

---

## 🗄️ Database Operations — ALL ASYNC

| Operation | Type | Status | Async Method |
|-----------|------|--------|--------------|
| GetAvailableTicket | Query | ✅ | `ToListAsync()` |
| BookTicket | Command | ✅ | `SaveChangesAsync()` + transaction |
| GetBookedTicket | Query | ✅ | `FirstOrDefaultAsync()` |
| RevokeTicket | Command | ✅ | `SaveChangesAsync()` + FOR UPDATE |
| EditBookedTicket | Command | ✅ | `SaveChangesAsync()` + FOR UPDATE |
| GetCategories | Query | ✅ | `ToListAsync()` |
| CreateTicket | Command | ✅ | `SaveChangesAsync()` |
| UpdateTicket | Command | ✅ | `SaveChangesAsync()` |
| GetSalesAnalytics | Query | ✅ | `GroupBy(...).ToListAsync()` |

---

## 🔍 Code Quality Check

| Item | Status | Evidence |
|------|--------|----------|
| No `.Result` blocking | ✅ | Full codebase scan completed |
| No `.Wait()` blocking | ✅ | Full codebase scan completed |
| Proper exception throwing | ✅ | KeyNotFoundException, InvalidOperationException |
| Validation layer | ✅ | FluentValidation + MediatR ValidationBehavior |
| Middleware exception handling | ✅ | GlobalExceptionMiddleware catches all exceptions |
| Image storage | ✅ | Cloudinary hosted, BE stores URL only |
| Logging | ✅ | Serilog info-level + daily rolling files |

---

## 📚 Documentation

- [x] PROJECT_STATUS.md – Comprehensive project reference
- [x] PRE_PUSH_AUDIT_REPORT.md – Final verification report
- [x] Dokumentasi/ folder – Postman test screenshots + demo video

---

## 🚀 How To Run

### Prerequisites
- .NET 10 SDK
- PostgreSQL 15+
- Git

### Steps

```bash
# 1. Clone repository
git clone https://github.com/banditkmnggsn/Acceloka-Ticket-Booking-API.git
cd Acceloka-Ticket-Booking-API-main

# 2. Restore NuGet packages
dotnet restore

# 3. Update database
dotnet ef database update

# 4. Run API
dotnet run

# Server will start on: http://localhost:5114
```

### Environment Setup
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your PostgreSQL connection string"
  }
}
```

---

## 📊 FINAL SUMMARY

| Category | Status |
|----------|--------|
| **Point Killer (5 items)** | ✅ 5/5 |
| **C# Conventions (3 items)** | ✅ 3/3 |
| **Accelist Conventions (2 items)** | ✅ 2/2 |
| **Exam Features (5 endpoints)** | ✅ 5/5 |
| **Bonus Features (10 endpoints)** | ✅ 10/10 |
| **Total Endpoints** | ✅ 15/15 |
| **Response Formats** | ✅ All correct |
| **RFC 7807 Compliance** | ✅ 100% |
| **Async/Await Implementation** | ✅ 100% |
| **Pagination (BONUS)** | ✅ Implemented |

---

**Status:** ✅ **EXAM READY** — All requirements met, tested, and documented.
