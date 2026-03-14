# Calendar Event Scheduling API

A backend API for managing calendar events, attendees, and notifications within a doctor's practice, built with .NET 10 and Entity Framework Core.

---

## Quick Start

```bash
# 1. Start PostgreSQL (Docker)
docker run -d --name postgres-scheduling -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres

# 2. Restore dependencies
dotnet restore

# 3. Run the API
dotnet run --project src/DoctorScheduling.Api

# 4. Run tests
dotnet test
```

The API launches at `https://localhost:7287` with Swagger UI at `/swagger`. Migrations and seed data are applied automatically on first startup.

> For detailed setup instructions, see [docs/how-to-run.md](docs/how-to-run.md)

---

## Documentation

| Document | Description |
|----------|-------------|
| [Architecture Overview](docs/architecture.md) | Layered project structure, design patterns, SOLID principles, request flow |
| [How to Run](docs/how-to-run.md) | Prerequisites, step-by-step setup, email configuration, troubleshooting |
| [Database Design](docs/database-design.md) | ER diagram, table definitions, indexes, concurrency control, seed data |

---

## Project Structure

```
DoctorScheduling.Models      → Domain entities, enums, DTOs, Result pattern
DoctorScheduling.Data        → DbContext, EF configurations, migrations, seed data
DoctorScheduling.Services    → Business logic services and interfaces
DoctorScheduling.Api         → Controllers, middleware, Program.cs
DoctorScheduling.Tests       → Unit tests (xUnit + FluentAssertions)
```

Dependencies flow inward: **Api → Services → Data → Models**. Each layer is a separate .csproj enforcing separation at the build level.

---

## API Endpoints

### Events
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/events` | Create event with optional attendees |
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

## Key Design Decisions

- **Result\<T\> Pattern** — Business failures return typed results instead of exceptions; controllers map `ResultType` to HTTP status codes consistently
- **Optimistic Concurrency** — `ETag`/`If-Match` headers prevent silent data loss from concurrent edits
- **Notification Abstraction** — `INotificationService` with SendGrid integration and iCal (.ics) attachments; gracefully degrades when disabled
- **No Repository Pattern** — EF Core's `DbSet<T>` already provides repository and unit-of-work; an extra layer would hide capabilities
- **Fixed Duration Types** — Events use Standard (15 min) or Extended (30 min) slots; EndTime is auto-calculated, enforcing consistent scheduling
- **Domain Logic in Entities** — `Event` owns duration calculation (`CalculateEndTime`), overlap detection (`OverlapsWith`), and cancellation logic
- **Global Exception Handler** — Unhandled exceptions return RFC 9457 `ProblemDetails` with no stack trace leakage

> For detailed architecture documentation, see [docs/architecture.md](docs/architecture.md)

---

## Email Notifications (Optional)

The API supports real email delivery via SendGrid with iCal calendar attachments. Update `appsettings.json` to enable:

```json
"SendGrid": {
    "Enabled": true,
    "ApiKey": "SG.your-api-key-here",
    "SenderEmail": "your-verified@email.com",
    "SenderName": "Calendar Scheduling"
}
```

When disabled (default), notifications are logged to console. See [docs/how-to-run.md](docs/how-to-run.md#email-notifications-optional) for full details.

---

## Auto-Generated API Client

Third-party clients can be generated from the OpenAPI spec:

```bash
nswag openapi2csclient /input:https://localhost:7287/swagger/v1/swagger.json /output:CalendarClient.cs
```

---

## Assumptions

1. All times are treated as UTC
2. Events use fixed duration types: **Standard (15 min)** or **Extended (30 min)** — EndTime is auto-calculated from StartTime
3. Attendees are identified by email address (unique per event)
4. Cancelling an event notifies all attendees but preserves the record
5. Deleting an event permanently removes it and all attendees
6. No authentication/authorization is implemented (see Deferred Features)

## Deferred Features

Given more time, the following would be prioritised:

- **Authentication/Authorization** — JWT bearer tokens with role-based policies
- **Recurring Events** — iCal RRULE field with expansion service
- **Pagination** — Cursor-based pagination on list endpoints
- **Audit Logging** — EF Core interceptors for change history
- **Attendee Availability** — Cross-referencing attendee schedules for conflict detection
- **Integration Tests** — API-level tests using `WebApplicationFactory` against a real database

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
