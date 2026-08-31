using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Extensions;
using Shop.Domain.Errors;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Успех → 200 OK со значением.</summary>
    protected IActionResult HandleResult<T>(Result<T, Error> result)
        => result.IsSuccess
            ? Ok(result.Value)
            : ToActionResult(result.Error);

    /// <summary>Успех → результат, который построит вызывающий (201, 202 и т.д.).</summary>
    protected IActionResult HandleResult<T>(Result<T, Error> result, Func<T, IActionResult> onSuccess)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : ToActionResult(result.Error);

    /// <summary>Успех без данных → 204 No Content.</summary>
    protected IActionResult HandleResult(UnitResult<Error> result)
        => result.IsSuccess
            ? NoContent()
            : ToActionResult(result.Error);

    private IActionResult ToActionResult(Error error)
    {
        var problemDetails = error.ToProblemDetails();

        return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
    }
}