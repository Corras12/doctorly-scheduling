using DoctorScheduling.Models.Domain.Entities;

namespace DoctorScheduling.Models.DTOs.Doctors;

public record DoctorResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Specialisation,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static DoctorResponse FromEntity(Doctor d) => new(
        d.Id,
        d.FirstName,
        d.LastName,
        d.FullName,
        d.Email,
        d.Specialisation,
        d.IsActive,
        d.CreatedAt,
        d.UpdatedAt);
}
