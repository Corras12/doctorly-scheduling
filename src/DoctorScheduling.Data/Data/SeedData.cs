using DoctorScheduling.Models.Domain.Entities;
using DoctorScheduling.Models.Domain.Enums;

namespace DoctorScheduling.Data;

public static class SeedData
{
    // Doctors
    public static readonly Guid Doctor1Id = Guid.Parse("aaaa1111-bbbb-cccc-dddd-eeee11111111");
    public static readonly Guid Doctor2Id = Guid.Parse("aaaa2222-bbbb-cccc-dddd-eeee22222222");
    public static readonly Guid Doctor3Id = Guid.Parse("aaaa3333-bbbb-cccc-dddd-eeee33333333");

    // Events
    public static readonly Guid Event1Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid Event2Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    public static readonly Guid Event3Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");
    public static readonly Guid Event4Id = Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123");

    // Attendees
    public static readonly Guid Attendee1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Attendee2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Attendee3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Attendee4Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Attendee5Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid Attendee6Id = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static Doctor[] Doctors =>
    [
        new()
        {
            Id = Doctor1Id,
            FirstName = "Sarah",
            LastName = "Mitchell",
            Email = "s.mitchell@practice.nhs.uk",
            Specialisation = "General Practice",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = Doctor2Id,
            FirstName = "Priya",
            LastName = "Sharma",
            Email = "p.sharma@practice.nhs.uk",
            Specialisation = "Paediatrics",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = Doctor3Id,
            FirstName = "David",
            LastName = "Thompson",
            Email = "d.thompson@practice.nhs.uk",
            Specialisation = "Dermatology",
            IsActive = true,
            CreatedAt = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static Event[] Events =>
    [
        new()
        {
            Id = Event1Id,
            DoctorId = Doctor1Id,
            Title = "Morning Clinical Huddle",
            Description = "Daily briefing to review patient schedules, flag complex cases, and coordinate care across the practice.",
            DurationType = EventDurationType.Standard,
            StartTime = new DateTime(2026, 3, 16, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 16, 9, 15, 0, DateTimeKind.Utc),
            Location = "Staff Room",
            IsCancelled = false,
            CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = Event2Id,
            DoctorId = Doctor2Id,
            Title = "Quarterly Clinical Governance Review",
            Description = "Review of clinical audit results, significant events, and practice performance against QOF targets.",
            DurationType = EventDurationType.Extended,
            StartTime = new DateTime(2026, 3, 17, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 17, 14, 30, 0, DateTimeKind.Utc),
            Location = "Practice Meeting Room",
            IsCancelled = false,
            CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = Event3Id,
            DoctorId = Doctor1Id,
            Title = "Practice Staff Meeting",
            Description = "Monthly all-staff meeting covering operational updates, policy changes, and flu vaccination clinic planning.",
            DurationType = EventDurationType.Extended,
            StartTime = new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 20, 10, 30, 0, DateTimeKind.Utc),
            Location = "Main Reception Area",
            IsCancelled = false,
            CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = Event4Id,
            DoctorId = Doctor3Id,
            Title = "New Patient Portal Training",
            Description = "Hands-on training session for the new online patient booking and triage system.",
            DurationType = EventDurationType.Standard,
            StartTime = new DateTime(2026, 3, 18, 13, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 18, 13, 15, 0, DateTimeKind.Utc),
            Location = "IT Suite",
            IsCancelled = true,
            CancellationReason = "Vendor delayed system deployment",
            CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static Attendee[] Attendees =>
    [
        new() { Id = Attendee1Id, EventId = Event1Id, Name = "Nurse James Okafor", Email = "j.okafor@practice.nhs.uk", Status = AttendanceStatus.Accepted, CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc) },
        new() { Id = Attendee2Id, EventId = Event1Id, Name = "Reception Manager Lisa Chen", Email = "l.chen@practice.nhs.uk", Status = AttendanceStatus.Accepted, CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc) },
        new() { Id = Attendee3Id, EventId = Event2Id, Name = "Dr Sarah Mitchell", Email = "s.mitchell@practice.nhs.uk", Status = AttendanceStatus.Pending, CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc) },
        new() { Id = Attendee4Id, EventId = Event2Id, Name = "Practice Manager Tom Ellis", Email = "t.ellis@practice.nhs.uk", Status = AttendanceStatus.Declined, CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc) },
        new() { Id = Attendee5Id, EventId = Event3Id, Name = "Nurse James Okafor", Email = "j.okafor@practice.nhs.uk", Status = AttendanceStatus.Tentative, CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc) },
        new() { Id = Attendee6Id, EventId = Event3Id, Name = "Reception Manager Lisa Chen", Email = "l.chen@practice.nhs.uk", Status = AttendanceStatus.Accepted, CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc) }
    ];
}
