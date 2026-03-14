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
    /// Registers a new doctor in the practice.
    /// </summary>
    /// <param name="request">The doctor's details including name, email, and specialisation.</param>
    [HttpPost]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request)
    {
        var result = await _doctorService.CreateAsync(request);

        if (!result.IsSuccess)
            return MapFailure(result);

        return CreatedAtAction(nameof(GetById), new { doctorId = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Lists all doctors. Filter by active status or search by name/email/specialisation.
    /// </summary>
    /// <param name="active">Filter by active status (true = active only, false = inactive only).</param>
    /// <param name="search">Search by name, email, or specialisation.</param>
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
    /// Gets a doctor by their unique identifier.
    /// </summary>
    /// <param name="doctorId">The unique identifier of the doctor.</param>
    [HttpGet("{doctorId:guid}")]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid doctorId)
    {
        var doctor = await _doctorService.GetByIdAsync(doctorId);

        if (doctor is null)
            return NotFound();

        return Ok(doctor);
    }

    /// <summary>
    /// Updates a doctor's details.
    /// </summary>
    /// <param name="doctorId">The unique identifier of the doctor to update.</param>
    /// <param name="request">The updated doctor details.</param>
    [HttpPut("{doctorId:guid}")]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid doctorId, [FromBody] UpdateDoctorRequest request)
    {
        return MapResult(await _doctorService.UpdateAsync(doctorId, request));
    }

    /// <summary>
    /// Deactivates a doctor (soft delete). Events are preserved but the doctor cannot be assigned new events.
    /// </summary>
    /// <param name="doctorId">The unique identifier of the doctor to deactivate.</param>
    [HttpPatch("{doctorId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid doctorId)
    {
        var result = await _doctorService.DeactivateAsync(doctorId);

        if (!result.IsSuccess)
            return MapFailure(result);

        return NoContent();
    }
}
