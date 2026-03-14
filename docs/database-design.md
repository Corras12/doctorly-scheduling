# Database Design

## Overview

The database uses **PostgreSQL** with **Entity Framework Core 10** as the ORM. The schema is managed entirely through EF Core migrations, ensuring the database structure is version-controlled and reproducible.

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────┐
│               Events                │
├─────────────────────────────────────┤
│ Id              GUID (PK)           │
│ Title           VARCHAR(200) NOT NULL│
│ Description     VARCHAR(2000)       │
│ DurationType    INT NOT NULL        │
│ StartTime       TIMESTAMP NOT NULL  │
│ EndTime         TIMESTAMP NOT NULL  │
│ Location        VARCHAR(500)        │
│ IsCancelled     BOOLEAN             │
│ CancellationReason VARCHAR(500)     │
│ RowVersion      UINT (Concurrency)  │
│ CreatedAt       TIMESTAMP           │
│ UpdatedAt       TIMESTAMP           │
├─────────────────────────────────────┤
│ IX_StartTime_EndTime (composite)    │
│ IX_IsCancelled                      │
└──────────────────┬──────────────────┘
                   │ 1
                   │
                   │ *
┌──────────────────┴──────────────────┐
│              Attendees              │
├─────────────────────────────────────┤
│ Id              GUID (PK)           │
│ EventId         GUID (FK) NOT NULL  │
│ Name            VARCHAR(100) NOT NULL│
│ Email           VARCHAR(254) NOT NULL│
│ Status          INT NOT NULL        │
│ CreatedAt       TIMESTAMP           │
│ UpdatedAt       TIMESTAMP           │
├─────────────────────────────────────┤
│ IX_EventId_Email (unique composite) │
└─────────────────────────────────────┘
```

---

## Tables

### Events

The central entity representing a calendar event.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | `uuid` | PK, auto-generated | Unique identifier |
| Title | `varchar(200)` | NOT NULL | Event title |
| Description | `varchar(2000)` | Nullable | Detailed description |
| DurationType | `integer` | NOT NULL | Duration type enum (0=Standard 15min, 1=Extended 30min) |
| StartTime | `timestamp` | NOT NULL | Event start (UTC) |
| EndTime | `timestamp` | NOT NULL | Event end (UTC) — auto-calculated from StartTime + DurationType |
| Location | `varchar(500)` | Nullable | Event location |
| IsCancelled | `boolean` | Default: false | Soft-delete flag |
| CancellationReason | `varchar(500)` | Nullable | Reason for cancellation |
| RowVersion | `xmin` | Concurrency token | PostgreSQL system column for optimistic concurrency |
| CreatedAt | `timestamp` | Auto-set | Record creation timestamp |
| UpdatedAt | `timestamp` | Nullable, auto-set | Last modification timestamp |

### Attendees

People invited to an event, with RSVP tracking.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | `uuid` | PK, auto-generated | Unique identifier |
| EventId | `uuid` | FK → Events.Id, NOT NULL | Parent event |
| Name | `varchar(100)` | NOT NULL | Attendee display name |
| Email | `varchar(254)` | NOT NULL | Email address (RFC 5321 max length) |
| Status | `integer` | NOT NULL, Default: 0 | Attendance status enum |
| CreatedAt | `timestamp` | Auto-set | Record creation timestamp |
| UpdatedAt | `timestamp` | Nullable, auto-set | Last modification timestamp |

---

## Indexes

| Table | Index | Columns | Type | Purpose |
|-------|-------|---------|------|---------|
| Events | IX_StartTime_EndTime | StartTime, EndTime | Composite | Efficient date range queries and overlap detection |
| Events | IX_IsCancelled | IsCancelled | Single | Fast filtering of active vs cancelled events |
| Attendees | IX_EventId_Email | EventId, Email | Unique composite | Prevents duplicate attendees per event, supports lookups |

---

## Relationships

| Relationship | Type | Cascade |
|-------------|------|---------|
| Event → Attendees | One-to-Many | Delete (removing an event removes all its attendees) |

---

## Event Duration Type Enum

| Value | Name | Duration | Description |
|-------|------|----------|-------------|
| 0 | Standard | 15 minutes | Standard GP consultation |
| 1 | Extended | 30 minutes | Extended consultation |

The `EndTime` is auto-calculated by the service layer: `EndTime = StartTime + DurationMinutes`. Clients provide `StartTime` and `DurationType` only — they never set `EndTime` directly.

---

## Attendance Status Enum

| Value | Name | Description |
|-------|------|-------------|
| 0 | Pending | Invitation sent, no response yet |
| 1 | Accepted | Attendee confirmed attendance |
| 2 | Declined | Attendee declined the invitation |
| 3 | Tentative | Attendee may attend |

---

## Concurrency Control

The `RowVersion` property maps to PostgreSQL's `xmin` system column — a transaction ID that changes automatically on every row update. This provides optimistic concurrency without requiring an explicit version column.

**Flow:**
1. Client fetches an event — the response includes an `ETag` header containing the `RowVersion`
2. Client sends an update with `If-Match: {etag}` header
3. EF Core includes the `RowVersion` in the `WHERE` clause of the `UPDATE` statement
4. If another transaction modified the row, the `WHERE` clause matches zero rows
5. EF Core throws `DbUpdateConcurrencyException`, which the service catches and returns a conflict result

This approach is lightweight (no locks) and well-suited for web APIs where users may have stale data in their browser.

---

## Auto-Timestamping

`CreatedAt` and `UpdatedAt` are set automatically by overriding `SaveChanges` in `AppDbContext`:

- **New entities** → `CreatedAt` is set to `DateTime.UtcNow`
- **Modified entities** → `UpdatedAt` is set to `DateTime.UtcNow`

This ensures consistent timestamps without requiring every service method to manage them manually.

---

## Primary Key Generation

All primary keys use `uuid` with `gen_random_uuid()` as the database default. This allows:
- Client-side ID generation when needed
- No sequential ID guessing (security consideration)
- Safe distributed ID generation without coordination

---

## Seed Data

The migration includes sample data for development and demonstration:

| Event | Duration | Start | Attendees | Status |
|-------|----------|-------|-----------|--------|
| Morning Clinical Huddle | Standard (15 min) | 2026-03-16 09:00 | 2 | Active |
| Quarterly Clinical Governance Review | Extended (30 min) | 2026-03-17 14:00 | 2 | Active |
| New Patient Portal Training | Standard (15 min) | 2026-03-18 13:00 | 0 | Cancelled |
| Practice Staff Meeting | Extended (30 min) | 2026-03-20 10:00 | 2 | Active |

Seed data uses fixed GUIDs for repeatable migrations.

---

## Migration Management

Migrations are managed via EF Core CLI and applied automatically on API startup:

```bash
# Create a new migration after model changes
dotnet ef migrations add MigrationName --project src/DoctorScheduling.Data --startup-project src/DoctorScheduling.Api

# Apply migrations manually (if needed)
dotnet ef database update --project src/DoctorScheduling.Data --startup-project src/DoctorScheduling.Api
```

Auto-migration on startup is skipped when running in the `Testing` environment to avoid interfering with in-memory test databases.
