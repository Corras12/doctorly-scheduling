using DoctorScheduling.Models.Domain.Enums;
using DoctorScheduling.Models.DTOs.Events;
using DoctorScheduling.Models.Domain;

namespace DoctorScheduling.Services.Interfaces;

public interface IEventService
{
    Task<Result<EventResponse>> CreateAsync(CreateEventRequest request);
    Task<EventResponse?> GetByIdAsync(Guid id);
    Task<Result<EventResponse>> UpdateAsync(Guid id, UpdateEventRequest request, uint? expectedVersion = null);
    Task<Result<EventResponse>> CancelAsync(Guid id, string? reason = null);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<List<EventSummaryResponse>> ListAsync(DateTime? from = null, DateTime? to = null, string? search = null, bool? isCancelled = null);
    Task<List<EventSummaryResponse>> SearchAsync(string query);
    Task<Result<AttendeeResponse>> RespondAsync(Guid eventId, Guid attendeeId, AttendanceStatus status);
    Task<Result<AttendeeResponse>> AddAttendeeAsync(Guid eventId, AttendeeRequest request);
    Task<Result<bool>> RemoveAttendeeAsync(Guid eventId, Guid attendeeId);
}
