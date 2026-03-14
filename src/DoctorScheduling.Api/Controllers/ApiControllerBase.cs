using DoctorScheduling.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DoctorScheduling.Api.Controllers;

/// <summary>
/// Base controller providing shared Result-to-HTTP response mapping.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult MapResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return MapFailure(result);
    }

    protected IActionResult MapFailure<T>(Result<T> result) => result.Type switch
    {
        ResultType.NotFound => NotFound(CreateProblem("Not found", result.Error!)),
        ResultType.Conflict => Conflict(CreateProblem("Conflict", result.Error!)),
        _ => BadRequest(CreateProblem("Bad request", result.Error!))
    };

    protected static ProblemDetails CreateProblem(string title, string detail) => new()
    {
        Title = title,
        Detail = detail
    };
}
