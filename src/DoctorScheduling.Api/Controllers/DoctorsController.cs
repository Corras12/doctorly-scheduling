using DoctorScheduling.Models.DTOs.Doctors;
using DoctorScheduling.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DoctorScheduling.Api.Controllers;

/// <summary>
/// Manages doctors within the practice.
/// </summary>
[Route("api/[controller]")]
public class DoctorsController : ApiControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>
    /// Creates a new doctor.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request)
    {
        var result = await _doctorService.CreateAsync(request);

        if (!result.IsSuccess)
            return MapFailure(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Lists all doctors. Filter by active status or search by name/email/specialisation.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DoctorResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] bool? active = null,
        [FromQuery] string? search = null)
    {
        var doctors = await _doctorService.ListAsync(active, search);
        return Ok(doctors);
    }

    /// <summary>
    /// Gets a doctor by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);

        if (doctor is null)
            return NotFound();

        return Ok(doctor);
    }

    /// <summary>
    /// Updates a doctor's details.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRequest request)
    {
        return MapResult(await _doctorService.UpdateAsync(id, request));
    }

    /// <summary>
    /// Deactivates a doctor (soft delete). Events are preserved but the doctor cannot be assigned new events.
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _doctorService.DeactivateAsync(id);

        if (!result.IsSuccess)
            return MapFailure(result);

        return NoContent();
    }
}
