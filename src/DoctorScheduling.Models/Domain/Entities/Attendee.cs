using DoctorScheduling.Models.Domain.Enums;

namespace DoctorScheduling.Models.Domain.Entities;

public class Attendee
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Pending;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Event Event { get; set; } = null!;
}
