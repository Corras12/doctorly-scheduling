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
    [HttpPost]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var result = await _eventService.CreateAsync(request);

        if (!result.IsSuccess)
            return BadRequest(CreateProblem("Invalid event", result.Error!));

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Gets a calendar event by ID, including all attendees.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var calendarEvent = await _eventService.GetByIdAsync(id);

        if (calendarEvent is null)
            return NotFound();

        Response.Headers["ETag"] = $"\"{calendarEvent.RowVersion}\"";
        return Ok(calendarEvent);
    }

    /// <summary>
    /// Updates an existing calendar event. Supports optimistic concurrency via If-Match header.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        uint? expectedVersion = null;
        if (Request.Headers.TryGetValue("If-Match", out var etag))
        {
            var etagValue = etag.ToString().Trim('"');
            if (uint.TryParse(etagValue, out var version))
                expectedVersion = version;
        }

        var result = await _eventService.UpdateAsync(id, request, expectedVersion);
        return MapResult(result);
    }

    /// <summary>
    /// Permanently deletes a calendar event and all its attendees.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _eventService.DeleteAsync(id);

        if (!result.IsSuccess)
            return MapFailure(result);

        return NoContent();
    }

    /// <summary>
    /// Cancels a calendar event without deleting it. Notifies all attendees.
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelEventRequest? request = null)
    {
        var result = await _eventService.CancelAsync(id, request?.Reason);
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
    [HttpPost("{id:guid}/attendees")]
    [ProducesResponseType(typeof(AttendeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAttendee(Guid id, [FromBody] AttendeeRequest request)
    {
        var result = await _eventService.AddAttendeeAsync(id, request);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetById), new { id }, result.Value);

        return MapFailure(result);
    }

    /// <summary>
    /// Removes an attendee from an event.
    /// </summary>
    [HttpDelete("{id:guid}/attendees/{attendeeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAttendee(Guid id, Guid attendeeId)
    {
        var result = await _eventService.RemoveAttendeeAsync(id, attendeeId);

        if (!result.IsSuccess)
            return MapFailure(result);

        return NoContent();
    }

    /// <summary>
    /// Allows an attendee to respond to an event invitation (Accept, Decline, or Tentative).
    /// </summary>
    [HttpPatch("{id:guid}/attendees/{attendeeId:guid}/respond")]
    [ProducesResponseType(typeof(AttendeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Respond(Guid id, Guid attendeeId, [FromBody] RespondToEventRequest request)
    {
        var result = await _eventService.RespondAsync(id, attendeeId, request.Status);
        return MapResult(result);
    }

}
