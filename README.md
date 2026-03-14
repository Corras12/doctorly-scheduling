# Calendar Event Scheduling API

A backend API for managing calendar events, attendees, and notifications within a doctor's practice, built with .NET 10 and Entity Framework Core.

---

## Requirements

| Requirement | Version / Details |
|------------|-------------------|
| **.NET SDK** | 10.0 or later ([Download](https://dotnet.microsoft.com/download)) |
| **PostgreSQL** | 13+ (via Docker, local install, or cloud-hosted) |
| **EF Core CLI** | `dotnet tool install --global dotnet-ef` |
| **Docker** *(optional)* | For running PostgreSQL without a local install |
| **SendGrid account** *(optional)* | For real email delivery — free trial available |

---

## How to Run the API

### Step 1: Start PostgreSQL

**Option A — Docker (recommended):**
```bash
docker run -d --name postgres-scheduling -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres
```

**Option B — Local PostgreSQL:**
Ensure PostgreSQL is running on `localhost:5432` with username `postgres` and password `postgres`. If your setup differs, update the connection string in `src/DoctorScheduling.Api/appsettings.json`:
```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DoctorSchedulingDb;Username=postgres;Password=postgres"
}
```

### Step 2: Restore dependencies
```bash
dotnet restore
```

### Step 3: Run the API
```bash
dotnet run --project src/DoctorScheduling.Api
```

The application automatically applies database migrations and seeds sample data on first startup. Swagger UI launches at the root URL — typically `https://localhost:7287` or `http://localhost:5123`.

### Step 4: Explore the API
Open the Swagger UI in your browser to see all available endpoints, try them out with the pre-seeded data, and view request/response schemas.

---

## Running Tests
```bash
dotnet test
```

Tests use an in-memory database provider — **no PostgreSQL setup required** to run tests.

---

## Email Notifications (Optional)

The API supports real email delivery via SendGrid with iCal (`.ics`) calendar attachments. To enable:

1. Create a [SendGrid account](https://sendgrid.com/) and generate an API key
2. Verify a sender email address in SendGrid
3. Update `src/DoctorScheduling.Api/appsettings.json`:

```json
"SendGrid": {
    "Enabled": true,
    "ApiKey": "SG.your-api-key-here",
    "SenderEmail": "your-verified@email.com",
    "SenderName": "Calendar Scheduling"
}
```

When **disabled** (default), notifications are logged to the console with generated iCal content. When **enabled**, real emails are sent with `.ics` calendar attachments that recipients can open in Outlook, Google Calendar, Apple Calendar, etc.

---

## Architecture & Design Decisions

### Project Structure

```
DoctorScheduling.Models      → Domain entities, enums, DTOs, Result pattern
DoctorScheduling.Data        → DbContext, EF configurations, migrations, seed data
DoctorScheduling.Services    → Business logic services and interfaces
DoctorScheduling.Api         → Controllers, middleware, Program.cs
DoctorScheduling.Tests       → Unit tests (xUnit + FluentAssertions)
```

Dependencies flow inward: **Api → Services → Data → Models**. Each layer has a single responsibility and can be tested or replaced independently.

### SOLID Principles
- **Single Responsibility** — Controllers handle HTTP mapping only; services coordinate business logic; domain entities own invariants
- **Open/Closed** — The `Result<T>` pattern with `ResultType` enum allows new error categories without modifying controller mapping logic
- **Liskov Substitution** — Service interfaces (`IEventService`, `IDoctorService`, `INotificationService`) allow swapping implementations (e.g., stub for testing)
- **Interface Segregation** — Separate interfaces for doctor management, event management, and notifications
- **Dependency Inversion** — Controllers depend on service abstractions, not concrete classes; `AppDbContext` is injected via DI

### Domain-Driven Design
Domain entities encapsulate business rules:
- **`Doctor`** manages practitioner identity, specialisation, and active status with a `FullName` computed property
- **`Event`** owns time range validation (`HasValidTimeRange`), overlap detection (`OverlapsWith`), duration calculation (`CalculateEndTime`), and cancellation logic (`Cancel`)
- The service layer orchestrates use cases (e.g., doctor availability checking) but defers invariant checks to the domain

### Result Pattern Over Exceptions
Business rule failures return `Result<T>` objects with typed `ResultType` enum (`ValidationError`, `NotFound`, `Conflict`) instead of throwing exceptions. The controller maps `ResultType` to HTTP status codes (400, 404, 409). The `GlobalExceptionHandler` catches only genuine unexpected errors and returns RFC 9457 `ProblemDetails`.

### Notification System
The API implements an `INotificationService` interface with iCal (.ics) generation capability. The current implementation logs notifications and generates valid iCal content (VCALENDAR/VEVENT with ATTENDEE entries and PARTSTAT). This demonstrates the notification pattern and can be extended with SMTP, message queue, or other delivery mechanisms.

### Optimistic Concurrency
Events use a `RowVersion` concurrency token. The API supports `ETag`/`If-Match` headers on GET and PUT operations, returning `409 Conflict` when a stale update is detected. This prevents lost updates when multiple users modify the same event.

### No Repository Pattern
EF Core's `DbSet<T>` already implements repository and unit-of-work patterns. Adding a generic repository on top would hide EF's capabilities behind a least-common-denominator interface.

### DTOs Everywhere
Domain entities are never exposed in API responses. Response DTOs use `record` types with static `FromEntity()` factory methods for manual mapping.

### Auto-Generated API Documentation
XML documentation comments on controllers are included in the Swagger/OpenAPI spec. Third-party clients can be generated from the OpenAPI spec using tools like NSwag or Kiota:

```bash
# Generate C# client from OpenAPI spec
nswag openapi2csclient /input:https://localhost:7287/swagger/v1/swagger.json /output:CalendarClient.cs
```

---

## API Endpoints

### Doctors
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/doctors` | Register a new doctor |
| GET | `/api/doctors` | List doctors (filter: `?isActive=&search=`) |
| GET | `/api/doctors/{id}` | Get doctor by ID |
| PUT | `/api/doctors/{id}` | Update doctor details |
| PATCH | `/api/doctors/{id}/deactivate` | Deactivate a doctor (soft delete) |

### Events
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/events` | Create event for a doctor with optional attendees |
| GET | `/api/events` | List events (filter: `?from=&to=&search=&cancelled=`) |
| GET | `/api/events/search` | Search events by title/description (`?q=`) |
| GET | `/api/events/{id}` | Get event with attendees (returns ETag) |
| PUT | `/api/events/{id}` | Update event (supports If-Match for concurrency) |
| DELETE | `/api/events/{id}` | Permanently delete event |
| PATCH | `/api/events/{id}/cancel` | Cancel event (soft delete with reason) |

### Attendees
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/events/{id}/attendees` | Add attendee to event |
| DELETE | `/api/events/{id}/attendees/{attendeeId}` | Remove attendee |
| PATCH | `/api/events/{id}/attendees/{attendeeId}/respond` | RSVP (Accept/Decline/Tentative) |

---

## Assumptions
1. All times are treated as UTC
2. Events use fixed duration types — Standard (15 min) for routine GP consultations and Extended (30 min) for complex cases
3. Each event belongs to a single doctor; the system prevents double-booking the same doctor
4. Doctors are soft-deleted (deactivated) rather than hard-deleted to preserve event history
5. Attendees are identified by email address (unique per event)
6. Cancelling an event notifies all attendees but preserves the record
7. Deleting an event permanently removes it and all attendees
8. No authentication/authorization is implemented (see Deferred Features)

## Deferred Features
Given more time, the following would be prioritised:
- **Authentication/Authorization** — JWT bearer tokens with role-based policies
- **Recurring Events** — iCal RRULE field with expansion service
- **Pagination** — Cursor-based pagination on list endpoints
- **Audit Logging** — EF Core interceptors for change history
- **Attendee Availability** — Cross-referencing attendee schedules for conflict detection
- **Doctor Schedule Management** — Configurable working hours and availability windows per doctor

## Tech Stack
- .NET 10 / ASP.NET Core
- Entity Framework Core 10 with PostgreSQL (Npgsql)
- Swagger / Swashbuckle with XML documentation
- xUnit + FluentAssertions + EF Core InMemory

## Third-Party Libraries

| Package | Version | Purpose | Project |
|---------|---------|---------|---------|
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 10.0.1 | PostgreSQL database provider for Entity Framework Core | Data, Api |
| **Microsoft.EntityFrameworkCore.Design** | 10.0.5 | EF Core tooling for migrations (`dotnet ef`) | Data, Api |
| **Swashbuckle.AspNetCore** | 10.1.5 | OpenAPI/Swagger documentation and interactive UI | Api |
| **SendGrid** | 9.29.3 | Email delivery service for sending notifications with iCal attachments | Services |
| **Microsoft.Extensions.Logging.Abstractions** | 10.0.5 | Logging interface for the service layer (no direct framework dependency) | Services |
| **xUnit** | 2.9.3 | Unit testing framework | Tests |
| **FluentAssertions** | 8.8.0 | Readable assertion syntax for tests (e.g., `result.Should().BeTrue()`) | Tests |
| **Microsoft.EntityFrameworkCore.InMemory** | 10.0.5 | In-memory database provider for unit tests (no PostgreSQL needed) | Tests |
| **coverlet.collector** | 6.0.4 | Code coverage collection during test runs | Tests |
