using DoctorScheduling.Models.Domain.Enums;

namespace DoctorScheduling.Models.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EventDurationType DurationType { get; set; } = EventDurationType.Standard;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }
    public uint RowVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Attendee> Attendees { get; set; } = new List<Attendee>();

    /// <summary>
    /// Gets the duration in minutes based on the DurationType.
    /// </summary>
    public int DurationMinutes => DurationType switch
    {
        EventDurationType.Standard => 15,
        EventDurationType.Extended => 30,
        _ => 15
    };

    /// <summary>
    /// Calculates the EndTime from StartTime and DurationType.
    /// </summary>
    public void CalculateEndTime()
    {
        EndTime = StartTime.AddMinutes(DurationMinutes);
    }

    public bool HasValidTimeRange() => EndTime > StartTime;

    public bool OverlapsWith(DateTime start, DateTime end) =>
        StartTime < end && EndTime > start;

    public void Cancel(string? reason = null)
    {
        IsCancelled = true;
        CancellationReason = reason;
    }
}
