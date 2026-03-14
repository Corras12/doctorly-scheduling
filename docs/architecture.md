# Overall Architecture

## Overview

The Calendar Event Scheduling API follows a **layered architecture** with strict dependency flow. Each layer is a separate .NET class library project, enforcing separation of concerns at the build level — not just by convention.

```
┌─────────────────────────────────────────────┐
│              DoctorScheduling.Api            │
│         (Controllers, Middleware, DI)        │
├─────────────────────────────────────────────┤
│           DoctorScheduling.Services          │
│       (Business Logic, Notifications)        │
├─────────────────────────────────────────────┤
│            DoctorScheduling.Data             │
│     (DbContext, EF Configurations, Seed)     │
├─────────────────────────────────────────────┤
│           DoctorScheduling.Models            │
│        (Entities, DTOs, Enums, Result)       │
└─────────────────────────────────────────────┘
```

**Dependency direction:** Api → Services → Data → Models. Each layer only references the layer directly below it.

---

## Layer Responsibilities

### Models (DoctorScheduling.Models)
- **Domain entities** (`Event`, `Attendee`) with encapsulated business rules
- **Enums** (`AttendanceStatus`, `EventDurationType`, `ResultType`)
- **DTOs** for API request/response contracts with data annotation validation
- **Result\<T\> pattern** for typed operation outcomes

This layer has **zero external dependencies** — it is a pure C# class library.

### Data (DoctorScheduling.Data)
- **AppDbContext** with auto-timestamping (`CreatedAt`/`UpdatedAt`) via `SaveChanges` override
- **Fluent API configurations** enforcing database constraints (max lengths, indexes, relationships)
- **Seed data** for development and demo purposes
- **Migrations** managed via EF Core CLI

### Services (DoctorScheduling.Services)
- **IEventService** — all business logic for event CRUD, attendee management, and RSVP
- **INotificationService** — email delivery via SendGrid with iCal (.ics) calendar attachments
- Validation, concurrency conflict handling, and orchestration live here
- Services depend on abstractions (interfaces) registered via DI

### Api (DoctorScheduling.Api)
- **Controllers** inherit from `ApiControllerBase` for consistent `Result<T>` → HTTP mapping
- **GlobalExceptionHandler** catches unhandled exceptions and returns RFC 9457 `ProblemDetails`
- **Program.cs** configures DI, middleware pipeline, Swagger, and auto-migration on startup
- Controllers are intentionally thin — they map HTTP to service calls and nothing more

### Tests (DoctorScheduling.Tests)
- **xUnit** with **FluentAssertions** for readable test assertions
- **EF Core InMemory** provider — no database required to run tests
- **StubNotificationService** replaces real notifications during testing (no mocking library needed)

---

## Key Design Patterns

### Result Pattern
Instead of throwing exceptions for business rule violations, services return `Result<T>` with a typed `ResultType`:

| ResultType | HTTP Status | Meaning |
|-----------|-------------|---------|
| Success | 200/201 | Operation completed |
| ValidationError | 400 | Invalid input or business rule violation |
| NotFound | 404 | Entity does not exist |
| Conflict | 409 | Concurrency conflict (stale update) |

The base controller maps these automatically, keeping controllers clean and consistent.

### Optimistic Concurrency
Events carry a `RowVersion` concurrency token. The API flow:
1. `GET /api/events/{id}` → response includes `ETag` header
2. `PUT /api/events/{id}` with `If-Match: {etag}` header
3. If another user updated the event in between, EF Core detects the mismatch and the API returns a user-friendly conflict message

This prevents silent data loss from concurrent edits without pessimistic locking.

### Notification System
`INotificationService` is an abstraction that decouples event logic from delivery:
- **Production**: SendGrid sends real emails with `.ics` calendar attachments
- **Disabled mode**: Notifications are logged to console (graceful degradation)
- **Testing**: `StubNotificationService` records calls without side effects

The feature toggle (`SendGrid:Enabled` in appsettings) allows the API to run without email infrastructure.

---

## Request Flow

```
HTTP Request
    │
    ▼
Controller (ApiControllerBase)
    │  Validates model state (data annotations)
    │  Extracts If-Match header for concurrency
    ▼
Service (IEventService)
    │  Business validation
    │  Database operations via AppDbContext
    │  Triggers INotificationService
    ▼
Result<T>
    │
    ▼
Controller maps Result → HTTP Response
    │  200/201 with body, or
    │  400/404/409 with ProblemDetails
    ▼
HTTP Response
```

Unhandled exceptions bypass this flow and are caught by `GlobalExceptionHandler`, which returns a generic 500 `ProblemDetails` response.
