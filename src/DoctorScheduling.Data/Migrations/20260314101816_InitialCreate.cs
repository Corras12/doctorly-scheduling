using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DoctorScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Specialisation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DurationType = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendees_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "Specialisation", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaa1111-bbbb-cccc-dddd-eeee11111111"), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), "s.mitchell@practice.nhs.uk", "Sarah", true, "Mitchell", "General Practice", null },
                    { new Guid("aaaa2222-bbbb-cccc-dddd-eeee22222222"), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), "p.sharma@practice.nhs.uk", "Priya", true, "Sharma", "Paediatrics", null },
                    { new Guid("aaaa3333-bbbb-cccc-dddd-eeee33333333"), new DateTime(2026, 2, 1, 8, 0, 0, 0, DateTimeKind.Utc), "d.thompson@practice.nhs.uk", "David", true, "Thompson", "Dermatology", null }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "CancellationReason", "CreatedAt", "Description", "DoctorId", "DurationType", "EndTime", "IsCancelled", "Location", "RowVersion", "StartTime", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), null, new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Daily briefing to review patient schedules, flag complex cases, and coordinate care across the practice.", new Guid("aaaa1111-bbbb-cccc-dddd-eeee11111111"), 0, new DateTime(2026, 3, 16, 9, 15, 0, 0, DateTimeKind.Utc), false, "Staff Room", 0L, new DateTime(2026, 3, 16, 9, 0, 0, 0, DateTimeKind.Utc), "Morning Clinical Huddle", null },
                    { new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), null, new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Review of clinical audit results, significant events, and practice performance against QOF targets.", new Guid("aaaa2222-bbbb-cccc-dddd-eeee22222222"), 1, new DateTime(2026, 3, 17, 14, 30, 0, 0, DateTimeKind.Utc), false, "Practice Meeting Room", 0L, new DateTime(2026, 3, 17, 14, 0, 0, 0, DateTimeKind.Utc), "Quarterly Clinical Governance Review", null },
                    { new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"), null, new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Monthly all-staff meeting covering operational updates, policy changes, and flu vaccination clinic planning.", new Guid("aaaa1111-bbbb-cccc-dddd-eeee11111111"), 1, new DateTime(2026, 3, 20, 10, 30, 0, 0, DateTimeKind.Utc), false, "Main Reception Area", 0L, new DateTime(2026, 3, 20, 10, 0, 0, 0, DateTimeKind.Utc), "Practice Staff Meeting", null },
                    { new Guid("d4e5f6a7-b8c9-0123-defa-234567890123"), "Vendor delayed system deployment", new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Hands-on training session for the new online patient booking and triage system.", new Guid("aaaa3333-bbbb-cccc-dddd-eeee33333333"), 0, new DateTime(2026, 3, 18, 13, 15, 0, 0, DateTimeKind.Utc), true, "IT Suite", 0L, new DateTime(2026, 3, 18, 13, 0, 0, 0, DateTimeKind.Utc), "New Patient Portal Training", null }
                });

            migrationBuilder.InsertData(
                table: "Attendees",
                columns: new[] { "Id", "CreatedAt", "Email", "EventId", "Name", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "j.okafor@practice.nhs.uk", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), "Nurse James Okafor", 1, null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "l.chen@practice.nhs.uk", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), "Reception Manager Lisa Chen", 1, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "s.mitchell@practice.nhs.uk", new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Dr Sarah Mitchell", 0, null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "t.ellis@practice.nhs.uk", new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Practice Manager Tom Ellis", 2, null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "j.okafor@practice.nhs.uk", new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"), "Nurse James Okafor", 3, null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "l.chen@practice.nhs.uk", new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"), "Reception Manager Lisa Chen", 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendees_EventId_Email",
                table: "Attendees",
                columns: new[] { "EventId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_Email",
                table: "Doctors",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_DoctorId_StartTime_EndTime",
                table: "Events",
                columns: new[] { "DoctorId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsCancelled",
                table: "Events",
                column: "IsCancelled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendees");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Doctors");
        }
    }
}
