using DoctorScheduling.Models.Domain.Entities;
using DoctorScheduling.Models.Domain.Enums;

namespace DoctorScheduling.Models.DTOs.Events;

public record EventSummaryResponse(
    Guid Id,
    Guid DoctorId,
    string DoctorName,
    string Title,
    EventDurationType DurationType,
    int DurationMinutes,
    DateTime StartTime,
    DateTime EndTime,
    bool IsCancelled,
    int AttendeeCount)
{
    public static EventSummaryResponse FromEntity(Event e) => new(
        e.Id,
        e.DoctorId,
        e.Doctor?.FullName ?? "Unknown",
        e.Title,
        e.DurationType,
        e.DurationMinutes,
        e.StartTime,
        e.EndTime,
        e.IsCancelled,
        e.Attendees.Count);
}
