using System.ComponentModel.DataAnnotations;

namespace DoctorScheduling.Models.DTOs.Doctors;

public class CreateDoctorRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(254), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Specialisation { get; set; }
}
