# 📊 PROJECT STATUS — Acceloka Ticket Booking API

**Build Status**: ✅ CLEAN  
**Framework**: ASP.NET Core 10 (.NET 10)  
**Database**: PostgreSQL (via Npgsql + EF Core 10)  
**Server**: `http://localhost:5114`

---

## 📁 PROJECT STRUCTURE

```
Acceloka.Api/
├── Application/Services/
│   └── AuthService.cs              → JWT + BCrypt + RefreshToken generation
├── Controllers/
│   ├── AuthController.cs           → /api/v1/auth/*
│   ├── TicketsController.cs        → ticket public + admin ticket management
│   ├── BookingsController.cs       → booked ticket operations
│   ├── UsersController.cs          → user order history
│   └── AdminAnalyticsController.cs → admin sales analytics dashboard endpoints
├── Features/
│   ├── Auth/
│   │   ├── Commands/               RegisterCommand, LogoutCommand + Handlers
│   │   ├── Queries/                LoginQuery, RefreshTokenQuery, GetCurrentUserQuery + Handlers
│   │   ├── Validators/             RegisterCommandValidator, LoginQueryValidator, RefreshTokenQueryValidator
│   │   └── Dtos/                   AuthDtos.cs
│   ├── AdminTickets/
│   │   ├── CreateTicket/           CreateTicketCommand/Handler/Validator
│   │   └── UpdateTicket/           UpdateTicketCommand/Handler/Validator
│   ├── AdminAnalytics/
│   │   └── GetTicketSalesAnalytics/ GetTicketSalesAnalyticsQuery/Handler/Validator
│   ├── Categories/
│   │   └── GetCategories/          GetCategoriesQuery/Handler
│   ├── BookTicket/                 BookTicketCommand/Handler/Validator
│   ├── GetAvailableTicket/         GetAvailableTicketQuery/Handler/Validator
│   ├── GetBookedTicket/            GetBookedTicketQuery/Handler/Validator
│   ├── EditBookedTicket/           EditBookedTicketCommand/Handler/Validator
│   ├── RevokeTicket/               RevokeTicketCommand/Handler/Validator
│   ├── Users/
│   │   ├── Queries/                GetUserOrderHistoryQuery + Handler
│   │   └── Dtos/                   OrderHistoryDtos.cs
│   └── Common/
│       └── Dtos/                   ResponseDtos.cs
├── Infrastructure/
│   ├── Behaviors/                  ValidationBehavior.cs (MediatR pipeline)
│   ├── Middleware/                 GlobalExceptionMiddleware.cs
│   └── Persistence/
│       ├── AccelokaDbContext.cs
│       └── Entities/               User, Ticket, Category, BookedTicket, BookedTicketDetail, RefreshToken
└── Migrations/
    ├── InitialCreate
    ├── AddQuantityCheckConstraints
    ├── AddAuthenticationTables
    ├── AddTicketImageUrl
    ├── AddTicketDescription
    └── AddBookedTicketBookingDateIndex
```

---

## 🗄️ DATABASE SCHEMA

### Tables & Entities

**Users**
| Column | Type | Constraint |
|--------|------|------------|
| Id | uuid | PK |
| Name | text | NOT NULL |
| Email | text | NOT NULL, UNIQUE |
| PasswordHash | text | NOT NULL |
| CreatedAt | timestamp | NOT NULL |

**Categories**
| Column | Type | Constraint |
|--------|------|------------|
| Id | uuid | PK |
| Name | text | NOT NULL |

**Tickets**
| Column | Type | Constraint |
|--------|------|------------|
| Id | uuid | PK |
| CategoryId | uuid | FK → Categories |
| TicketCode | text | NOT NULL, UNIQUE |
| TicketName | text | NOT NULL |
| ImageUrl | text | NULL |
| Description | text | NULL |
| EventDate | timestamp | NOT NULL |
| Price | decimal | NOT NULL |
| Quota | int | NOT NULL, CHECK > 0 |

**BookedTickets**
| Column | Type | Constraint |
|--------|------|------------|
| Id | uuid | PK |
| BookingDate | timestamp | NOT NULL |
| UserId | uuid? | FK → Users (nullable, ON DELETE SET NULL) |

