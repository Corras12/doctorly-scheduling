using DoctorScheduling.Models.Domain;
using DoctorScheduling.Models.DTOs.Doctors;

namespace DoctorScheduling.Services.Interfaces;

public interface IDoctorService
{
    Task<Result<DoctorResponse>> CreateAsync(CreateDoctorRequest request);
    Task<DoctorResponse?> GetByIdAsync(Guid id);
    Task<Result<DoctorResponse>> UpdateAsync(Guid id, UpdateDoctorRequest request);
    Task<Result<bool>> DeactivateAsync(Guid id);
    Task<List<DoctorResponse>> ListAsync(bool? isActive = null, string? search = null);
}
