using DoctorScheduling.Data;
using DoctorScheduling.Models.Domain;
using DoctorScheduling.Models.Domain.Entities;
using DoctorScheduling.Models.DTOs.Doctors;
using DoctorScheduling.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DoctorScheduling.Services;

public class DoctorService : IDoctorService
{
    private readonly AppDbContext _db;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(AppDbContext db, ILogger<DoctorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<DoctorResponse>> CreateAsync(CreateDoctorRequest request)
    {
        var emailExists = await _db.Doctors
            .AnyAsync(d => d.Email.ToLower() == request.Email.ToLower());

        if (emailExists)
        {
            _logger.LogWarning("Doctor creation failed — duplicate email {Email}", request.Email);
            return Result<DoctorResponse>.ConflictFailure(
                $"A doctor with email '{request.Email}' already exists.");
        }

        var doctor = new Doctor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Specialisation = request.Specialisation
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Doctor {DoctorId} created: {DoctorName} ({Email})", doctor.Id, doctor.FullName, doctor.Email);

        return Result<DoctorResponse>.Success(DoctorResponse.FromEntity(doctor));
    }

    public async Task<DoctorResponse?> GetByIdAsync(Guid id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        return doctor is null ? null : DoctorResponse.FromEntity(doctor);
    }

    public async Task<Result<DoctorResponse>> UpdateAsync(Guid id, UpdateDoctorRequest request)
    {
        var doctor = await _db.Doctors.FindAsync(id);

        if (doctor is null)
            return Result<DoctorResponse>.NotFound("Doctor not found.");

        var emailExists = await _db.Doctors
            .AnyAsync(d => d.Id != id && d.Email.ToLower() == request.Email.ToLower());

        if (emailExists)
        {
            _logger.LogWarning("Doctor update failed — duplicate email {Email} for doctor {DoctorId}", request.Email, id);
            return Result<DoctorResponse>.ConflictFailure(
                $"A doctor with email '{request.Email}' already exists.");
        }

        doctor.FirstName = request.FirstName;
        doctor.LastName = request.LastName;
        doctor.Email = request.Email;
        doctor.Specialisation = request.Specialisation;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Doctor {DoctorId} updated: {DoctorName}", id, doctor.FullName);

        return Result<DoctorResponse>.Success(DoctorResponse.FromEntity(doctor));
    }

    public async Task<Result<bool>> DeactivateAsync(Guid id)
    {
        var doctor = await _db.Doctors.FindAsync(id);

        if (doctor is null)
            return Result<bool>.NotFound("Doctor not found.");

        if (!doctor.IsActive)
            return Result<bool>.Failure("Doctor is already deactivated.");

        doctor.IsActive = false;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Doctor {DoctorId} deactivated: {DoctorName}", id, doctor.FullName);

        return Result<bool>.Success(true);
    }

    public async Task<List<DoctorResponse>> ListAsync(bool? isActive = null, string? search = null)
    {
        var query = _db.Doctors.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(d => d.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(d =>
                d.FirstName.ToLower().Contains(term) ||
                d.LastName.ToLower().Contains(term) ||
                d.Email.ToLower().Contains(term) ||
                (d.Specialisation != null && d.Specialisation.ToLower().Contains(term)));
        }

        var doctors = await query.OrderBy(d => d.LastName).ThenBy(d => d.FirstName).ToListAsync();
        return doctors.Select(DoctorResponse.FromEntity).ToList();
    }
}
