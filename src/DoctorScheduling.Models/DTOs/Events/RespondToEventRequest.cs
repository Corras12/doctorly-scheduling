using System.ComponentModel.DataAnnotations;
using DoctorScheduling.Models.Domain.Enums;

namespace DoctorScheduling.Models.DTOs.Events;

public class RespondToEventRequest
{
    [Required]
    public AttendanceStatus Status { get; set; }
}
