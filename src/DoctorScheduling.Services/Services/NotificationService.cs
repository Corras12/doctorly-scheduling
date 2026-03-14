using System.Text;
using DoctorScheduling.Models.Domain.Entities;
using DoctorScheduling.Models.Domain.Enums;
using DoctorScheduling.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DoctorScheduling.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ISendGridClient? _sendGridClient;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly bool _enabled;

    public NotificationService(ILogger<NotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _enabled = bool.TryParse(configuration["SendGrid:Enabled"], out var enabled) && enabled;
        _senderEmail = configuration["SendGrid:SenderEmail"] ?? "noreply@scheduling.local";
        _senderName = configuration["SendGrid:SenderName"] ?? "Calendar Scheduling";

        if (_enabled)
        {
            var apiKey = configuration["SendGrid:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                _sendGridClient = new SendGridClient(apiKey);
                _logger.LogInformation("SendGrid enabled — emails will be delivered");
            }
            else
            {
                _logger.LogWarning("SendGrid enabled but API key is missing — emails will be logged only");
                _enabled = false;
            }
        }
        else
        {
            _logger.LogInformation("SendGrid disabled — emails will be logged only");
        }
    }

    public async Task NotifyEventCreatedAsync(Event calendarEvent)
    {
        var ical = GenerateICalContent(calendarEvent);

        foreach (var attendee in calendarEvent.Attendees)
        {
            await SendEmailAsync(
                attendee.Email,
                attendee.Name,
                $"Invitation: {calendarEvent.Title}",
                $"You have been invited to '{calendarEvent.Title}' on {calendarEvent.StartTime:f}.\n\nLocation: {calendarEvent.Location ?? "TBD"}\n\n{calendarEvent.Description ?? ""}",
                ical);
        }
    }

    public async Task NotifyEventUpdatedAsync(Event calendarEvent)
    {
        var ical = GenerateICalContent(calendarEvent);

        foreach (var attendee in calendarEvent.Attendees)
        {
            await SendEmailAsync(
                attendee.Email,
                attendee.Name,
                $"Updated: {calendarEvent.Title}",
                $"The event '{calendarEvent.Title}' has been updated.\n\nNew time: {calendarEvent.StartTime:f} - {calendarEvent.EndTime:t}\nLocation: {calendarEvent.Location ?? "TBD"}",
                ical);
        }
    }

    public async Task NotifyEventCancelledAsync(Event calendarEvent)
    {
        var ical = GenerateICalContent(calendarEvent);

        foreach (var attendee in calendarEvent.Attendees)
        {
            await SendEmailAsync(
                attendee.Email,
                attendee.Name,
                $"Cancelled: {calendarEvent.Title}",
                $"The event '{calendarEvent.Title}' scheduled for {calendarEvent.StartTime:f} has been cancelled.\n\nReason: {calendarEvent.CancellationReason ?? "No reason provided"}",
                ical);
        }
    }

    public async Task SendInvitationAsync(Event calendarEvent, Attendee attendee)
    {
        var ical = GenerateICalContent(calendarEvent);

        await SendEmailAsync(
            attendee.Email,
            attendee.Name,
            $"Invitation: {calendarEvent.Title}",
            $"You have been invited to '{calendarEvent.Title}' on {calendarEvent.StartTime:f}.\n\nLocation: {calendarEvent.Location ?? "TBD"}\n\n{calendarEvent.Description ?? ""}",
            ical);
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string body, string? icalContent = null)
    {
        _logger.LogInformation("Sending email to {Email}: {Subject}", toEmail, subject);

        if (!_enabled || _sendGridClient is null)
        {
            _logger.LogDebug("Email body:\n{Body}", body);
            if (icalContent != null)
                _logger.LogDebug("iCal attachment:\n{ICal}", icalContent);
            return;
        }

        var msg = new SendGridMessage
        {
            From = new EmailAddress(_senderEmail, _senderName),
            Subject = subject,
            PlainTextContent = body
        };
        msg.AddTo(new EmailAddress(toEmail, toName));

        if (icalContent != null)
        {
            var icalBytes = Encoding.UTF8.GetBytes(icalContent);
            var base64 = Convert.ToBase64String(icalBytes);
            msg.AddAttachment("event.ics", base64, "text/calendar");
        }

        var response = await _sendGridClient.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        else
        {
            var responseBody = await response.Body.ReadAsStringAsync();
            _logger.LogError("Failed to send email to {Email}. Status: {Status}. Response: {Response}",
                toEmail, response.StatusCode, responseBody);
        }
    }

    internal static string GenerateICalContent(Event calendarEvent)
    {
        var attendeeLines = string.Join("\r\n",
            calendarEvent.Attendees.Select(a =>
            {
                var partStat = a.Status switch
                {
                    AttendanceStatus.Accepted => "ACCEPTED",
                    AttendanceStatus.Declined => "DECLINED",
                    AttendanceStatus.Tentative => "TENTATIVE",
                    _ => "NEEDS-ACTION"
                };
                return $"ATTENDEE;PARTSTAT={partStat};CN={a.Name}:mailto:{a.Email}";
            }));

        var status = calendarEvent.IsCancelled ? "CANCELLED" : "CONFIRMED";

        return $"""
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//DoctorScheduling//CalendarAPI//EN
            METHOD:REQUEST
            BEGIN:VEVENT
            UID:{calendarEvent.Id}
            DTSTART:{calendarEvent.StartTime:yyyyMMddTHHmmssZ}
            DTEND:{calendarEvent.EndTime:yyyyMMddTHHmmssZ}
            SUMMARY:{calendarEvent.Title}
            DESCRIPTION:{calendarEvent.Description ?? string.Empty}
            LOCATION:{calendarEvent.Location ?? string.Empty}
            STATUS:{status}
            {attendeeLines}
            END:VEVENT
            END:VCALENDAR
            """;
    }
}