**BookedTicketDetails**
| Column | Type | Constraint |
|--------|------|------------|
| Id | uuid | PK |
| BookedTicketId | uuid | FK → BookedTickets (CASCADE) |
| TicketId | uuid | FK → Tickets (CASCADE) |
| Quantity | int | NOT NULL, CHECK > 0 |

**RefreshTokens**
| Column | Type | Constraint |
|--------|------|------------|
| Id | uuid | PK |
| UserId | uuid | FK → Users (CASCADE) |
| Token | text | NOT NULL |
| ExpiresAt | timestamp | NOT NULL |
| CreatedAt | timestamp | NOT NULL |
| IsRevoked | boolean | NOT NULL |

### Database Rules (CHECK Constraints)
- `CK_Tickets_Quota_Positive` → Ticket.Quota > 0
- `CK_BookedTicketDetails_Quantity_Positive` → BookedTicketDetail.Quantity > 0

### Relationships
- User → BookedTickets (1:N, ON DELETE SET NULL)
- User → RefreshTokens (1:N, ON DELETE CASCADE)
- BookedTicket → Details (1:N, ON DELETE CASCADE)
- BookedTicketDetail → Ticket (N:1, ON DELETE CASCADE)
- Ticket → Category (N:1)
- Category → Tickets (1:N)

### Indexes
- `IX_Users_Email` — UNIQUE
- `IX_Tickets_TicketCode` — UNIQUE
- `IX_BookedTickets_UserId`
- `IX_RefreshTokens_UserId`
- `IX_BookedTickets_BookingDate`

---

## 🔐 AUTHENTICATION & AUTHORIZATION

### JWT Configuration (`appsettings.json`)
- **Secret**: `Jwt:Secret` (min 32 chars)
- **Issuer**: `AccelokaAPI`
- **Audience**: `AccelokaClient`
- **Access Token Expiry**: 1 hour
- **Refresh Token Expiry**: 7 days

### Auth Flow
1. **Register** → hash password (BCrypt) → save User → generate JWT + RefreshToken → return
2. **Login** → verify email + password → generate new JWT + RefreshToken → return
3. **Refresh** → validate old RefreshToken (not revoked, not expired) → revoke old → create new pair → return
4. **Logout** → revoke all active RefreshTokens for user
5. **Get Me** → read UserId from JWT claim → return user info

### Password Hashing
- Library: `BCrypt.Net-Next v4.1.0`
- Hash: `BCrypt.HashPassword(password)`
- Verify: `BCrypt.Verify(password, hash)`

### Token Generation
- **Access Token**: JWT signed with HMAC-SHA256, claims: `NameIdentifier`, `Email`, `Name`
- **Refresh Token**: random 32-byte Base64 string (~44 chars)

---

## 🌐 ENDPOINT MAP (USER vs ADMIN)

### X = No auth required, 🔒 = Auth requir

### A) User/Public Endpoints

#### Auth (`/api/v1/auth`)
| Method | Route | Auth | Handler |
|--------|-------|------|---------|
| POST | `/auth/register` | ❌ | RegisterCommandHandler |
| POST | `/auth/login` | ❌ | LoginQueryHandler |
| GET | `/auth/me` | 🔒 | GetCurrentUserQueryHandler |
| POST | `/auth/logout` | 🔒 | LogoutCommandHandler |
| POST | `/auth/refresh` | ❌ | RefreshTokenQueryHandler |

#### Tickets + Booking (`/api/v1`)
| Method | Route | Auth | Handler |
|--------|-------|------|---------|
| GET | `/get-available-ticket` | ❌ | GetAvailableTicketHandler |
| POST | `/book-ticket` | 🔒 | BookTicketHandler |
| GET | `/get-booked-ticket/{bookedTicketId}` | ❌ | GetBookedTicketHandler |
| PUT | `/edit-booked-ticket/{bookedTicketId}` | 🔒 | EditBookedTicketHandler |
| DELETE | `/revoke-ticket/{bookedTicketId}/{kodeTiket}/{qty}` | 🔒 | RevokeTicketHandler |
| GET | `/users/me/orders` | 🔒 | GetUserOrderHistoryQueryHandler |

