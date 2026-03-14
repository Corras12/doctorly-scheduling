# How to Run

## Prerequisites

| Requirement | Version | Notes |
|------------|---------|-------|
| .NET SDK | 10.0+ | [Download](https://dotnet.microsoft.com/download) |
| PostgreSQL | 13+ | Via Docker, local install, or cloud-hosted |
| EF Core CLI | Latest | `dotnet tool install --global dotnet-ef` |
| Docker *(optional)* | Latest | For running PostgreSQL without a local install |
| SendGrid *(optional)* | — | Free account for real email delivery |

---

## Step 1: Start PostgreSQL

### Option A — Docker (recommended)

```bash
docker run -d \
  --name postgres-scheduling \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres
```

### Option B — Local PostgreSQL

Ensure PostgreSQL is running on `localhost:5432` with username `postgres` and password `postgres`.

If your setup differs, update the connection string in `src/DoctorScheduling.Api/appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DoctorSchedulingDb;Username=postgres;Password=postgres"
}
```

---

## Step 2: Restore Dependencies

```bash
dotnet restore
```

---

## Step 3: Run the API

```bash
dotnet run --project src/DoctorScheduling.Api
```

On first startup the application will:
1. **Apply all database migrations** automatically (creates tables, indexes, constraints)
2. **Seed sample data** (4 events with attendees for testing)
3. **Launch the API** on `https://localhost:7287` and `http://localhost:5123`

---

## Step 4: Explore the API

Open Swagger UI in your browser:

```
https://localhost:7287/swagger
```

From Swagger UI you can:
- Browse all available endpoints with descriptions
- View request/response schemas and validation rules
- Use **"Try it out"** to execute requests against the seeded data
- Download the OpenAPI spec at `/swagger/v1/swagger.json`

---

## Running Tests

```bash
dotnet test
```

Tests use the **EF Core InMemory provider** — no PostgreSQL setup is required. All 28 tests should pass.

To run with verbose output:

```bash
dotnet test --verbosity normal
```

---

## Email Notifications (Optional)

The API supports real email delivery via SendGrid with iCal (`.ics`) calendar attachments.

### To Enable

1. Create a [SendGrid account](https://sendgrid.com/) (free tier available — 100 emails/day)
2. Generate an API key in SendGrid dashboard
3. Verify a sender email address
4. Update `src/DoctorScheduling.Api/appsettings.json`:

```json
"SendGrid": {
    "Enabled": true,
    "ApiKey": "SG.your-api-key-here",
    "SenderEmail": "your-verified@email.com",
    "SenderName": "Calendar Scheduling"
}
```

### When Disabled (Default)

Notifications are logged to the console with the generated iCal content. The API functions normally — email delivery is not a hard dependency.

### What Gets Sent

- **Event created** → All attendees receive an invitation with `.ics` attachment
- **Event updated** → All attendees receive an updated calendar entry
- **Event cancelled** → All attendees are notified of the cancellation
- **Attendee added** → New attendee receives an invitation

Recipients can open `.ics` attachments directly in Outlook, Google Calendar, Apple Calendar, etc.

---

## Generating an API Client

The OpenAPI specification can be used to auto-generate client libraries:

```bash
# C# client via NSwag
nswag openapi2csclient /input:https://localhost:7287/swagger/v1/swagger.json /output:CalendarClient.cs
```

This enables third-party systems to consume the API with strongly-typed models and method calls.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Connection refused on port 5432 | Ensure PostgreSQL is running: `docker ps` or check your local service |
| SSL certificate warning in browser | Expected for localhost — click "Advanced" → "Proceed" |
| Migrations fail | Ensure EF Core CLI is installed: `dotnet tool install --global dotnet-ef` |
| Tests fail with package errors | Run `dotnet restore` before `dotnet test` |
