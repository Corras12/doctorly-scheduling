using DoctorScheduling.Models.Domain.Entities;

namespace DoctorScheduling.Services.Interfaces;

public interface INotificationService
{
    Task NotifyEventCreatedAsync(Event calendarEvent);
    Task NotifyEventUpdatedAsync(Event calendarEvent);
    Task NotifyEventCancelledAsync(Event calendarEvent);
    Task SendInvitationAsync(Event calendarEvent, Attendee attendee);
}
