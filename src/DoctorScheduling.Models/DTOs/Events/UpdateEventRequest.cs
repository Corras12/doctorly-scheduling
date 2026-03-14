using System.ComponentModel.DataAnnotations;

namespace DoctorScheduling.Models.DTOs.Events;

public class UpdateEventRequest
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }
}
