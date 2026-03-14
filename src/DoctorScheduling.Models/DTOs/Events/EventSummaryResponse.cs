using DoctorScheduling.Models.Domain.Entities;

namespace DoctorScheduling.Models.DTOs.Events;

public record EventSummaryResponse(
    Guid Id,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    bool IsCancelled,
    int AttendeeCount)
{
    public static EventSummaryResponse FromEntity(Event e) => new(
        e.Id,
        e.Title,
        e.StartTime,
        e.EndTime,
        e.IsCancelled,
        e.Attendees.Count);
}
