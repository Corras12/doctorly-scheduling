using System.ComponentModel.DataAnnotations;

namespace DoctorScheduling.Models.DTOs.Events;

public class AttendeeRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(254), EmailAddress]
    public string Email { get; set; } = string.Empty;
}
