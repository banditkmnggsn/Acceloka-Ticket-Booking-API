# 📋 EXAM REQUIREMENT CHECKLIST — Acceloka API

### Stephen Chuang - IoT Developer

---

## 🎯 POINT KILLER — 5/5 Items ✅

| # | Requirement | Status | Bukti |
|---|-------------|--------|--------|
| 1 | MediatR + FluentValidation | ✅ | Features/ folder — semua pakai CQRS |
| 2 | RFC 7807 error handling | ✅ | GlobalExceptionMiddleware.cs |
| 3 | HTTP response code benar | ✅ | 201 Created, 200 OK, 404, 400, 409, 500 |
| 4 | Serilog file sink config | ✅ | Program.cs — Log-{date}.txt di /logs |
| 5 | Async await DB operations | ✅ | Semua handler pakai `await _context.SaveChangesAsync()` |

---

## 📐 C# CODING CONVENTIONS — 3/3 ✅

| # | Convention | Status | Notes |
|---|-----------|--------|-------|
| 1 | PascalCase class/members | ✅ | Checked all handlers, DTOs, entities |
| 2 | camelCase local variables | ✅ | Semua `var` lokal pakai camelCase |
| 3 | No `.Result`/`.Wait()` blocking | ✅ | All async methods properly awaited |

---

## 🏗️ ACCELIST CODING CONVENTIONS — 2/2 ✅

| # | Convention | Status | Notes |
|---|-----------|--------|-------|
| 1 | {} untuk semua struktur | ✅ | if-else, foreach semua pakai {} |
| 2 | LINQ lambda, not query syntax | ✅ | `.Where(x => ...)` bukan `from x in ...` |

---

## 🎟️ CORE FEATURES — 5/5 Endpoints

### 1️⃣ GET `/api/v1/get-available-ticket`

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

**Validation:**
- Page >= 1 ✅
- PageSize 1..100 ✅
- HargaMaksimal > 0 if provided ✅
- TanggalEventMinimal <= TanggalEventMaksimal if both provided ✅
- OrderState = asc/desc ✅

---

### 2️⃣ POST `/api/v1/book-ticket`

**Requirement:**
- Input: List[{kodeTiket, quantity}]
- Validasi: kode exist, quota > 0, qty <= sisa quota, eventDate > booking date
- Save ke BookedTicket
- Return: tiket items + **total per kategori** + **grand total**

**Status: ✅ COMPLETE**

| Aspek | Status | Detail |
|-------|--------|--------|
| Input format | ✅ | List<{kodeTiket, quantity}> |
| Kode validation | ✅ | Throw KeyNotFoundException jika tidak exist |
| Quota validation | ✅ | Throw InvalidOperationException jika habis |
| Qty validation | ✅ | Throw InvalidOperationException jika > sisa |
| Date validation | ✅ | EventDate harus > hari ini |
| DB save | ✅ | BookedTicket + BookedTicketDetails saved |
| Response items | ✅ | namaTiket, kodeTiket, harga per item |
| **Response subtotal** | ✅ | Total per kategori |
| **Response grand total** | ✅ | Total semua kategori |
| Error handling | ✅ | 400/404/409 + RFC 7807 |

**Response format:**
```json
{
  "bookingId": "uuid",
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

---

### 3️⃣ GET `/api/v1/get-booked-ticket/{BookedTicketId}`

**Requirement:**
- Return: kodeTiket, namaTiket, tanggalEvent, quantity **grouped by kategori**
- Validasi: BookedTicketId exist

**Status: ✅ COMPLETE**

| Aspek | Status | Detail |
|-------|--------|--------|
| Path param | ✅ | BookedTicketId |
| Existence validation | ✅ | Throw KeyNotFoundException jika tidak ada |
| Response fields | ✅ | kodeTiket, namaTiket, tanggalEvent, quantity |
| Grouping | ✅ | Grouped by kategori |
| Error handling | ✅ | 404 + RFC 7807 |

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

---

### 4️⃣ DELETE `/api/v1/revoke-ticket/{BookedTicketId}/{KodeTicket}/{Qty}`

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
| Response fields | ✅ | kodeTiket, namaTiket, namaKategori, sisaQuantity |
| Error handling | ✅ | 400/404 + RFC 7807 |
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

---

### 5️⃣ PUT `/api/v1/edit-booked-ticket/{BookedTicketId}`

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
| Response fields | ✅ | kodeTiket, namaTiket, namaKategori, sisaQuantity |
| Error handling | ✅ | 400/404 + RFC 7807 |
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

---

## 📋 Response Format Check

### ✅ All responses must be JSON with proper structure

| Endpoint | Success HTTP | Error HTTP | RFC 7807 |
|----------|-------------|-----------|----------|
| GET available | 200 | 400 | ✅ |
| POST book | 201 | 400/404/409 | ✅ |
| GET detail | 200 | 404 | ✅ |
| DELETE revoke | 200 | 400/404 | ✅ |
| PUT edit | 200 | 400/404 | ✅ |

---

## 🗄️ Database Operations Check

| Operation | Status | Async |
|-----------|--------|-------|
| GetAvailableTicket | ✅ | await ToListAsync() |
| BookTicket | ✅ | await SaveChangesAsync() + transaction |
| GetBookedTicket | ✅ | await FirstOrDefaultAsync() |
| RevokeTicket | ✅ | await SaveChangesAsync() + transaction |
| EditBookedTicket | ✅ | await SaveChangesAsync() + transaction |

---

## 🔍 Code Quality Check

| Item | Status | Notes |
|------|--------|-------|
| No `.Result` blocking | ✅ | Checked all async calls |
| No `.Wait()` blocking | ✅ | Checked all async calls |
| Proper exception throwing | ✅ | KeyNotFoundException, InvalidOperationException |
| Validation layer | ✅ | FluentValidation + MediatR pipeline |
| Middleware exception handling | ✅ | GlobalExceptionMiddleware catches all |

---

## 📊 SUMMARY

| Category | Status |
|----------|--------|
| **Point Killer (5 items)** | ✅ 5/5 |
| **C# Conventions (3 items)** | ✅ 3/3 |
| **Accelist Conventions (2 items)** | ✅ 2/2 |
| **Core Features (5 endpoints)** | ✅ 5/5 |
| **Response Formats** | ✅ All correct |
| **Pagination (BONUS)** | ✅ Implemented |
