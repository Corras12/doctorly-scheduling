using System.ComponentModel.DataAnnotations;

namespace DoctorScheduling.Models.DTOs.Events;

public class CancelEventRequest
{
    [StringLength(500)]
    public string? Reason { get; set; }
}
