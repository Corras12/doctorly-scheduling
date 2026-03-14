using DoctorScheduling.Data;
using DoctorScheduling.Models.Domain;
using DoctorScheduling.Models.Domain.Entities;
using DoctorScheduling.Models.Domain.Enums;
using DoctorScheduling.Models.DTOs.Events;
using DoctorScheduling.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorScheduling.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public EventService(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<EventResponse>> CreateAsync(CreateEventRequest request)
    {
        var calendarEvent = new Event
        {
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Location = request.Location
        };

        if (!calendarEvent.HasValidTimeRange())
            return Result<EventResponse>.Failure("End time must be after start time.");

        if (request.Attendees != null)
        {
            foreach (var attendeeReq in request.Attendees)
            {
                calendarEvent.Attendees.Add(new Attendee
                {
                    Name = attendeeReq.Name,
                    Email = attendeeReq.Email,
                    Status = AttendanceStatus.Pending
                });
            }
        }

        _db.Events.Add(calendarEvent);
        await _db.SaveChangesAsync();

        var created = await _db.Events
            .Include(e => e.Attendees)
            .FirstAsync(e => e.Id == calendarEvent.Id);

        await _notifications.NotifyEventCreatedAsync(created);

        return Result<EventResponse>.Success(EventResponse.FromEntity(created));
    }

    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var calendarEvent = await _db.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id);

        return calendarEvent is null ? null : EventResponse.FromEntity(calendarEvent);
    }

    public async Task<Result<EventResponse>> UpdateAsync(Guid id, UpdateEventRequest request, uint? expectedVersion = null)
    {
        var calendarEvent = await _db.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (calendarEvent is null)
            return Result<EventResponse>.NotFound("Event not found.");

        if (calendarEvent.IsCancelled)
            return Result<EventResponse>.Failure("Cannot update a cancelled event.");

        if (expectedVersion.HasValue && calendarEvent.RowVersion != expectedVersion.Value)
            return Result<EventResponse>.ConflictFailure(
                "The event has been modified by another user. Please refresh and try again.");

        calendarEvent.Title = request.Title;
        calendarEvent.Description = request.Description;
        calendarEvent.StartTime = request.StartTime;
        calendarEvent.EndTime = request.EndTime;
        calendarEvent.Location = request.Location;

        if (!calendarEvent.HasValidTimeRange())
            return Result<EventResponse>.Failure("End time must be after start time.");

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<EventResponse>.ConflictFailure(
                "The event has been modified by another user. Please refresh and try again.");
        }

        await _notifications.NotifyEventUpdatedAsync(calendarEvent);

        return Result<EventResponse>.Success(EventResponse.FromEntity(calendarEvent));
    }

    public async Task<Result<EventResponse>> CancelAsync(Guid id, string? reason = null)
    {
        var calendarEvent = await _db.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (calendarEvent is null)
            return Result<EventResponse>.NotFound("Event not found.");

        if (calendarEvent.IsCancelled)
            return Result<EventResponse>.Failure("Event is already cancelled.");

        calendarEvent.Cancel(reason);
        await _db.SaveChangesAsync();

        await _notifications.NotifyEventCancelledAsync(calendarEvent);

        return Result<EventResponse>.Success(EventResponse.FromEntity(calendarEvent));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var calendarEvent = await _db.Events.FindAsync(id);

        if (calendarEvent is null)
            return Result<bool>.NotFound("Event not found.");

        _db.Events.Remove(calendarEvent);
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<List<EventSummaryResponse>> ListAsync(
        DateTime? from = null, DateTime? to = null, string? search = null, bool? isCancelled = null)
    {
        var query = _db.Events.Include(e => e.Attendees).AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.StartTime >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.EndTime <= to.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(term) ||
                (e.Description != null && e.Description.ToLower().Contains(term)));
        }

        if (isCancelled.HasValue)
            query = query.Where(e => e.IsCancelled == isCancelled.Value);

        var events = await query.OrderBy(e => e.StartTime).ToListAsync();
        return events.Select(EventSummaryResponse.FromEntity).ToList();
    }

    public async Task<List<EventSummaryResponse>> SearchAsync(string searchQuery)
    {
        return await ListAsync(search: searchQuery);
    }

    public async Task<Result<AttendeeResponse>> RespondAsync(Guid eventId, Guid attendeeId, AttendanceStatus status)
    {
        var calendarEvent = await _db.Events.FindAsync(eventId);

        if (calendarEvent is null)
            return Result<AttendeeResponse>.NotFound("Event not found.");

        if (calendarEvent.IsCancelled)
            return Result<AttendeeResponse>.Failure("Cannot respond to a cancelled event.");

        var attendee = await _db.Attendees
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.EventId == eventId);

        if (attendee is null)
            return Result<AttendeeResponse>.NotFound("Attendee not found.");

        if (status == AttendanceStatus.Pending)
            return Result<AttendeeResponse>.Failure("Cannot set status back to Pending.");

        attendee.Status = status;
        await _db.SaveChangesAsync();

        return Result<AttendeeResponse>.Success(AttendeeResponse.FromEntity(attendee));
    }

    public async Task<Result<AttendeeResponse>> AddAttendeeAsync(Guid eventId, AttendeeRequest request)
    {
        var calendarEvent = await _db.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (calendarEvent is null)
            return Result<AttendeeResponse>.NotFound("Event not found.");

        if (calendarEvent.IsCancelled)
            return Result<AttendeeResponse>.Failure("Cannot add attendees to a cancelled event.");

        var duplicate = calendarEvent.Attendees.Any(a =>
            a.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            return Result<AttendeeResponse>.ConflictFailure(
                $"Attendee with email '{request.Email}' is already invited to this event.");

        var attendee = new Attendee
        {
            EventId = eventId,
            Name = request.Name,
            Email = request.Email,
            Status = AttendanceStatus.Pending
        };

        _db.Attendees.Add(attendee);
        await _db.SaveChangesAsync();

        await _notifications.SendInvitationAsync(calendarEvent, attendee);

        return Result<AttendeeResponse>.Success(AttendeeResponse.FromEntity(attendee));
    }

    public async Task<Result<bool>> RemoveAttendeeAsync(Guid eventId, Guid attendeeId)
    {
        var attendee = await _db.Attendees
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.EventId == eventId);

        if (attendee is null)
            return Result<bool>.NotFound("Attendee not found.");

        _db.Attendees.Remove(attendee);
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
