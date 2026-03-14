using DoctorScheduling.Models.DTOs.Events;
using DoctorScheduling.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DoctorScheduling.Api.Controllers;

/// <summary>
/// Manages calendar events and their attendees.
/// </summary>
[Route("api/[controller]")]
public class EventsController : ApiControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// Creates a new calendar event with optional attendees.
    /// </summary>
    /// <param name="request">The event details including doctor, title, duration type, and start time.</param>
    [HttpPost]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var result = await _eventService.CreateAsync(request);

        if (!result.IsSuccess)
            return BadRequest(CreateProblem("Invalid event", result.Error!));

        return CreatedAtAction(nameof(GetById), new { eventId = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Gets a calendar event by ID, including all attendees.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    [HttpGet("{eventId:guid}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid eventId)
    {
        var calendarEvent = await _eventService.GetByIdAsync(eventId);

        if (calendarEvent is null)
            return NotFound();

        Response.Headers["ETag"] = $"\"{calendarEvent.RowVersion}\"";
        return Ok(calendarEvent);
    }

    /// <summary>
    /// Updates an existing calendar event. Supports optimistic concurrency via If-Match header.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to update.</param>
    /// <param name="request">The updated event details.</param>
    [HttpPut("{eventId:guid}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid eventId, [FromBody] UpdateEventRequest request)
    {
        uint? expectedVersion = null;
        if (Request.Headers.TryGetValue("If-Match", out var etag))
        {
            var etagValue = etag.ToString().Trim('"');
            if (uint.TryParse(etagValue, out var version))
                expectedVersion = version;
        }

        var result = await _eventService.UpdateAsync(eventId, request, expectedVersion);
        return MapResult(result);
    }

    /// <summary>
    /// Permanently deletes a calendar event and all its attendees.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to delete.</param>
    [HttpDelete("{eventId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid eventId)
    {
        var result = await _eventService.DeleteAsync(eventId);

        if (!result.IsSuccess)
            return MapFailure(result);

        return NoContent();
    }

    /// <summary>
    /// Cancels a calendar event without deleting it. Notifies all attendees.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to cancel.</param>
    /// <param name="request">Optional cancellation reason.</param>
    [HttpPatch("{eventId:guid}/cancel")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid eventId, [FromBody] CancelEventRequest? request = null)
    {
        var result = await _eventService.CancelAsync(eventId, request?.Reason);
        return MapResult(result);
    }

    /// <summary>
    /// Lists calendar events with optional filters for date range, search text, and cancellation status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? cancelled = null)
    {
        var events = await _eventService.ListAsync(from, to, search, cancelled);
        return Ok(events);
    }

    /// <summary>
    /// Searches for events by title or description.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<EventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q = "")
    {
        var events = await _eventService.SearchAsync(q);
        return Ok(events);
    }

    /// <summary>
    /// Adds an attendee to an event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="request">The attendee's name and email address.</param>
    [HttpPost("{eventId:guid}/attendees")]
    [ProducesResponseType(typeof(AttendeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAttendee(Guid eventId, [FromBody] AttendeeRequest request)
    {
        var result = await _eventService.AddAttendeeAsync(eventId, request);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetById), new { eventId }, result.Value);

        return MapFailure(result);
    }

    /// <summary>
    /// Removes an attendee from an event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="attendeeId">The unique identifier of the attendee to remove.</param>
    [HttpDelete("{eventId:guid}/attendees/{attendeeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAttendee(Guid eventId, Guid attendeeId)
    {
        var result = await _eventService.RemoveAttendeeAsync(eventId, attendeeId);

        if (!result.IsSuccess)
            return MapFailure(result);

        return NoContent();
    }

    /// <summary>
    /// Allows an attendee to respond to an event invitation (Accept, Decline, or Tentative).
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="attendeeId">The unique identifier of the attendee responding.</param>
    /// <param name="request">The attendance status (1=Accepted, 2=Declined, 3=Tentative).</param>
    [HttpPatch("{eventId:guid}/attendees/{attendeeId:guid}/respond")]
    [ProducesResponseType(typeof(AttendeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Respond(Guid eventId, Guid attendeeId, [FromBody] RespondToEventRequest request)
    {
        var result = await _eventService.RespondAsync(eventId, attendeeId, request.Status);
        return MapResult(result);
    }

}
