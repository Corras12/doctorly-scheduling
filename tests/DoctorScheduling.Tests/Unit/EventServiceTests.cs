using DoctorScheduling.Data;
using DoctorScheduling.Models.Domain.Entities;
using DoctorScheduling.Models.Domain.Enums;
using DoctorScheduling.Models.DTOs.Events;
using DoctorScheduling.Services;
using DoctorScheduling.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DoctorScheduling.Tests.Unit;

public class EventServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly EventService _service;
    private readonly StubNotificationService _notifications;

    public EventServiceTests()
    {
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _notifications = new StubNotificationService();
        _service = new EventService(_db, _notifications);
    }

    public void Dispose() => _db.Dispose();

    private static CreateEventRequest ValidRequest(DateTime? start = null) => new()
    {
        Title = "Test Event",
        Description = "A test event",
        StartTime = start ?? DateTime.UtcNow.AddDays(1),
        EndTime = (start ?? DateTime.UtcNow.AddDays(1)).AddHours(1),
        Location = "Room A",
        Attendees =
        [
            new AttendeeRequest { Name = "Alice", Email = "alice@test.com" },
            new AttendeeRequest { Name = "Bob", Email = "bob@test.com" }
        ]
    };

    // --- Create ---

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsSuccess()
    {
        var result = await _service.CreateAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Test Event");
        result.Value.Attendees.Should().HaveCount(2);
        result.Value.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTimeRange_ReturnsFailure()
    {
        var request = ValidRequest();
        request.EndTime = request.StartTime.AddHours(-1);

        var result = await _service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("End time");
    }

    [Fact]
    public async Task CreateAsync_WithAttendees_SetsStatusToPending()
    {
        var result = await _service.CreateAsync(ValidRequest());
        result.Value!.Attendees.Should().AllSatisfy(a => a.Status.Should().Be(AttendanceStatus.Pending));
    }

    [Fact]
    public async Task CreateAsync_SendsNotification()
    {
        await _service.CreateAsync(ValidRequest());
        _notifications.SentNotifications.Should().Contain(n => n.StartsWith("Created:"));
    }

    // --- GetById ---

    [Fact]
    public async Task GetByIdAsync_ExistingEvent_ReturnsEvent()
    {
        var created = await _service.CreateAsync(ValidRequest());
        var result = await _service.GetByIdAsync(created.Value!.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Event");
        result.Attendees.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    // --- Update ---

    [Fact]
    public async Task UpdateAsync_ValidUpdate_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(ValidRequest());
        var updateReq = new UpdateEventRequest
        {
            Title = "Updated Title",
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(2),
            Location = "Room B"
        };

        var result = await _service.UpdateAsync(created.Value!.Id, updateReq);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated Title");
        result.Value.Location.Should().Be("Room B");
    }

    [Fact]
    public async Task UpdateAsync_CancelledEvent_ReturnsFailure()
    {
        var created = await _service.CreateAsync(ValidRequest());
        await _service.CancelAsync(created.Value!.Id);

        var updateReq = new UpdateEventRequest
        {
            Title = "Updated",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        var result = await _service.UpdateAsync(created.Value.Id, updateReq);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cancelled");
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ReturnsNotFound()
    {
        var updateReq = new UpdateEventRequest
        {
            Title = "Updated",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        var result = await _service.UpdateAsync(Guid.NewGuid(), updateReq);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(Models.Domain.ResultType.NotFound);
    }

    // --- Cancel ---

    [Fact]
    public async Task CancelAsync_ActiveEvent_SetsCancelledAndReason()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var result = await _service.CancelAsync(created.Value!.Id, "No longer needed");

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCancelled.Should().BeTrue();
        result.Value.CancellationReason.Should().Be("No longer needed");
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ReturnsFailure()
    {
        var created = await _service.CreateAsync(ValidRequest());
        await _service.CancelAsync(created.Value!.Id);

        var result = await _service.CancelAsync(created.Value.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already cancelled");
    }

    [Fact]
    public async Task CancelAsync_SendsNotification()
    {
        var created = await _service.CreateAsync(ValidRequest());

        await _service.CancelAsync(created.Value!.Id);

        _notifications.SentNotifications.Should().Contain(n => n.StartsWith("Cancelled:"));
    }

    // --- Delete ---

    [Fact]
    public async Task DeleteAsync_ExistingEvent_RemovesEvent()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var result = await _service.DeleteAsync(created.Value!.Id);

        result.IsSuccess.Should().BeTrue();
        var check = await _service.GetByIdAsync(created.Value.Id);
        check.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ReturnsNotFound()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(Models.Domain.ResultType.NotFound);
    }

    // --- List ---

    [Fact]
    public async Task ListAsync_FiltersByDateRange()
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var nextWeek = DateTime.UtcNow.Date.AddDays(7);

        await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Tomorrow Event",
            StartTime = tomorrow.AddHours(10),
            EndTime = tomorrow.AddHours(11)
        });
        await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Next Week Event",
            StartTime = nextWeek.AddHours(10),
            EndTime = nextWeek.AddHours(11)
        });

        var result = await _service.ListAsync(from: tomorrow, to: tomorrow.AddDays(1));

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Tomorrow Event");
    }

    [Fact]
    public async Task ListAsync_FiltersBySearchTerm()
    {
        await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Team Standup",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        });

        await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Project Review",
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(1)
        });

        var result = await _service.ListAsync(search: "standup");

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Team Standup");
    }

    [Fact]
    public async Task ListAsync_FiltersByCancelledStatus()
    {
        var created = await _service.CreateAsync(ValidRequest());
        await _service.CancelAsync(created.Value!.Id);
        await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Active Event",
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(1)
        });

        var activeOnly = await _service.ListAsync(isCancelled: false);

        activeOnly.Should().HaveCount(1);
        activeOnly[0].Title.Should().Be("Active Event");
    }

    // --- Search ---

    [Fact]
    public async Task SearchAsync_FindsByTitleOrDescription()
    {
        await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Morning Standup",
            Description = "Daily sync",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        });
        await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Design Review",
            Description = "Standup alternative",
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(1)
        });

        var result = await _service.SearchAsync("standup");

        result.Should().HaveCount(2);
    }

    // --- Respond (RSVP) ---

    [Fact]
    public async Task RespondAsync_ValidResponse_UpdatesStatus()
    {
        var created = await _service.CreateAsync(ValidRequest());
        var attendeeId = created.Value!.Attendees[0].Id;

        var result = await _service.RespondAsync(created.Value.Id, attendeeId, AttendanceStatus.Accepted);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(AttendanceStatus.Accepted);
    }

    [Fact]
    public async Task RespondAsync_EventCancelled_ReturnsFailure()
    {
        var created = await _service.CreateAsync(ValidRequest());
        await _service.CancelAsync(created.Value!.Id);
        var attendeeId = created.Value.Attendees[0].Id;

        var result = await _service.RespondAsync(created.Value.Id, attendeeId, AttendanceStatus.Accepted);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cancelled");
    }

    [Fact]
    public async Task RespondAsync_NonExistentAttendee_ReturnsNotFound()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var result = await _service.RespondAsync(created.Value!.Id, Guid.NewGuid(), AttendanceStatus.Accepted);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(Models.Domain.ResultType.NotFound);
    }

    [Fact]
    public async Task RespondAsync_SetToPending_ReturnsFailure()
    {
        var created = await _service.CreateAsync(ValidRequest());
        var attendeeId = created.Value!.Attendees[0].Id;

        var result = await _service.RespondAsync(created.Value.Id, attendeeId, AttendanceStatus.Pending);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Pending");
    }

    // --- Add Attendee ---

    [Fact]
    public async Task AddAttendeeAsync_ValidAttendee_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        });

        var result = await _service.AddAttendeeAsync(
            created.Value!.Id,
            new AttendeeRequest { Name = "Charlie", Email = "charlie@test.com" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Charlie");
        result.Value.Status.Should().Be(AttendanceStatus.Pending);
    }

    [Fact]
    public async Task AddAttendeeAsync_DuplicateEmail_ReturnsConflict()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var result = await _service.AddAttendeeAsync(
            created.Value!.Id,
            new AttendeeRequest { Name = "Alice Duplicate", Email = "alice@test.com" });

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(Models.Domain.ResultType.Conflict);
    }

    [Fact]
    public async Task AddAttendeeAsync_CancelledEvent_ReturnsFailure()
    {
        var created = await _service.CreateAsync(ValidRequest());
        await _service.CancelAsync(created.Value!.Id);

        var result = await _service.AddAttendeeAsync(
            created.Value.Id,
            new AttendeeRequest { Name = "Charlie", Email = "charlie@test.com" });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cancelled");
    }

    [Fact]
    public async Task AddAttendeeAsync_SendsInvitation()
    {
        var created = await _service.CreateAsync(new CreateEventRequest
        {
            Title = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        });

        await _service.AddAttendeeAsync(
            created.Value!.Id,
            new AttendeeRequest { Name = "Charlie", Email = "charlie@test.com" });

        _notifications.SentNotifications.Should()
            .Contain(n => n.Contains("charlie@test.com"));
    }

    // --- Remove Attendee ---

    [Fact]
    public async Task RemoveAttendeeAsync_ExistingAttendee_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(ValidRequest());
        var attendeeId = created.Value!.Attendees[0].Id;

        var result = await _service.RemoveAttendeeAsync(created.Value.Id, attendeeId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAttendeeAsync_NonExistent_ReturnsNotFound()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var result = await _service.RemoveAttendeeAsync(created.Value!.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(Models.Domain.ResultType.NotFound);
    }
}
