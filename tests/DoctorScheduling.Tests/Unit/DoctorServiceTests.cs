using DoctorScheduling.Data;
using DoctorScheduling.Models.DTOs.Doctors;
using DoctorScheduling.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DoctorScheduling.Tests.Unit;

public class DoctorServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _service = new DoctorService(_db, NullLogger<DoctorService>.Instance);
    }

    public void Dispose() => _db.Dispose();

    private static CreateDoctorRequest ValidRequest() => new()
    {
        FirstName = "Sarah",
        LastName = "Mitchell",
        Email = "s.mitchell@practice.nhs.uk",
        Specialisation = "General Practice"
    };

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsSuccess()
    {
        var result = await _service.CreateAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be("Sarah");
        result.Value.LastName.Should().Be("Mitchell");
        result.Value.FullName.Should().Be("Sarah Mitchell");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsConflict()
    {
        await _service.CreateAsync(ValidRequest());

        var duplicate = ValidRequest();
        duplicate.FirstName = "Another";

        var result = await _service.CreateAsync(duplicate);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(Models.Domain.ResultType.Conflict);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDoctor_ReturnsDoctor()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var result = await _service.GetByIdAsync(created.Value!.Id);

        result.Should().NotBeNull();
        result!.FullName.Should().Be("Sarah Mitchell");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ValidUpdate_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var result = await _service.UpdateAsync(created.Value!.Id, new UpdateDoctorRequest
        {
            FirstName = "Sarah",
            LastName = "Mitchell-Jones",
            Email = "s.mitchell@practice.nhs.uk",
            Specialisation = "Paediatrics"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.LastName.Should().Be("Mitchell-Jones");
        result.Value.Specialisation.Should().Be("Paediatrics");
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ReturnsNotFound()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateDoctorRequest
        {
            FirstName = "Test",
            LastName = "Doctor",
            Email = "test@practice.nhs.uk"
        });

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(Models.Domain.ResultType.NotFound);
    }

    [Fact]
    public async Task DeactivateAsync_ActiveDoctor_Succeeds()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var result = await _service.DeactivateAsync(created.Value!.Id);

        result.IsSuccess.Should().BeTrue();
        var doctor = await _service.GetByIdAsync(created.Value.Id);
        doctor!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateAsync_AlreadyInactive_ReturnsFailure()
    {
        var created = await _service.CreateAsync(ValidRequest());
        await _service.DeactivateAsync(created.Value!.Id);

        var result = await _service.DeactivateAsync(created.Value.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already deactivated");
    }

    [Fact]
    public async Task ListAsync_FiltersByActiveStatus()
    {
        await _service.CreateAsync(ValidRequest());

        var request2 = new CreateDoctorRequest
        {
            FirstName = "Priya",
            LastName = "Sharma",
            Email = "p.sharma@practice.nhs.uk"
        };
        var created2 = await _service.CreateAsync(request2);
        await _service.DeactivateAsync(created2.Value!.Id);

        var activeOnly = await _service.ListAsync(isActive: true);

        activeOnly.Should().HaveCount(1);
        activeOnly[0].FullName.Should().Be("Sarah Mitchell");
    }

    [Fact]
    public async Task ListAsync_SearchesByName()
    {
        await _service.CreateAsync(ValidRequest());
        await _service.CreateAsync(new CreateDoctorRequest
        {
            FirstName = "Priya",
            LastName = "Sharma",
            Email = "p.sharma@practice.nhs.uk",
            Specialisation = "Paediatrics"
        });

        var result = await _service.ListAsync(search: "sharma");

        result.Should().HaveCount(1);
        result[0].FullName.Should().Be("Priya Sharma");
    }
}
