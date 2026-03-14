using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorScheduling.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventDurationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationType",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                columns: new[] { "DurationType", "EndTime" },
                values: new object[] { 0, new DateTime(2026, 3, 16, 9, 15, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                columns: new[] { "DurationType", "EndTime" },
                values: new object[] { 1, new DateTime(2026, 3, 17, 14, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                columns: new[] { "DurationType", "EndTime" },
                values: new object[] { 1, new DateTime(2026, 3, 20, 10, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("d4e5f6a7-b8c9-0123-defa-234567890123"),
                columns: new[] { "DurationType", "EndTime" },
                values: new object[] { 0, new DateTime(2026, 3, 18, 13, 15, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationType",
                table: "Events");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                column: "EndTime",
                value: new DateTime(2026, 3, 16, 9, 30, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                column: "EndTime",
                value: new DateTime(2026, 3, 17, 15, 30, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                column: "EndTime",
                value: new DateTime(2026, 3, 20, 11, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("d4e5f6a7-b8c9-0123-defa-234567890123"),
                column: "EndTime",
                value: new DateTime(2026, 3, 18, 16, 0, 0, 0, DateTimeKind.Utc));
        }
    }
}
