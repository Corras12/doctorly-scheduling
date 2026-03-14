using DoctorScheduling.Models.Domain.Entities;
using DoctorScheduling.Services.Interfaces;

namespace DoctorScheduling.Tests.Helpers;

public class StubNotificationService : INotificationService
{
    public List<string> SentNotifications { get; } = [];

    public Task NotifyEventCreatedAsync(Event calendarEvent)
    {
        SentNotifications.Add($"Created:{calendarEvent.Title}");
        return Task.CompletedTask;
    }

    public Task NotifyEventUpdatedAsync(Event calendarEvent)
    {
        SentNotifications.Add($"Updated:{calendarEvent.Title}");
        return Task.CompletedTask;
    }

    public Task NotifyEventCancelledAsync(Event calendarEvent)
    {
        SentNotifications.Add($"Cancelled:{calendarEvent.Title}");
        return Task.CompletedTask;
    }

    public Task SendInvitationAsync(Event calendarEvent, Attendee attendee)
    {
        SentNotifications.Add($"Invitation:{attendee.Email}:{calendarEvent.Title}");
        return Task.CompletedTask;
    }
}
