# Acceloka API – Requirement Checklist

Candidate: Stephen Chuang 
Position: IoT Developer 
Project: Online Ticket Booking API  
Framework: ASP.NET 10 Web API  
Architecture: Clean Architecture (CQRS + MediatR)

---

## 1. Point Killer Requirements

### MARVEL Pattern (CQRS)
- [x] MediatR implemented
- [x] Command & Query separated per feature
- [x] Business logic not placed in Controller
- [x] Validation using FluentValidation
- [x] ValidationBehavior Pipeline implemented

### Error Handling (RFC 7807)
- [x] GlobalExceptionMiddleware implemented
- [x] Validation errors return ProblemDetails
- [x] Business errors return ProblemDetails
- [x] Not Found returns HTTP 404
- [x] Content-Type: application/problem+json

### HTTP Status Code Standard
- [x] 200 OK – GET success
- [x] 201/200 – POST success
- [x] 204/200 – DELETE success
- [x] 400 – Validation / business rule
- [x] 404 – Data not found

### Async Database Access
- [x] All EF Core operations async
- [x] No .Result or .Wait()
- [x] CancellationToken used

### Serilog Logging
- [x] Minimum level: Information
- [x] Log file created automatically
- [x] Format: Log-yyyyMMdd.txt
- [x] Location: /logs
- [x] Logs request & response

---

## 2. API Feature Requirements

### GET /api/v1/get-available-ticket
- [x] Show available ticket
- [x] Multi column search
- [x] Price <= filter
- [x] Date range filter
- [x] Sorting asc/desc
- [x] Default sorting applied

### POST /api/v1/book-ticket
- [x] Book ticket
- [x] Validate ticket existence
- [x] Validate quota availability
- [x] Validate quantity
- [x] Validate event date
- [x] Return total price per category
- [x] Return grand total

### GET /api/v1/get-booked-ticket/{id}
- [x] Return grouped ticket detail
- [x] 404 if not found

### DELETE /api/v1/revoke-ticket/{BookedTicketId}/{KodeTicket}/{Qty}
- [x] Revoke ticket quantity
- [x] Delete row if qty = 0
- [x] Delete parent booking if empty

### PUT /api/v1/edit-booked-ticket/{BookedTicketId}
- [x] Update ticket quantity
- [x] Validate quota
- [x] Validate minimal qty

---

## 3. Bonus Feature
- [x] Pagination implemented (10 items per page)

---

## 4. How To Run

```bash
dotnet restore
dotnet ef database update
dotnet run
