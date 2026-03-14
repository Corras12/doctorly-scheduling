using DoctorScheduling.Models.Domain.Entities;

namespace DoctorScheduling.Models.DTOs.Events;

public record EventResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string? Location,
    bool IsCancelled,
    string? CancellationReason,
    List<AttendeeResponse> Attendees,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    uint RowVersion)
{
    public static EventResponse FromEntity(Event e) => new(
        e.Id,
        e.Title,
        e.Description,
        e.StartTime,
        e.EndTime,
        e.Location,
        e.IsCancelled,
        e.CancellationReason,
        e.Attendees.Select(AttendeeResponse.FromEntity).ToList(),
        e.CreatedAt,
        e.UpdatedAt,
        e.RowVersion);
}