### B) Admin Support Endpoints (`/api/v1`)
| Method | Route | Auth | Handler |
|--------|-------|------|---------|
| GET | `/categories/list` | 🔒 | GetCategoriesHandler |
| POST | `/admin/tickets` | 🔒 | CreateTicketHandler |
| PUT | `/admin/tickets/{ticketCode}` | 🔒 | UpdateTicketHandler |
| GET | `/admin/analytics/ticket-sales` | 🔒 | GetTicketSalesAnalyticsHandler |

---

## ✅ VALIDATION RULES (SEPARATED)

### A) User/Auth & Booking Validators

**Register**
- `Name` required, min 2 chars
- `Email` required, valid format
- `Password` required, min 6 chars

**Login**
- `Email` required, valid format
- `Password` required

**Refresh Token**
- `RefreshToken` required

**Book Ticket**
- `Tickets` required, min 1 item
- each `KodeTiket` required
- each `Quantity` > 0

**Get Available Ticket**
- `Page >= 1`
- `PageSize 1..100`
- `HargaMaksimal > 0` (if provided)
- `TanggalEventMinimal <= TanggalEventMaksimal` (if both provided)
- `OrderState` must be `asc` or `desc`

**Get Booked Ticket**
- `BookedTicketId` required

**Edit Booked Ticket**
- `BookedTicketId` required
- `Tickets` required, min 1 item
- each `KodeTiket` required
- each `Quantity >= 1`

**Revoke Ticket**
- `BookedTicketId` required
- `KodeTiket` required
- `Qty > 0`

### B) Admin Validators

**Create Ticket**
- `TicketCode` required
- `TicketName` required
- `CategoryId` required
- `EventDate` must be future date
- `Price > 0`
- `Quota > 0`
- `ImageUrl` must be valid `http/https` URL (if provided)

**Update Ticket**
- `CurrentTicketCode` required (route source)
- `TicketCode` required
- `TicketName` required
- `CategoryId` required
- `EventDate` must be future date
- `Price > 0`
- `Quota > 0`
- `ImageUrl` must be valid `http/https` URL (if provided)

**Ticket Sales Analytics**
- `GroupBy` must be `day` or `month`
- `Top` must be between 1 and 50
- `From <= To` when both provided

---

## ⚠️ ERROR HANDLING (RFC 7807)

Global exception handling via `GlobalExceptionMiddleware`:

| Exception Type | HTTP Status | Title |
|----------------|-------------|-------|
| `ValidationException` | 400 | One or more validation errors occurred |
| `KeyNotFoundException` | 404 | Resource not found |
| `UnauthorizedAccessException` | 401 | Unauthorized |
| `InvalidOperationException` | 400 | Business rule violation |
| `DbUpdateException` | 409 | Database conflict occurred |
| Unhandled exception | 500 | Internal server error |

