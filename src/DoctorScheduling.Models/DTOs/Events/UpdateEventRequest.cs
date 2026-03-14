using System.ComponentModel.DataAnnotations;
using DoctorScheduling.Models.Domain.Enums;

namespace DoctorScheduling.Models.DTOs.Events;

public class UpdateEventRequest
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// The duration type: Standard (15 minutes) or Extended (30 minutes).
    /// </summary>
    [Required]
    public EventDurationType DurationType { get; set; } = EventDurationType.Standard;

    [Required]
    public DateTime StartTime { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }
}
