using DoctorScheduling.Models.Domain.Entities;
using DoctorScheduling.Models.Domain.Enums;

namespace DoctorScheduling.Models.DTOs.Events;

public record AttendeeResponse(Guid Id,string Name,string Email,AttendanceStatus Status)
{
    public static AttendeeResponse FromEntity(Attendee a) => new(
        a.Id,
        a.Name,
        a.Email,
        a.Status);
}