ProblemDetails format:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Error title",
  "status": 400,
  "detail": "Error detail message",
  "instance": "/api/v1/endpoint-path"
}
```

---

## 🔒 BUSINESS RULES

### Booking Rules
- Ticket code must exist
- Event date must be future
- Request qty cannot exceed remaining quota
- Remaining quota = `Ticket.Quota - SUM(BookedTicketDetails.Quantity)`
- `Book/Edit/Revoke` use DB transaction + `FOR UPDATE` locking (anti race condition)
- User can only edit/revoke own bookings

### Token Rules
- Refresh token rotation enabled (old token revoked)
- Logout revokes all active refresh tokens of user

### Admin Ticket Rules
- Category must exist (checked by `CategoryId`)
- Ticket code must be unique
- Admin create/update supports: code, name, category, event date, price, quota, image URL, description
- Single update path is enforced: use `PUT /api/v1/admin/tickets/{ticketCode}` for all ticket field updates including `imageUrl`
- Public ticket list already returns `imageUrl` and `description`

### Image Asset Storage (Cloudinary)
- Ticket images are hosted on **Cloudinary** (cloud-based image/media management)
- Backend only stores the Cloudinary URL string in `Tickets.ImageUrl` (no local file storage)
- Image upload/management is handled externally (e.g., via Cloudinary dashboard or FE upload widget)
- `imageUrl` field accepts any valid `http`/`https` URL (validated by FluentValidation)

### Admin Analytics Rules
- Analytics based on `BookedTickets.BookingDate` and `BookedTicketDetails`
- Supports period filter (`from`, `to`)
- Supports graph grouping (`day`/`month`)
- Returns summary, trend for chart, and top-selling tickets for comparison
- Optimization added: DB index on `BookedTickets.BookingDate`

### Admin Analytics API Contract (SOURCE OF TRUTH)

Endpoint:
- `GET /api/v1/admin/analytics/ticket-sales`
- Auth: `Bearer <accessToken>` required

Query params:
- `from` (optional): format **`yyyy-MM-dd`**
- `to` (optional): format **`yyyy-MM-dd`**
- `groupBy` (optional): `day` | `month` (default: `day`)
- `top` (optional): `1..50` (default: `10`)

Valid example:
```
GET /api/v1/admin/analytics/ticket-sales?from=2026-03-01&to=2026-03-31&groupBy=day&top=10
```

Important date notes for FE:
- Gunakan format **ISO date** `yyyy-MM-dd` (contoh: `2026-03-01`)
- Jangan kirim placeholder seperti `dd-----yyyy`
- Jika `from` > `to`, backend return validation error (400)
- Jika `from`/`to` kosong, backend pakai default range terakhir

Response shape (saat ini):
```json
{
  "from": "2026-03-01T00:00:00Z",
  "to": "2026-03-31T00:00:00Z",
  "groupBy": "day",
  "summary": {
    "totalOrders": 10,
    "totalTicketsSold": 27,
    "totalRevenue": 33000000,
    "uniqueBuyers": 1
  },
  "trend": [
    {
      "periodStart": "2026-03-01T00:00:00Z",
      "orders": 3,
      "ticketsSold": 8,
      "revenue": 10000000
    }
  ],
  "topTickets": [
    {
      "ticketId": "guid",
      "ticketCode": "CON001",
      "ticketName": "Coldplay Concert Jakarta",
      "soldQuantity": 15,
      "revenue": 22500000
    }
  ]
}
```

FE mapping wajib (nama field exact):
- Card total order → `summary.totalOrders`
- Card total sold → `summary.totalTicketsSold`
- Card revenue → `summary.totalRevenue`
- Card buyer unik → `summary.uniqueBuyers`
- Top ticket qty sold → `topTickets[i].soldQuantity`
- Top ticket revenue → `topTickets[i].revenue`

Current limitation:
- Endpoint ini **belum** mengembalikan list detail order per akun (raw order rows).
- Yang tersedia saat ini hanya: `summary`, `trend`, `topTickets`.

---

## 🌍 CORS

Configured in `Program.cs`:
- Allowed origins: `http://localhost:3000`, `https://yourdomain.com`
- Methods: Any
- Headers: Any
- Credentials: Allowed

---

## 📦 NUGET PACKAGES

| Package | Version |
|---------|---------|
| BCrypt.Net-Next | 4.1.0 |
| FluentValidation | 12.1.1 |
| MediatR | 11.1.0 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.3 |
| Microsoft.EntityFrameworkCore (Design + Tools) | 10.0.3 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 |
| Serilog.AspNetCore | 10.0.0 |
| Serilog.Sinks.File | 7.0.0 |
| Swashbuckle.AspNetCore | 10.1.4 |
| System.IdentityModel.Tokens.Jwt | 8.16.0 |

---

## 🏗️ ARCHITECTURE PATTERN

- CQRS via MediatR (command/query separated)
- ValidationBehavior pipeline (FluentValidation before handler)
- GlobalExceptionMiddleware (RFC7807 response standard)
- Transaction + row locking on critical ticket-booking operations
- Serilog structured logging (console + file)

---

## 🗄️ DB CONNECTION

```
Host=localhost;Port=5432;Database=acceloka;Username=postgres;Password=postgres
```

### Migrations
1. `InitialCreate` — base domain tables
2. `AddQuantityCheckConstraints` — quota/quantity checks
3. `AddAuthenticationTables` — users + refresh tokens + bookedtickets.userid
4. `AddTicketImageUrl` — `Tickets.ImageUrl`
5. `AddTicketDescription` — `Tickets.Description`
6. `AddBookedTicketBookingDateIndex` — index `BookedTickets.BookingDate` for analytics
